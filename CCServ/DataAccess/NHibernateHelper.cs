using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using CCServ.Entities;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Caches.SysCache;
using NHibernate.Cfg;
using NHibernate.Metadata;
using System.Linq;
using AtwoodUtils;
using CCServ.Logging;

namespace CCServ.DataAccess
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
        /// Initializes the NHibernate Helper with the given connection settings.
        /// </summary>
        /// <param name="settings"></param>
        private static void ConfigureNHibernate(string username, string password, string server, string database, bool printSQL)
        {
            if (printSQL)
            {
                config = Fluently.Configure().Database(
                MySQLConfiguration.Standard.ConnectionString(
                    builder => builder.Database(database)
                        .Username(username)
                        .Password(password)
                        .Server(server))
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
                    builder => builder.Database(database)
                        .Username(username)
                        .Password(password)
                        .Server(server)))
                .Cache(x => x.UseQueryCache()
                    .ProviderClass<SysCacheProvider>())
                .Mappings(x => x.FluentMappings.AddFromAssemblyOf<Person>())
                .BuildConfiguration();
            }
            
            //We're going to save the schema in case the host wants to use it later.
            _schema = new NHibernate.Tool.hbm2ddl.SchemaExport(config);
        }

        /// <summary>
        /// Builds the session factory and extracts the class metadata. 
        /// </summary>
        private static void FinishNHibernateSetup()
        {
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
        }



        #region Helper Methods

        /// <summary>
        /// Creates a new session from the session factory.
        /// </summary>
        /// <returns></returns>
        public static ISession CreateStatefulSession()
        {
            return _sessionFactory.OpenSession();
        }

        /// <summary>
        /// Craeates a new session from the session factory. This session is stateless and has no cache.
        /// </summary>
        /// <returns></returns>
        public static IStatelessSession CreateStatelessSession()
        {
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
            if (entity == null)
                return null;

            return entity.GetType().GetProperty(identifierPropertyName).GetValue(entity);
        }

        /// <summary>
        /// Released the session factory and disposes its resources.
        /// </summary>
        public static void Release()
        {
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
            return _allClassMetadata[entityName];
        }

        /// <summary>
        /// Gets all entities' metadata.
        /// </summary>
        /// <returns></returns>
        public static IDictionary<string, IClassMetadata> GetAllEntityMetadata()
        {
            return _allClassMetadata;
        }

        /// <summary>
        /// Executes the create schema script against the database, optionally dropping the current schema first.
        /// </summary>
        public static void CreateSchema(bool dropFirst)
        {
            //TODO have the schema shit pritn to the logger.
            
            if (dropFirst)
                _schema.Drop(Console.Out, true);

            _schema.Create(Console.Out, true);
        }

        #endregion

        #region Startup Methods

        /// <summary>
        /// We're going back to our roots here.  This method, using native sql, will determine if the database that NHIbernate is expecting actually exists.
        /// <para />
        /// If it doesn't, we'll make it.  Then since we just had to make it, we'll then run the schema generation script.
        /// </summary>
        [ServiceManagement.StartMethod(Priority = 99)]
        private static void SetupDatabaseAndNHibernate(CLI.Options.LaunchOptions launchOptions)
        {
            Log.Info("Beginning database integrity check...");

            //First, we need to ping the database and make sure it's replying.
            Log.Info("Confirming connection to database : '{0}'...".FormatS(launchOptions.Server));
            try
            {
                var connectionString = String.Format("server={0};uid={1};pwd={2}", launchOptions.Server, launchOptions.Username, launchOptions.Password);

                using (MySql.Data.MySqlClient.MySqlConnection connection = new MySql.Data.MySqlClient.MySqlConnection(connectionString))
                {
                    connection.Open();

                    Log.Info("Database connection established.");

                    //Ok, the connection to the database is good.  Now let's see if the schema is valid.
                    Log.Info("Confirming schema...");

                    using (MySql.Data.MySqlClient.MySqlCommand command = 
                        new MySql.Data.MySqlClient.MySqlCommand("SELECT COUNT(SCHEMA_NAME) FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = @schema", connection))
                    {
                        command.Parameters.AddWithValue("@schema", launchOptions.Database);

                        var result = command.ExecuteScalar();

                        var exists = (Convert.ToInt32(command.ExecuteScalar())) != 0;

                        if (exists)
                        {
                            Log.Info("Database schema found.");

                            Log.Info("Configuring NHibernate...");
                            ConfigureNHibernate(launchOptions.Username, launchOptions.Password, launchOptions.Server, launchOptions.Database, launchOptions.PrintSQL);
                            Log.Info("Finished configuring NHibernate. {0} class map(s) found.".FormatS(config.ClassMappings.Count));

                            Log.Info("Scanning for associated tables...");

                            List<string> nonexistantTables = new List<string>();

                            //Ok the schema was found, now we need to check to see that all the tables NHibernate expects are there.
                            //If the tables aren't there, we need to fail.
                            foreach (var table in config.ClassMappings.Select(x => x.Table))
                            {
                                command.Parameters.Clear();
                                command.CommandText = "SELECT COUNT(*) FROM information_schema.TABLES WHERE TABLE_SCHEMA = @schema AND TABLE_NAME = @table";
                                command.Parameters.AddWithValue("@schema", launchOptions.Database);
                                command.Parameters.AddWithValue("@table", table.Name);

                                //Ok is the table there?  If not, add it to a collection.
                                if ((Convert.ToInt32(command.ExecuteScalar())) == 0)
                                    nonexistantTables.Add(table.Name);
                            }

                            if (nonexistantTables.Any())
                            {
                                var exception = new Exception("One or more tables were not found in the database that NHibernate expected to exist.  Tables : {0}".FormatS(String.Join(",", nonexistantTables)));
                                Log.Exception(exception, exception.Message);
                            }
                            else
                            {
                                Log.Info("All tables found.");

                                //If we got down here, then we're ready to initialize the factory.
                                Log.Info("Initializing session factory...");
                                FinishNHibernateSetup();
                                Log.Info("Initialized session factory.");
                            }
                        }
                        else
                        {
                            Log.Warning("The database schema, '{0}', was not found.  Creating it now.".FormatS(launchOptions.Database));

                            //Database not found! Uh oh!
                            //In this case, we need to make the schema.
                            command.CommandText = "CREATE DATABASE " + launchOptions.Database;

                            command.ExecuteNonQuery();

                            Log.Info("Database created.");

                            Log.Info("Configuring NHibernate...");
                            ConfigureNHibernate(launchOptions.Username, launchOptions.Password, launchOptions.Server, launchOptions.Database, launchOptions.PrintSQL);
                            Log.Info("Finished configuring NHibernate. {0} class map(s) found.".FormatS(config.ClassMappings.Count));

                            //Since the database was just created, let's go ahead and populate it.
                            Log.Info("Populating database schema with NHibernate expected schema...");
                            CreateSchema(true);
                            Log.Info("Schema created.");

                            //If we got down here, then we're ready to initialize the factory.
                            Log.Info("Initializing session factory...");
                            FinishNHibernateSetup();
                            Log.Info("Initialized session factory.");

                            //Also, since we made everything a new, we can go ahead and ingest the old database into this database.
                            Log.Info("Ingesting old database...");
                            DataAccess.Importer.IngestOldDatabase();
                            Log.Info("Ingest complete.");
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
                            Log.Exception(ex, "Database could not be contacted!");
                            break;
                        }
                    case 1045:
                        {
                            Log.Exception(ex, "The Username/password combination for the database was invalid!");
                            break;
                        }
                    default:
                        {
                            Log.Exception(ex, "An unexpected error occurred while connecting to the database!");
                            break;
                        }
                }
            }
        }

        #endregion

    }
}
