using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;

namespace CommandCentral.Entities
{
    /// <summary>
    /// Describes the different account history types.
    /// </summary>
    public enum AccountHistoryEventTypes
    {
        /// <summary>
        /// A login event.
        /// </summary>
        Login,
        /// <summary>
        /// A failed login event.
        /// </summary>
        Failed_Login,
        /// <summary>
        /// The password of an account was reset.
        /// </summary>
        Password_Reset,
        /// <summary>
        /// The registration process was successfully begun.
        /// </summary>
        Registration_Started,
        /// <summary>
        /// The registration process was completed.
        /// </summary>
        Registration_Completed,
        /// <summary>
        /// The password reset process was started.
        /// </summary>
        Password_Reset_Initiated,
        /// <summary>
        /// The password of a person account was reset.
        /// </summary>
        Password_Reset_Completed
    }

    /// <summary>
    /// Describes a single account history event.
    /// </summary>
    public class AccountHistoryEvent
    {

        #region Properties

        /// <summary>
        /// The unique GUID of this account history event.
        /// </summary>
        public virtual string ID { get; set; }

        /// <summary>
        /// The person on whose account this event occurred.
        /// </summary>
        public virtual Person Person { get; set; }

        /// <summary>
        /// The type of history event that occurred.
        /// </summary>
        public virtual AccountHistoryEventTypes AccountHistoryEventType { get; set; }

        /// <summary>
        /// The time at which this event occurred.
        /// </summary>
        public virtual DateTime EventTime { get; set; }

        #endregion

        /// <summary>
        /// Maps an account history event to the database.
        /// </summary>
        public class AccountHistoryEventMapping : ClassMap<AccountHistoryEvent>
        {
            /// <summary>
            /// Maps an account history event to the database.
            /// </summary>
            public AccountHistoryEventMapping()
            {
                Table("account_history_events");

                Id(x => x.ID).GeneratedBy.Guid();

                Map(x => x.EventTime).Not.Nullable();
                Map(x => x.AccountHistoryEventType).Not.Nullable().Length(20);

                References(x => x.Person).Not.Nullable();
            }
        }
    }
}
