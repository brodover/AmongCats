﻿namespace Server.Helpers
{
    public static class Common
    {
        public static class ServerError
        {
            public const string NoPlayer = "Player not found.";
            public const string AlrInMatch = "Already in match.";
            public const string NoRoomId = "Room id not found.";
            public const string NoActiveRoom = "Active room not found.";
        }

        public static class HubMessage
        {
            public const string MatchCreated = "MatchCreated";
        }
    }
}
