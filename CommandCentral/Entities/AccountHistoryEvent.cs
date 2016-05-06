using System;
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
        /// A logout event that results in the invalidation of a session.
        /// </summary>
        Logout,
        /// <summary>
        /// A failed login event.
        /// </summary>
        FailedLogin,
        /// <summary>
        /// The password of an account was reset.
        /// </summary>
        PasswordReset,
        /// <summary>
        /// The registration process was successfully begun.
        /// </summary>
        RegistrationStarted,
        /// <summary>
        /// The registration process was completed.
        /// </summary>
        RegistrationCompleted,
        /// <summary>
        /// The password reset process was started.
        /// </summary>
        PasswordResetInitiated,
        /// <summary>
        /// The password of a person account was reset.
        /// </summary>
        PasswordResetCompleted
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
        public virtual Guid Id { get; set; }

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

                Id(x => x.Id).GeneratedBy.Guid();

                Map(x => x.EventTime).Not.Nullable();
                Map(x => x.AccountHistoryEventType).Not.Nullable().Length(50);

                References(x => x.Person).Not.Nullable();
            }
        }
    }
}
