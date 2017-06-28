using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AtwoodUtils;
using CommandCentral.Authorization;
using CommandCentral.Logging;

namespace CommandCentral.ClientAccess
{
    /// <summary>
    /// Provides members that describe an endpoint.  Intended to allow dynamic endpoint invocation.
    /// </summary>
    public class ServiceEndpoint
    {

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
        private static void ScanEndpoints(CLI.Options.LaunchOptions launchOptions)
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

                        Log.Info("Found endpoint : {0}".FormatS(endpointMethodAttribute.EndpointName));

                        return new ServiceEndpoint
                        {
                            EndpointMethod = endpointMethod,// lambda.Compile(),
                            EndpointMethodAttribute = endpointMethodAttribute,
                            IsActive = true
                        };
                    });

            var groupings = endpoints.GroupBy(x => x.EndpointMethodAttribute.EndpointName);

            if (groupings.Any(x => x.Count() != 1))
            {
                throw new Exception("Two endpoints may not be named the same thing.  Endpoints with multiple entries: {0}.".FormatS(String.Join(", ", groupings.Where(x => x.Count() != 1).Select(x => x.Key))));
            }
            
            var finalEndpoints = endpoints.ToDictionary(x => x.EndpointMethodAttribute.EndpointName, StringComparer.OrdinalIgnoreCase);

            Log.Info("Found {0} endpoint methods.".FormatS(finalEndpoints.Count));

            ServiceManagement.ServiceManager.EndpointDescriptions = new ConcurrentDictionary<string, ServiceEndpoint>(finalEndpoints, StringComparer.OrdinalIgnoreCase);
        }

        #endregion

    }
}
