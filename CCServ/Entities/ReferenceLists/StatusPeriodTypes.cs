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
    /// Contains all the status period types.
    /// </summary>
    public static class StatusPeriodTypes
    {
        static StatusPeriodTypes()
        {
            var types = typeof(StatusPeriodTypes).GetFields().Where(x => x.FieldType == typeof(StatusPeriodType)).Select(x => (StatusPeriodType)x.GetValue(null)).ToList();

            AllStatusPeriodTypes = new ConcurrentBag<StatusPeriodType>(types);
        }

        /// <summary>
        /// Contains all the status period types.
        /// </summary>
        public static ConcurrentBag<StatusPeriodType> AllStatusPeriodTypes;

        public static StatusPeriodType Present = new StatusPeriodType { Id = Guid.Parse("{97742B09-0DB3-46D7-A77F-BDA83AFE0CA8}"), Value = "Present", Description = "" };
        public static StatusPeriodType TerminalLeave = new StatusPeriodType { Id = Guid.Parse("{0F599535-4158-474E-98D8-81A5F1AFDBE1}"), Value = "Terminal Leave", Description = "" };
        public static StatusPeriodType Leave = new StatusPeriodType { Id = Guid.Parse("{17451BAC-8E95-4C66-B83C-435B1073F1C7}"), Value = "Leave", Description = "" };
        public static StatusPeriodType SIQ = new StatusPeriodType { Id = Guid.Parse("{7F28A561-F961-498E-B9AD-32B1D78A820F}"), Value = "SIQ", Description = "" };
        public static StatusPeriodType UA = new StatusPeriodType { Id = Guid.Parse("{A8025EB5-BCCB-4045-9E59-F6F81A81FC31}"), Value = "UA", Description = "" };
        public static StatusPeriodType TAD = new StatusPeriodType { Id = Guid.Parse("{3F4E9C19-C578-4C8D-A5A3-3A63F5BE4281}"), Value = "TAD", Description = "" };
        public static StatusPeriodType AA = new StatusPeriodType { Id = Guid.Parse("{670e888f-14b5-42aa-ba4b-0b947bb41003}"), Value = "AA", Description = "" };
        public static StatusPeriodType Deployed = new StatusPeriodType { Id = Guid.Parse("{012dbd01-e7c9-4d72-b4e0-cd8e1acc0858}"), Value = "Deployed", Description = "" };

        /// <summary>
        /// Ensures that all status period types are persisted in the database and that they look the same as they do here.
        /// </summary>
        /// <param name="options"></param>
        [ServiceManagement.StartMethod(Priority = 10)]
        private static void EnsureMusterStatusesPersistence(CLI.Options.LaunchOptions options)
        {
            Logging.Log.Info("Checking status period types...");

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                var current = session.QueryOver<StatusPeriodType>().List();

                var missing = AllStatusPeriodTypes.Except(current).ToList();

                Logging.Log.Info("Persisting {0} missing status periods type(s)...".FormatS(missing.Count));
                foreach (var status in missing)
                {
                    session.Save(status);
                }

                transaction.Commit();
            }
        }
    }
}
