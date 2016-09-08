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
        public static PersonType Civilian = new PersonType { Id = Guid.Parse("{681624A2-10FD-4124-8D7D-BB854EA9C5B5}"), Value = "Civilian", Description = "" };

        /// <summary>
        /// Ensures that all muster statuses are persisted in the database and that they look the same as they do here.
        /// </summary>
        /// <param name="options"></param>
        [ServiceManagement.StartMethod(Priority = 10)]
        private static void EnsureMusterStatusesPersistence(CLI.Options.LaunchOptions options)
        {
            Logging.Log.Info("Checking muster statuses...");

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                var current = session.QueryOver<MusterStatus>().List();

                var missing = AllMusterStatuses.Except(current).ToList();

                Logging.Log.Info("Persisting {0} missing muster statuses(s)...".FormatS(missing.Count));
                foreach (var status in missing)
                {
                    session.Save(status);
                }

                transaction.Commit();
            }
        }
    }
}
