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
using MySql.Data.MySqlClient;
using System.IO;

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
        /// Indicates if the underlying session factory is ready to start creating sessions for database access.
        /// </summary>
        public static bool IsReady
        {
            get
            {
                return _sessionFactory != null;
            }
        }

        /// <summary>
        /// Initializes the NHibernate Helper with the given connection settings.
        /// </summary>
        /// <param name="options"></param>
        private static void ConfigureNHibernate(CLI.Options.LaunchOptions options)
        {
            

            
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
        /// Creates a new session from the session factory. This session is stateless and has no cache.
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
        /// Executes the create schema script against the database.
        /// </summary>
        public static void CreateSchema(bool printSQL)
        {
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
            Log.Info("Configuring NHibernate...");
            string connectionString = "server={0};database={1};user={2};password={3};CertificatePassword={4};SSL Mode={5}"
                .FormatS(launchOptions.Server, launchOptions.Database, launchOptions.Username, launchOptions.Password, 
                launchOptions.CertificatePassword, launchOptions.SecurityMode == CLI.SecurityModes.Both || launchOptions.SecurityMode == CLI.SecurityModes.DBOnly ? "Required" : "None");

            if (launchOptions.PrintSQL)
            {
                config = Fluently.Configure().Database(MySQLConfiguration.Standard.ConnectionString(connectionString)
                            .ShowSql())
                            .Cache(x => x.UseSecondLevelCache().UseQueryCache()
                            .ProviderClass<SysCacheProvider>())
                            .Mappings(x => x.FluentMappings.AddFromAssemblyOf<Person>())
                            .BuildConfiguration();
            }
            else
            {
                config = Fluently.Configure().Database(MySQLConfiguration.Standard.ConnectionString(connectionString))
                            .Cache(x => x.UseSecondLevelCache().UseQueryCache()
                            .ProviderClass<SysCacheProvider>())
                            .Mappings(x => x.FluentMappings.AddFromAssemblyOf<Person>())
                            .BuildConfiguration();
            }

            //We're going to save the schema in case the host wants to use it later.
            _schema = new NHibernate.Tool.hbm2ddl.SchemaExport(config);
            Log.Info("Finished configuring NHibernate. {0} class map(s) found.".FormatS(config.ClassMappings.Count));

            Log.Info("Beginning database integrity check...");

            //First, we need to ping the database and make sure it's replying.
            Log.Info("Confirming connection to database : '{0}'...".FormatS(launchOptions.Server));
            try
            {
                var rawConnectionString = String.Format("server={0};uid={1};pwd={2}", launchOptions.Server, launchOptions.Username, launchOptions.Password);

                using (MySqlConnection connection = new MySqlConnection(rawConnectionString))
                {
                    connection.Open();

                    Log.Info("Database connection established.");

                    //Does the client want us to drop the schema?
                    if (launchOptions.Rebuild)
                    {
                        Log.Info("Dropping database if it exists...");
                        using (var command = new MySql.Data.MySqlClient.MySqlCommand("DROP DATABASE IF EXISTS {0}".FormatS(launchOptions.Database), connection))
                        {
                            command.ExecuteNonQuery();
                        }
                        Log.Info("Dropped database (if it exists).");

                        using (var command = new MySql.Data.MySqlClient.MySqlCommand("CREATE DATABASE {0}".FormatS(launchOptions.Database), connection))
                        {
                            command.ExecuteNonQuery();
                        }

                        //Since we just dropped it, let's build it.
                        CreateSchema(launchOptions.PrintSQL);
                    }

                    //Ok, the connection to the database is good.  Now let's see if the schema is valid.
                    Log.Info("Confirming schema...");

                    using (MySqlCommand command = 
                        new MySqlCommand("SELECT COUNT(SCHEMA_NAME) FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = @schema", connection))
                    {
                        command.Parameters.AddWithValue("@schema", launchOptions.Database);

                        var result = command.ExecuteScalar();

                        var exists = (Convert.ToInt32(command.ExecuteScalar())) != 0;

                        if (!exists)
                        {
                            throw new Exception("The database schema, '{0}', does not exist.  Please consider running Command Central with the --rebuild option to build the schema.".FormatS(launchOptions.Database));
                        }
                    }

                    Log.Info("Database schema found.");
                    Log.Info("Building NHibernate session factory...");
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
                            .ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase), StringComparer.OrdinalIgnoreCase);
                    Log.Info("NHibernate session factory built.");
                    Log.Info("Scanning for associated tables...");

                    
                    //Ok the schema was found, now we need to check to see that all the tables NHibernate expects are there.
                    //If the tables aren't there, we need to fail.
                    List<string> nonexistantTables = new List<string>();

                    foreach (var table in config.ClassMappings.Select(x => x.Table))
                    {
                        using (MySqlCommand command = 
                            new MySqlCommand("SELECT COUNT(*) FROM information_schema.TABLES WHERE TABLE_SCHEMA = @schema AND TABLE_NAME = @table", connection))
                        {
                            command.Parameters.AddWithValue("@schema", launchOptions.Database);
                            command.Parameters.AddWithValue("@table", table.Name);

                            if ((Convert.ToInt32(command.ExecuteScalar())) == 0)
                                nonexistantTables.Add(table.Name);
                        }
                    }

                    if (nonexistantTables.Any())
                    {
                        throw new Exception("One or more tables were not found in the database that NHibernate expected to exist.  Tables : {0}".FormatS(String.Join(",", nonexistantTables)));
                    }
                    else
                    {
                        Log.Info("All tables found.");
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
