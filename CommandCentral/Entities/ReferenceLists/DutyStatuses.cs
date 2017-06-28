using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;

namespace CommandCentral.Entities.ReferenceLists
{
    /// <summary>
    /// Contains all the duty statuses.
    /// </summary>
    public static class DutyStatuses
    {
        static DutyStatuses()
        {
            var dutyStatuses = typeof(DutyStatuses).GetFields().Where(x => x.FieldType == typeof(DutyStatus)).Select(x => (DutyStatus)x.GetValue(null)).ToList();

            AllDutyStatuses = new ConcurrentBag<DutyStatus>(dutyStatuses);
        }

        /// <summary>
        /// Contains all the duty statuses.
        /// </summary>
        public static ConcurrentBag<DutyStatus> AllDutyStatuses;

        public static DutyStatus Active = new DutyStatus { Id = Guid.Parse("C06D1123-60D9-4F06-B4DE-DD15EAE724A2"), Value = "Active", Description = "Indicates that a person is a member of the Active Duty component of the US armed forces." };
        public static DutyStatus Reserves = new DutyStatus { Id = Guid.Parse("{CE393F64-6ADB-4CA8-9E76-1691CD473CFC}"), Value = "Reserves", Description = "Indicates that a person is a member of the Reserve component of the US armed forces." };
        public static DutyStatus Contractor = new DutyStatus { Id = Guid.Parse("{412B00F7-5B33-49D2-B6C4-7F9F2DE4B0CE}"), Value = "Contractor", Description = "Indicates that a person is a contractor." };
        public static DutyStatus Civilian = new DutyStatus { Id = Guid.Parse("{87B778F8-6373-4A00-902C-10A8AC1207C5}"), Value = "Civilian", Description = "Indicates that a person is a civilian." };
        public static DutyStatus Loss = new DutyStatus { Id = Guid.Parse("{63E1584E-47AF-44C2-BEEC-7814C2356489}"), Value = "Loss", Description = "Indicates a person has left the scope of the application either by leaving the Navy or moving to an unsupported command." };
        public static DutyStatus SecondParty = new DutyStatus { Id = Guid.Parse("{B3C550A0-E824-4B99-BA9B-0FDEA971D297}"), Value = "Second Party", Description = "Any second party members of the command." };
        public static DutyStatus TADToCommand = new DutyStatus { Id = Guid.Parse("{64ED0490-04D9-443A-BAE0-FD714623233A}"), Value = "TAD To Command", Description = "Indicates the member is TAD to the given command." };

        /// <summary>
        /// Ensures that all duty statuses are persisted in the database and that they look the same as they do here.
        /// </summary>
        /// <param name="options"></param>
        [ServiceManagement.StartMethod(Priority = 15)]
        private static void EnsureDutyStatusesPersistence(CLI.Options.LaunchOptions options)
        {
            Logging.Log.Info("Checking duty statuses...");

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                var currentDutyStatuses = session.QueryOver<DutyStatus>().List();

                var missingDutyStatuses = AllDutyStatuses.Except(currentDutyStatuses).ToList();

                Logging.Log.Info("Persisting {0} missing duty statuses(s)...".FormatS(missingDutyStatuses.Count));
                foreach (var dutyStatus in missingDutyStatuses)
                {
                    session.Save(dutyStatus);
                }

                transaction.Commit();
            }
        }
    }
}
