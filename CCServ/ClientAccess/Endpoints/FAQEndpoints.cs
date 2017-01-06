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
                    var faqs = session.QueryOver<FAQ>().List();

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
        /// Creates or updates an FAQ.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(EndpointName = "CreateOrUpdateFAQ", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void EndpointMethod_CreateOrUpdateFAQ(MessageToken token)
        {
            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to create or update an FAQ.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
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

            FAQ faqFromClient;

            try
            {
                faqFromClient = token.Args["faq"].CastJToken<FAQ>();
            }
            catch (Exception e)
            {
                token.AddErrorMessage("There was an error while trying to parse the FAQ you sent.  Error: {0}".FormatWith(e.Message), ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            //Ok now we know we have the FAQ.  Now let's see if it's valid.
            var validationResult = new FAQ.FAQValidator().Validate(faqFromClient);

            if (!validationResult.IsValid)
            {
                token.AddErrorMessages(validationResult.Errors.Select(x => x.ErrorMessage), ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            //We passed validation, let's get a sesssion and do ze work.
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    //First, let's try to load the FAQ item.  If it doesn't exist we need to insert it.  If it does exist, we need to update it.
                    var faqFromDB = session.Get<FAQ>(faqFromClient.Id);

                    //If null, then create
                    if (faqFromDB == null)
                    {
                        //Ok, let's create it.  That's easy, just reassign the Id, save, then return the Id.
                        faqFromClient.Id = Guid.NewGuid();

                        session.Save(faqFromClient);

                        token.SetResult(faqFromClient);
                    }
                    else //else, upate.
                    {
                        //Ok to update we need to ensure the object is unique, then merge it.
                        //Here we ask for any FAQ where the question or name equals the desired one which is not the current one's Id.
                        var resultsCount = session.QueryOver<FAQ>().Where(x => (x.Name.IsInsensitiveLike(faqFromClient.Name) || x.Question.IsInsensitiveLike(faqFromClient.Question)) && x.Id != faqFromClient.Id).RowCount();

                        if (resultsCount != 0)
                        {
                            token.AddErrorMessage("It appears as though an FAQ with that name or question already exists.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                            return;
                        }

                        //Ok we're good on duplicates.  Now we can merge the FAQ.
                        session.Merge(faqFromClient);

                        //Now let's return it.  May as well.
                        token.SetResult(faqFromClient);
                    }

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
        /// Deletes an FAQ.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(EndpointName = "DeleteFAQ", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void EndpointMethod_DeleteFAQ(MessageToken token)
        {
            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to manage the FAQ.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            //Make sure the client has permission to manage the FAQ.
            if (!token.AuthenticationSession.Person.PermissionGroups.CanAccessSubmodules(SubModules.EditFAQ.ToString()))
            {
                token.AddErrorMessage("You do not have permission to manage the FAQ.", ErrorTypes.Authorization, System.Net.HttpStatusCode.Unauthorized);
            }

            //Let's see if the parameters are here.
            if (!token.Args.ContainsKey("faq"))
            {
                token.AddErrorMessage("You didn't send an 'faq' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            //Get the faq
            FAQ faqFromClient;

            try
            {
                faqFromClient = token.Args["faq"].CastJToken<FAQ>();
            }
            catch (Exception e)
            {
                token.AddErrorMessage("There was an error while trying to parse the FAQ you sent.  Error: {0}".FormatWith(e.Message), ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            //We can head right into the session since we're going to delete this FAQ.
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    var faqFromDB = session.Get<FAQ>(faqFromClient.Id);

                    if (faqFromDB == null)
                    {
                        token.AddErrorMessage("A faq with that Id was not found in the database.", ErrorTypes.Validation, System.Net.HttpStatusCode.NotFound);
                        return;
                    }

                    //Everything is good to go.  Let's delete it.
                    session.Delete(faqFromDB);

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    return;
                }
            }
        }

    }
}
