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
    public static class SessionProvider
    {

        private static readonly ISessionFactory _sessionFactory;

        static SessionProvider()
        {
            Configuration configuration = Fluently.Configure().Database(
                MySQLConfiguration.Standard.ConnectionString(
                    builder => builder.Database("test_tdb")
                                      .Username("niocga")
                                      .Password("niocga")
                                      .Server("147.51.62.100"))
                    .ShowSql())
                    .Cache(x => x.UseQueryCache()
                                 .ProviderClass<NHibernate.Caches.SysCache.SysCacheProvider>())
                    .Mappings(x => x.FluentMappings.AddFromAssemblyOf<CommandCentral.Entities.Person>())
                    .BuildConfiguration();

            _sessionFactory = configuration.BuildSessionFactory();                    
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


    }
}
