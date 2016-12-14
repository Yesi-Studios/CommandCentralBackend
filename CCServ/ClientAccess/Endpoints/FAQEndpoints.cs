using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;
using CCServ.Authorization;
using CCServ.Entities;
using Humanizer;
using NHibernate.Criterion;

namespace CCServ.ClientAccess.Endpoints
{
    /// <summary>
    /// All the FAQ endpoints.
    /// </summary>
    static class FAQEndpoints
    {
        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Loads a single FAQ for the given Id or returns null if it does not exist.
        /// <para />
        /// Client Parameters: <para />
        ///     faqid - The Id of the FAQ we want to load.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(EndpointName = "LoadFAQ", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = false)]
        private static void EndpointMethod_LoadFAQ(MessageToken token)
        {
            //Get the FAQ Id we're supposed to load.
            if (!token.Args.ContainsKey("faqid"))
            {
                token.AddErrorMessage("You didn't send an 'faqid' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            //Get the Id.
            Guid faqId;
            if (!Guid.TryParse(token.Args["faqid"] as string, out faqId))
            {
                token.AddErrorMessage("The faqid parameter was not in the correct format.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            //We passed validation, let's get a sesssion and do ze work.
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {

                    //Ok, well it's a GUID.   Do we have it in the database?...
                    var faq = session.Get<FAQ>(faqId);

                    //Unlike News Items, we don't need a DTO cause we don't have any transformations to do.  We'll just hand back exactly what we loaded.
                    token.SetResult(faq);

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Loads all FAQs.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(EndpointName = "LoadFAQs", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = false)]
        private static void EndpointMethod_LoadFAQs(MessageToken token)
        {
            //We passed validation, let's get a sesssion and do ze work.
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    var faqs = session.QueryOver<FAQ>();

                    token.SetResult(faqs);

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Creates an FAQ.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(EndpointName = "CreateFAQ", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void EndpointMethod_CreateFAQ(MessageToken token)
        {
            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to create an FAQ.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            //Make sure the client has permission to manage the FAQ.
            if (!token.AuthenticationSession.Person.PermissionGroups.CanAccessSubmodules(SubModules.EditFAQ.ToString()))
            {
                token.AddErrorMessage("You do not have permission to manage the FAQ.", ErrorTypes.Authorization, System.Net.HttpStatusCode.Unauthorized);
            }

            if (!token.Args.ContainsKey("faq"))
            {
                token.AddErrorMessage("You failed to send an 'faq' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            FAQ faq;

            try
            {
                faq = token.Args["faq"].CastJToken<FAQ>();
            }
            catch (Exception e)
            {
                token.AddErrorMessage("There was an error while trying to parse the FAQ you sent.  Error: {0}".FormatWith(e.Message), ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            //Ok now we know we have the FAQ.  Now let's see if it's valid.
            var validationResult = new FAQ.FAQValidator().Validate(faq);

            if (!validationResult.IsValid)
            {
                token.AddErrorMessages(validationResult.Errors.Select(x => x.ErrorMessage), ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            //Well it looks like the faq is valid.  Now we need to reset the Id and then go into a sesion to test the uniqueness of the name and the question.
            faq.Id = Guid.NewGuid();

            //We passed validation, let's get a sesssion and do ze work.
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    //First, let's make sure the name and the question are both unique.
                    var results = session.QueryOver<FAQ>().Where(x => x.Name.IsInsensitiveLike(faq.Name) || x.Question.IsInsensitiveLike(faq.Question)).RowCount();

                    if (results != 0)
                    {
                        token.AddErrorMessage("That name or that question have already been used.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        return;
                    }

                    //Cool!  We know that we have a unique thing now.
                    //We've already reset the Id, so from here, we're good to insert it.
                    session.Save(faq);

                    //Finally, send the Id back to the client.
                    token.SetResult(faq.Id);

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
    }
}
