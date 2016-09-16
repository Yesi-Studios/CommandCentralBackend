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
    /// Contains all the muster statuses.
    /// </summary>
    public static class MusterStatuses
    {
        static MusterStatuses()
        {
            var statuses = typeof(MusterStatuses).GetFields().Where(x => x.FieldType == typeof(MusterStatus)).Select(x => (MusterStatus)x.GetValue(null)).ToList();

            AllMusterStatuses = new ConcurrentBag<MusterStatus>(statuses);
        }

        /// <summary>
        /// Contains all the muster statuses.
        /// </summary>
        public static ConcurrentBag<MusterStatus> AllMusterStatuses;

        public static MusterStatus Present = new MusterStatus { Id = Guid.Parse("{97742B09-0DB3-46D7-A77F-BDA83AFE0CA8}"), Value = "Present", Description = "" };
        public static MusterStatus TerminalLeave = new MusterStatus { Id = Guid.Parse("{0F599535-4158-474E-98D8-81A5F1AFDBE1}"), Value = "Terminal Leave", Description = "" };
        public static MusterStatus Leave = new MusterStatus { Id = Guid.Parse("{17451BAC-8E95-4C66-B83C-435B1073F1C7}"), Value = "Leave", Description = "" };
        public static MusterStatus SIQ = new MusterStatus { Id = Guid.Parse("{7F28A561-F961-498E-B9AD-32B1D78A820F}"), Value = "SIQ", Description = "" };
        public static MusterStatus UA = new MusterStatus { Id = Guid.Parse("{A8025EB5-BCCB-4045-9E59-F6F81A81FC31}"), Value = "UA", Description = "" };
        public static MusterStatus Transferred = new MusterStatus { Id = Guid.Parse("{F81F05DF-EDC5-4925-94D2-AA6B8E3276F8}"), Value = "Transferred", Description = "" };

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
