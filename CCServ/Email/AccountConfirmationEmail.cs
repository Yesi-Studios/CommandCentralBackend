using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentEmail;

namespace CCServ.Email
{
    /// <summary>
    /// The account confirmation email.
    /// </summary>
    public class AccountConfirmationEmail : CCEmailMessage
    {
        /// <summary>
        /// The template of this email.
        /// </summary>
        public override string Template
        {
            get
            {
                return Templates.TemplateManager.AllTemplates["AccountConfirmationEmail.html"];
            }
        }

        /// <summary>
        /// Creates a new account confirmation email.
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="context"></param>
        /// <param name="smtpHost"></param>
        public AccountConfirmationEmail(string subject, Args.AccountConfirmationEmailArgs args)
            : base(subject)
        {
        }

        [ServiceManagement.StartMethod(Priority = 8)]
        private static void TestSend(CLI.Options.LaunchOptions options)
        {
            new AccountConfirmationEmail("test", new Args.AccountConfirmationEmailArgs { AddressTo = "sundevilgoalie13@gmail.com", ConfirmationId = Guid.NewGuid(), SSN = "sf3weegr" }).Send("localhost");
        }
    }
}
