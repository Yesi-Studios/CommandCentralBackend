using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Collections.Concurrent;
using CCServ.ClientAccess;
using AtwoodUtils;

namespace CCServ.Logging
{
    /// <summary>
    /// Encapsulates and abstracts logging operations.  Provides logging to the windows event system, the console, the database, and email in the case of errors.
    /// </summary>
    public static class Log
    {

        private static ConcurrentBag<ILogger> _loggers = new ConcurrentBag<ILogger>();
        
        /// <summary>
        /// Registers a logger, returning a boolean indicating if the registration succeeded.
        /// </summary>
        /// <param name="logger"></param>
        /// <returns></returns>
        public static void RegisterLogger(ILogger logger)
        {
            try
            {

                logger.EnabledMessageTypes = new List<MessageTypes>
                    {
                        MessageTypes.CRITICAL,
                        MessageTypes.DEBUG,
                        MessageTypes.ERROR,
                        MessageTypes.INFORMATION,
                        MessageTypes.WARNING
                    };

                _loggers.Add(logger);

                Log.Info("Hello {0}, you were registered successfully!".FormatS(logger.Name), null);

            }
            catch
            {
                throw;
            }

        }

        /// <summary>
        /// Logs the message, but only if debug logging is true.  This value is set in the Logger class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="source">The name of the app/process calling the logging method. If not provided,
        /// an attempt will be made to get the name of the calling process.</param>
        public static void Debug(string message, MessageToken token = null, string source = "", [CallerMemberName] string callerMemberName = "unknown", [CallerLineNumber] int callerLineNumber = 0, [CallerFilePath] string callerFilePath = "")
        {
            Parallel.ForEach<ILogger>(_loggers, logger =>
            {
                logger.LogDebug(message, token, callerMemberName, callerLineNumber, callerFilePath);
            });
        }

        /// <summary>
        /// Logs the information.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="source">The name of the app/process calling the logging method. If not provided,
        /// an attempt will be made to get the name of the calling process.</param>
        public static void Info(string message, MessageToken token = null, string source = "", [CallerMemberName] string callerMemberName = "unknown", [CallerLineNumber] int callerLineNumber = 0, [CallerFilePath] string callerFilePath = "")
        {
            Parallel.ForEach<ILogger>(_loggers, logger =>
            {
                logger.LogInformation(message, token, callerMemberName, callerLineNumber, callerFilePath);
            });
        }

        /// <summary>
        /// Logs the warning.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="source">The name of the app/process calling the logging method. If not provided,
        /// an attempt will be made to get the name of the calling process.</param>
        public static void Warning(string message, MessageToken token = null, string source = "", [CallerMemberName] string callerMemberName = "unknown", [CallerLineNumber] int callerLineNumber = 0, [CallerFilePath] string callerFilePath = "")
        {
            Parallel.ForEach<ILogger>(_loggers, logger =>
            {
                logger.LogWarning(message, token, callerMemberName, callerLineNumber, callerFilePath);
            });
        }

        /// <summary>
        /// Logs the critical event.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="token"></param>
        /// <param name="source"></param>
        /// <param name="callerMemberName"></param>
        /// <param name="callerLineNumber"></param>
        /// <param name="callerFilePath"></param>
        public static void Critical(string message, MessageToken token = null, string source = "", [CallerMemberName] string callerMemberName = "unknown", [CallerLineNumber] int callerLineNumber = 0, [CallerFilePath] string callerFilePath = "")
        {
            Parallel.ForEach<ILogger>(_loggers, logger =>
            {
                logger.LogCritical(message, token, callerMemberName, callerLineNumber, callerFilePath);
            });
        }

        /// <summary>
        /// Logs the exception.
        /// </summary>
        /// <param name="ex">The ex.</param>
        /// <param name="token">The message token representing the transaction during which the exception occurred.</param>
        /// <param name="source">The name of the app/process calling the logging method. If not provided,
        /// an attempt will be made to get the name of the calling process.</param>
        public static void Exception(Exception ex, string message, MessageToken token = null, string source = "", [CallerMemberName] string callerMemberName = "unknown", [CallerLineNumber] int callerLineNumber = 0, [CallerFilePath] string callerFilePath = "")
        {
            Parallel.ForEach<ILogger>(_loggers, logger =>
            {
                logger.LogException(ex, message, token, callerMemberName, callerLineNumber, callerFilePath);
            });
        }

        /// <summary>
        /// Ensures that the log message entry text length does not exceed the given length.  If it does, a friendly string is appended to the end and the message is truncated.
        /// </summary>
        /// <param name="logMessage"></param>
        /// <returns></returns>
        private static string EnsureLogMessageLimit(string logMessage, int maxMessageLength)
        {
            if (logMessage.Length > maxMessageLength)
            {
                string truncatedWarningText = string.Format(CultureInfo.CurrentCulture, "... | Log Message Truncated [ Limit: {0} ]", maxMessageLength);

                // Set the message to the max minus enough room to add the truncate warning.
                logMessage = logMessage.Substring(0, maxMessageLength - truncatedWarningText.Length);

                logMessage = string.Format(CultureInfo.CurrentCulture, "{0}{1}", logMessage, truncatedWarningText);
            }

            return logMessage;
        }

        /// <summary>
        /// Scans and registers all implementations of ILogger.
        /// </summary>
        /// <param name="options"></param>
        [ServiceManagement.StartMethod(Priority = 100)]
        private static void RegisterLoggers(CLI.Options.LaunchOptions options)
        {
            var loggers = Assembly.GetExecutingAssembly().GetTypes().Where(x => x != typeof(ILogger) && typeof(ILogger).IsAssignableFrom(x))
                .Select(x => Activator.CreateInstance(x) as ILogger);

            foreach (var logger in loggers)
            {
                RegisterLogger(logger);
            }

            Info("{0} logger(s) have been registered : {1}".FormatS(_loggers.Count, String.Join(", ", _loggers.Select(x => x.Name))), null);
        }

    }
}
