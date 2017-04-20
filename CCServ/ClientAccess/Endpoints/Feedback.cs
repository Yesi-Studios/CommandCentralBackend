using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.ClientAccess.Endpoints
{
    /// <summary>
    /// Contains the feedback endpoints.
    /// </summary>
    static class Feedback
    {
        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Receives a feedback from the client and then emails it to the developers.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowResponseLogging = true, AllowArgumentLogging = true, RequiresAuthentication = true)]
        private static void SubmitFeedback(MessageToken token)
        {
            token.AssertLoggedIn();
            token.Args.AssertContainsKeys("title", "body");

            string title = token.Args["title"] as string;

            if (string.IsNullOrWhiteSpace(title) || title.Length > 50)
                throw new CommandCentralException("The title of a feedback must not be blank and its title must not be longer than 50 characters.", ErrorTypes.Validation);

            string body = token.Args["body"] as string;

            if (string.IsNullOrWhiteSpace(body) || body.Length > 1000)
                throw new CommandCentralException("Your 'body' parameter must not be empty or greater than 1000 characters.", ErrorTypes.Validation);

            var model = new Email.Models.FeedbackEmailModel
            {
                Body = body,
                Title = title,
                FriendlyName = token.AuthenticationSession.Person.ToString()
            };

            //Let's go get the client's email addresses... preferring their preferred email addresses, then contactable, then DOD.
            var clientEmailAddresses = token.AuthenticationSession.Person.EmailAddresses.Where(x => x.IsPreferred).ToList();

            if (!clientEmailAddresses.Any())
            {
                clientEmailAddresses.AddRange(token.AuthenticationSession.Person.EmailAddresses.Where(x => x.IsContactable));

                if (!clientEmailAddresses.Any())
                {
                    clientEmailAddresses.AddRange(token.AuthenticationSession.Person.EmailAddresses.Where(x => x.IsDodEmailAddress));
                }
            }

            //Ok, we have everything we need.
            Email.EmailInterface.CCEmailMessage
                .CreateDefault()
                .To(clientEmailAddresses.Select(x => new System.Net.Mail.MailAddress(x.Address, model.FriendlyName)))
                .CC(new System.Net.Mail.MailAddress(
                        ServiceManagement.ServiceManager.CurrentConfigState.DeveloperDistroAddress,
                        ServiceManagement.ServiceManager.CurrentConfigState.DeveloperDistroDisplayName))
                .BCC(ServiceManagement.ServiceManager.CurrentConfigState.DeveloperPersonalAddresses)
                .Subject("Command Central Feedback")
                .HTMLAlternateViewUsingTemplateFromEmbedded("CCServ.Email.Templates.Feedback_HTML.html", model)
                .SendWithRetryAndFailure(TimeSpan.FromSeconds(1));
        }
    }
}
