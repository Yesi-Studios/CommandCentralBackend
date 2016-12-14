using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CCServ.Entities;

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
    }
}
