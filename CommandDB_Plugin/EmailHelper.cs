using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Reflection;
using System.IO;


namespace CommandDB_Plugin
{
    /// <summary>
    /// Provides methods that help the service interact with users via email.
    /// </summary>
    public static class EmailHelper
    {
        /// <summary>
        /// The address to be used by all emails as their sender address.
        /// </summary>
        private static readonly MailAddress _emailSenderAddress = new MailAddress("usn.gordon.inscom.list.nsag-nioc-ga-webmaster@mail.mil", "NIOC GA Command DB Communications");

        /// <summary>
        /// The address that should be used as the "reply to" address on all mail messages.
        /// </summary>
        private static readonly MailAddress _replyToAddress = new MailAddress("usn.gordon.inscom.list.nsag-nioc-ga-webmaster@mail.mil", "NIOC GA Command DB Communications");

        /// <summary>
        /// The SMTP server to use to send our emails.
        /// </summary>
        public static readonly string SmtpHost = "smtp.gordon.army.mil";

        /// <summary>
        /// The template URI for where the complete registration page can be found. 147.51.62.19
        /// </summary>
        private static readonly string _completeRegistrationPageLocation = @"147.51.62.19/CC/#/finishregistration/";

        /// <summary>
        /// The template URI for where the reset password page can be found.
        /// </summary>
        private static readonly string _passwordResetPageLocation = @"file:///E:/CommandDB Frontend/ResetPassword.html?ResetPasswordID=";

        /// <summary>
        /// The required "host" portion of an email address for an email address to be considered a DOD email address.
        /// </summary>
        public static string RequiredDODEmailHost
        {
            get
            {
                return "mail.mil";
            }
        }

