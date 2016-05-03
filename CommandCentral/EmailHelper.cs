using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Mail;
using System.Reflection;
using System.Threading.Tasks;
using CommandCentral.ClientAccess;


namespace CommandCentral
{
    /// <summary>
    /// Provides methods that help the service interact with users via email.
    /// </summary>
    public static class EmailHelper
    {
        /// <summary>
        /// The address to be used by all emails as their sender address.
        /// </summary>
        private static readonly MailAddress emailSenderAddress = new MailAddress("usn.gordon.inscom.list.nsag-nioc-ga-webmaster@mail.mil", "NIOC GA Command DB Communications");

        /// <summary>
        /// The address that should be used as the "reply to" address on all mail messages.
        /// </summary>
        private static readonly MailAddress replyToAddress = new MailAddress("usn.gordon.inscom.list.nsag-nioc-ga-webmaster@mail.mil", "NIOC GA Command DB Communications");

        /// <summary>
        /// The SMTP server to use to send our emails.
        /// </summary>
        public static readonly string SmtpHost = "smtp.gordon.army.mil";

        /// <summary>
        /// The template URI for where the complete registration page can be found. 147.51.62.19
        /// </summary>
        private static readonly string completeRegistrationPageLocation = @"147.51.62.19/CC/#/finishregistration/";

        /// <summary>
        /// The template URI for where the reset password page can be found.
        /// </summary>
        private static readonly string passwordResetPageLocation = @"file:///E:/CommandDB Frontend/ResetPassword.html?ResetPasswordID=";

        /// <summary>
        /// The required "host" portion of an email address for an email address to be considered a DOD email address.
        /// </summary>
        public static string RequiredDODEmailHost
        {
            get { return "mail.mil"; }
        }

        private static readonly List<string> developerEmailAddresses = new List<string>
        { 
            "daniel.k.atwood.mil@mail.mil", 
            "sundevilgoalie13@gmail.com",
            "anguslmm@gmail.com",
            "angus.l.mclean5.mil@mail.mil"
        };

        /// <summary>
        /// Sends an email to the specified email address, intended for the specified user account that informs the user that a login attempt failed on their account.
        /// <para />
        /// Uses the FailedAccountLogin.html template.
        /// </summary>
        /// <param name="emailAddressTo"></param>
        /// <param name="personId"></param>
        /// <returns></returns>
        public static async Task SendFailedAccountLoginEmail(string emailAddressTo, Guid personId)
        {
            MailMessage message = BuildStandardMessage();
            message.To.Add(emailAddressTo);
            message.Subject = "Failed Account Login";
            message.Body = string.Format(await LoadEmailResource("FailedAccountLogin.html"), DateTime.Now, personId);
            SmtpClient client = new SmtpClient(SmtpHost) { DeliveryMethod = SmtpDeliveryMethod.Network };
            await client.SendMailAsync(message);
        }

        /// <summary>
        /// Sends an email to the specified email address with the information necessary to complete account registration.
        /// <para />
        /// Uses the ConfirmAccount.html email template.
        /// </summary>
        /// <param name="emailAddressTo"></param>
        /// <param name="confirmationId"></param>
        /// <param name="ssn"></param>
        /// <returns></returns>
        public static async Task SendConfirmAccountEmail(string emailAddressTo, Guid confirmationId, string ssn)
        {
            MailMessage message = BuildStandardMessage();
            message.To.Add(emailAddressTo);
            message.Subject = "Confirm Email Address";
            message.Body = string.Format(await LoadEmailResource("ConfirmAccount.html"), DateTime.Now, completeRegistrationPageLocation + confirmationId, ssn.Substring((ssn.Length - 1) - 4));
            SmtpClient client = new SmtpClient(SmtpHost) { DeliveryMethod = SmtpDeliveryMethod.Network };
            await client.SendMailAsync(message);
        }
        
        /// <summary>
        /// Sends an email to a client informing the client that an error occurred during registration.
        /// </summary>
        /// <param name="emailAddressTo"></param>
        /// <param name="personId"></param>
        /// <returns></returns>
        public static async Task SendBeginRegistrationErrorEmail(string emailAddressTo, Guid personId)
        {
            MailMessage message = new MailMessage();
            developerEmailAddresses.ForEach(x => message.To.Add(x));
            message.To.Add(emailAddressTo);
            message.Subject = "IMPORTANT! Registration - Important Security Error";
            message.From = emailSenderAddress;
            message.Body = string.Format(await LoadEmailResource("BeginRegistrationError.html"), DateTime.Now, personId);
            SmtpClient client = new SmtpClient(SmtpHost);
            await client.SendMailAsync(message);
        }

