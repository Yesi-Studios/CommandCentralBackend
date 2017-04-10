using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AtwoodUtils;
using CCServ.Authorization;
using CCServ.Logging;

namespace CCServ.ClientAccess.Endpoints
{
    static class EndpointManagement
    {
        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Returns the names of all endpoints.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = false)]
        static void GetEndpoints(MessageToken token)
        {
            token.SetResult(ServiceManagement.ServiceManager.EndpointDescriptions.Keys.ToList());
        }
    }
}
