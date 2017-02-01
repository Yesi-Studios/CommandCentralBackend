using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;

namespace CCServ.Logging.Loggers
{
    /// <summary>
    /// The logger that writes messages to the database.
    /// </summary>
    public class DatabaseLogger : ILogger
    {

        /// <summary>
        /// Returns the Database target type.
        /// </summary>
        public LoggingTargetTypes TargetType
        {
            get
            {
                return LoggingTargetTypes.DATABASE;
            }
        }

        /// <summary>
        /// Returns the name of this logger.
        /// </summary>
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
                using (var transaction = session.BeginTransaction())
                {
                    try
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
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Inserts an information entry log to the database.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="token"></param>
        /// <param name="callerMemberName"></param>
        /// <param name="callerLineNumber"></param>
        /// <param name="callerFilePath"></param>
        public void LogInformation(string message, ClientAccess.MessageToken token, string callerMemberName, int callerLineNumber, string callerFilePath)
        {
            if (DataAccess.NHibernateHelper.IsReady)
            {
                using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
                using (var transaction = session.BeginTransaction())
                {
                    try
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
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Inserts a critical message entry log to the database.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="token"></param>
        /// <param name="callerMemberName"></param>
        /// <param name="callerLineNumber"></param>
        /// <param name="callerFilePath"></param>
        public void LogCritical(string message, ClientAccess.MessageToken token, string callerMemberName, int callerLineNumber, string callerFilePath)
        {
            if (DataAccess.NHibernateHelper.IsReady)
            {
                using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
                using (var transaction = session.BeginTransaction())
                {
                    try
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
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Inserts a warning entry log to the database.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="token"></param>
        /// <param name="callerMemberName"></param>
        /// <param name="callerLineNumber"></param>
        /// <param name="callerFilePath"></param>
        public void LogWarning(string message, ClientAccess.MessageToken token, string callerMemberName, int callerLineNumber, string callerFilePath)
        {
            if (DataAccess.NHibernateHelper.IsReady)
            {
                using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
                using (var transaction = session.BeginTransaction())
                {
                    try
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
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Inserts an exception entry log to the database.
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="message"></param>
        /// <param name="token"></param>
        /// <param name="callerMemberName"></param>
        /// <param name="callerLineNumber"></param>
        /// <param name="callerFilePath"></param>
        public void LogException(Exception ex, string message, ClientAccess.MessageToken token, string callerMemberName, int callerLineNumber, string callerFilePath)
        {

            if (DataAccess.NHibernateHelper.IsReady)
            {
                using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
                using (var transaction = session.BeginTransaction())
                {
                    try
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
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }

        }
    }
}
