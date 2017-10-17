using AtwoodUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace CommandCentral.Test
{
    public static class DatabaseTests
    {
        public static void SetupInMemoryDatabase()
        {

            var server = MySql.Server.MySqlServer.Instance;

            server.StartServer(3000);

            var dbName = "commandcentraltestdatabase_" + Utilities.RandomString(5);

            DataAccess.DataProvider.InitializeAndRebuild(new MySql.Data.MySqlClient.MySqlConnectionStringBuilder(server.GetConnectionString(dbName)), dbName);

            Assert.IsTrue(DataAccess.DataProvider.IsReady);
        }

        public static void SetupRealDatabase()
        {
            var connectionString = "server={0};database={1};user={2};password={3};"
                .With(TestSettings.Server, TestSettings.Database, TestSettings.Username, TestSettings.Password);

            var result = MySql.Data.MySqlClient.MySqlHelper.ExecuteScalar("server={0};user={1};password={2};".With(TestSettings.Server, TestSettings.Username, TestSettings.Password),
                "SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{0}'".With(TestSettings.Database));

            if (result == null || TestSettings.RebuildIfExists)
            {
                DataAccess.DataProvider.InitializeAndRebuild(new MySql.Data.MySqlClient.MySqlConnectionStringBuilder(connectionString), TestSettings.Database);
            }
            else
            {
                DataAccess.DataProvider.Initialize(new MySql.Data.MySqlClient.MySqlConnectionStringBuilder(connectionString));
            }

            Assert.IsTrue(DataAccess.DataProvider.IsReady);
        }

        public static void UpdateForeignKeyRuleForWatchAssignment()
        {
            Entities.Watchbill.WatchAssignment.WatchAssignmentMapping.UpdateForeignKeyRule();
        }
    }
}