        /// <summary>
        /// Sends a generic error.
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <param name="subject"></param>
        /// <returns></returns>
        public static async Task SendGenericErrorEmail(string errorMessage, string subject)
        {
            MailMessage message = new MailMessage();
            developerEmailAddresses.ForEach(x => message.To.Add(x));
            message.Subject = subject;
            message.From = emailSenderAddress;
            message.Body = string.Format(await LoadEmailResource("GenericError.html"), DateTime.Now, errorMessage);
            SmtpClient client = new SmtpClient(SmtpHost);
            await client.SendMailAsync(message);
        }

        /// <summary>
        /// Sends an email informing a client that a password reset request has begun.
        /// </summary>
        /// <param name="passwordResetId"></param>
        /// <param name="emailAddressTo"></param>
        /// <returns></returns>
        public static async Task SendBeginPasswordResetEmail(Guid passwordResetId, string emailAddressTo)
        {
            MailMessage message = BuildStandardMessage();
            message.To.Add(emailAddressTo);
            message.Subject = "CommandDB Password Reset";
            message.Body = string.Format(await LoadEmailResource("InitiatePasswordReset.html"), DateTime.Now, passwordResetPageLocation + passwordResetId, emailAddressTo);
            SmtpClient client = new SmtpClient(SmtpHost) { DeliveryMethod = SmtpDeliveryMethod.Network };
            await client.SendMailAsync(message);
        }

        /// <summary>
        /// Load an email template.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static async Task<string> LoadEmailResource(string fileName)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = string.Format("CommandCentral.Resources.EmailTemplates.{0}", fileName);

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
                From = emailSenderAddress,
                Sender = emailSenderAddress,
                ReplyTo = replyToAddress,
                Priority = MailPriority.High
            };
            #pragma warning restore 612, 618

            message.CC.Add(emailSenderAddress);

            return message;

        }

        /// <summary>
        /// Sends an email message to the unified service developers alerting them that a fatal error has occurred.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public static void SendFatalErrorEmail(MessageToken token, Exception e)
        {
            //The warning disable here is to supress the warning that tells us that using the "ReplyTo" field is obsolete.  
            //The only other options is to use the ReplyToList and then use the .Add method on it.  This is easier so Yolo.
            /*#pragma warning disable 612, 618
            MailMessage message = new MailMessage()
            {
                IsBodyHtml = true,
                From = _unifiedSenderAddress,
                Sender = _unifiedSenderAddress,
                ReplyTo = _unifiedSenderAddress,
                Priority = MailPriority.High,
                Subject = "IMPORTANT!  Unified Service Crash Error Report"
            };
            #pragma warning restore 612, 618

            if (token.Session == null)
            {
                message.Body = string.Format(await LoadEmailResource("FatalError.html"), DateTime.Now.ToUniversalTime(),
                    "NULL", "NULL", "NULL", "NULL", "NULL", "NULL",
                    token.Id, token.APIKey, token.CallTime.ToUniversalTime(), token.Args.Serialize(), token.Endpoint, token.Result.Serialize(), token.State.ToString(), token.HandledTime.ToUniversalTime(),
                    e.Message, (e.InnerException == null) ? "NULL" : e.InnerException.Message, e.StackTrace, e.Source, e.TargetSite);
            }
            else
            {
                message.Body = string.Format(await LoadEmailResource("FatalError.html"), DateTime.Now.ToUniversalTime(),
                    token.Session.Id, token.Session.LoginTime.ToUniversalTime(), token.Session.PersonID, token.Session.LogoutTime.ToUniversalTime(),
                    token.Session.IsActive, token.Session.PermissionIDs.Serialize(),
                    token.Id, token.APIKey, token.CallTime.ToUniversalTime(), token.Args.Serialize(), token.Endpoint, token.Result.Serialize(), token.State.ToString(), token.HandledTime.ToUniversalTime(),
                    e.Message, e.InnerException.Message, e.StackTrace, e.Source, e.TargetSite);
            }


            _developerEmailAddresses.ForEach(x => message.To.Add(x));

            SmtpClient client = new SmtpClient(SmtpHost);
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            await client.SendMailAsync(message);*/

        }

    }
}
