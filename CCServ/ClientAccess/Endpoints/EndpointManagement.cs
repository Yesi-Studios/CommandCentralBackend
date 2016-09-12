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
        /// Given the name of an endpoint, switches it from active to inactive or vice versa.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(EndpointName = "SwitchEndpoint", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void EndpointMethod_SwitchEndpoint(MessageToken token)
        {
            //Just make sure the client is logged in.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to manage endpoints.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            //You have permission?
            if (!token.AuthenticationSession.Person.PermissionGroups.CanAccessSubmodules(SubModules.AdminTools.ToString()))
            {
                token.AddErrorMessage("You don't have permission to manage endpoints - you must be a developer.", ErrorTypes.Authorization, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            //Ok, the client has permission to manage endpoints, let's see what endpoint they're talking about.
            if (!token.Args.ContainsKey("endpoint"))
            {
                token.AddErrorMessage("You failed to send an 'endpoint' parameter!", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            string endpoint = token.Args["endpoint"] as string;

            ServiceEndpoint serviceEndpoint;
            if (!ServiceManagement.ServiceManager.EndpointDescriptions.TryGetValue(endpoint, out serviceEndpoint))
            {
                token.AddErrorMessage("The endpoint parameter you sent was not a real endpoint.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            //Since we got a real endpoint let's go ahead and switch it.
            serviceEndpoint.IsActive = !serviceEndpoint.IsActive;

            //Now give the client the state of all endpoints.
            token.SetResult(ServiceManagement.ServiceManager.EndpointDescriptions.Select(x => new
            {
                x.Value.EndpointMethodAttribute.EndpointName,
                x.Value.IsActive
            }).ToList());

            Log.Critical("The service endpoint, '{0}', was switched to the state, '{1}', by {2}.".FormatS(serviceEndpoint.EndpointMethodAttribute.EndpointName, serviceEndpoint.IsActive, token.AuthenticationSession.Person.ToString()));
        }
    }
}
