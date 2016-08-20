using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using FluentEmail;
using Polly;
using AtwoodUtils;

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
        public IFluentEmail UnderlyingMailMessage { get; set; }

        /// <summary>
        /// The email args of the mail object.
        /// </summary>
        public Args.BaseEmailArgs EmailArgs { get; set; }

        /// <summary>
        /// Creates a new mail message with the defaults.
        /// </summary>
        /// <param name="subject"></param>
        public CCEmailMessage(Args.BaseEmailArgs args)
        {
            UnderlyingMailMessage = FluentEmail.Email
                .From(Config.Email.DeveloperDistroAddress.Address, Config.Email.DeveloperDistroAddress.DisplayName)
                .CC(Config.Email.DeveloperDistroAddress.Address, Config.Email.DeveloperDistroAddress.DisplayName)
                .ReplyTo(Config.Email.DeveloperDistroAddress.Address, Config.Email.DeveloperDistroAddress.DisplayName)
                .BodyAsHtml()
                .HighPriority()
                .UsingTemplate(Email.Templates.TemplateManager.AllTemplates[Template], args, true)
                .Subject(args.Subject)
                .To(args.ToAddressList.Select(x => new MailAddress(x)).ToList());
        }

        /// <summary>
        /// Sends the mail message.
        /// </summary>
        public void Send(string smtpHost = "localhost", string alternateSMTPHost = "smtp.gordon.army.mil")
        {

            string attemptServer = smtpHost;
            Task.Run(() =>
            {
                var result = Policy
                .Handle<SmtpException>()
                .WaitAndRetry(1, count => TimeSpan.FromSeconds(1), (exception, waitDuration) =>
                {
                    Logging.Log.Critical("A critical error occurred while trying to send an email.  The SMTP server was not contactable! Trying again in {0} second(s) with server '{1}'...".FormatS(waitDuration.TotalSeconds, alternateSMTPHost));
                    attemptServer = alternateSMTPHost;

                })
                .ExecuteAndCapture(() =>
                {
                    UnderlyingMailMessage
                        .UsingClient(new SmtpClient { Host = attemptServer })
                        .Send();
                });

                if (result.Outcome == OutcomeType.Failure)
                {
                    Logging.Log.Critical("A critical error occurred while trying to send an email.  The SMTP server was not contactable!  Reached maximum number of retries.  Error message: {0}".FormatS(result.FinalException.Message));
                }
            });
        }
    }
}
