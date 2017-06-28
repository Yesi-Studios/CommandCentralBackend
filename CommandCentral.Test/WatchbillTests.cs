using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommandCentral.Test
{
    [TestClass]
    public class WatchbillTests
    {

        [TestMethod]
        public void CreateEligibilityGroup()
        {
            using (var session = DataAccess.DataProvider.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {



                }
            }
        }

        [TestMethod]
        public void CreateWatchbill()
        {
            
        }
    }
}
