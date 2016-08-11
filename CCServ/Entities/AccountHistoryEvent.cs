using System;
using FluentNHibernate.Mapping;
using AtwoodUtils;

namespace CCServ.Entities
{
    

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
        /// The type of history event that occurred.
        /// </summary>
        public virtual AccountHistoryEventTypes AccountHistoryEventType { get; set; }

        /// <summary>
        /// The time at which this event occurred.
        /// </summary>
        public virtual DateTime EventTime { get; set; }

        #endregion

        #region Overrides

        /// <summary>
        /// Returns the EventType @ Time
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "{0} @ {1}".FormatS(this.AccountHistoryEventType, this.EventTime);
        }

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
                Id(x => x.Id).GeneratedBy.Guid();

                Map(x => x.EventTime).Not.Nullable();
                Map(x => x.AccountHistoryEventType).Not.Nullable().Length(50);
            }
        }
    }
}
