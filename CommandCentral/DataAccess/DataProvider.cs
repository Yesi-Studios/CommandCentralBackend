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
using CommandCentral.Logging;
using MySql.Data.MySqlClient;
using System.IO;
using CommandCentral.ServiceManagement;
using NHibernate.Tool.hbm2ddl;

namespace CommandCentral.DataAccess
{
    /// <summary>
    /// Provides singleton managed access to NHibernate sessions.
    /// </summary>
    public static class DataProvider
    {

        #region Properties

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

        #endregion

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
        private static void CreateSchema(bool printSQL)
        {
            _schema.Create(str =>
            {
                if (printSQL)
                    str.WriteLine();
            }, true);
        }

        /// <summary>
        /// We're going back to our roots here.  This method, using native sql, will determine if the database that NHibernate is expecting actually exists.
        /// <para />
        /// If it doesn't, we'll make it.  Then since we just had to make it, we'll then run the schema generation script.
        /// </summary>
        public static void Initialize(MySqlConnectionStringBuilder connectionString)
        {
            Log.Info("Configuring NHibernate...");

            config = Fluently.Configure().Database(MySQLConfiguration.Standard.ConnectionString(connectionString.GetConnectionString(true))
                //.ShowSql()
                )
                .Cache(x => x.UseSecondLevelCache().UseQueryCache()
                .ProviderClass<SysCacheProvider>())
                .Mappings(x => x.FluentMappings.AddFromAssemblyOf<Person>())
                .BuildConfiguration();

            //We're going to save the schema in case the host wants to use it later.
            _schema = new SchemaExport(config);
            Log.Info("Finished configuring NHibernate. {0} class map(s) found.".With(config.ClassMappings.Count));

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
        }

        /// <summary>
        /// We're going back to our roots here.  This method, using native sql, will determine if the database that NHibernate is expecting actually exists.
        /// <para />
        /// If it doesn't, we'll make it.  Then since we just had to make it, we'll then run the schema generation script.
        /// </summary>
        public static void InitializeAndRebuild(MySqlConnectionStringBuilder connectionString, string database)
        {
            Log.Info("Configuring NHibernate...");

            config = Fluently.Configure().Database(MySQLConfiguration.Standard.ConnectionString(connectionString.GetConnectionString(true))
                //.ShowSql()
                )
                .Cache(x => x.UseSecondLevelCache().UseQueryCache()
                .ProviderClass<SysCacheProvider>())
                .Mappings(x => x.FluentMappings.AddFromAssemblyOf<Person>())
                .BuildConfiguration();

            //We're going to save the schema in case the host wants to use it later.
            _schema = new SchemaExport(config);
            Log.Info("Finished configuring NHibernate. {0} class map(s) found.".With(config.ClassMappings.Count));

            connectionString.Database = null;
            using (var connection = new MySqlConnection(connectionString.GetConnectionString(true)))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "DROP DATABASE IF EXISTS `{0}`".With(database);
                    command.ExecuteNonQuery();

                    command.CommandText = "CREATE DATABASE `{0}`".With(database);
                    command.ExecuteNonQuery();
                }
            }

            _schema.Create(false, true);

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
        }

        #endregion
    }
}
