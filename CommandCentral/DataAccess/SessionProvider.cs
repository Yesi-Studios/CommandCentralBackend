using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate;
using NHibernate.Cfg;
using FluentNHibernate.Cfg;
using FluentNHibernate.Data;
using FluentNHibernate.Cfg.Db;

namespace CommandCentral.DataAccess
{
    /// <summary>
    /// Provides singleton managed access to NHibernate sessions.
    /// </summary>
    public static class NHibernateHelper
    {

        private static readonly ISessionFactory _sessionFactory;

        static NHibernateHelper()
        {
            try
            {
                Configuration configuration = Fluently.Configure().Database(
                MySQLConfiguration.Standard.ConnectionString(
                    builder => builder.Database("test_db")
                                        .Username("xanneth")
                                        .Password("douglas0678")
                                        .Server("localhost"))
                    .ShowSql())
                    .Cache(x => x.UseQueryCache()
                                    .ProviderClass<NHibernate.Caches.SysCache.SysCacheProvider>())
                    .Mappings(x => x.FluentMappings.AddFromAssemblyOf<Entities.Person>())
                    .BuildConfiguration();

                _sessionFactory = configuration.BuildSessionFactory();
            }
            catch
            {

                throw;
            }
        }

        public static NHibernate.ISession CreateSession()
        {
            return _sessionFactory.OpenSession();
        }

        public static void Release()
        {
            _sessionFactory.Close();
            _sessionFactory.Dispose();
        }

        public static NHibernate.Metadata.IClassMetadata GetEntityMetadata(string entityName)
        {
            return _sessionFactory.GetClassMetadata(entityName);
        }

        public static void CreateDatabaseSchema()
        {
        }


    }
}
