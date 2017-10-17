using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CommandCentral.Test
{
    public static class EmailTests
    {
        public static void SetupEmail()
        {
            Email.EmailInterface.CCEmailMessage.InitializeEmail(TestSettings.SMTPHosts.ToArray());
        }

    }
}
