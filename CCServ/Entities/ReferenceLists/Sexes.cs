using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;

namespace CCServ.Entities.ReferenceLists
{
    /// <summary>
    /// Contains all the sexes.
    /// </summary>
    public static class Sexes
    {
        /*Male, Female*/

        static Sexes()
        {
            var types = typeof(Sexes).GetFields().Where(x => x.FieldType == typeof(Sex)).Select(x => (Sex)x.GetValue(null)).ToList();

            AllSexes = new ConcurrentBag<Sex>(types);
        }

        /// <summary>
        /// Contains all the sexes.
        /// </summary>
        public static ConcurrentBag<Sex> AllSexes;

        public static Sex Male = new Sex { Id = Guid.Parse("{649A70D4-CFE1-446D-B028-2E223CB60C11}"), Value = "Male", Description = "" };
        public static Sex Female = new Sex { Id = Guid.Parse("{8EF19709-1402-4401-B0E2-03F978E11251}"), Value = "Female", Description = "" };

        /// <summary>
        /// Ensures that all sexes are persisted in the database and that they look the same as they do here.
        /// </summary>
        /// <param name="options"></param>
        [ServiceManagement.StartMethod(Priority = 14)]
        private static void EnsureSexesPersistence(CLI.Options.LaunchOptions options)
        {
            Logging.Log.Info("Checking phone number types...");

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                var current = session.QueryOver<Sex>().List();

                var missing = AllSexes.Except(current).ToList();

                Logging.Log.Info("Persisting {0} missing sex(es)...".FormatS(missing.Count));
                foreach (var sex in missing)
                {
                    session.Save(sex);
                }

                transaction.Commit();
            }
        }
    }
}
