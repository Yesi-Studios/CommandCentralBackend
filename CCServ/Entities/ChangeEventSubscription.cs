using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;

namespace CCServ.Entities
{
    /// <summary>
    /// Persists a person's subscription to a particular change event.
    /// </summary>
    public class ChangeEventSubscription
    {
        /// <summary>
        /// The Id.
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The name of a change event.  Change events are declared in code.
        /// </summary>
        public virtual string ChangeEventName { get; set; }

        /// <summary>
        /// The person who subscribed to the event.
        /// </summary>
        public virtual Person Subscriber { get; set; }

        /// <summary>
        /// The level at which the subscriber would like to be alerted when this event fires on a profile.
        /// </summary>
        public virtual ChainOfCommandLevels ChainOfCommandLevel { get; set; }

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

                Map(x => x.ChangeEventName).Not.Nullable();
                Map(x => x.ChainOfCommandLevel).Not.Nullable();

                References(x => x.Subscriber).Not.Nullable();
            }
        }

    }
}
