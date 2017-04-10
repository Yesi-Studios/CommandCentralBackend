using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.Entities.ReferenceLists.Watchbill
{
    /// <summary>
    /// Contains all the predefined watchbill statuses.
    /// </summary>
    public static class WatchbillStatuses
    {
        static WatchbillStatuses()
        {
            var statuses = typeof(WatchbillStatuses).GetFields().Where(x => x.FieldType == typeof(WatchbillStatus)).Select(x => (WatchbillStatus)x.GetValue(null)).ToList();

            AllWatchbillStatuses = new ConcurrentBag<WatchbillStatus>(statuses);
        }

        /// <summary>
        /// Contains all the predefined watchbill statuses.
        /// </summary>
        public static ConcurrentBag<WatchbillStatus> AllWatchbillStatuses;

        /// <summary>
        /// The watchbill has just been created and its days/shifts are being defined.  It is not open for inputs.
        /// </summary>
        public static WatchbillStatus Initial = new WatchbillStatus { Id = Guid.Parse("{FA1D4185-6A36-40DE-81C6-843E6EE352F0}"), Value = "Initial",
            Description = "The watchbill has just been created and its days/shifts are being defined.  It is not open for inputs." };

        /// <summary>
        /// The watchbill is now accepting watch inputs from all Sailors or their chains of command.
        /// </summary>
        public static WatchbillStatus OpenForInputs = new WatchbillStatus { Id = Guid.Parse("{0AD04630-679A-4275-A51B-6F7663BB103E}"), Value = "Open for Inputs",
            Description = "The watchbill is now accepting watch inputs from all Sailors or their chains of command." };

        /// <summary>
        /// The watchbill is no longer accepting watch inputs.  Soon, the it will be populated and released to the watchbill cooridinators for review.
        /// </summary>
        public static WatchbillStatus ClosedForInputs = new WatchbillStatus { Id = Guid.Parse("{34092469-541A-40DF-9729-A74396362131}"), Value = "Closed for Inputs",
            Description = "The watchbill is no longer accepting watch inputs.  Soon, the it will be populated and released to the watchbill cooridinators for review." };

        /// <summary>
        /// The watchbill has been populated and is currently under review.  Any last minute watch swaps should happen now.
        /// </summary>
        public static WatchbillStatus UnderReview = new WatchbillStatus { Id = Guid.Parse("{579F6625-3966-446A-85FC-CC6335407D38}"), Value = "Under Review",
            Description = "The watchbill has been populated and is currently under review.  Any last minute watch swaps should happen now." };

        /// <summary>
        /// The watchbill has been published to the command.  Changes to it will now require the command watchbill coordinator's intervention.
        /// </summary>
        public static WatchbillStatus Published = new WatchbillStatus { Id = Guid.Parse("{17D7339B-21EC-429E-B4C0-E34E36691521}"), Value = "Published",
            Description = "The watchbill has been published to the command.  Changes to it will now require the command watchbill coordinator's intervention." };

        /// <summary>
        /// Ensures that all of the watchbill statuses are persisted in the database and they look the same as they do here.
        /// </summary>
        /// <param name="options"></param>
        [ServiceManagement.StartMethod(Priority = 11)]
        private static void EnsureWatchbillStatusesPersistence(CLI.Options.LaunchOptions options)
        {
            Logging.Log.Info("Checking watchbill statuses...");

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    var currentStatuses = session.QueryOver<WatchbillStatus>().List();

                    var missingStatuses = AllWatchbillStatuses.Except(currentStatuses).ToList();

                    if (missingStatuses.Any())
                    {
                        foreach (var status in missingStatuses)
                        {
                            session.Save(status);
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
