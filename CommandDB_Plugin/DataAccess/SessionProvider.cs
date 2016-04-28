using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.DataAccess
{
    public static class SessionProvider
    {

        private static NHibernate.ISessionFactory factory = new NHibernate.Cfg.Configuration().Configure().BuildSessionFactory();

        public static NHibernate.ISession CreateSession()
        {
            return factory.OpenSession();
        }

        public static void Release()
        {
            factory.Close();
            factory.Dispose();
        }

        public static NHibernate.Metadata.IClassMetadata GetEntityMetadata(string entityName)
        {
            return factory.GetClassMetadata(entityName);
        }


    }
}
