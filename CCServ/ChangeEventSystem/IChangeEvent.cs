using CCServ.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;
using NHibernate.Criterion;
using NHibernate.Linq;

namespace CCServ.ChangeEventSystem
{
    /// <summary>
    /// Defines the basic members of a change event.
    /// </summary>
    public interface IChangeEvent
    {

        #region Properties

        /// <summary>
        /// The unique Id of this change event.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// The name of this change event.
        /// </summary>
        string EventName { get; }

        /// <summary>
        /// A short description of this change event.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Indicates that this event will only be sent to members of the chain of command.
        /// </summary>
        bool RestrictToChainOfCommand { get; }

        /// <summary>
        /// The person/client who raised this event.  If this is null, then the system should be assumed to have raised the event.
        /// </summary>
        Person EventRaisedBy { get; }

        #endregion

        #region Methods

        void SendEmail();

        #endregion

    }
}
