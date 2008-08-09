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

namespace Lidgren.Network
{
	public enum NetMessageType
	{
		None,
		Data,
		StatusChanged,
		ServerDiscovered,
		Receipt,
		LogMessage,
	}

	/// <summary>
	/// Internal type of the message
	/// </summary>
	internal enum NetMessageLibraryType
	{
		None = 0,				  // No message or packet padding
		User = 1,				  // Application message
		UserFragmented = 2,		  // Library/Application message
		Acknowledge = 3,		  // Library message
		AckBitVector = 4,		  // Library message
		Unused1 = 5,			  // Library message
		Unused2 = 6,			  // Library message
		System = 7,				  // Library message
	}

	/// <summary>
	/// Messages of type NetMessageType.System has this enum encoded in the first byte
	/// </summary>
	internal enum NetSystemType
	{
		Error = 0,
		Connect = 1,
		ConnectResponse = 2,
		ConnectionEstablished = 3,
		Disconnect = 4,
		Discovery = 50,
		DiscoveryResponse = 51,
		Ping = 100,
		Pong = 101
	}

	/// <summary>
	/// Status for a connection
	/// </summary>
	public enum NetConnectionStatus
	{
		Disconnected,
		Connecting,
		Connected,
		Reconnecting,
		Disconnecting,
	}

	/// <summary>
	/// Specifies what guarantees a message send is given
	/// </summary>
	public enum NetChannel
	{
		/// <summary>
		/// Messages are not guaranteed to arrive
		/// </summary>
		Unreliable = 0,

		/// <summary>
		/// Messages are not guaranteed to arrive, but out-of-order message, ie. late messages, are dropped
		/// </summary>
		UnreliableInOrder1 = 1,
		UnreliableInOrder2 = 2,
		UnreliableInOrder3 = 3,
		UnreliableInOrder4 = 4,
		UnreliableInOrder5 = 5,
		UnreliableInOrder6 = 6,
		UnreliableInOrder7 = 7,
		UnreliableInOrder8 = 8,
		UnreliableInOrder9 = 9,
		UnreliableInOrder10 = 10,
		UnreliableInOrder11 = 11,
		UnreliableInOrder12 = 12,
		UnreliableInOrder13 = 13,
		UnreliableInOrder14 = 14,
		UnreliableInOrder15 = 15,

		/// <summary>
		/// Messages are guaranteed to arrive, but not necessarily in the same order as they were sent
		/// </summary>
		ReliableUnordered = 16,

		/// <summary>
		/// Messages are guaranteed to arrive, in the same order as they were sent
		/// </summary>
		ReliableInOrder1 = 17,
		ReliableInOrder2 = 18,
		ReliableInOrder3 = 19,
		ReliableInOrder4 = 20,
		ReliableInOrder5 = 21,
		ReliableInOrder6 = 22,
		ReliableInOrder7 = 23,
		ReliableInOrder8 = 24,
		ReliableInOrder9 = 25,
		ReliableInOrder10 = 26,
		ReliableInOrder11 = 27,
		ReliableInOrder12 = 28,
		ReliableInOrder13 = 29,
		ReliableInOrder14 = 30,
		ReliableInOrder15 = 31,
	}
}
