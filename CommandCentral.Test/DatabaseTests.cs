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
        public void SetupDatabase()
        {
            MySql.Server.MySqlServer server = MySql.Server.MySqlServer.Instance;

            server.StartServer(3000);

            var dbName = "commandcentraltestdatabase_" + AtwoodUtils.Utilities.RandomString(5);

            MySql.Data.MySqlClient.MySqlHelper.ExecuteNonQuery(server.GetConnectionString(), "DROP DATABASE IF EXISTS " + dbName);
            MySql.Data.MySqlClient.MySqlHelper.ExecuteNonQuery(server.GetConnectionString(), "CREATE DATABASE IF NOT EXISTS " + dbName);

            DataAccess.DataProvider.InitializeAndRebuild(new MySql.Data.MySqlClient.MySqlConnectionStringBuilder(server.GetConnectionString(dbName)), dbName);

            server.ShutDown();

            Assert.IsTrue(DataAccess.DataProvider.IsReady);
        }
    }
}
