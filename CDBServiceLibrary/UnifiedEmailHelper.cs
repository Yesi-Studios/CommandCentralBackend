using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using System.IO;
using AtwoodUtils;

namespace UnifiedServiceFramework
{
    /// <summary>
    /// Provides methods for sending emails when/if the underying service framework crashes.
    /// </summary>
    public static class UnifiedEmailHelper
    {

        private static MailAddress _unifiedSenderAddress = new MailAddress("USF-Communications@unifiedservice.com", "United Service Communications");

        private static readonly List<string> _developerEmailAddresses = new List<string>() 
        { 
            "daniel.k.atwood.mil@mail.mil", 
            "sundevilgoalie13@gmail.com",
            "anguslmm@gmail.com",
            "angus.l.mclean5.mil@mail.mil"
        };

        /// <summary>
        /// The unified email helper's SMTP host
        /// </summary>
        public static readonly string SmtpHost = "smtp.gordon.army.mil";

        /// <summary>
        /// Sends an email message to the unified service developers alerting them that a fatal error has occurred.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public static async Task SendFatalErrorEmail(Framework.MessageTokens.MessageToken token, Exception e)
        {
            //The warning disable here is to supress the warning that tells us that using the "ReplyTo" field is obsolete.  
            //The only other options is to use the ReplyToList and then use the .Add method on it.  This is easier so Yolo.
            #pragma warning disable 612, 618
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
                    token.ID, token.APIKey, token.CallTime.ToUniversalTime(), token.Args.Serialize(), token.Endpoint, token.Result.Serialize(), token.State.ToString(), token.HandledTime.ToUniversalTime(),
                    e.Message, (e.InnerException == null) ? "NULL" : e.InnerException.Message, e.StackTrace, e.Source, e.TargetSite);
            }
            else
            {
                message.Body = string.Format(await LoadEmailResource("FatalError.html"), DateTime.Now.ToUniversalTime(),
                    token.Session.ID, token.Session.LoginTime.ToUniversalTime(), token.Session.PersonID, token.Session.LogoutTime.ToUniversalTime(),
                    token.Session.IsActive, token.Session.PermissionIDs.Serialize(),
                    token.ID, token.APIKey, token.CallTime.ToUniversalTime(), token.Args.Serialize(), token.Endpoint, token.Result.Serialize(), token.State.ToString(), token.HandledTime.ToUniversalTime(),
                    e.Message, e.InnerException.Message, e.StackTrace, e.Source, e.TargetSite);
            }


            _developerEmailAddresses.ForEach(x => message.To.Add(x));

            SmtpClient client = new SmtpClient(SmtpHost);
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            await client.SendMailAsync(message);

        }

        private static async Task<string> LoadEmailResource(string fileName)
        {
            try
            {
                System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                string resourceName = string.Format("UnifiedServiceFramework.Resources.EmailTemplates.{0}", fileName);

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

    }
}
