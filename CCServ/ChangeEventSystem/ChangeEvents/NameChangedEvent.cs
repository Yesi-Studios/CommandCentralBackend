using CCServ.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.ChangeEventSystem.ChangeEvents
{
    /// <summary>
    /// The name changed event, meant to be raised if a person's name changes.
    /// </summary>
    public class NameChangedEvent : ChangeEventBase
    {
        /// <summary>
        /// Raises the name changed event.
        /// </summary>
        /// <param name="eventArgs"></param>
        public override void RaiseEvent(object state, Person client)
        {
            Task.Run(() =>
                {
                    var args = state as Email.Models.NameChangedEventEmailModel;

                    if (args == null)
                        throw new ArgumentException("The state object was either of the wrong type or null.");

                    if (args.NewName == args.OldName)
                        throw new Exception("The name changed event was raised; however, the two names appear to be the same.");

                    //Ok, so we have an email we can use to contact the person!
                    Email.EmailInterface.CCEmailMessage
                        .CreateDefault()
                        .To(GetValidSubscriptionEmailAddresses(client))
                        .Subject("Name Changed Event")
                        .HTMLAlternateViewUsingTemplateFromEmbedded("CCServ.Email.Templates.NameChangedEvent_HTML.html", args)
                        .SendWithRetryAndFailure(TimeSpan.FromSeconds(1));
                }).ConfigureAwait(false);
        }

        /// <summary>
        /// Set up the name changed event.
        /// </summary>
        public NameChangedEvent()
        {
            Name = "Name Changed";
            Description = "Occurs when a person's first name, last name or middle name changes.";

            RequiresChainOfCommand = false;
        }
    }
}
