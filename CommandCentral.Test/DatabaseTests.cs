using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Test
{
    [TestClass]
    public class DatabaseTests
    {
        [TestMethod]
        public void SetupInMemoryDatabase()
        {

            MySql.Server.MySqlServer server = MySql.Server.MySqlServer.Instance;

            server.StartServer(3000);

            var dbName = "commandcentraltestdatabase_" + AtwoodUtils.Utilities.RandomString(5);

            DataAccess.DataProvider.InitializeAndRebuild(new MySql.Data.MySqlClient.MySqlConnectionStringBuilder(server.GetConnectionString(dbName)), dbName);

            Assert.IsTrue(DataAccess.DataProvider.IsReady);
        }

        [TestMethod]
        public void SetupRealDatabase()
        {
            MySql.Server.MySqlServer server = MySql.Server.MySqlServer.Instance;

            server.StartServer(3000);

            var dbName = "commandcentraltestdatabase_" + AtwoodUtils.Utilities.RandomString(5);

            DataAccess.DataProvider.InitializeAndRebuild(new MySql.Data.MySqlClient.MySqlConnectionStringBuilder(server.GetConnectionString(dbName)), dbName);

            Assert.IsTrue(DataAccess.DataProvider.IsReady);
        }

        [TestMethod]
        public void UpdateForeignKeyRuleForWatchAssignment()
        {
            Entities.Watchbill.WatchAssignment.WatchAssignmentMapping.UpdateForeignKeyRule();
        }
    }
}
