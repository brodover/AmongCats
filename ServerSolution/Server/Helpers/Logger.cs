namespace Server.Helpers
{
    public static class Logger
    {
        public static ILoggerFactory LogFactory { get; set; }
        private static ILogger _default;

        public static ILogger Default => _default;

        public static void Init()
        {
            _default = LogFactory.CreateLogger("Logger");
            _default.LogDebug("Init Logger");
        }
    }
}
