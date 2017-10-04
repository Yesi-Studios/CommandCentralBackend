using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CommandCentral.Test
{
    public static class LoggingTests
    {

        public static void InitializeLogger()
        {
            Logging.Log.RegisterLoggers();
            Logging.Log.Info("test info message");
            Logging.Log.Debug("test debug message");
            Logging.Log.Critical("test critical message");
            Logging.Log.Exception(new Exception("test ex message"), "test ex message");
            Logging.Log.Warning("test warning message");
        }
    }
}
