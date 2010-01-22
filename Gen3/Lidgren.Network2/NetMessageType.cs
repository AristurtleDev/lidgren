﻿using System;

namespace Lidgren.Network2
{
	// public visible subset of NetMessageType
	public enum NetMessageChannel : byte
	{
		Unreliable = 14,

		Sequenced1 = 15,
		Sequenced2 = 16,
		Sequenced3 = 17,
		Sequenced4 = 18,
		Sequenced5 = 19,
		Sequenced6 = 20,
		Sequenced7 = 21,
		Sequenced8 = 22,
		Sequenced9 = 23,
		Sequenced10 = 24,
		Sequenced11 = 25,
		Sequenced12 = 26,
		Sequenced13 = 27,
		Sequenced14 = 28,
		Sequenced15 = 29,
		Sequenced16 = 30,

		ReliableUnordered = 31,

		ReliableSequenced1 = 32,
		ReliableSequenced2 = 33,
		ReliableSequenced3 = 34,
		ReliableSequenced4 = 35,
		ReliableSequenced5 = 36,
		ReliableSequenced6 = 37,
		ReliableSequenced7 = 38,
		ReliableSequenced8 = 39,
		ReliableSequenced9 = 40,
		ReliableSequenced10 = 41,
		ReliableSequenced11 = 42,
		ReliableSequenced12 = 43,
		ReliableSequenced13 = 44,
		ReliableSequenced14 = 45,
		ReliableSequenced15 = 46,
		ReliableSequenced16 = 47,

		ReliableOrdered1 = 48,
		ReliableOrdered2 = 49,
		ReliableOrdered3 = 50,
		ReliableOrdered4 = 51,
		ReliableOrdered5 = 52,
		ReliableOrdered6 = 53,
		ReliableOrdered7 = 54,
		ReliableOrdered8 = 55,
		ReliableOrdered9 = 56,
		ReliableOrdered10 = 57,
		ReliableOrdered11 = 58,
		ReliableOrdered12 = 59,
		ReliableOrdered13 = 60,
		ReliableOrdered14 = 61,
		ReliableOrdered15 = 62,
		ReliableOrdered16 = 63
	}

	internal enum NetMessageType : byte
	{
		Error = 0,

		LibraryAcknowledge = 1,
		LibraryAckNack = 2,
		LibraryKeepAlive = 3,
		LibraryConnect = 4,
		LibraryConnectResponse = 5,
		LibraryConnectionEstablished = 6,
		LibraryDisconnect = 7,
		LibraryDiscovery = 8,
		LibraryDiscoveryResponse = 9,
		LibraryNatIntroduction = 10,

		Unused1 = 11,
		Unused2 = 12,
		Unused3 = 13,

		UserUnreliable = 14,

		UserSequenced1 = 15,
		UserSequenced2 = 16,
		UserSequenced3 = 17,
		UserSequenced4 = 18,
		UserSequenced5 = 19,
		UserSequenced6 = 20,
		UserSequenced7 = 21,
		UserSequenced8 = 22,
		UserSequenced9 = 23,
		UserSequenced10 = 24,
		UserSequenced11 = 25,
		UserSequenced12 = 26,
		UserSequenced13 = 27,
		UserSequenced14 = 28,
		UserSequenced15 = 29,
		UserSequenced16 = 30,

		UserReliableUnordered = 31,

		UserReliableSequenced1 = 32,
		UserReliableSequenced2 = 33,
		UserReliableSequenced3 = 34,
		UserReliableSequenced4 = 35,
		UserReliableSequenced5 = 36,
		UserReliableSequenced6 = 37,
		UserReliableSequenced7 = 38,
		UserReliableSequenced8 = 39,
		UserReliableSequenced9 = 40,
		UserReliableSequenced10 = 41,
		UserReliableSequenced11 = 42,
		UserReliableSequenced12 = 43,
		UserReliableSequenced13 = 44,
		UserReliableSequenced14 = 45,
		UserReliableSequenced15 = 46,
		UserReliableSequenced16 = 47,

		UserReliableOrdered1 = 48,
		UserReliableOrdered2 = 49,
		UserReliableOrdered3 = 50,
		UserReliableOrdered4 = 51,
		UserReliableOrdered5 = 52,
		UserReliableOrdered6 = 53,
		UserReliableOrdered7 = 54,
		UserReliableOrdered8 = 55,
		UserReliableOrdered9 = 56,
		UserReliableOrdered10 = 57,
		UserReliableOrdered11 = 58,
		UserReliableOrdered12 = 59,
		UserReliableOrdered13 = 60,
		UserReliableOrdered14 = 61,
		UserReliableOrdered15 = 62,
		UserReliableOrdered16 = 63
	}
}