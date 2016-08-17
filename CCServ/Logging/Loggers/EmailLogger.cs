using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CCServ.ClientAccess;

namespace CCServ.Logging.Loggers
{
    class EmailLogger : ILogger
    {
        public List<MessageTypes> EnabledMessageTypes { get; set; }
        
        public string Name
        {
            get
            {
                return "CCSERV Email Logger";
            }
        }

        public LoggingTargetTypes TargetType
        {
            get
            {
                return LoggingTargetTypes.EMAIL;
            }
        }

        public bool IsValid()
        {
            //TODO do something better here. Maybe... the email sender can handle errors if they occur so it's not a big deal.
            return true;
        }

        public void LogCritical(string message, MessageToken token, string callerMemberName, int callerLineNumber, string callerFilePath)
        {
            //TODO create an email that gets sent by this critical event.
        }

        public void LogException(Exception ex, string message, MessageToken token, string callerMemberName, int callerLineNumber, string callerFilePath)
        {
            if (EnabledMessageTypes.Contains(MessageTypes.ERROR))
            {
                new Email.FatalErrorEmail(new Email.Args.FatalErrorEmailArgs
                {
                    Exception = ex,
                    OriginalMessage = message,
                    Subject = "CommandCentral Fatal Error",
                    Token = token,
                    ToAddressList = new List<string> { Config.Email.AtwoodAddress.Address, Config.Email.DeveloperDistroAddress.Address, Config.Email.McLean.Address }
                }).Send();
            }
        }

        public void LogDebug(string message, MessageToken token, string callerMemberName, int callerLineNumber, string callerFilePath)
        {
            //Note: the email logger does nothing for these messages.
        }

        public void LogInformation(string message, MessageToken token, string callerMemberName, int callerLineNumber, string callerFilePath)
        {
            //Note: the email logger does nothing for these messages.
        }

        public void LogWarning(string message, MessageToken token, string callerMemberName, int callerLineNumber, string callerFilePath)
        {
            //Note: the email logger does nothing for these messages.
        }
    }
}
