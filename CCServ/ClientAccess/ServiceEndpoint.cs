using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AtwoodUtils;
using CCServ.Authorization;
using CCServ.Logging;

namespace CCServ.ClientAccess
{
    /// <summary>
    /// Provides members that describe an endpoint.  Intended to allow dynamic endpoint invocation.
    /// </summary>
    public class ServiceEndpoint
    {

        /// <summary>
        /// The list of all endpoints contained throughout the application.
        /// </summary>
        public static ConcurrentDictionary<string, ServiceEndpoint> EndpointDescriptions { get; private set; }

        #region Properties

        /// <summary>
        /// The data method that this endpoint will use to retrieve its data.  All data methods must take a message token.
        /// </summary>
        public Action<MessageToken> EndpointMethod { get; set; }

        /// <summary>
        /// Indicates whether or not calls to the endpoint should be allowed.  This will allow us to disable endpoints for maintenance if needed.
        /// </summary>
        public bool IsActive { get; set; }
        
        /// <summary>
        /// The endpoint method's attribute which describes information about it.
        /// </summary>
        public EndpointMethodAttribute EndpointMethodAttribute { get; set; }

        #endregion

        #region Startup Methods

        /// <summary>
        /// Scans the entire executing assembly for any service endpoint methods and reads them into a dictionary.
        /// </summary>
        /// <returns></returns>
        [ServiceManagement.StartMethod(Priority = 4)]
        private static void SetupEndpoints(CLI.Options.LaunchOptions launchOptions)
        {
            Log.Info("Scanning for endpoint methods.");

            var endpoints = Assembly.GetExecutingAssembly().GetTypes()
                    .SelectMany(x => x.GetMethods(BindingFlags.NonPublic | BindingFlags.Static))
                    .Where(x => x.GetCustomAttribute<EndpointMethodAttribute>() != null)
                    .Select(x =>
                    {
                        if (x.ReturnType != typeof(void) || x.GetParameters().Length != 1 || x.GetParameters()[0].ParameterType != typeof(MessageToken))
                            throw new ArgumentException("The method, '{0}', in the type, '{1}', does not match the signature of an endpoint method!".FormatS(x.Name, x.DeclaringType.Name));

                        var parameters = x.GetParameters()
                           .Select(p => Expression.Parameter(p.ParameterType, p.Name))
                           .ToArray();
                        var call = Expression.Call(null, x, parameters);
                        var endpointMethod = (Action<MessageToken>)Expression.Lambda(call, parameters).Compile();

                        var endpointMethodAttribute = x.GetCustomAttribute<EndpointMethodAttribute>();
                        if (string.IsNullOrWhiteSpace(endpointMethodAttribute.EndpointName))
                            endpointMethodAttribute.EndpointName = x.Name;

                        return new ServiceEndpoint
                        {
                            EndpointMethod = endpointMethod,// lambda.Compile(),
                            EndpointMethodAttribute = endpointMethodAttribute,
                            IsActive = true
                        };
                    }).ToDictionary(x => x.EndpointMethodAttribute.EndpointName, StringComparer.OrdinalIgnoreCase);

            Log.Info("Found {0} endpoint methods.".FormatS(endpoints.Count));

            EndpointDescriptions = new ConcurrentDictionary<string, ServiceEndpoint>(endpoints, StringComparer.OrdinalIgnoreCase);
        }

        #endregion

        #region Client Methods

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Returns all endpoints being served by the service.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(EndpointName = "GetEndpoints", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void EndpointMethod_LoadEndpoints(MessageToken token)
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

            token.SetResult(EndpointDescriptions.Select(x => new
            {
                x.Value.EndpointMethodAttribute.EndpointName,
                x.Value.IsActive
            }).ToList());
        }

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
            if (!EndpointDescriptions.TryGetValue(endpoint, out serviceEndpoint))
            {
                token.AddErrorMessage("The endpoint parameter you sent was not a real endpoint.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            //Since we got a real endpoint let's go ahead and switch it.
            serviceEndpoint.IsActive = !serviceEndpoint.IsActive;

            //Now give the client the state of all endpoints.
            token.SetResult(EndpointDescriptions.Select(x => new
            {
                x.Value.EndpointMethodAttribute.EndpointName,
                x.Value.IsActive
            }).ToList());

            //TODO send an email to the devs informing them of the state change of an endpoint.
        }

        #endregion

    }
}
