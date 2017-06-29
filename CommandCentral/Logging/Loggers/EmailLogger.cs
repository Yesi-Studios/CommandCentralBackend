using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandCentral.ClientAccess;

namespace CommandCentral.Logging.Loggers
{
    class EmailLogger : ILogger
    {
        public string Name
        {
            get
            {
                return "CommandCentral Email Logger";
            }
        }

        public LoggingTargetTypes TargetType
        {
            get
            {
                return LoggingTargetTypes.EMAIL;
            }
        }

        public void LogCritical(string message, MessageToken token, string callerMemberName, int callerLineNumber, string callerFilePath)
        {
            var model = new Email.Models.CriticalMessageEmailModel
            {
                CallerFilePath = callerFilePath,
                CallerLineNumber = callerLineNumber,
                CallerMemberName = callerMemberName,
                Message = message,
                Token = token
            };

            Email.EmailInterface.CCEmailMessage
                .CreateDefault()
                .To(Email.EmailInterface.CCEmailMessage.DeveloperAddress)
                .CC(Email.EmailInterface.CCEmailMessage.PersonalDeveloperAddresses)
                .Subject("Command Central Critical Message")
                .HTMLAlternateViewUsingTemplateFromEmbedded("CommandCentral.Email.Templates.CriticalMessage_HTML.html", model)
                .SendWithRetryAndFailure(TimeSpan.FromSeconds(1));
        }

        public void LogException(Exception ex, string message, MessageToken token, string callerMemberName, int callerLineNumber, string callerFilePath)
        {
            var model = new Email.Models.FatalErrorEmailModel
            {
                Exception = ex,
                OriginalMessage = message,
                Token = token
            };

            Email.EmailInterface.CCEmailMessage
                .CreateDefault()
                .To(Email.EmailInterface.CCEmailMessage.DeveloperAddress)
                .CC(Email.EmailInterface.CCEmailMessage.PersonalDeveloperAddresses)
                .Subject("Command Central Fatal Error")
                .HTMLAlternateViewUsingTemplateFromEmbedded("CommandCentral.Email.Templates.FatalError_HTML.html", model)
                .SendWithRetryAndFailure(TimeSpan.FromSeconds(1));
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
