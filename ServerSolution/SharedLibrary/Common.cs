﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SharedLibrary
{
    public static class Common
    {
        public enum Role
        {
            None,
            Human,
            Cat,
            Random
        }

        public static class ServerError
        {
            public const string NoPlayer = "Player not found.";
            public const string AlrInMatch = "Already in match.";
            public const string NoRoomId = "Room id not found.";
            public const string NoActiveRoom = "Active room not found.";
        }

        public static class HubMsg
        {
            public static class ToClient
            {
                public const string PlayerConnected = "PlayerConnected"; // temp
                public const string ReceiveMessage = "ReceiveMessage"; // temp
                public const string MatchCreated = "MatchCreated";
            }

            public static class ToServer
            {
                public const string SendMessage = "SendMessage";
                public const string JoinQueue = "JoinQueue";
                public const string LeaveQueue = "LeaveQueue";
            }
        }

    }
}
