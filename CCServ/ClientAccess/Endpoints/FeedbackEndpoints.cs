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
    static class FeedbackEndpoints
    {
        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Receives a feedback from the client and then emails it to the developers.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [EndpointMethod(AllowResponseLogging = true, AllowArgumentLogging = true, RequiresAuthentication = true)]
        private static void SubmitFeedback(MessageToken token, DTOs.FeedbackEndpoints.SubmitFeedback dto)
        {
            token.AssertLoggedIn();

            var model = new Email.Models.FeedbackEmailModel
            {
                Body = dto.Body,
                Title = dto.Title,
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
                .CC(Email.EmailInterface.CCEmailMessage.DeveloperAddress)
                .BCC(Email.EmailInterface.CCEmailMessage.PersonalDeveloperAddresses)
                .Subject("Command Central Feedback")
                .HTMLAlternateViewUsingTemplateFromEmbedded("CCServ.Email.Templates.Feedback_HTML.html", model)
                .SendWithRetryAndFailure(TimeSpan.FromSeconds(1));
        }
    }
}
