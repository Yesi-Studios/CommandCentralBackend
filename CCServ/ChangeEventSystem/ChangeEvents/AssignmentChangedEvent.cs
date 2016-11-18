using CCServ.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.ChangeEventSystem.ChangeEvents
{
    /// <summary>
    /// The event that should be raised when a person's division, department and command changes.
    /// </summary>
    public class AssignmentChangedEvent : ChangeEventBase
    {
        /// <summary>
        /// Raises the event.
        /// </summary>
        /// <param name="eventArgs"></param>
        public override void RaiseEvent(object state, Person client)
        {
            var args = state as Email.Models.AssignmentChangedEventEmailModel;

            if (args == null)
                throw new ArgumentException("The state object was either of the wrong type or null.");
        }

        /// <summary>
        /// Set up name and description.
        /// </summary>
        public AssignmentChangedEvent()
        {
            Name = "Assignement Changed";
            Description = "Occurs when a person's assigned division, department, or command changes.";
        }
    }
}
