using System;
using FluentNHibernate.Mapping;
using AtwoodUtils;
using CCServ.Entities.ReferenceLists;

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
        public virtual AccountHistoryType AccountHistoryEventType { get; set; }

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

        #region ctors

        public AccountHistoryEvent()
        {
            if (Id == default(Guid))
                Id = Guid.NewGuid();
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
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.EventTime).Not.Nullable();
                References(x => x.AccountHistoryEventType).LazyLoad(Laziness.False);
            }
        }
    }
}
