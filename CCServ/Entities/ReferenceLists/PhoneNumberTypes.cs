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
    /// Contains all the phone number types.
    /// </summary>
    public static class PhoneNumberTypes
    {
        static PhoneNumberTypes()
        {
            var types = typeof(PhoneNumberTypes).GetFields().Where(x => x.FieldType == typeof(PhoneNumberType)).Select(x => (PhoneNumberType)x.GetValue(null)).ToList();

            AllPhoneNumberTypes = new ConcurrentBag<PhoneNumberType>(types);
        }

        /// <summary>
        /// Contains all the phone number types.
        /// </summary>
        public static ConcurrentBag<PhoneNumberType> AllPhoneNumberTypes;

        public static PhoneNumberType Mobile = new PhoneNumberType { Id = Guid.Parse("{2DC87F0E-6862-40E8-B56C-EEE0BE4396C2}"), Value = "Mobile", Description = "" };
        public static PhoneNumberType Home = new PhoneNumberType { Id = Guid.Parse("{915BA277-C1D2-439D-AFA7-E02FC230EEEE}"), Value = "Home", Description = "" };
        public static PhoneNumberType Work = new PhoneNumberType { Id = Guid.Parse("{E95BB8C6-1A1D-4CEC-970C-0D5A7E5A04FF}"), Value = "Work", Description = "" };

        /// <summary>
        /// Ensures that all phone number types are persisted in the database and that they look the same as they do here.
        /// </summary>
        /// <param name="options"></param>
        [ServiceManagement.StartMethod(Priority = 13)]
        private static void EnsurePhoneNumberTypesPersistence(CLI.Options.LaunchOptions options)
        {
            Logging.Log.Info("Checking phone number types...");

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                var current = session.QueryOver<PhoneNumberType>().List();

                var missing = AllPhoneNumberTypes.Except(current).ToList();

                Logging.Log.Info("Persisting {0} missing phone number types(s)...".FormatS(missing.Count));
                foreach (var type in missing)
                {
                    session.Save(type);
                }

                transaction.Commit();
            }
        }
    }
}
