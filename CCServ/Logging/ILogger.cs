using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.Logging
{
    public interface ILogger
    {
        List<MessageTypes> EnabledMessageTypes { get; set; }
                                                          
        LoggingTargetTypes TargetType { get; }

        string Name { get; }

        void LogDebug(string message, ClientAccess.MessageToken token, string callerMemberName, int callerLineNumber, string callerFilePath);

        void LogInformation(string message, ClientAccess.MessageToken token, string callerMemberName, int callerLineNumber, string callerFilePath);

        void LogCritical(string message, ClientAccess.MessageToken token, string callerMemberName, int callerLineNumber, string callerFilePath);

        void LogWarning(string message, ClientAccess.MessageToken token, string callerMemberName, int callerLineNumber, string callerFilePath);

        void LogException(Exception ex, string message, ClientAccess.MessageToken token, string callerMemberName, int callerLineNumber, string callerFilePath);

    }
}
