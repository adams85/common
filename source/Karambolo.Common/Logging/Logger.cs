using System;

namespace Karambolo.Common.Logging
{
    public enum LoggerEventType
    {
        Verbose = -10,
        Info = 0,
        Warning = 10,
        Error = 100,
        Critical = 1000,
    }

    public interface ILogger
    {
        void LogEvent(LoggerEventType level, string message, object[] args);
    }

    public static class LoggerExtensions
    {
        public static void LogCritical(this ILogger logger, string message)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            logger.LogEvent(LoggerEventType.Critical, message, null);
        }

        public static void LogCritical(this ILogger logger, string message, params object[] args)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            logger.LogEvent(LoggerEventType.Critical, message, args);
        }

        public static void LogError(this ILogger logger, string message)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            logger.LogEvent(LoggerEventType.Error, message, null);
        }

        public static void LogError(this ILogger logger, string message, params object[] args)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            logger.LogEvent(LoggerEventType.Error, message, args);
        }

        public static void LogWarning(this ILogger logger, string message)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            logger.LogEvent(LoggerEventType.Warning, message, null);
        }

        public static void LogWarning(this ILogger logger, string message, params object[] args)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            logger.LogEvent(LoggerEventType.Warning, message, args);
        }

        public static void LogInfo(this ILogger logger, string message)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            logger.LogEvent(LoggerEventType.Info, message, null);
        }

        public static void LogInfo(this ILogger logger, string message, params object[] args)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            logger.LogEvent(LoggerEventType.Info, message, args);
        }

        public static void LogVerbose(this ILogger logger, string message)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            logger.LogEvent(LoggerEventType.Verbose, message, null);
        }

        public static void LogVerbose(this ILogger logger, string message, params object[] args)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            logger.LogEvent(LoggerEventType.Verbose, message, args);
        }
    }
}
