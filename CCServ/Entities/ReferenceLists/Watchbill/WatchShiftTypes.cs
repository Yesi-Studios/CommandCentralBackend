using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.Entities.ReferenceLists.Watchbill
{
    /// <summary>
    /// Contains all the predefined watch shift types.
    /// </summary>
    public static class WatchShiftTypes
    {

        static WatchShiftTypes()
        {
            var types = typeof(WatchShiftTypes).GetFields().Where(x => x.FieldType == typeof(WatchShiftType)).Select(x => (WatchShiftType)x.GetValue(null)).ToList();

            AllWatchShiftTypes = new ConcurrentBag<WatchShiftType>(types);
        }

        /// <summary>
        /// Contains all the predefined watch shift types.
        /// </summary>
        public static ConcurrentBag<WatchShiftType> AllWatchShiftTypes;

        /// <summary>
        /// This watch shift is for JOODs.
        /// </summary>
        public static WatchShiftType JOOD = new WatchShiftType
        {
            Id = Guid.Parse("{858F6B78-5508-4A59-97A0-6E45AADAB2A9}"),
            Value = "JOOD",
            Description = "This watch shift is for JOODs."
        };

        /// <summary>
        /// This watch shift is for OODs.
        /// </summary>
        public static WatchShiftType OOD = new WatchShiftType
        {
            Id = Guid.Parse("{F3F818F8-55D3-45DC-AF2F-1528ED290384}"),
            Value = "OOD",
            Description = "This watch shift is for OODs."
        };

        /// <summary>
        /// This watch shift is for CDOs.
        /// </summary>
        public static WatchShiftType CDO = new WatchShiftType
        {
            Id = Guid.Parse("{69F31AF9-F77B-4744-B23D-297625F8FC0C}"),
            Value = "CDO",
            Description = "This watch shift is for CDOs."
        };

        /// <summary>
        /// This is the super watch shift.
        /// </summary>
        public static WatchShiftType Super = new WatchShiftType
        {
            Id = Guid.Parse("{1095A185-7CC4-4AE5-94F8-BB0503CDE008}"),
            Value = "Super",
            Description = "This is the super watch shift."
        };

        /// <summary>
        /// Ensures that all of the watch shift types are persisted in the database and they look the same as they do here.
        /// </summary>
        /// <param name="options"></param>
        [ServiceManagement.StartMethod(Priority = 11)]
        private static void EnsureWatchShiftTypesPersistence(CLI.Options.LaunchOptions options)
        {
            Logging.Log.Info("Checking watch shift types...");

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    var currentTypes = session.QueryOver<WatchShiftType>().List();

                    var missingTypes = AllWatchShiftTypes.Except(currentTypes).ToList();

                    if (missingTypes.Any())
                    {
                        foreach (var type in missingTypes)
                        {
                            session.Save(type);
                        }
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }

        }

    }
}
