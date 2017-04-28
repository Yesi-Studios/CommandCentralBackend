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
    /// Contains all of the predefined watch qualifications.
    /// </summary>
    public static class WatchQualifications
    {
        static WatchQualifications()
        {
            var quals = typeof(WatchQualifications).GetFields().Where(x => x.FieldType == typeof(WatchQualification)).Select(x => (WatchQualification)x.GetValue(null)).ToList();

            AllWatchQualifications = new ConcurrentBag<WatchQualification>(quals);
        }

        /// <summary>
        /// Contains all of the predefined watch qualifications types.
        /// </summary>
        public static ConcurrentBag<WatchQualification> AllWatchQualifications;

        public static WatchQualification JOOD = new WatchQualification { Id = Guid.Parse("{726A2088-AE51-4E41-B10A-952F0CBD73C3}"), Value = "JOOD", Description = "" };
        public static WatchQualification OOD = new WatchQualification { Id = Guid.Parse("{91B60F40-C991-4DB6-A51F-3889989C2D33}"), Value = "OOD", Description = "" };
        public static WatchQualification CDO = new WatchQualification { Id = Guid.Parse("{0B8F03E9-11ED-40BF-8E0D-18C49F9C25E2}"), Value = "CDO", Description = "" };
        public static WatchQualification FHD = new WatchQualification { Id = Guid.Parse("{57BE1E02-DD6B-41D6-B73B-15B2DB01F468}"), Value = "FHD", Description = "" };
        public static WatchQualification Exempt = new WatchQualification { Id = Guid.Parse("{AC0DDBE2-E34E-468B-968E-8B4695ACF5A4}"), Value = "Exempt", Description = "" };

        /// <summary>
        /// Ensures that all watch qualifications are persisted in the database and that they look the same as they do here.
        /// </summary>
        /// <param name="options"></param>
        [ServiceManagement.StartMethod(Priority = 16)]
        private static void EnsureWatchQualificationPersistence(CLI.Options.LaunchOptions options)
        {
            Logging.Log.Info("Checking watch qualifications...");

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                var currentWatchQuals = session.QueryOver<WatchQualification>().List();

                var missingQuals = AllWatchQualifications.Except(currentWatchQuals).ToList();

                Logging.Log.Info("Persisting {0} missing watch qualification(s)...".FormatS(missingQuals.Count));
                foreach (var type in missingQuals)
                {
                    session.Save(type);
                }

                transaction.Commit();
            }
        }

    }
}
