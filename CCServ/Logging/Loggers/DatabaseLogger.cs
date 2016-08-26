using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.Logging.Loggers
{
    public class DatabaseLogger : ILogger
    {

        private NHibernate.ISession _session;

        /// <summary>
        /// Ensures that the logging session is initialized and returns a boolean indicating if logging operations using it are valid.
        /// </summary>
        /// <returns></returns>
        private bool EnsureSession()
        {
            if (_session != null)
                return true;

            if (!DataAccess.NHibernateHelper.IsReady)
                return false;

            _session = DataAccess.NHibernateHelper.CreateStatefulSession();
            
            return true;
        }


        public List<MessageTypes> EnabledMessageTypes { get; set; }

        public LoggingTargetTypes TargetType
        {
            get
            {
                return LoggingTargetTypes.DATABASE;
            }
        }

        public string Name
        {
            get
            {
                return "CCSEERV Database Logger.";
            }
        }

        /// <summary>
        /// Logs a debug message to the database.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="token"></param>
        /// <param name="callerMemberName"></param>
        /// <param name="callerLineNumber"></param>
        /// <param name="callerFilePath"></param>
        public void LogDebug(string message, ClientAccess.MessageToken token, string callerMemberName, int callerLineNumber, string callerFilePath)
        {
            if (EnsureSession())
            {
                _session.Save(new LogEntry
                    {
                        CallerFilePath = callerFilePath,
                        CallerLineNumber = callerLineNumber,
                        CallerMemberName = callerMemberName,
                        Message = message,
                        Token = token,
                        MessageType = "Debug"
                    });

                _session.Flush();
            }
        }

        public void LogInformation(string message, ClientAccess.MessageToken token, string callerMemberName, int callerLineNumber, string callerFilePath)
        {
            if (EnsureSession())
            {
                _session.Save(new LogEntry
                    {
                        CallerFilePath = callerFilePath,
                        CallerLineNumber = callerLineNumber,
                        CallerMemberName = callerMemberName,
                        Message = message,
                        Token = token,
                        MessageType = "Information"
                    });

                _session.Flush();
            }
        }

        public void LogCritical(string message, ClientAccess.MessageToken token, string callerMemberName, int callerLineNumber, string callerFilePath)
        {
            if (EnsureSession())
            {
                _session.Save(new LogEntry
                {
                    CallerFilePath = callerFilePath,
                    CallerLineNumber = callerLineNumber,
                    CallerMemberName = callerMemberName,
                    Message = message,
                    Token = token,
                    MessageType = "Critical"
                });
                _session.Flush();

            }
        }

        public void LogWarning(string message, ClientAccess.MessageToken token, string callerMemberName, int callerLineNumber, string callerFilePath)
        {
            if (EnsureSession())
            {
                _session.Save(new LogEntry
                {
                    CallerFilePath = callerFilePath,
                    CallerLineNumber = callerLineNumber,
                    CallerMemberName = callerMemberName,
                    Message = message,
                    Token = token,
                    MessageType = "Warning"
                });

                _session.Flush();
            }
        }

        public void LogException(Exception ex, string message, ClientAccess.MessageToken token, string callerMemberName, int callerLineNumber, string callerFilePath)
        {
            if (EnsureSession())
            {
                _session.Save(new LogEntry
                {
                    CallerFilePath = callerFilePath,
                    CallerLineNumber = callerLineNumber,
                    CallerMemberName = callerMemberName,
                    Message = message + "||BREAK EXCEPTION||" + ex.ToString(),
                    Token = token,
                    MessageType = "Exception"
                });

                _session.Flush();
            }
        }
    }
}
