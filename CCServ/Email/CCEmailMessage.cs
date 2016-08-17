using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using FluentEmail;

namespace CCServ.Email
{
    /// <summary>
    /// The base of the mail system.
    /// </summary>
    public abstract class CCEmailMessage
    {
        /// <summary>
        /// The template to use for the given mail message.
        /// </summary>
        public abstract string Template { get; }

        /// <summary>
        /// The FluentEmail mail object.
        /// </summary>
        private IFluentEmail _emailMessage;

        /// <summary>
        /// Creates a new mail message with the defaults.
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="context"></param>
        public CCEmailMessage(string subject)
        {
            _emailMessage = FluentEmail.Email
                .From("usn.gordon.inscom.list.nsag-nioc-ga-webmaster@mail.mil", "Command Central Communications")
                .To("usn.gordon.inscom.list.nsag-nioc-ga-webmaster@mail.mil", "Command Central Communications")
                .CC("usn.gordon.inscom.list.nsag-nioc-ga-webmaster@mail.mil", "Command Central Communications")
                .ReplyTo("usn.gordon.inscom.list.nsag-nioc-ga-webmaster@mail.mil", "Command Central Communications")
                .BodyAsHtml()
                .HighPriority()
                .UsingTemplate(Template, Context)
                .Subject(subject);
        }

        /// <summary>
        /// Sends the mail message.
        /// </summary>
        public void Send(string smtpHost)
        {
            _emailMessage
                .UsingClient(new SmtpClient { Host = smtpHost })
                .Send();
        }
    }
}
