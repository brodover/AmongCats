namespace Server.Helpers
{
    public static class ServerCommon
    {
        public enum PlayerPriority
        {
            High = 1, // Higher priority players
            Normal = 2, // Active players
            Canceled = 3 // Canceled players
        }
    }
}
