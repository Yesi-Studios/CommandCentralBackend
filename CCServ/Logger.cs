using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CCServ
{
    /// <summary>
    /// Encapsulates logging operations and provides an abstraction between logging to the windows event system or to the console.
    /// </summary>
    public static class Logger
    {
        // Note: The actual limit is higher than this, but different Microsoft operating systems actually have
        //       different limits. So just use 30,000 to be safe.
        private const int MaxEventLogEntryLength = 30000;

        /// <summary>
        /// Indicates that debug messages should be logged.
        /// </summary>
        public static bool DebugLoggingEnabled { get; set; }

        /// <summary>
        /// Gets or sets the source for logging messages.
        /// </summary>
        public static string Source
        {
            get
            {
                return String.Format("CCSERV v{0}", Config.Version.RELEASE_VERSION);
            }
        }

        /// <summary>
        /// Logs the message, but only if debug logging is true.  This value is set in the Logger class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="source">The name of the app/process calling the logging method. If not provided,
        /// an attempt will be made to get the name of the calling process.</param>
        public static void LogDebug(string message, string source = "")
        {
            if (!DebugLoggingEnabled) 
                return;

            Log(message, EventLogEntryType.Information, source);
        }

        /// <summary>
        /// Logs the information.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="source">The name of the app/process calling the logging method. If not provided,
        /// an attempt will be made to get the name of the calling process.</param>
        public static void LogInformation(string message, string source = "")
        {
            Log(message, EventLogEntryType.Information, source);
        }

        /// <summary>
        /// Logs the warning.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="source">The name of the app/process calling the logging method. If not provided,
        /// an attempt will be made to get the name of the calling process.</param>
        public static void LogWarning(string message, string source = "")
        {
            Log(message, EventLogEntryType.Warning, source);
        }

        /// <summary>
        /// Logs the exception.
        /// </summary>
        /// <param name="ex">The ex.</param>
        /// <param name="token">The message token representing the transaction during which the exception occurred.</param>
        /// <param name="source">The name of the app/process calling the logging method. If not provided,
        /// an attempt will be made to get the name of the calling process.</param>
        public static void LogException(Exception ex, string message, ClientAccess.MessageToken token, string source = "")
        {
            if (ex == null)
                throw new ArgumentNullException("ex"); 

            Log(String.Format("ERROR: Message : {0} ||| Token: {1} ||| Exception : {1}", message, (token == null) ? "null" : token.ToString(), ex.ToString()), EventLogEntryType.Error, source);

            //And send an email.
            EmailHelper.SendFatalErrorEmail(token, ex);
        }

        /// <summary>
        /// Logs a message to the event viewer and, if possible, to the console.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="entryType"></param>
        /// <param name="source"></param>
        private static void Log(string message, EventLogEntryType entryType, string source)
        {
            // Note: I got an error that the security log was inaccessible. To get around it, I ran the app as administrator
            //       just once, then I could run it from within VS.

            //If the source is blank, set it to the default source.
            if (String.IsNullOrEmpty(source))
                source = Source;

            string possiblyTruncatedMessage = EnsureLogMessageLimit(message);
            //EventLog.WriteEntry(source, possiblyTruncatedMessage, entryType);

            // If we're running a console app, also write the message to the console window.
            if (Environment.UserInteractive)
            {
                Console.WriteLine("[{0} Service Message @ {1}]: {2}", entryType, DateTime.Now, message);
            }
        }

        /// <summary>
        /// Ensures that the log message entry text length does not exceed the event log viewer maximum length of 32766 characters.
        /// </summary>
        /// <param name="logMessage"></param>
        /// <returns></returns>
        private static string EnsureLogMessageLimit(string logMessage)
        {
            if (logMessage.Length > MaxEventLogEntryLength)
            {
                string truncateWarningText = string.Format(CultureInfo.CurrentCulture, "... | Log Message Truncated [ Limit: {0} ]", MaxEventLogEntryLength);

                // Set the message to the max minus enough room to add the truncate warning.
                logMessage = logMessage.Substring(0, MaxEventLogEntryLength - truncateWarningText.Length);

                logMessage = string.Format(CultureInfo.CurrentCulture, "{0}{1}", logMessage, truncateWarningText);
            }

            return logMessage;
        }

    }
}
