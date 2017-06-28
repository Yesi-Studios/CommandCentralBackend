using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;
using CommandCentral.ClientAccess;

namespace CommandCentral.Logging.Loggers
{
    class ConsoleLogger : ILogger
    {

        public ConsoleLogger()
        {
        }

        public string Name
        {
            get
            {
                return "CommandCentral Console Logger";
            }
        }

        public LoggingTargetTypes TargetType
        {
            get
            {
                return LoggingTargetTypes.CONSOLE;
            }
        }

        public void LogCritical(string message, MessageToken token, string callerMemberName = "unknown",  int callerLineNumber = 0,  string callerFilePath = "")
        {
            Console.WriteLine("[{0}] [{1}] : {2}\n\tToken : {3}\n\tCaller Member Name : {4}\n\tCaller Line Number : {5}\n\tCaller File Path : {6}".FormatS(DateTime.UtcNow, MessageTypes.CRITICAL, message, Utilities.ToSafeString(token), callerMemberName, callerLineNumber, callerFilePath));
        }

        public void LogDebug(string message, MessageToken token,  string callerMemberName = "unknown",  int callerLineNumber = 0,  string callerFilePath = "")
        {
            Console.WriteLine("[{0}] [{1}] : {2}\n\tToken : {3}\n\tCaller Member Name : {4}\n\tCaller Line Number : {5}\n\tCaller File Path : {6}".FormatS(DateTime.UtcNow, MessageTypes.DEBUG, message, Utilities.ToSafeString(token), callerMemberName, callerLineNumber, callerFilePath));
        }

        public void LogException(Exception ex, string message, MessageToken token,  string callerMemberName = "unknown",  int callerLineNumber = 0,  string callerFilePath = "")
        {
            Console.WriteLine("[{0}] [{1}] : {2}\n\tToken : {3}\n\tException Details : {4}\n\tCaller Member Name : {5}\n\tCaller Line Number : {6}\n\tCaller File Path : {7}".FormatS(DateTime.UtcNow, MessageTypes.ERROR, message, Utilities.ToSafeString(token), ex.ToString(), callerMemberName, callerLineNumber, callerFilePath));
        }

        public void LogInformation(string message, MessageToken token,  string callerMemberName = "unknown",  int callerLineNumber = 0,  string callerFilePath = "")
        {
            Console.WriteLine("[{0}] [{1}] : {2}".FormatS(DateTime.UtcNow, MessageTypes.INFORMATION, message));
        }

        public void LogWarning(string message, MessageToken token,  string callerMemberName = "unknown",  int callerLineNumber = 0, string callerFilePath = "")
        {
            Console.WriteLine("[{0}] [{1}] : {2}\n\tToken : {3}\n\tCaller Member Name : {4}\n\tCaller Line Number : {5}\n\tCaller File Path : {6}".FormatS(DateTime.UtcNow, MessageTypes.WARNING, message, Utilities.ToSafeString(token), callerMemberName, callerLineNumber, callerFilePath));
        }
    }
}
