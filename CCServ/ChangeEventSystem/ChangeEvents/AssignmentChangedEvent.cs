using CCServ.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;

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
            Task.Run(() =>
            {
                var args = state as Email.Models.AssignmentChangedEventEmailModel;

                if (args == null)
                    throw new ArgumentException("The state object was either of the wrong type or null.");

                if (args.NewAssignment == args.OldAssignment)
                    throw new Exception("The assignment changed event was raised; however, the two assignments appear to be the same.");

                //Ok, so we have an email we can use to contact the person!
                Email.EmailInterface.CCEmailMessage
                    .CreateDefault()
                    .To(GetValidSubscriptionEmailAddresses(client))
                    .Subject("{0} Event".FormatS(this.Name))
                    .HTMLAlternateViewUsingTemplateFromEmbedded("CCServ.Email.Templates.AssignmentChangedEvent_HTML.html", args)
                    .SendWithRetryAndFailure(TimeSpan.FromSeconds(1));
            }).ConfigureAwait(false);
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
