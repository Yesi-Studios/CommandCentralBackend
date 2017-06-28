using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandCentral.Authorization;

namespace CommandCentral.ClientAccess.Endpoints
{
    /// <summary>
    /// Contains all the endpoints for the API Keys.
    /// </summary>
    static class APIKeyEndpoints
    {
        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Returns all API keys and the application names that correspond with them.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void LoadAPIKeys(MessageToken token)
        {
            token.AssertLoggedIn();

            //You have permission?
            if (!token.AuthenticationSession.Person.PermissionGroups.CanAccessSubmodules(SubModules.AdminTools.ToString()))
                throw new CommandCentralException("You don't have permission to view API keys - you must be a developer.", ErrorTypes.Authorization);

            //Client has permission, show them the api keys and the names.
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                token.SetResult(session.QueryOver<APIKey>().List());
            }
        }
    }
}
