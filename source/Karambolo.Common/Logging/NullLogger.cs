namespace Karambolo.Common.Logging
{
    public class NullLogger : ILogger
    {
        public static readonly NullLogger Instance = new NullLogger(null);

        public NullLogger(string sourceName) { }

        public void LogEvent(LoggerEventType level, string message, params object[] args) { }
    }
}
