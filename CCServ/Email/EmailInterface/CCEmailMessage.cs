using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using Polly;
using AtwoodUtils;
using System.Reflection;
using System.IO;
using System.Net.Mime;

namespace CCServ.Email.EmailInterface
{
    /// <summary>
    /// The base of the mail system.
    /// </summary>
    public class CCEmailMessage
    {
        /// <summary>
        /// The underlying mail message.
        /// </summary>
        public MailMessage Message { get; set; }
        
        /// <summary>
        /// The SMTP clients to use to send the email.
        /// </summary>
        private List<SmtpClient> _clients;

        /// <summary>
        /// Creates a new CCEmailMessage with the following defaults:
        /// <para />
        /// From : Dev Distro
        /// <para />
        /// BCC : Atwood
        /// <para />
        /// ReplyTo : Dev Distro
        /// <para />
        /// HighPriority
        /// <para />
        /// SMTP Hosts : DODSMTP,localhost
        /// </summary>
        /// <returns></returns>
        public static CCEmailMessage CreateDefault()
        {
            return CCEmailMessage
                    .From(new System.Net.Mail.MailAddress(
                        ServiceManagement.ServiceManager.CurrentConfigState.DeveloperDistroAddress, 
                        ServiceManagement.ServiceManager.CurrentConfigState.DeveloperDistroDisplayName))
                    .BCC(new System.Net.Mail.MailAddress(
                        ServiceManagement.ServiceManager.CurrentConfigState.DeveloperDistroAddress, 
                        ServiceManagement.ServiceManager.CurrentConfigState.DeveloperDistroDisplayName))
                    .ReplyTo(new System.Net.Mail.MailAddress(
                        ServiceManagement.ServiceManager.CurrentConfigState.DeveloperDistroAddress, 
                        ServiceManagement.ServiceManager.CurrentConfigState.DeveloperDistroDisplayName))
                    .HighProperty()
                    .UsingSMTPHosts(ServiceManagement.ServiceManager.CurrentConfigState.DODSMTPAddress, "localhost");
        }

        /// <summary>
        /// Starts a new mail message and sets the from field.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static CCEmailMessage From(MailAddress address)
        {
            return new CCEmailMessage { Message = new MailMessage { From = address } };
        }

        /// <summary>
        /// Starts a new mail message and sets the from field.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="displayName"></param>
        /// <returns></returns>
        public static CCEmailMessage From(string address, string displayName = "")
        {
            return new CCEmailMessage { Message = new MailMessage { From = new MailAddress(address, displayName) } };
        }

        /// <summary>
        /// The addresses to add to the To collection.
        /// </summary>
        /// <param name="addresses"></param>
        /// <returns></returns>
        public CCEmailMessage To(params MailAddress[] addresses)
        {
            foreach (var address in addresses)
            {
                Message.To.Add(address);
            }
            return this;
        }
        
        /// <summary>
        /// The address to which to send this email.  Adds to the To collection.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="displayName"></param>
        /// <returns></returns>
        public CCEmailMessage To(string address, string displayName = "")
        {
            Message.To.Add(new MailAddress(address, displayName));
            return this;
        }

        /// <summary>
        /// The addresses to add to the To collection.
        /// </summary>
        /// <param name="addresses"></param>
        /// <returns></returns>
        public CCEmailMessage To(IEnumerable<MailAddress> addresses)
        {
            foreach (var address in addresses)
            {
                Message.To.Add(address);
            }
            return this;
        }

        /// <summary>
        /// The addresses to add to the CC collection.
        /// </summary>
        /// <param name="addresses"></param>
        /// <returns></returns>
        public CCEmailMessage CC(params MailAddress[] addresses)
        {
            foreach (var address in addresses)
            {
                Message.CC.Add(address);
            }
            return this;
        }

        /// <summary>
        /// The address to add to the CC collection.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="displayName"></param>
        /// <returns></returns>
        public CCEmailMessage CC(string address, string displayName = "")
        {
            Message.CC.Add(new MailAddress(address, displayName));
            return this;
        }

        /// <summary>
        /// The addresses to add to the CC collection.
        /// </summary>
        /// <param name="addresses"></param>
        /// <returns></returns>
        public CCEmailMessage CC(IEnumerable<MailAddress> addresses)
        {
            foreach (var address in addresses)
            {
                Message.CC.Add(address);
            }
            return this;
        }

        /// <summary>
        /// The addresses to add to the BCC collection.
        /// </summary>
        /// <param name="addresses"></param>
        /// <returns></returns>
        public CCEmailMessage BCC(params MailAddress[] addresses)
        {
            foreach (var address in addresses)
            {
                Message.Bcc.Add(address);
            }
            return this;
        }

        /// <summary>
        /// The address to add to the BCC collection.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="displayName"></param>
        /// <returns></returns>
        public CCEmailMessage BCC(string address, string displayName = "")
        {
            Message.Bcc.Add(new MailAddress(address, displayName));
            return this;
        }

