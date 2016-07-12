using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandCentral.ClientAccess;
using System.Reflection;
using AtwoodUtils;
using System.Linq.Expressions;
using System.IO;

namespace CommandCentral.ServiceManagement
{
    /// <summary>
    /// Manages service start up and operation.
    /// </summary>
    public static class ServiceManager
    {
        /// <summary>
        /// The endpoint descriptions describe what endpoints we're exposing.
        /// </summary>
        public static ConcurrentDictionary<string, ServiceEndpoint> EndpointDescriptions { get; private set; }

        /// <summary>
        /// Initializes the service.
        /// </summary>
        /// <param name="writer">The text writer to which messages from the service should be written.</param>
        public static void InitializeService(TextWriter writer)
        {
            if (!DataAccess.NHibernateHelper.IsInitialized)
                throw new Exception("Please select a database to connect to before starting the service.");

            //Initialize the communicator first so that everyone else can use it.
            Communicator.InitializeCommunicator(writer);

            SetupEndpoints();

            RunStartupMethods();

        }

        /// <summary>
        /// Scans the entire executing assembly for any service endpoint methods and reads them into a dictionary.
        /// </summary>
        /// <returns></returns>
        private static void SetupEndpoints()
        {
            Communicator.PostMessageToHost("Scanning for endpoint methods.", Communicator.MessageTypes.Informational);

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
                            ExampleOutput = "TODO",
                            IsActive = true
                        };
                    }).ToDictionary(x => x.EndpointMethodAttribute.EndpointName, StringComparer.OrdinalIgnoreCase);

            Communicator.PostMessageToHost("Found {0} endpoint methods.".FormatS(endpoints.Count), Communicator.MessageTypes.Informational);

            EndpointDescriptions = new ConcurrentDictionary<string, ServiceEndpoint>(endpoints, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Scans the entire executing assembly for any methods that want to be run at service start up.
        /// </summary>
        /// <returns></returns>
        private static void RunStartupMethods()
        {
            Communicator.PostMessageToHost("Scanning for startup methods.", Communicator.MessageTypes.Informational);

            //Scan for all start up methods.
            var startupMethods = Assembly.GetExecutingAssembly().GetTypes()
                    .SelectMany(x => x.GetMethods(BindingFlags.NonPublic | BindingFlags.Static))
                    .Where(x => x.GetCustomAttribute<StartMethodAttribute>() != null)
                    .Select(x =>
                    {
                        //Make sure all start up methods follow the same pattern.
                        if (x.ReturnType != typeof(void) || x.GetParameters().Length != 0)
                            throw new ArgumentException("The method, '{0}', in the type, '{1}', does not match the signature of a startup method!".FormatS(x.Name, x.DeclaringType.Name));

                        //Create the method's call and compile it.
                        var parameters = x.GetParameters()
                           .Select(p => Expression.Parameter(p.ParameterType, p.Name))
                           .ToArray();
                        var call = Expression.Call(null, x, parameters);
                        var startupMethod = (Action)Expression.Lambda(call, parameters).Compile();


                        var startupMethodAttribute = x.GetCustomAttribute<StartMethodAttribute>();

                        return new
                        {
                            Method = startupMethod,
                            Priority = startupMethodAttribute.Priority,
                            Name = x.Name
                        };

                    }).GroupBy(x => x.Priority).OrderByDescending(x => x.Key);

            //Make sure no methods share the same priority
            if (startupMethods.Any(x => x.Count() > 1))
            {
                throw new Exception("One or more start up methods share the same startup priority.");
            }

            //Now run them all in order.
            Communicator.PostMessageToHost("Executing {0} startup method(s).".FormatS(startupMethods.Count()), Communicator.MessageTypes.Informational);
            foreach (var group in startupMethods)
            {
                //We can say first because we know there's only one.
                var info = group.ToList().First();

                info.Method();
                Communicator.PostMessageToHost("Executed startup method {0} with priority {1}.".FormatS(info.Name, info.Priority), Communicator.MessageTypes.Informational);
            }
        }
    }
}
