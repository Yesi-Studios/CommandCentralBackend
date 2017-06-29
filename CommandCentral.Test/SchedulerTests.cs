using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Test
{
    [TestClass]
    public class SchedulerTests
    {
        [TestMethod]
        public void InitializeFluentScheduler()
        {
            ServiceManagement.ServiceManager.InitializeFluentScheduler();
        }

    }
}
