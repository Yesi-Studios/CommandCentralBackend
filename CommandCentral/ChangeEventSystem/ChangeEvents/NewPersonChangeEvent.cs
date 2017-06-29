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
    /// The event that should be raised when a new person is created.
    /// </summary>
    public class NewPersonChangeEvent : IChangeEvent
    {

        #region Properties

        /// <summary>
        /// The unique Id of this event.
        /// </summary>
        public Guid Id => Guid.Parse("{C7743796-95BC-40B7-8971-0BCD98978CD1}");

        /// <summary>
        /// The name of this event.
        /// </summary>
        public string EventName => "New Person";

        /// <summary>
        /// The description of this event.
        /// </summary>
        public string Description => "When a new person is entered into Command Central.";

        /// <summary>
        /// Instructs the service to restrict the chain of command query.
        /// </summary>
        public bool RestrictToChainOfCommand => false;

        /// <summary>
        /// The valid levels for this event (command).
        /// </summary>
        public List<ChainOfCommandLevels> ValidLevels => new List<ChainOfCommandLevels> { ChainOfCommandLevels.Command };

        /// <summary>
        /// The person who raised this event.  This is the person who created the person.
        /// </summary>
        public Person EventRaisedBy { get; private set; }

        /// <summary>
        /// The person who was created.
        /// </summary>
        public Person EventRaisedAbout { get; private set; }

        #endregion

        #region ctors

        /// <summary>
        /// Creates a default change event.
        /// </summary>
        public NewPersonChangeEvent()
        {
        }

        /// <summary>
        /// Creates a new change event.
        /// </summary>
        /// <param name="by"></param>
        /// <param name="about"></param>
        public NewPersonChangeEvent(Person by, Person about)
        {
            this.EventRaisedBy = by;
            this.EventRaisedAbout = about;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sends the email for this change event.
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
                    .HTMLAlternateViewUsingTemplateFromEmbedded("CommandCentral.Email.Templates.NewPersonChangeEvent_HTML.html", new Email.Models.NewPersonChangeEventEmailModel { ChangeEvent = this }));
            }

            Parallel.ForEach(emails, email =>
            {
                email.SendWithRetryAndFailure(TimeSpan.FromSeconds(1));
            });
        }

        #endregion

    }
}
