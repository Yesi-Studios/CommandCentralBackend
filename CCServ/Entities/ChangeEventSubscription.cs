using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using AtwoodUtils;

namespace CCServ.Entities
{
    /// <summary>
    /// Persists a person's subscription to a particular change event.
    /// </summary>
    public class ChangeEventSubscription
    {

        #region Properties

        /// <summary>
        /// The Id.
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The name of a change event.  Change events are declared in code.
        /// </summary>
        public virtual Guid ChangeEventId { get; set; }

        /// <summary>
        /// The person who subscribed to the event.
        /// </summary>
        public virtual Person Subscriber { get; set; }

        /// <summary>
        /// The level at which the subscriber would like to be alerted when this event fires on a profile.
        /// </summary>
        public virtual ChainOfCommandLevels ChainOfCommandLevel { get; set; }

        #endregion

        #region Overrides

        /// <summary>
        /// Returns the name of this 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var changeEvent = ChangeEventSystem.ChangeEventHelper.AllChangeEvents.FirstOrDefault(x => x.Id == ChangeEventId);

            var eventName = changeEvent == null ? "UNKNOWN EVENT" : changeEvent.EventName;

            return "{0} at {1} level".FormatS(eventName, this.ChainOfCommandLevel);
        }

        #endregion

        /// <summary>
        /// Maps this subscription to the database.
        /// </summary>
        public class ChangeEventSubscriptionMapping : ClassMap<ChangeEventSubscription>
        {
            /// <summary>
            /// Maps this subscription to the database.
            /// </summary>
            public ChangeEventSubscriptionMapping()
            {
                Id(x => x.Id).GeneratedBy.Guid();

                Map(x => x.ChangeEventId).Not.Nullable();
                Map(x => x.ChainOfCommandLevel).Not.Nullable();

                References(x => x.Subscriber).Not.Nullable();
            }
        }
    }
}
