using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using Karambolo.Common.Properties;

namespace Karambolo.Common.Logging
{
    public class TraceSourceLogger : ILogger
    {
        private static TraceEventType MapLevelToEventType(LoggerEventType level)
        {
            switch (level)
            {
                case LoggerEventType.Critical:
                    return TraceEventType.Critical;
                case LoggerEventType.Error:
                    return TraceEventType.Error;
                case LoggerEventType.Warning:
                    return TraceEventType.Warning;
                case LoggerEventType.Info:
                    return TraceEventType.Information;
                case LoggerEventType.Verbose:
                    return TraceEventType.Verbose;
                default:
                    throw new NotSupportedException();
            }
        }

        private readonly string _sourceName;

        public TraceSourceLogger(string sourceName)
        {
            if (sourceName == null)
                throw new ArgumentNullException(nameof(sourceName));
            if (sourceName == string.Empty)
                throw new ArgumentException(Resources.InvalidValue, nameof(sourceName));

            _sourceName = sourceName;
        }

        private readonly ConcurrentDictionary<string, TraceSource> _traceSources = new ConcurrentDictionary<string, TraceSource>();

        protected virtual TraceSource CreateTraceSource(string sourceName)
        {
            return new TraceSource(sourceName, SourceLevels.Information);
        }

        public void LogEvent(LoggerEventType level, string message, object[] args)
        {
            TraceSource traceSource = _traceSources.GetOrAdd(_sourceName, CreateTraceSource);

            if (ArrayUtils.IsNullOrEmpty(args))
                traceSource.TraceEvent(MapLevelToEventType(level), 0, message);
            else
                traceSource.TraceEvent(MapLevelToEventType(level), 0, message, args);
        }
    }
}
