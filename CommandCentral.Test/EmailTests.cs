using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Test
{
    [TestClass]
    public class EmailTests
    {
        [TestMethod]
        public void SetupEmail()
        {
            Email.EmailInterface.CCEmailMessage.InitializeEmail(TestSettings.SMTPHosts.ToArray());
        }

    }
}
