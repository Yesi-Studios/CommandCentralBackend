using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.ClientAccess.Endpoints
{
    /// <summary>
    /// Contains all the endpoints relating to change events.
    /// </summary>
    static class ChangeEventEndpoints
    {

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Returns all the change events.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void GetChangeEvents(MessageToken token)
        {
            token.AssertLoggedIn();

            token.SetResult(ChangeEventSystem.ChangeEventHelper.AllChangeEvents);
        }

    }
}
