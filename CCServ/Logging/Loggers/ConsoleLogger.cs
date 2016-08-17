﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;
using CCServ.ClientAccess;

namespace CCServ.Logging.Loggers
{
    class ConsoleLogger : ILogger
    {

        public ConsoleLogger()
        {
        }

        public List<MessageTypes> EnabledMessageTypes { get; set; }

        public int MaxEntryLength
        {
            get
            {
                return 2000;
            }
        }

        public string Name
        {
            get
            {
                return "CCSERV Console Logger";
            }
        }

        public LoggingTargetTypes TargetType
        {
            get
            {
                return LoggingTargetTypes.CONSOLE;
            }
        }

        public bool IsValid()
        {
            return Environment.UserInteractive;
        }

        public void LogCritical(string message, MessageToken token, string callerMemberName = "unknown",  int callerLineNumber = 0,  string callerFilePath = "")
        {
            if (EnabledMessageTypes.Contains(MessageTypes.CRITICAL))
                Console.WriteLine("[{0}] [{1}] : {2}\n\tToken : {3}\n\tCaller Member Name : {4}\n\tCaller Line Number : {5}\n\tCaller File Path : {6}".FormatS(DateTime.Now, MessageTypes.CRITICAL, message, Utilities.ToSafeString(token), callerMemberName, callerLineNumber, callerFilePath));
        }

        public void LogDebug(string message, MessageToken token,  string callerMemberName = "unknown",  int callerLineNumber = 0,  string callerFilePath = "")
        {
            if (EnabledMessageTypes.Contains(MessageTypes.DEBUG))
                Console.WriteLine("[{0}] [{1}] : {2}\n\tToken : {3}\n\tCaller Member Name : {4}\n\tCaller Line Number : {5}\n\tCaller File Path : {6}".FormatS(DateTime.Now, MessageTypes.DEBUG, message, Utilities.ToSafeString(token), callerMemberName, callerLineNumber, callerFilePath));
        }

        public void LogException(Exception ex, string message, MessageToken token,  string callerMemberName = "unknown",  int callerLineNumber = 0,  string callerFilePath = "")
        {
            if (EnabledMessageTypes.Contains(MessageTypes.ERROR))
                Console.WriteLine("[{0}] [{1}] : {2}\n\tToken : {3}\n\tException Details : {4}\n\tCaller Member Name : {5}\n\tCaller Line Number : {6}\n\tCaller File Path : {7}".FormatS(DateTime.Now, MessageTypes.ERROR, message, Utilities.ToSafeString(token), ex.ToString(), callerMemberName, callerLineNumber, callerFilePath));
        }

        public void LogInformation(string message, MessageToken token,  string callerMemberName = "unknown",  int callerLineNumber = 0,  string callerFilePath = "")
        {
            if (EnabledMessageTypes.Contains(MessageTypes.INFORMATION))
                Console.WriteLine("[{0}] [{1}] : {2}".FormatS(DateTime.Now, MessageTypes.INFORMATION, message));
        }

        public void LogWarning(string message, MessageToken token,  string callerMemberName = "unknown",  int callerLineNumber = 0, string callerFilePath = "")
        {
            if (EnabledMessageTypes.Contains(MessageTypes.WARNING))
                Console.WriteLine("[{0}] [{1}] : {2}\n\tToken : {3}\n\tCaller Member Name : {4}\n\tCaller Line Number : {5}\n\tCaller File Path : {6}".FormatS(DateTime.Now, MessageTypes.WARNING, message, Utilities.ToSafeString(token), callerMemberName, callerLineNumber, callerFilePath));
        }
    }
}