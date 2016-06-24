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

        private static bool isInitialized = false;

        /// <summary>
        /// Initializes the NHibernate Helper with the given connection settings.  Failure to call this method prior to DB interaction will cause all calls to fail.
        /// </summary>
        /// <param name="settings"></param>
        public static void InitializeNHibernate(ConnectionSettings settings)
        {
            Configuration configuration = null;
            
            if (settings.VerboseLogging)
            {
                configuration = Fluently.Configure().Database(
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
                configuration = Fluently.Configure().Database(
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
            _schema = new NHibernate.Tool.hbm2ddl.SchemaExport(configuration);

            _sessionFactory = configuration.BuildSessionFactory();

            _allClassMetadata = new ConcurrentDictionary<string, IClassMetadata>(
                _sessionFactory.GetAllClassMetadata()
                    .ToList()
                    .Select(x =>
                    {
                        return new KeyValuePair<string, IClassMetadata>(
                            x.Key.Split('.').Last(),
                            x.Value);
                    })
                    .ToDictionary(x => x.Key, x => x.Value));

            isInitialized = true;
        }

        /// <summary>
        /// Creates a new session from the session factory.
        /// </summary>
        /// <returns></returns>
        public static ISession CreateStatefulSession()
        {
            if (!isInitialized)
                throw new Exception("The NHibernate Helper has not yet been initialized.");

            return _sessionFactory.OpenSession();
        }

        /// <summary>
        /// Craeates a new session from the session factory. This session is stateless and has no cache.
        /// </summary>
        /// <returns></returns>
        public static IStatelessSession CreateStatelessSession()
        {
            if (!isInitialized)
                throw new Exception("The NHibernate Helper has not yet been initialized.");

            return _sessionFactory.OpenStatelessSession();
        }

        /// <summary>
        /// Released the session factory and disposes its resources.
        /// </summary>
        public static void Release()
        {
            if (!isInitialized)
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
            if (!isInitialized)
                throw new Exception("The NHibernate Helper has not yet been initialized.");

            return _allClassMetadata[entityName];
        }

        /// <summary>
        /// Gets all entities' metadata.
        /// </summary>
        /// <returns></returns>
        public static IDictionary<string, IClassMetadata> GetAllEntityMetadata()
        {
            if (!isInitialized)
                throw new Exception("The NHibernate Helper has not yet been initialized.");

            return _allClassMetadata;
        }

        /// <summary>
        /// Executes the create schema script against the database, optionally dropping the current schema first.
        /// </summary>
        public static void CreateSchema(bool dropFirst)
        {
            if (!isInitialized)
                throw new Exception("The NHibernate Helper has not yet been initialized.");

            System.IO.TextWriter writer = Communicator.TextWriter ?? Console.Out;
            
            if (dropFirst)
                _schema.Drop(writer, true);

            _schema.Create(writer, true);
        }
        


    }
}
