/* Copyright (c) 2008 Michael Lidgren

Permission is hereby granted, free of charge, to any person obtaining a copy of this software
and associated documentation files (the "Software"), to deal in the Software without
restriction, including without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom
the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or
substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE
USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace Lidgren.Network
{
	/// <summary>
	/// A client which can connect to a single NetServer
	/// </summary>
	public class NetClient : NetBase
	{
		private NetConnection m_serverConnection;

		private bool m_connectRequested;
		private byte[] m_hailData;
		private IPEndPoint m_connectEndpoint;
		private object m_startLock;

		public NetConnection ServerConnection { get { return m_serverConnection; } }

		public NetConnectionStatus Status
		{
			get
			{
				if (m_serverConnection == null)
					return NetConnectionStatus.Disconnected;
				return m_serverConnection.Status;
			}
		}

		public NetClient(NetConfiguration config)
			: base(config)
		{
			m_messagePool = new NetPool<NetMessage>(64, 4);
			m_bufferPool = new NetPool<NetBuffer>(64, 4);
			m_startLock = new object();
			m_lockedMessagePool = new NetQueue<NetMessage>();
			m_lockedBufferPool = new NetQueue<NetBuffer>();
		}

		/// <summary>
		/// Connects to the specified host on the specified port; passing hailData to the server
		/// </summary>
		public void Connect(string host, int port, byte[] hailData)
		{
			IPAddress ip = NetUtility.Resolve(host);
			if (ip == null)
				throw new NetException("Unable to resolve host");
			Connect(new IPEndPoint(ip, port), hailData);
		}

		public void Connect(IPEndPoint remoteEndpoint)
		{
			Connect(remoteEndpoint, null);
		}

		public void Connect(IPEndPoint remoteEndpoint, byte[] hailData)
		{
			m_connectRequested = true;
			m_connectEndpoint = remoteEndpoint;
			m_hailData = hailData;

			Start(); // start heartbeat thread etc
		}

		internal void PerformConnect()
		{
			// ensure we're bound to socket
			Start();

			m_connectRequested = false;

			if (m_serverConnection != null)
			{
				m_serverConnection.Disconnect("New connect", 0, m_serverConnection.Status == NetConnectionStatus.Connected);
				if (m_serverConnection.RemoteEndpoint.Equals(m_connectEndpoint))
					m_serverConnection = new NetConnection(this, m_connectEndpoint, m_hailData);
			}
			else
			{
				m_serverConnection = new NetConnection(this, m_connectEndpoint, m_hailData);
			}

			// connect
			m_serverConnection.Connect();

			m_connectEndpoint = null;
			m_hailData = null;
		}

		/// <summary>
		/// Initiate explicit disconnect
		/// </summary>
		public void Disconnect(string message)
		{
			if (m_serverConnection == null || m_serverConnection.Status == NetConnectionStatus.Disconnected)
			{
				LogWrite("Disconnect - Not connected!");
				return;
			}
			m_serverConnection.Disconnect(message, 1.0f, true);
		}

		/// <summary>
		/// Sends unsent messages and reads new messages from the wire
		/// </summary>
		protected override void Heartbeat()
		{
			double now = NetTime.Now;

			//
			// Drain locked pools
			//
			// m_messagePool and m_bufferPool is only accessed from this thread; thus no locking
			// is required for those objects
			//
			lock (m_lockedBufferPool)
			{
				NetBuffer lb;
				while ((lb = m_lockedBufferPool.Dequeue()) != null)
					m_bufferPool.Push(lb);
			}
			lock (m_lockedMessagePool)
			{
				NetMessage lm;
				while ((lm = m_lockedMessagePool.Dequeue()) != null)
					m_messagePool.Push(lm);
			}

			if (m_shutdownRequested)
			{
				PerformShutdown(m_shutdownReason);
				return;
			}

			if (m_connectRequested)
			{
				PerformConnect();
			}

			// read messages from network
			BaseHeartbeat(now);

			if (m_serverConnection != null)
				m_serverConnection.Heartbeat(now); // will send unsend messages etc.
		}

		internal override NetConnection GetConnection(IPEndPoint remoteEndpoint)
		{
			if (m_serverConnection != null && m_serverConnection.RemoteEndpoint.Equals(remoteEndpoint))
				return m_serverConnection;
			return null;
		}

		internal override void HandleReceivedMessage(NetMessage message, IPEndPoint senderEndpoint)
		{
			//LogWrite("NetClient received message " + message);
			double now = NetTime.Now;

			int payLen = message.m_data.LengthBytes;

			// Discovery response?
			if (message.m_type == NetMessageLibraryType.System)
			{
				if (payLen > 0)
				{
					NetSystemType sysType = (NetSystemType)message.m_data.Data[0];
					if (sysType == NetSystemType.DiscoveryResponse)
					{
						// DiscoveryResponse found
						if ((m_enabledMessageTypes & NetMessageType.ServerDiscovered) != NetMessageType.ServerDiscovered)
							return; // disabled

						byte[] discoverData = new byte[payLen - 1];
						if (payLen > 1)
							Buffer.BlockCopy(message.m_data.Data, 1, discoverData, 0, payLen - 1);

						NetMessage resMsg = CreateMessage();
						resMsg.m_msgType = NetMessageType.ServerDiscovered;

						NetBuffer resBuf = CreateBuffer();
						resMsg.m_data = resBuf;

						// write sender, assume ipv4
						resBuf.Write(senderEndpoint);
						resBuf.Write(discoverData);

						resBuf.Write(BitConverter.ToUInt32(senderEndpoint.Address.GetAddressBytes(), 0));
						resBuf.Write(senderEndpoint.Port);
						resBuf.Write(discoverData);

						lock (m_receivedMessages)
							m_receivedMessages.Enqueue(resMsg);

						return;
					}
				}
			}

			if (message.m_sender != m_serverConnection)
				return; // don't talk to strange senders

			if (message.m_type == NetMessageLibraryType.Acknowledge)
			{
				m_serverConnection.HandleAckMessage(message);
				return;
			}

			// Handle system types
			if (message.m_type == NetMessageLibraryType.System)
			{
				if (payLen < 1)
				{
					if ((m_enabledMessageTypes & NetMessageType.BadMessageReceived) == NetMessageType.BadMessageReceived)
						NotifyApplication(NetMessageType.BadMessageReceived, "Received malformed system message: " + message, m_serverConnection);
					return;
				}
				NetSystemType sysType = (NetSystemType)message.m_data.Data[0];
				switch (sysType)
				{
					case NetSystemType.ConnectResponse:
					case NetSystemType.Ping:
					case NetSystemType.Pong:
					case NetSystemType.Disconnect:
						m_serverConnection.HandleSystemMessage(message, now);
						return;
					case NetSystemType.Connect:
					case NetSystemType.ConnectionEstablished:
					case NetSystemType.Discovery:
					case NetSystemType.Error:
					default:
						if ((m_enabledMessageTypes & NetMessageType.BadMessageReceived) == NetMessageType.BadMessageReceived)
							NotifyApplication(NetMessageType.BadMessageReceived, "Undefined behaviour for client and " + sysType, m_serverConnection);
						return;
				}
			}

			Debug.Assert(message.m_type == NetMessageLibraryType.User || message.m_type == NetMessageLibraryType.UserFragmented);

			// add to pick-up queue
			m_serverConnection.HandleUserMessage(message);
		}

		/*
		/// <summary>
		/// Reads any received message in queue
		/// </summary>
		public NetBuffer ReadMessage()
		{
			if (m_receivedMessages.Count < 1)
				return null;
			NetMessage msg = m_receivedMessages.Dequeue();

			NetBuffer retval = msg.m_data;
			msg.m_data = null;
			m_messagePool.Push(msg);
			return retval;
		}
		*/

		/// <summary>
		/// Reads the content of an available message into 'intoBuffer' and returns true. If no message is available it returns false.
		/// </summary>
		/// <param name="intoBuffer">A NetBuffer whose content will be overwritten with the read message</param>
		/// <returns>true if a message was read</returns>
		public bool ReadMessage(NetBuffer intoBuffer, out NetMessageType type)
		{
			NetMessage msg;
			lock(m_receivedMessages)
				msg = m_receivedMessages.Dequeue();

			if (msg == null)
			{
				type = NetMessageType.None;
				return false;
			}

			// recycle NetMessage object
			NetBuffer content = msg.m_data;
			msg.m_data = null;
			type = msg.m_msgType;

			lock(m_lockedMessagePool)
				m_lockedMessagePool.Enqueue(msg);

			// swap content of buffers
			byte[] tmp = intoBuffer.Data;
			intoBuffer.Data = content.Data;
			content.Data = tmp;

			// set correct values for returning value (ignore the other, it's being recycled anyway)
			intoBuffer.m_bitLength = content.m_bitLength;
			intoBuffer.m_readPosition = 0;

			// recycle
			content.m_refCount = 0;

			lock(m_lockedBufferPool)
				m_lockedBufferPool.Enqueue(content);

			return true;
		}

		/// <summary>
		/// Sends a message using the specified channel
		/// </summary>
		public void SendMessage(NetBuffer data, NetChannel channel)
		{
			if (m_serverConnection == null || m_serverConnection.Status != NetConnectionStatus.Connected)
				throw new NetException("You must be connected first!");
			m_serverConnection.SendMessage(data, channel);
		}

		/// <summary>
		/// Emit a discovery signal to your subnet
		/// </summary>
		public void DiscoverLocalServers(int serverPort)
		{
			if (!m_isBound)
				Start();

			NetBuffer data = new NetBuffer(m_config.ApplicationIdentifier.Length);
			//data.Write((byte)NetSystemType.Discovery);
			data.Write(m_config.ApplicationIdentifier);

			LogWrite("Broadcasting server discovery ping...");
			//BroadcastUnconnectedMessage(data, serverPort);
			SendSingleUnreliableSystemMessage(NetSystemType.Discovery, data, new IPEndPoint(IPAddress.Broadcast, serverPort), true);
		}
		
		/// <summary>
		/// Emit a discovery signal to a single host
		/// </summary>
		public void DiscoverKnownServer(string host, int serverPort)
		{
			IPAddress address = NetUtility.Resolve(host);
			IPEndPoint ep = new IPEndPoint(address, serverPort);
			DiscoverKnownServer(ep, false);
		}

		/// <summary>
		/// Emit a discovery signal to a host or subnet
		/// </summary>
		public void DiscoverKnownServer(IPEndPoint address, bool useBroadcast)
		{
			if (!m_isBound)
				Start();

			NetBuffer data = new NetBuffer(m_config.ApplicationIdentifier.Length);
			data.Write(m_config.ApplicationIdentifier);
			
			LogWrite("Discovering known server " + address.ToString() + "...");
			SendSingleUnreliableSystemMessage(NetSystemType.Discovery, data, address, useBroadcast);
		}

		internal override void HandleConnectionForciblyClosed(NetConnection connection, SocketException sex)
		{
			if (m_serverConnection == null)
				return;

			if (m_serverConnection.Status == NetConnectionStatus.Connecting)
			{
				// failed to connect; server is not listening
				m_serverConnection.Disconnect("Failed to connect; server is not listening", 0, false);
				return;
			}

			m_connectRequested = false;
			m_serverConnection.Disconnect("Connection forcibly closed by server", 0, false);
			return;
		}

		/// <summary>
		/// Disconnects from server and closes socket
		/// </summary>
		protected override void PerformShutdown(string reason)
		{
			if (m_serverConnection != null)
				m_serverConnection.Disconnect(reason, 0, true);
			m_connectRequested = false;
			base.PerformShutdown(reason);
		}
	}
}
