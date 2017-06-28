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
    /// Contains all the predefined billet assignments.
    /// </summary>
    public static class BilletAssignments
    {
        static BilletAssignments()
        {
            var assignments = typeof(BilletAssignments).GetFields().Where(x => x.FieldType == typeof(BilletAssignment)).Select(x => (BilletAssignment)x.GetValue(null)).ToList();

            AllBilletAssignments = new ConcurrentBag<BilletAssignment>(assignments);
        }

        /// <summary>
        /// Contains all the predefined billet assignments.
        /// </summary>
        public static ConcurrentBag<BilletAssignment> AllBilletAssignments;

        public static BilletAssignment P2 = new BilletAssignment { Id = Guid.Parse("{5DB723D6-0C5A-4E7F-A5FD-0C14CDA31F94}"), Value = "P2", Description = "Indicates a person is assigned to a P2 billet." };
        public static BilletAssignment P3 = new BilletAssignment { Id = Guid.Parse("{2249A988-6D11-4786-8116-ED7C1651C0E4}"), Value = "P3", Description = "Indicates a person is assigned to a P3 billet." };

        /// <summary>
        /// Ensures that all bilelt assignments are persisted.
        /// </summary>
        /// <param name="options"></param>
        [ServiceManagement.StartMethod(Priority = 11)]
        private static void EnsureBilletASsignmentPersistence(CLI.Options.LaunchOptions options)
        {
            Logging.Log.Info("Checking billet assignments...");

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                var currentAssignments = session.QueryOver<BilletAssignment>().List();

                var missingAssignments = AllBilletAssignments.Except(currentAssignments).ToList();

                Logging.Log.Info("Persisting {0} missing billet assignment(s)...".FormatS(missingAssignments.Count));
                foreach (var type in missingAssignments)
                {
                    session.Save(type);
                }

                transaction.Commit();
            }
        }
    }
}
