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
        /// </summary>
        /// <param name="token"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = false)]
        private static void LoadFAQ(MessageToken token, DTOs.FAQEndpoints.LoadFAQ dto)
        {
            //We passed validation, let's get a sesssion and do ze work.
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    //Ok, well it's a GUID.   Do we have it in the database?...
                    var faq = session.Get<FAQ>(dto.Id);

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
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = false)]
        private static void LoadFAQs(MessageToken token)
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
        /// <param name="dto"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void CreateOrUpdateFAQ(MessageToken token, DTOs.FAQEndpoints.CreateOrUpdateFAQ dto)
        {
            token.AssertLoggedIn();

            //Make sure the client has permission to manage the FAQ.
            if (!token.AuthenticationSession.Person.PermissionGroups.CanAccessSubmodules(SubModules.EditFAQ.ToString()))
                throw new CommandCentralException("You do not have permission to manage the FAQ.", ErrorTypes.Validation);

            //We passed validation, let's get a sesssion and do ze work.
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    //First, let's try to load the FAQ item.  If it doesn't exist we need to insert it.  If it does exist, we need to update it.
                    var faqFromDB = session.Get<FAQ>(dto.FAQ.Id);

                    //If null, then create
                    if (faqFromDB == null)
                    {
                        //Ok, let's create it.  That's easy, just reassign the Id, save, then return the Id.
                        dto.FAQ.Id = Guid.NewGuid();

                        session.Save(dto.FAQ);
                    }
                    else //else, upate.
                    {
                        //Ok to update we need to ensure the object is unique, then merge it.
                        //Here we ask for any FAQ where the question or name equals the desired one which is not the current one's Id.
                        var resultsCount = session.QueryOver<FAQ>()
                            .Where(x => (x.Name.IsInsensitiveLike(dto.FAQ.Name) || x.Question.IsInsensitiveLike(dto.FAQ.Question)) && x.Id != dto.FAQ.Id)
                            .RowCount();

                        if (resultsCount != 0)
                            throw new CommandCentralException("It appears as though an FAQ with that name or question already exists.", ErrorTypes.Validation);

                        //Ok we're good on duplicates.  Now we can merge the FAQ.
                        session.Merge(dto.FAQ);
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
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void DeleteFAQ(MessageToken token, DTOs.FAQEndpoints.DeleteFAQ dto)
        {
            token.AssertLoggedIn();

            //Make sure the client has permission to manage the FAQ.
            if (!token.AuthenticationSession.Person.PermissionGroups.CanAccessSubmodules(SubModules.EditFAQ.ToString()))
                throw new CommandCentralException("You do not have permission to manage the FAQ.", ErrorTypes.Validation);

            //We can head right into the session since we're going to delete this FAQ.
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    var faqFromDB = session.Get<FAQ>(dto.Id) ??
                        throw new CommandCentralException("A faq with that Id was not found in the database.", ErrorTypes.Validation);

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
