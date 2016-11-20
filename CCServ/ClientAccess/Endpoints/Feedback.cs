using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.ClientAccess.Endpoints
{
    public static class Feedback
    {

        [EndpointMethod(EndpointName = "SubmitFeedback", AllowResponseLogging = true, AllowArgumentLogging = true, RequiresAuthentication = true)]
        private static void EndpointMethod_SubmitFeedback(MessageToken token)
        {
            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to submit feedback.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            //Here we're going to get the two required parameters, title and body, and then do some basic validation.
            if (!token.Args.ContainsKey("title"))
            {
                token.AddErrorMessage("You failed to send a 'title' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            string title = token.Args["title"] as string;

            if (string.IsNullOrWhiteSpace(title))
            {
                token.AddErrorMessage("Your 'title' parameter must not be empty.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            if (!token.Args.ContainsKey("body"))
            {
                token.AddErrorMessage("You failed to send a 'body' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            string body = token.Args["body"] as string;

            if (string.IsNullOrWhiteSpace(body) || body.Length > 1000)
            {
                token.AddErrorMessage("Your 'body' parameter must not be empty or greater than 1000 characters.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

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
                .BCC(new System.Net.Mail.MailAddress(ServiceManagement.ServiceManager.CurrentConfigState.AtwoodGmailAddress),
                    new System.Net.Mail.MailAddress(ServiceManagement.ServiceManager.CurrentConfigState.McLeanGmailAddress))
                .Subject("Command Central Feedback")
                .HTMLAlternateViewUsingTemplateFromEmbedded("CCServ.Email.Templates.Feedback_HTML.html", model)
                .SendWithRetryAndFailure(TimeSpan.FromSeconds(1));
        }

    }
}
