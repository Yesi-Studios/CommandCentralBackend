using System;
using FluentNHibernate.Mapping;
using AtwoodUtils;
using CommandCentral.Entities.ReferenceLists;
using NHibernate.Type;

namespace CommandCentral.Entities
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
            return "{0} @ {1}".With(this.AccountHistoryEventType, this.EventTime);
        }

        /// <summary>
        /// Compares deep equality.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var other = obj as AccountHistoryEvent;
            if (other == null)
                return false;

            return Object.Equals(other.Id, this.Id) &&
                   Object.Equals(other.AccountHistoryEventType, this.AccountHistoryEventType) &&
                   Object.Equals(other.EventTime, this.EventTime);
        }

        /// <summary>
        /// Gets the hash code.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;

                hash = hash * 23 + Id.GetHashCode();
                hash = hash * 23 + Utilities.GetSafeHashCode(AccountHistoryEventType);
                hash = hash * 23 + Utilities.GetSafeHashCode(EventTime);

                return hash;
            }
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

                Map(x => x.EventTime).Not.Nullable().CustomType<UtcDateTimeType>();
                
                References(x => x.AccountHistoryEventType).LazyLoad(Laziness.False);
            }
        }
    }
}