        private static readonly List<string> _developerEmailAddresses = new List<string>() 
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
        /// <param name="personID"></param>
        /// <returns></returns>
        public static async Task SendFailedAccountLoginEmail(string emailAddressTo, string personID)
        {
            try
            {
                MailMessage message = BuildStandardMessage();
                message.To.Add(emailAddressTo);
                message.Subject = "Failed Account Login";
                message.Body = string.Format(await LoadEmailResource("FailedAccountLogin.html"), DateTime.Now, personID);
                SmtpClient client = new SmtpClient(SmtpHost);
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                await client.SendMailAsync(message);
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        /// <summary>
        /// Sends an email to the specified email address with the information necessary to complete account registration.
        /// <para />
        /// Uses the ConfirmAccount.html email template.
        /// </summary>
        /// <param name="emailAddressTo"></param>
        /// <param name="confirmationID"></param>
        /// <param name="ssn"></param>
        /// <returns></returns>
        public static async Task SendConfirmAccountEmail(string emailAddressTo, string confirmationID, string ssn)
        {
            try
            {

                MailMessage message = BuildStandardMessage();
                message.To.Add(emailAddressTo);
                message.Subject = "Confirm Email Address";
                message.Body = string.Format(await LoadEmailResource("ConfirmAccount.html"), DateTime.Now, _completeRegistrationPageLocation + confirmationID, ssn.Substring((ssn.Length - 1) - 4));
                SmtpClient client = new SmtpClient(SmtpHost);
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                await client.SendMailAsync(message);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Sends an email to the given list of recipients informing them that the muster has rolled over.
        /// <para />
        /// Uses the FinalMusterReport.html email template.
        /// </summary>
        /// <param name="emailAddressTo"></param>
        /// <param name="confirmationID"></param>
        /// <param name="ssn"></param>
        /// <returns></returns>
        public static async Task SendFinalMusterReportEmail(List<string> emailAddressesTo, DateTime musterDay, int total, int totalMustered, int officers, int officersMustered, 
            int enlisted, int enlistedMustered, int don, int donMustered, int reservists, int reservistsMustered, int contractors, int contractorsMustered,
            int pep, int pepMustered, int td, int present, int aa, int tad, int leave, int terminalLeave, int deployed, int siq, int ua, int other, int unaccounted, string unaccountedString)
        {
            try
            {
                MailMessage message = BuildStandardMessage();
                message.Subject = string.Format("Final Muster Report - {0}", musterDay.ToShortDateString());
                message.Body = string.Format(await LoadEmailResource("ConfirmAccount.html"), DateTime.Now, musterDay.ToShortDateString(), total, totalMustered, officers, officersMustered,
                    enlisted, enlistedMustered, don, donMustered, reservists, reservistsMustered, contractors, contractorsMustered, pep, pepMustered,
                    td, present, aa, tad, leave, terminalLeave, deployed, siq, ua, other, unaccounted, unaccountedString);
                SmtpClient client = new SmtpClient(SmtpHost);
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                emailAddressesTo.ForEach(x => message.To.Add(x));
                await client.SendMailAsync(message);
                
            }
            catch
            {
                throw;
            }
        }

        public static async Task SendAccountUpdatedEmail(string emailAddressTo, string editorID, List<Changes.Change> changes)
        {
            try
            {
                //Build the body of the email
                string body = "";
                changes.ForEach(x =>
                    {
                        body += x.ToString() + Environment.NewLine + "<br />";
                    });


                MailMessage message = BuildStandardMessage();
                message.To.Add(emailAddressTo);
                message.Subject = "Your Account Has Been Updated!";
                message.Body = string.Format(await LoadEmailResource("AccountUpdated.html"), editorID, DateTime.Now, body);
                SmtpClient client = new SmtpClient(SmtpHost);
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                await client.SendMailAsync(message);
            }
            catch
            {
                throw;
            }
        }

        public static async Task SendChangeEventOccuredEmail(string emailAddressTo, string editorID, string modelPrimaryID, string eventName, string modelName, List<Changes.Change> changes)
        {
            try
            {
                //Build the body of the email
                string body = "";
                changes.ForEach(x =>
                {
                    body += x.ToString() + Environment.NewLine + "<br />";
                });


                MailMessage message = BuildStandardMessage();
                message.To.Add(emailAddressTo);
                message.Subject = "Your Account Has Been Updated!";
                message.Body = string.Format(await LoadEmailResource("ChangeEventOccured.html"), editorID, eventName, modelName, modelPrimaryID, DateTime.Now, body);
                SmtpClient client = new SmtpClient(SmtpHost);
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                await client.SendMailAsync(message);
            }
            catch
            {
                throw;
            }
        }

        public static async Task SendBilletsUpdatedEmail(string emailAddressTo, string editorID, List<Changes.Change> changes)
        {
            try
            {
                //Build the body of the email
                string body = "";
                changes.ForEach(x =>
                {
                    body += x.ToString() + Environment.NewLine + "<br />";
                });


                MailMessage message = BuildStandardMessage();
                message.To.Add(emailAddressTo);
                message.Subject = "Billets have been updated!";
                message.Body = string.Format(await LoadEmailResource("BilletsUpdated.html"), editorID, DateTime.Now, body);
                SmtpClient client = new SmtpClient(SmtpHost);
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                await client.SendMailAsync(message);
            }
            catch
            {
                throw;
            }
        }

        public static async Task SendBeginRegistrationErrorEmail(string emailAddressTo, string personID)
        {
            try
            {
                MailMessage message = new MailMessage();
                _developerEmailAddresses.ForEach(x => message.To.Add(x));
                message.To.Add(emailAddressTo);
                message.Subject = "IMPORTANT! Registration - Important Security Error";
                message.From = _emailSenderAddress;
                message.Body = string.Format(await LoadEmailResource("BeginRegistrationError.html"), DateTime.Now, personID);
                SmtpClient client = new SmtpClient(SmtpHost);
                await client.SendMailAsync(message);
            }
            catch
            {
                throw;
            }
        }

        public static async Task SendGenericErrorEmail(string errorMessage, string subject)
        {
            try
            {
                MailMessage message = new MailMessage();
                _developerEmailAddresses.ForEach(x => message.To.Add(x));
                message.Subject = subject;
                message.From = _emailSenderAddress;
                message.Body = string.Format(await LoadEmailResource("GenericError.html"), DateTime.Now, errorMessage);
                SmtpClient client = new SmtpClient(SmtpHost);
                await client.SendMailAsync(message);
            }
            catch
            {
                throw;
            }
        }

        

        public static async Task SendInitiatePasswordResetEmail(string passwordResetID, string emailAddressTO)
        {
            try
            {
                MailMessage message = BuildStandardMessage();
                message.To.Add(emailAddressTO);
                message.Subject = "CommandDB Password Reset";
                message.Body = string.Format(await LoadEmailResource("InitiatePasswordReset.html"), DateTime.Now, _passwordResetPageLocation + passwordResetID, emailAddressTO);
                SmtpClient client = new SmtpClient(SmtpHost);
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                await client.SendMailAsync(message);


            }
            catch
            {
                throw;
            }
        }

        private static async Task<string> LoadEmailResource(string fileName)
        {
            try
            {
                System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                string resourceName = string.Format("CommandDB_Plugin.Resources.EmailTemplates.{0}", fileName);

                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                using (StreamReader reader = new StreamReader(stream))
                {
                    return await reader.ReadToEndAsync();
                }

            }
            catch
            {
                throw;
            }
        }

        private static MailMessage BuildStandardMessage()
        {
            //The warning disable here is to supress the warning that tells us that using the "ReplyTo" field is obsolete.  
            //The only other options is to use the ReplyToList and then use the .Add method on it.  This is easier so Yolo.
            #pragma warning disable 612, 618
            MailMessage message = new MailMessage()
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
