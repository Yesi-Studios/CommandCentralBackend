using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.Entities.ReferenceLists.Watchbill
{
    /// <summary>
    /// Contains all the predefined watch elligibility groups.
    /// </summary>
    public static class WatchElligibilityGroups
    {

        static WatchElligibilityGroups()
        {
            var groups = typeof(WatchElligibilityGroups).GetFields().Where(x => x.FieldType == typeof(WatchElligibilityGroup)).Select(x => (WatchElligibilityGroup)x.GetValue(null)).ToList();

            AllWatchElligibilityGroups = new ConcurrentBag<WatchElligibilityGroup>(groups);
        }

        /// <summary>
        /// Contains all the predefined watch elligibility groups.
        /// </summary>
        public static ConcurrentBag<WatchElligibilityGroup> AllWatchElligibilityGroups;

        /// <summary>
        /// This watch shift is for JOODs.
        /// </summary>
        public static WatchElligibilityGroup Quarterdeck = new WatchElligibilityGroup
        {
            Id = Guid.Parse("{1F88A266-02BE-4E9B-ADBD-BE1D814B1CD8}"),
            Value = "Quarterdeck",
            Description = "This is the group of people who are elligible to stand the quarterdeck watch.",
            OwningChainOfCommand = Authorization.ChainsOfCommand.QuarterdeckWatchbill
        };

        /// <summary>
        /// Ensures that all of the watch elligibility groups are persisted in the database and they look the same as they do here.
        /// </summary>
        /// <param name="options"></param>
        [ServiceManagement.StartMethod(Priority = 11)]
        private static void EnsureWatchEllGroupPersistence(CLI.Options.LaunchOptions options)
        {
            Logging.Log.Info("Checking watch groups...");

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    var currentGroups = session.QueryOver<WatchElligibilityGroup>().List();

                    var missingGroups = AllWatchElligibilityGroups.Except(currentGroups).ToList();

                    if (missingGroups.Any())
                    {
                        foreach (var type in missingGroups)
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
