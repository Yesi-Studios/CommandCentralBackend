using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using CommandCentral.Entities;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Caches.SysCache;
using NHibernate.Cfg;
using NHibernate.Metadata;
using System.Linq;
using AtwoodUtils;

namespace CommandCentral.DataAccess
{
    /// <summary>
    /// Provides singleton managed access to NHibernate sessions.
    /// </summary>
    public static class NHibernateHelper
    {

        private static ISessionFactory _sessionFactory;

        private static NHibernate.Tool.hbm2ddl.SchemaExport _schema;

        private static ConcurrentDictionary<string, IClassMetadata> _allClassMetadata;

        private static Configuration config = null;

        /// <summary>
        /// Indicates that the NHibernateHelper has been initialized.
        /// </summary>
        public static bool IsInitialized { get; set; }

        /// <summary>
        /// Initializes the NHibernate Helper with the given connection settings.  Failure to call this method prior to DB interaction will cause all calls to fail.
        /// </summary>
        /// <param name="settings"></param>
        public static void InitializeNHibernate(ConnectionSettings settings)
        {

            if (settings.VerboseLogging)
            {
                config = Fluently.Configure().Database(
                MySQLConfiguration.Standard.ConnectionString(
                    builder => builder.Database(settings.Database)
                        .Username(settings.Username)
                        .Password(settings.Password)
                        .Server(settings.Server))
                    .ShowSql())
                .Cache(x => x.UseQueryCache()
                    .ProviderClass<SysCacheProvider>())
                .Mappings(x => x.FluentMappings.AddFromAssemblyOf<Person>())
                .BuildConfiguration();
            }
            else
            {
                config = Fluently.Configure().Database(
                MySQLConfiguration.Standard.ConnectionString(
                    builder => builder.Database(settings.Database)
                        .Username(settings.Username)
                        .Password(settings.Password)
                        .Server(settings.Server)))
                .Cache(x => x.UseQueryCache()
                    .ProviderClass<SysCacheProvider>())
                .Mappings(x => x.FluentMappings.AddFromAssemblyOf<Person>())
                .BuildConfiguration();
            }

            //We're going to save the schema in case the host wants to use it later.
            _schema = new NHibernate.Tool.hbm2ddl.SchemaExport(config);

            _sessionFactory = config.BuildSessionFactory();

            _allClassMetadata = new ConcurrentDictionary<string, IClassMetadata>(
                _sessionFactory.GetAllClassMetadata()
                    .ToList()
                    .Select(x =>
                    {
                        return new KeyValuePair<string, IClassMetadata>(
                            x.Key.Split('.').Last(),
                            x.Value);
                    })
                    .ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase));

            IsInitialized = true;
        }

        #region Helper Methods

        /// <summary>
        /// Creates a new session from the session factory.
        /// </summary>
        /// <returns></returns>
        public static ISession CreateStatefulSession()
        {
            if (!IsInitialized)
                throw new Exception("The NHibernate Helper has not yet been initialized.");

            return _sessionFactory.OpenSession();
        }

        /// <summary>
        /// Craeates a new session from the session factory. This session is stateless and has no cache.
        /// </summary>
        /// <returns></returns>
        public static IStatelessSession CreateStatelessSession()
        {
            if (!IsInitialized)
                throw new Exception("The NHibernate Helper has not yet been initialized.");

            return _sessionFactory.OpenStatelessSession();
        }

        /// <summary>
        /// Gets the Id for a given object.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="identifierPropertyName"></param>
        /// <returns></returns>
        public static object GetIdentifier(object entity, string identifierPropertyName = "Id")
        {
            return entity.GetType().GetProperty(identifierPropertyName).GetValue(entity);
        }

        /// <summary>
        /// Released the session factory and disposes its resources.
        /// </summary>
        public static void Release()
        {
            if (!IsInitialized)
                throw new Exception("The NHibernate Helper has not yet been initialized.");

            _sessionFactory.Close();
            _sessionFactory.Dispose();
        }

        /// <summary>
        /// Returns the entity data for a given entity.  This is all the information included in the mapping file.
        /// </summary>
        /// <param name="entityName"></param>
        /// <returns></returns>
        public static IClassMetadata GetEntityMetadata(string entityName)
        {
            if (!IsInitialized)
                throw new Exception("The NHibernate Helper has not yet been initialized.");

            return _allClassMetadata[entityName];
        }

        /// <summary>
        /// Gets all entities' metadata.
        /// </summary>
        /// <returns></returns>
        public static IDictionary<string, IClassMetadata> GetAllEntityMetadata()
        {
            if (!IsInitialized)
                throw new Exception("The NHibernate Helper has not yet been initialized.");

            return _allClassMetadata;
        }

        /// <summary>
        /// Executes the create schema script against the database, optionally dropping the current schema first.
        /// </summary>
        public static void CreateSchema(bool dropFirst)
        {
            if (!IsInitialized)
                throw new Exception("The NHibernate Helper has not yet been initialized.");

            System.IO.TextWriter writer = Communicator.TextWriter ?? Console.Out;
            
            if (dropFirst)
                _schema.Drop(writer, true);

            _schema.Create(writer, true);
        }

