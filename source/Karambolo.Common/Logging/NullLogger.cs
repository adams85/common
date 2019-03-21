namespace Karambolo.Common.Logging
{
    public sealed class NullLogger : ILogger
    {
        public static readonly NullLogger Instance = new NullLogger();

        private NullLogger() { }

        public void LogEvent(LoggerEventType level, string message, params object[] args) { }
    }
}
