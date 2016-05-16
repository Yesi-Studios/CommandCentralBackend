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

        private static readonly ISessionFactory _sessionFactory;

        private static readonly NHibernate.Tool.hbm2ddl.SchemaExport _schema;

        private static readonly ConcurrentDictionary<string, IClassMetadata> _allClassMetadata;

        /// <summary>
        /// Static initializer sets up the NHibernate configuration and scans the assembly for all class maps.
        /// </summary>
        static NHibernateHelper()
        {
           Configuration configuration = Fluently.Configure().Database(
                MySQLConfiguration.Standard.ConnectionString(
                    builder => builder.Database("test_db")
                        .Username("xanneth")
                        .Password("douglas0678")
                        .Server("localhost"))
                    .ShowSql())
                .Cache(x => x.UseQueryCache()
                    .ProviderClass<SysCacheProvider>())
                .Mappings(x => x.FluentMappings.AddFromAssemblyOf<Person>())
                .BuildConfiguration();

            /*Configuration configuration = Fluently.Configure().Database(
                MySQLConfiguration.Standard.ConnectionString(
                    builder => builder.Database("test")
                        .Username("niocga")
                        .Password("niocga")
                        .Server("gord14ec204")))
                .Cache(x => x.UseQueryCache()
                    .ProviderClass<SysCacheProvider>())
                .Mappings(x => x.FluentMappings.AddFromAssemblyOf<Person>())
                .BuildConfiguration();*/

            /*Configuration configuration = Fluently.Configure().Database(
                MySQLConfiguration.Standard.ConnectionString(
                    builder => builder.Database("test")
                        .Username("admin")
                        .Password("password")
                        .Server("localhost")))
                .Cache(x => x.UseQueryCache()
                    .ProviderClass<SysCacheProvider>())
                .Mappings(x => x.FluentMappings.AddFromAssemblyOf<Person>())
                .BuildConfiguration();*/

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
                    .ToDictionary(x => x.Key, x=> x.Value));
        }

        /// <summary>
        /// Creates a new session from the session factory.
        /// </summary>
        /// <returns></returns>
        public static ISession CreateSession()
        {
            return _sessionFactory.OpenSession();
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
            return _sessionFactory.GetAllClassMetadata();
        }

        /// <summary>
        /// Executes the create schema script against the database, optionally dropping the current schema first.
        /// </summary>
        public static void CreateSchema(bool dropFirst)
        {
            System.IO.TextWriter writer = Communicator.TextWriter ?? Console.Out;
            
            if (dropFirst)
                _schema.Drop(writer, true);

            _schema.Create(writer, true);
        }
        


    }
}
