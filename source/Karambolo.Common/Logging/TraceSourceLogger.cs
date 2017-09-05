using System;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace Karambolo.Common.Logging
{
    public class TraceSourceLogger : ILogger
    {
        static TraceEventType MapLevelToEventType(LoggerEventType level)
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
                    throw new ArgumentException(null, nameof(level));
            }
        }

        readonly string _sourceName;

        public TraceSourceLogger(string sourceName)
        {
            if (sourceName == null)
                throw new ArgumentNullException(nameof(sourceName));
            if (sourceName == string.Empty)
                throw new ArgumentException(null, nameof(sourceName));

            _sourceName = sourceName;
        }

        readonly ConcurrentDictionary<string, TraceSource> _traceSources = new ConcurrentDictionary<string, TraceSource>();

        public void LogEvent(LoggerEventType level, string message, object[] args)
        {
            var traceSource = _traceSources.GetOrAdd(_sourceName, sn => new TraceSource(sn, SourceLevels.Information));

            if (ArrayUtils.IsNullOrEmpty(args))
                traceSource.TraceEvent(MapLevelToEventType(level), 0, message);
            else
                traceSource.TraceEvent(MapLevelToEventType(level), 0, message, args);
        }
    }
}
