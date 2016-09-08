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
    /// Contains all of the predefined account history types.
    /// </summary>
    public static class AccountHistoryTypes
    {
        static AccountHistoryTypes()
        {
            var types = typeof(AccountHistoryTypes).GetFields().Where(x => x.FieldType == typeof(AccountHistoryType)).Select(x => (AccountHistoryType)x.GetValue(null)).ToList();

            AllAccountHistoryTypes = new ConcurrentBag<AccountHistoryType>(types);
        }

        /// <summary>
        /// Contains all of the predefined account history types.
        /// </summary>
        public static ConcurrentBag<AccountHistoryType> AllAccountHistoryTypes;

        public static AccountHistoryType Creation = new AccountHistoryType { Id = Guid.Parse("{64C5B78D-D5FD-496A-9579-D54549A40257}"), Value = "Creation", Description = "Indicates an account creation event.  This should only occur once." };
        public static AccountHistoryType Login = new AccountHistoryType { Id = Guid.Parse("{C38606E8-9A1E-4653-A944-8010B00682C0}"), Value = "Login", Description = "A routine, successful login." };
        public static AccountHistoryType Logout = new AccountHistoryType { Id = Guid.Parse("{35D5F49F-9EBD-48BA-A526-26C5596EA508}"), Value = "Logout", Description = "A logout event." };
        public static AccountHistoryType FailedLogin = new AccountHistoryType { Id = Guid.Parse("{4AEDD7AC-DCF4-4AEE-98C1-C73FB422CF86}"), Value = "Failed Login", Description = "A failed attempt to login to the account. Many of these may indicate malicious activity." };
        public static AccountHistoryType RegistrationStarted = new AccountHistoryType { Id = Guid.Parse("{7580B641-2059-49FA-8E00-6D44CECE5034}"), Value = "Registration Started", Description = "The beginning of the registration process, when the user inputs the SSN and receives an email." };
        public static AccountHistoryType RegistrationCompleted = new AccountHistoryType { Id = Guid.Parse("{CD626AF1-BCFA-448B-992E-4D65A934571B}"), Value = "Registration Completed", Description = "The end of the registration process, after which the user should have account access." };
        public static AccountHistoryType PasswordResetInitiated = new AccountHistoryType { Id = Guid.Parse("{214B8471-5087-4C64-A91C-2FCC0232CE9F}"), Value = "Password Reset Initiated", Description = "The beginning of the password reset process, during which the user receives an email with instructions for password reset." };
        public static AccountHistoryType PasswordResetCompleted = new AccountHistoryType { Id = Guid.Parse("{609D71A8-5FB7-44A7-B5E8-242C3BE20079}"), Value = "Password Reset Completed", Description = "The end of the password reset process." };

        /// <summary>
        /// Ensures that all account history types are persisted in the database and that they look the same as they do here.
        /// </summary>
        /// <param name="options"></param>
        [ServiceManagement.StartMethod(Priority = 11)]
        private static void EnsureAccountHistoryTypePersistence(CLI.Options.LaunchOptions options)
        {
            Logging.Log.Info("Checking account history types...");

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                var currentTypes = session.QueryOver<AccountHistoryType>().List();

                var missingTypes = AllAccountHistoryTypes.Except(currentTypes).ToList();

                Logging.Log.Info("Persisting {0} missing account history type(s)...".FormatS(missingTypes.Count));
                foreach (var type in missingTypes)
                {
                    session.Save(type);
                }

                transaction.Commit();
            }
        }
    }
}
