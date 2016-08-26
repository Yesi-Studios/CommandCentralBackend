using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.Logging.Loggers
{
    public class DatabaseLogger : ILogger
    {
        public DatabaseLogger()
        {
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
                return "CCSERV Database Logger NOT IMPLEMENTED";
            }
        }

        public bool IsValid()
        {
            return true;
        }

        public void LogDebug(string message, ClientAccess.MessageToken token, string callerMemberName, int callerLineNumber, string callerFilePath)
        {
            //TODO
        }

        public void LogInformation(string message, ClientAccess.MessageToken token, string callerMemberName, int callerLineNumber, string callerFilePath)
        {
            //TODO
        }

        public void LogCritical(string message, ClientAccess.MessageToken token, string callerMemberName, int callerLineNumber, string callerFilePath)
        {
            //TODO
        }

        public void LogWarning(string message, ClientAccess.MessageToken token, string callerMemberName, int callerLineNumber, string callerFilePath)
        {
            //TODO
        }

        public void LogException(Exception ex, string message, ClientAccess.MessageToken token, string callerMemberName, int callerLineNumber, string callerFilePath)
        {
            //TODO
        }
    }
}
