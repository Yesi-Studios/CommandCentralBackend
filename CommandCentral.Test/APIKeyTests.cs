using CommandCentral.ClientAccess;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Test
{
    [TestClass]
    public class APIKeyTests
    {

        /// <summary>
        /// This is the expected primary API key that should exist at all times in the database.
        /// </summary>
        private static APIKey _primaryAPIKey = new APIKey
        {
            ApplicationName = "Command Central Official Frontend",
            Id = Guid.Parse("90FDB89F-282B-4BD6-840B-CEF597615728")
        };

        [TestMethod]
        public void EnsureDefaultAPIKeyExistsAndAddIfItDoesnt()
        {
            using (var session = DataAccess.DataProvider.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    if (session.QueryOver<APIKey>().Where(x => x.Id == _primaryAPIKey.Id).RowCount() == 0)
                    {
                        session.Save(_primaryAPIKey);
                    }

                    transaction.Commit();
                }
            }

            using (var session = DataAccess.DataProvider.CreateStatefulSession())
            {
                Assert.IsTrue(session.Get<APIKey>(_primaryAPIKey.Id) != null);
            }
        }
    }
}