        #endregion

        #region Startup Methods

        /// <summary>
        /// We're going back to our roots here.  This method, using native sql, will determine if the database that NHIbernate is expecting actually exists.
        /// <para />
        /// If it doesn't, we'll make it.  Then since we just had to make it, we'll then run the schema generation script.
        /// </summary>
        [ServiceManagement.StartMethod(Priority = 100)]
        private static void ConfirmDatabaseIntegrity()
        {
            var currentSettings = DataAccess.ConnectionSettings.PredefinedConnectionSettings[DataAccess.ConnectionSettings.CurrentSettingsKey];

            Communicator.PostMessage("Beginning database integrity check...", Communicator.MessageTypes.Informational);

            //First, we need to ping the database and make sure it's replying.
            Communicator.PostMessage("Confirming connection to database : '{0}'...".FormatS(currentSettings.Server), Communicator.MessageTypes.Informational);
            try
            {
                var connectionString = String.Format("server={0};uid={1};pwd={2}", currentSettings.Server, currentSettings.Username, currentSettings.Password);

                using (MySql.Data.MySqlClient.MySqlConnection connection = new MySql.Data.MySqlClient.MySqlConnection(connectionString))
                {
                    connection.Open();

                    Communicator.PostMessage("Database connection established.", Communicator.MessageTypes.Informational);

                    //Ok, the connection to the database is good.  Now let's see if the schema is valid.
                    Communicator.PostMessage("Confirming schema...", Communicator.MessageTypes.Informational);

                    using (MySql.Data.MySqlClient.MySqlCommand command = 
                        new MySql.Data.MySqlClient.MySqlCommand("SELECT COUNT(SCHEMA_NAME) FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = @schema", connection))
                    {
                        command.Parameters.AddWithValue("@schema", currentSettings.Database);

                        var exists = ((int)command.ExecuteScalar()) != 0;

                        if (exists)
                        {
                            Communicator.PostMessage("Database schema found.", Communicator.MessageTypes.Informational);
                            Communicator.PostMessage("Scanning for tables...", Communicator.MessageTypes.Informational);

                            List<string> nonexistantTables = new List<string>();

                            //Ok the schema was found, now we need to check to see that all the tables NHibernate expects are there.
                            //If the tables aren't there, we need to fail.
                            foreach (var table in config.ClassMappings.Select(x => x.Table))
                            {
                                command.Parameters.Clear();
                                command.CommandText = "SELECT COUNT(*) FROM information_schema.TABLES WHERE TABLE_SCHEMA = @schema AND TABLE_NAME = @table";
                                command.Parameters.AddWithValue("@schema", currentSettings.Database);
                                command.Parameters.AddWithValue("@table", table.Name);

                                //Ok is the table there?  If not, add it to a collection.
                                if (((int)command.ExecuteScalar()) != 0)
                                    nonexistantTables.Add(table.Name);
                            }

                            if (nonexistantTables.Any())
                            {
                                Communicator.PostMessage("One or more tables were not found in the database that NHibernate expected to exist.  Tables : {0}".FormatS(String.Join(",", nonexistantTables)), Communicator.MessageTypes.Critical);
                                throw new Exception("One or more tables were not found in the database that NHibernate expected to exist.  Tables : {0}".FormatS(String.Join(",", nonexistantTables)));
                            }
                            else
                            {
                                Communicator.PostMessage("All tables found.", Communicator.MessageTypes.Informational);
                            }
                        }
                        else
                        {
                            Communicator.PostMessage("The database schema, '{0}', was not found.  Creating it now.".FormatS(currentSettings.Database), Communicator.MessageTypes.Warning);

                            //Database not found! Uh oh!
                            //In this case, we need to make the schema.
                            command.CommandText = "CREATE DATABASE IF NOT EXISTS @schema";

                            command.ExecuteNonQuery();

                            Communicator.PostMessage("Database schema created.", Communicator.MessageTypes.Warning);

                            //Since the database was just created, let's go ahead and populate it.
                            CreateSchema(true);
                        }
                    }
                }

            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                switch (ex.Number)
                {
                    case 0:
                        {
                            Communicator.PostMessage("Database could not be contacted!  Throwing exception!", Communicator.MessageTypes.Critical);
                            throw new Exception("The server at '{0}' did not reply.".FormatS(currentSettings.Server));
                        }
                    case 1045:
                        {
                            Communicator.PostMessage("The Username/password combination for the database was invalid! Throwing exception!", Communicator.MessageTypes.Critical);
                            throw new Exception("The Username/password combination for the database was invalid");
                        }
                    default:
                        {
                            Communicator.PostMessage("An unexpected error occured while connecting to the database! Throwing exception!  Error: {0}".FormatS(ex.Message), Communicator.MessageTypes.Critical);
                            throw ex;
                        }
                }
            }
        }

        #endregion

    }
}
