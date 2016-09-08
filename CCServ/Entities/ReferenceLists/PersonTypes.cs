using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;

namespace CCServ.Entities.ReferenceLists
{
    public static class PersonTypes
    {
        /*Civilian, Military*/

        static PersonTypes()
        {
            var types = typeof(PersonTypes).GetFields().Where(x => x.FieldType == typeof(PersonType)).Select(x => (PersonType)x.GetValue(null)).ToList();

            AllPersonTypes = new ConcurrentBag<PersonType>(types);
        }

        /// <summary>
        /// Contains all the muster statuses.
        /// </summary>
        public static ConcurrentBag<PersonType> AllPersonTypes;

        public static PersonType Civilian = new PersonType { Id = Guid.Parse("{681624A2-10FD-4124-8D7D-BB854EA9C5B5}"), Value = "Civilian", Description = "" };
        public static PersonType Military = new PersonType { Id = Guid.Parse("{7D8B76CB-ACBD-4269-86D2-751DECAA106E}"), Value = "Military", Description = "" };

        /// <summary>
        /// Ensures that all person types are persisted in the database and that they look the same as they do here.
        /// </summary>
        /// <param name="options"></param>
        [ServiceManagement.StartMethod(Priority = 12)]
        private static void EnsureMusterStatusesPersistence(CLI.Options.LaunchOptions options)
        {
            Logging.Log.Info("Checking person types...");

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                var current = session.QueryOver<PersonType>().List();

                var missing = AllPersonTypes.Except(current).ToList();

                Logging.Log.Info("Persisting {0} missing person types(s)...".FormatS(missing.Count));
                foreach (var type in missing)
                {
                    session.Save(type);
                }

                transaction.Commit();
            }
        }
    }
}