        /// <summary>
        /// The addresses to add to the BCC collection.
        /// </summary>
        /// <param name="addresses"></param>
        /// <returns></returns>
        public CCEmailMessage BCC(IEnumerable<MailAddress> addresses)
        {
            foreach (var address in addresses)
            {
                Message.Bcc.Add(address);
            }
            return this;
        }

        public CCEmailMessage ReplyTo(params MailAddress[] addresses)
        {
            foreach (var address in addresses)
            {
                Message.ReplyToList.Add(address);
            }
            return this;
        }

        public CCEmailMessage ReplyTo(string address, string displayName = "")
        {
            Message.ReplyToList.Add(new MailAddress(address, displayName));
            return this;
        }

        public CCEmailMessage ReplyTo(IEnumerable<MailAddress> addresses)
        {
            foreach (var address in addresses)
            {
                Message.ReplyToList.Add(address);
            }
            return this;
        }

        public CCEmailMessage Subject(string subject)
        {
            Message.Subject = subject;
            return this;
        }

        public CCEmailMessage HighProperty()
        {
            Message.Priority = MailPriority.High;
            return this;
        }

        public CCEmailMessage LowPriority()
        {
            Message.Priority = MailPriority.Low;
            return this;
        }
        
        public CCEmailMessage Attach(Attachment attachment)
        {
            if (Message.Attachments.Contains(attachment))
                throw new ArgumentException("Your attachment is already in the attachments!", "attachment");

            Message.Attachments.Add(attachment);

            return this;
        }

        public CCEmailMessage Attach(IEnumerable<Attachment> attachments)
        {
            foreach (var attachment in attachments)
            {
                if (Message.Attachments.Contains(attachment))
                    throw new ArgumentException("Your attachment is already in the attachments!", "attachment");

                Message.Attachments.Add(attachment);
            }

            return this;
        }

        /// <summary>
        /// Uses a template embedded in the assembly to generate an alternate view with the given HTML.  The templates are cached, so if the template has already been used once, it doesn't have to be loaded again.
        /// </summary>
        /// <param name="resourcePath"></param>
        /// <param name="model"></param>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public CCEmailMessage HTMLAlternateViewUsingTemplateFromEmbedded(string resourcePath, object model, Assembly assembly = null)
        {
            assembly = assembly ?? Assembly.GetCallingAssembly();

            ContentType mimeType = new System.Net.Mime.ContentType("text/html");

            var content = TemplateHelper.RenderTemplate(resourcePath, model, assembly);

            AlternateView view = AlternateView.CreateAlternateViewFromString(content, mimeType);

            Message.AlternateViews.Add(view);

            return this;
        }

        /// <summary>
        /// Uses the given email client.
        /// </summary>
        /// <param name="client">Smtp client to send the email.</param>
        /// <returns>Instance of the Email class</returns>
        public CCEmailMessage UsingClient(SmtpClient client)
        {
            _clients = new List<SmtpClient> { client };
            return this;
        }

        /// <summary>
        /// Uses the given smtp hosts to send the email message.  The order they are tried in is the order in which they are passed.
        /// </summary>
        /// <param name="hosts"></param>
        /// <returns></returns>
        public CCEmailMessage UsingSMTPHosts(params string[] hosts)
        {
            _clients = hosts.Select(x => new SmtpClient(x)).ToList();
            return this;
        }

        /// <summary>
        /// Attempts to send your email, trying a number of times equal to the number of SMTP hosts that were passed during configuration and calling the callbacks as necessary.
        /// <para/>
        /// WARNING: This method is thread safe and non-blocking; however, changes to this mail object after calling this method are not.
        /// </summary>
        /// <param name="retryCallback">The callback that will be called before a re-attempt is made.  The exception that occurred that caused the retry will be passed along with how long we're going to wait until the next retry and which retry attempt we're on.</param>
        /// <param name="failureCallback">The callback that will be called if all sending attempts fail.  The final exception will be passed.  If this callback is called, the email was never sent.</param>
        /// <param name="retryDelay">The delay to use between retry attempts.  Note: It takes a few seconds for the email to fail by hitting the timeout.</param>
        /// <returns></returns>
        public CCEmailMessage SendWithRetryAndFailure(TimeSpan retryDelay, Action<Exception, TimeSpan, int> retryCallback = null, Action<Exception> failureCallback = null)
        {
            if (_clients == null || !_clients.Any())
            {
                throw new Exception("Clients must be configured prior to calling a send method.");
            }

            SmtpClient attemptClient = _clients.First();

            Task.Run(() =>
            {
                var result = Policy
                .Handle<SmtpException>()
                .WaitAndRetry(_clients.Count - 1, count => retryDelay, (exception, waitDuration, retryCount, context) =>
                {
                    attemptClient = _clients[retryCount];
                    if (retryCallback != null)
                        retryCallback(exception, waitDuration, retryCount);
                })
                .ExecuteAndCapture(() =>
                {
                    attemptClient.Send(Message);
                });

                if (result.Outcome == OutcomeType.Failure)
                {
                    if (failureCallback != null)
                        failureCallback(result.FinalException);
                }
            });

            return this;
        }
    }
}
