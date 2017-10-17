using AtwoodUtils;
using CommandCentral.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.ChangeEventSystem.ChangeEvents
{
    /// <summary>
    /// The event that should be raised when a person's name changes.  A name change is reflected by the First, Middle, and Last names.
    /// </summary>
    public class NameChangedEvent : IChangeEvent
    {

        #region Properties

        /// <summary>
        /// The unique id of this event.
        /// </summary>
        public Guid Id => Guid.Parse("eb5815b0-f2db-47cf-9fb9-c90ff519ca47");

        /// <summary>
        /// The name of this event.
        /// </summary>
        public string EventName => "Name Changed";

        /// <summary>
        /// A short desc of this event describing when the event should fire.
        /// </summary>
        public string Description => "When a person's first, middle, or last name changes.";

        /// <summary>
        /// Indicates that the system should take chain of command into account when determining to whom to send the email.
        /// </summary>
        public bool RestrictToChainOfCommand => false;

        /// <summary>
        /// The valid levels of this event.
        /// </summary>
        public List<ChainOfCommandLevels> ValidLevels => new List<ChainOfCommandLevels> { ChainOfCommandLevels.Command, ChainOfCommandLevels.Department, ChainOfCommandLevels.Division };

        /// <summary>
        /// The person who raised this event by changing the person's profile.
        /// </summary>
        public Person EventRaisedBy { get; private set; }

        /// <summary>
        /// The person whomst've the event was raised about.
        /// </summary>
        public Person EventRaisedAbout { get; private set; }

        /// <summary>
        /// The name of the persion prior to the database update.
        /// </summary>
        public string OldName { get; private set; }

        /// <summary>
        /// The name of the person after the database update.
        /// </summary>
        public string NewName { get; private set; }

        #endregion

        #region ctors

        /// <summary>
        /// Creates a default name changed event.
        /// </summary>
        public NameChangedEvent()
        {
        }

        /// <summary>
        /// Creates a new name changed event with the given parameters.
        /// </summary>
        /// <param name="by"></param>
        /// <param name="about"></param>
        /// <param name="oldName"></param>
        /// <param name="newName"></param>
        public NameChangedEvent(Person by, Person about, string oldName, string newName)
        {
            EventRaisedBy = by;
            EventRaisedAbout = about;
            OldName = oldName;
            NewName = newName;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sends the email for this event.
        /// </summary>
        public void SendEmail()
        {
            var emailAddresses = ChangeEventHelper.GetValidSubscriptionEmailAddresses(EventRaisedAbout, this).ToList();
            var emails = new List<Email.EmailInterface.CCEmailMessage>();

            foreach (var emailAddress in emailAddresses)
            {
                emails.Add(Email.EmailInterface.CCEmailMessage
                    .CreateDefault()
                    .To(emailAddress)
                    .Subject("{0} Event".With(EventName))
                    .HTMLAlternateViewUsingTemplateFromEmbedded("CommandCentral.Email.Templates.NameChangedEvent_HTML.html", new Email.Models.NameChangedEventEmailModel { ChangeEvent = this }));
            }

            Parallel.ForEach(emails, email =>
            {
                email.SendWithRetryAndFailure(TimeSpan.FromSeconds(1));
            });
        }

        #endregion
    }
}
