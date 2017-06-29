using CommandCentral.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;

namespace CommandCentral.ChangeEventSystem.ChangeEvents
{
    /// <summary>
    /// The event that should be raised when a person's division, department and/or command changes.
    /// </summary>
    public class AssignmentChangedEvent : IChangeEvent
    {

        #region Properties

        /// <summary>
        /// The Id of this event.
        /// </summary>
        public Guid Id => Guid.Parse("{64DC5965-B301-46E9-93DA-923F7EB93862}");

        /// <summary>
        /// The name of this event.
        /// </summary>
        public string EventName => "Assignment Changed";

        /// <summary>
        /// The description of this event.
        /// </summary>
        public string Description => "When a person's division, department or command changes.";

        /// <summary>
        /// Instructs the system to restrict sending emails about this event to members outside a user's chain of command.
        /// </summary>
        public bool RestrictToChainOfCommand => false;

        /// <summary>
        /// The person who raised this event.
        /// </summary>
        public Person EventRaisedBy { get; private set; }

        /// <summary>
        /// The person whomst've the event was raised about.
        /// </summary>
        public Person EventRaisedAbout { get; private set; }

        /// <summary>
        /// The assignment before the change occurred.
        /// </summary>
        public Assignment OldAssignment { get; private set; }

        /// <summary>
        /// The assignment after the change occurred.
        /// </summary>
        public Assignment NewAssignment { get; private set; }

        /// <summary>
        /// The valid levels for this change event.
        /// </summary>
        public List<ChainOfCommandLevels> ValidLevels => new List<ChainOfCommandLevels> { ChainOfCommandLevels.Command, ChainOfCommandLevels.Department, ChainOfCommandLevels.Division };

        #endregion

        #region ctors

        /// <summary>
        /// Creates a blank change event with the default values.
        /// </summary>
        public AssignmentChangedEvent()
        {
        }

        /// <summary>
        /// Creates a new assignment changed event.
        /// </summary>
        /// <param name="eventRaisedBy"></param>
        /// <param name="eventRaisedAbout"></param>
        /// <param name="oldAssignment"></param>
        /// <param name="newAssignment"></param>
        public AssignmentChangedEvent(Person eventRaisedBy, Person eventRaisedAbout, Assignment oldAssignment, Assignment newAssignment)
        {
            this.EventRaisedBy = eventRaisedBy;
            this.EventRaisedAbout = eventRaisedAbout;
            this.OldAssignment = oldAssignment;
            this.NewAssignment = newAssignment;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sends the change event email.  Template compilation is done on the calling thread, but sending is done in parallel.
        /// </summary>
        public void SendEmail()
        {
            var emailAddresses = ChangeEventHelper.GetValidSubscriptionEmailAddresses(this.EventRaisedAbout, this).ToList();
            List<Email.EmailInterface.CCEmailMessage> emails = new List<Email.EmailInterface.CCEmailMessage>();

            foreach (var emailAddress in emailAddresses)
            {
                emails.Add(Email.EmailInterface.CCEmailMessage
                    .CreateDefault()
                    .To(emailAddress)
                    .Subject("{0} Event".With(this.EventName))
                    .HTMLAlternateViewUsingTemplateFromEmbedded("CommandCentral.Email.Templates.AssignmentChangedEvent_HTML.html", new Email.Models.AssignmentChangedEventEmailModel { ChangeEvent = this }));
            }

            Parallel.ForEach(emails, email =>
            {
                email.SendWithRetryAndFailure(TimeSpan.FromSeconds(1));
            });
        }

        #endregion
    }
}
