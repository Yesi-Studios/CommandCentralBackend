using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;

namespace CCServ.Logging.Loggers
{
    public class DatabaseLogger : ILogger
    {

        /// <summary>
        /// Ensures that the logging session is initialized and returns a boolean indicating if logging operations using it are valid.
        /// </summary>
        /// <returns></returns>
        private bool TryGetSession(out NHibernate.ISession session)
        {
            if (!DataAccess.NHibernateHelper.IsReady)
            {
                session = null;
                return false;
            }
            else
            {
                session = DataAccess.NHibernateHelper.CreateStatefulSession(); 
                return true;
            }
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
            if (DataAccess.NHibernateHelper.IsReady)
            {
                using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
                {
                    session.Save(new LogEntry
                    {
                        CallerFilePath = callerFilePath,
                        CallerLineNumber = callerLineNumber,
                        CallerMemberName = callerMemberName,
                        Message = message.Truncate(10000),
                        Token = token,
                        MessageType = "Debug"
                    });
                }
            }
        }

        public void LogInformation(string message, ClientAccess.MessageToken token, string callerMemberName, int callerLineNumber, string callerFilePath)
        {
            if (DataAccess.NHibernateHelper.IsReady)
            {
                using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
                {
                    session.Save(new LogEntry
                    {
                        CallerFilePath = callerFilePath,
                        CallerLineNumber = callerLineNumber,
                        CallerMemberName = callerMemberName,
                        Message = message.Truncate(10000),
                        Token = token,
                        MessageType = "Information"
                    });
                }
            }
        }

        public void LogCritical(string message, ClientAccess.MessageToken token, string callerMemberName, int callerLineNumber, string callerFilePath)
        {
            if (DataAccess.NHibernateHelper.IsReady)
            {
                using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
                {
                    session.Save(new LogEntry
                    {
                        CallerFilePath = callerFilePath,
                        CallerLineNumber = callerLineNumber,
                        CallerMemberName = callerMemberName,
                        Message = message.Truncate(10000),
                        Token = token,
                        MessageType = "Critical"
                    });
                }
            }
        }

        public void LogWarning(string message, ClientAccess.MessageToken token, string callerMemberName, int callerLineNumber, string callerFilePath)
        {
            if (DataAccess.NHibernateHelper.IsReady)
            {
                using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
                {
                    session.Save(new LogEntry
                    {
                        CallerFilePath = callerFilePath,
                        CallerLineNumber = callerLineNumber,
                        CallerMemberName = callerMemberName,
                        Message = message.Truncate(10000),
                        Token = token,
                        MessageType = "Warning"
                    });
                }
            }
        }

        public void LogException(Exception ex, string message, ClientAccess.MessageToken token, string callerMemberName, int callerLineNumber, string callerFilePath)
        {

            if (DataAccess.NHibernateHelper.IsReady)
            {
                using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
                {
                    session.Save(new LogEntry
                    {
                        CallerFilePath = callerFilePath,
                        CallerLineNumber = callerLineNumber,
                        CallerMemberName = callerMemberName,
                        Message = (message.Truncate(10000) + "||BREAK EXCEPTION||" + ex.ToString()).Truncate(10000),
                        Token = token,
                        MessageType = "Exception"
                    });
                }
            }

        }
    }
}
