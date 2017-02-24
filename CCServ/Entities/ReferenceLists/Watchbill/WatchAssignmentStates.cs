using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.Entities.ReferenceLists.Watchbill
{
    /// <summary>
    /// Contains all the predefined watch assignment states.
    /// </summary>
    public static class WatchAssignmentStates
    {
        static WatchAssignmentStates()
        {
            var states = typeof(WatchAssignmentStates).GetFields().Where(x => x.FieldType == typeof(WatchAssignmentState)).Select(x => (WatchAssignmentState)x.GetValue(null)).ToList();

            AllWatchAssignmentStates = new ConcurrentBag<WatchAssignmentState>(states);
        }

        /// <summary>
        /// Contains all the predefined watch assignment states.
        /// </summary>
        public static ConcurrentBag<WatchAssignmentState> AllWatchAssignmentStates;

        /// <summary>
        /// The watch has been assigned, but the assigned person has not acknowledged the watch.
        /// </summary>
        public static WatchAssignmentState Assigned = new WatchAssignmentState
        {
            Id = Guid.Parse("{2BB06DD9-0442-489C-B741-8A0AEE2DDEC6}"),
            Value = "Assigned",
            Description = "The watch has been assigned, but the assigned person has not acknowledged the watch."
        };

        /// <summary>
        /// The assigned person, or a member of his or her chain of command, has acknowledged this watch assignment.
        /// </summary>
        public static WatchAssignmentState Acknowledged = new WatchAssignmentState
        {
            Id = Guid.Parse("{9BC40FB1-D860-4CAF-8E5F-182D8394D764}"),
            Value = "Acknowledged",
            Description = "The assigned person, or a member of his or her chain of command, has acknowledged this watch assignment."
        };

        /// <summary>
        /// This watch is no longer valid and is retained only for historical purposes.  Another watch assignment now supercedes this one.
        /// </summary>
        public static WatchAssignmentState Superceded = new WatchAssignmentState
        {
            Id = Guid.Parse("{6EC2A8D0-CE54-4681-9285-53AF2C795E12}"),
            Value = "Superceded",
            Description = "This watch is no longer valid and is retained only for historical purposes.  Another watch assignment now supercedes this one."
        };

        /// <summary>
        /// The assigned person stood the watch in question.
        /// </summary>
        public static WatchAssignmentState Completed = new WatchAssignmentState
        {
            Id = Guid.Parse("{9783A684-AEE7-42A4-9F37-F9450A3F7F16}"),
            Value = "Completed",
            Description = "The assigned person stood the watch in question."
        };

        /// <summary>
        /// The assigned person failed to stand his or her watch.
        /// </summary>
        public static WatchAssignmentState Missed = new WatchAssignmentState
        {
            Id = Guid.Parse("{28F03326-7A8F-4893-AE24-605628DBD71E}"),
            Value = "Missed",
            Description = "The assigned person failed to stand his or her watch."
        };

        /// <summary>
        /// The assigned person failed to stand his or her watch; however, the absence was excued.
        /// </summary>
        public static WatchAssignmentState Excused = new WatchAssignmentState
        {
            Id = Guid.Parse("{DEF49073-3FA7-489C-BBE9-5024295C4794}"),
            Value = "Excused",
            Description = "The assigned person failed to stand his or her watch; however, the absence was excued."
        };

        /// <summary>
        /// Ensures that all of the watch assignment states are persisted in the database and they look the same as they do here.
        /// </summary>
        /// <param name="options"></param>
        [ServiceManagement.StartMethod(Priority = 11)]
        private static void EnsureWatchAssignmentStatesPersistence(CLI.Options.LaunchOptions options)
        {
            Logging.Log.Info("Checking watch assignment states...");

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    var currentStates = session.QueryOver<WatchAssignmentState>().List();

                    var missingStates = AllWatchAssignmentStates.Except(currentStates).ToList();

                    if (missingStates.Any())
                    {
                        foreach (var state in missingStates)
                        {
                            session.Save(state);
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
