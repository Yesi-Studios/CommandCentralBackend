using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Mail;
using System.Reflection;
using System.Threading.Tasks;
using CCServ.ClientAccess;
using AtwoodUtils;
using System.Linq;

namespace CCServ
{
    /// <summary>
    /// Provides methods that help the service interact with users via email.
    /// </summary>
    public static class EmailHelper
    {
        /// <summary>
        /// The address to be used by all emails as their sender address.
        /// </summary>
        private static readonly MailAddress _emailSenderAddress = new MailAddress("usn.gordon.inscom.list.nsag-nioc-ga-webmaster@mail.mil", "Command Central Communications");

        /// <summary>
        /// The address that should be used as the "reply to" address on all mail messages.
        /// </summary>
        private static readonly MailAddress _replyToAddress = new MailAddress("usn.gordon.inscom.list.nsag-nioc-ga-webmaster@mail.mil", "Command Central Communications");

        /// <summary>
        /// The SMTP server to use to send our emails.
        /// </summary>
        public static readonly string SmtpHost = "localhost";
        //public static readonly string SmtpHost = "smtp.gordon.army.mil";

        /// <summary>
        /// The template URI for where the reset password page can be found.
        /// </summary>
        private static readonly string _passwordResetPageLocation = @"http://147.51.62.19/livebeta/#/finishreset/";

        /// <summary>
        /// The required "host" portion of an email address for an email address to be considered a DOD email address.
        /// </summary>
        public static string RequiredDODEmailHost
        {
            get { return "mail.mil"; }
        }

        /// <summary>
        /// A list of the developers' emails.
        /// </summary>
        private static readonly List<string> _developerEmailAddresses = new List<string>
        { 
            "daniel.k.atwood.mil@mail.mil", 
            "sundevilgoalie13@gmail.com",
            "anguslmm@gmail.com",
            "angus.l.mclean5.mil@mail.mil"
        };

        /// <summary>
        /// Sends a generic error.
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <param name="subject"></param>
        /// <returns></returns>
        public static async Task SendGenericErrorEmail(string errorMessage, string subject)
        {
            MailMessage message = new MailMessage();
            _developerEmailAddresses.ForEach(x => message.To.Add(x));
            message.Subject = subject;
            message.From = _emailSenderAddress;
            message.Body = string.Format(await LoadEmailResource("GenericError.html"), DateTime.Now, errorMessage);
            SmtpClient client = new SmtpClient(SmtpHost);
            await client.SendMailAsync(message);
        }
        
        /// <summary>
        /// Sends a muster report.
        /// </summary>
        /// <param name="report"></param>
        public static void SendMusterReportEmail(Entities.Muster.MusterReport report)
        {
            MailMessage message = BuildStandardMessage();
            message.To.Add(_emailSenderAddress);
            message.Subject = "Muster Report";
            message.Body = "Muster rolled over!";
            SmtpClient client = new SmtpClient(SmtpHost) { DeliveryMethod = SmtpDeliveryMethod.Network };
            client.Send(message);
        }

        /// <summary>
        /// Load an email template.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static async Task<string> LoadEmailResource(string fileName)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = string.Format("CCServ.Resources.EmailTemplates.{0}", fileName);

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                Debug.Assert(stream != null, "stream != null");
                using (StreamReader reader = new StreamReader(stream))
                {
                    return await reader.ReadToEndAsync();
                }
            }
        }

        /// <summary>
        /// Builds the basic parts of a message and prepares it to be sent.
        /// </summary>
        /// <returns></returns>
        private static MailMessage BuildStandardMessage()
        {
            //The warning disable here is to suppress the warning that tells us that using the "ReplyTo" field is obsolete.  
            //The only other options is to use the ReplyToList and then use the .Add method on it.  This is easier so Yolo.
            #pragma warning disable 612, 618
            MailMessage message = new MailMessage
            {
                IsBodyHtml = true,
                From = _emailSenderAddress,
                Sender = _emailSenderAddress,
                ReplyTo = _replyToAddress,
                Priority = MailPriority.High
            };
            #pragma warning restore 612, 618

            message.CC.Add(_emailSenderAddress);

            return message;

        }
        

    }
}
