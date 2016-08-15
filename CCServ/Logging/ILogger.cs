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
        int MaxEntryLength { get; }

        List<MessageTypes> EnabledMessageTypes { get; set; }
                                                          
        LoggingTargetTypes TargetType { get; }

        string Name { get; }

        bool IsValid();

        void LogDebug(string message, ClientAccess.MessageToken token, [CallerMemberName] string callerMemberName = "unknown", [CallerLineNumber] int callerLineNumber = 0, [CallerFilePath] string callerFilePath = "");

        void LogInformation(string message, ClientAccess.MessageToken token, [CallerMemberName] string callerMemberName = "unknown", [CallerLineNumber] int callerLineNumber = 0, [CallerFilePath] string callerFilePath = "");

        void LogCritical(string message, ClientAccess.MessageToken token, [CallerMemberName] string callerMemberName = "unknown", [CallerLineNumber] int callerLineNumber = 0, [CallerFilePath] string callerFilePath = "");

        void LogWarning(string message, ClientAccess.MessageToken token, [CallerMemberName] string callerMemberName = "unknown", [CallerLineNumber] int callerLineNumber = 0, [CallerFilePath] string callerFilePath = "");

        void LogException(Exception ex, string message, ClientAccess.MessageToken token, [CallerMemberName] string callerMemberName = "unknown", [CallerLineNumber] int callerLineNumber = 0, [CallerFilePath] string callerFilePath = "");

    }
}
