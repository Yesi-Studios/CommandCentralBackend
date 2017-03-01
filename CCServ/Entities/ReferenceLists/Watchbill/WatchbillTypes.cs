using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.Entities.ReferenceLists.Watchbill
{

    /// <summary>
    /// Contains all the predefined watchbill types.
    /// </summary>
    public static class WatchbillTypes
    {

        static WatchbillTypes()
        {
            var types = typeof(WatchbillTypes).GetFields().Where(x => x.FieldType == typeof(WatchbillType)).Select(x => (WatchbillType)x.GetValue(null)).ToList();

            AllWatchbillTypes = new ConcurrentBag<WatchbillType>(types);
        }

        /// <summary>
        /// Contains all the predefined watch bill types.
        /// </summary>
        public static ConcurrentBag<WatchbillType> AllWatchbillTypes;

        /// <summary>
        /// This is the quarterdeck watchbill type.
        /// </summary>
        public static WatchbillType Quarterdeck = new WatchbillType
        {
            Id = Guid.Parse("{0C6F353F-A8C9-4204-B6F2-568230E2642B}"),
            Value = "Quarterdeck",
            Description = "This watchbill is used to determine the watch on the Quarterdeck."
        };

        /// <summary>
        /// Ensures that all of the watch bill types are persisted in the database and they look the same as they do here.
        /// </summary>
        /// <param name="options"></param>
        [ServiceManagement.StartMethod(Priority = 11)]
        private static void EnsureWatchbillTypesPersistence(CLI.Options.LaunchOptions options)
        {
            Logging.Log.Info("Checking watch bill types...");

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    var currentTypes = session.QueryOver<WatchbillType>().List();

                    var missingTypes = AllWatchbillTypes.Except(currentTypes).ToList();

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
