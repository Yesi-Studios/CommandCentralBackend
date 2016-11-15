using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;
using CCServ.Logging;
using CCServ.ServiceManagement;
using CCServ.ServiceManagement.Service;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CCServ.ServiceManagement
{
    /// <summary>
    /// Contains the actual web host instance and manages stopping and starting the service.
    /// </summary>
    public static class ServiceManager
    {
        private static WebServiceHost _host = null;

        private static CLI.Options.LaunchOptions _options = null;

        /// <summary>
        /// The list of all endpoints contained throughout the application.
        /// </summary>
        public static ConcurrentDictionary<string, ClientAccess.ServiceEndpoint> EndpointDescriptions { get; set; }

        /// <summary>
        /// The current config state that should be used by the application.
        /// </summary>
        public static ConfigState CurrentConfigState { get; set; }

        /// <summary>
        /// Starts the service with the given parameters.  By the end of this method, the application will be listening on the assigned port or it will fail.
        /// </summary>
        /// <param name="launchOptions"></param>
        public static void StartService(CLI.Options.LaunchOptions launchOptions)
        {
            try
            {
                _options = launchOptions;

                Log.Info("Starting service startup...");

                //Now we need to run all start up methods.
                RunStartupMethods(launchOptions);

                //All startup methods have run, now we need to launch the service itself.

                //Let's determine if our given port is usable.
                //Make sure the port hasn't been claimed by any other application.
                if (!Utilities.IsPortAvailable(launchOptions.Port))
                {
                    Log.Critical("It appears the port '{0}' is already in use. We cannot continue from this.");
                    Environment.Exit(0);
                }

                //Ok, so now we have a valid port.  Let's set up the service.
                if (launchOptions.SecurityMode == CLI.SecurityModes.HTTPSOnly || launchOptions.SecurityMode == CLI.SecurityModes.Both)
                {
                    _host = new WebServiceHost(typeof(CommandCentralService), new Uri("https://localhost:" + launchOptions.Port));
                    _host.AddServiceEndpoint(typeof(ICommandCentralService), new WebHttpBinding() { Security = new WebHttpSecurity { Mode = WebHttpSecurityMode.Transport }, MaxBufferPoolSize = 2147483647, MaxReceivedMessageSize = 2147483647, MaxBufferSize = 2147483647, TransferMode = TransferMode.Streamed }, "");
                    ServiceDebugBehavior stp = _host.Description.Behaviors.Find<ServiceDebugBehavior>();
                    stp.HttpHelpPageEnabled = false;
                }
                else
                {
                    _host = new WebServiceHost(typeof(CommandCentralService), new Uri("http://localhost:" + launchOptions.Port));
                    _host.AddServiceEndpoint(typeof(ICommandCentralService), new WebHttpBinding() { Security = new WebHttpSecurity { Mode = WebHttpSecurityMode.None }, MaxBufferPoolSize = 2147483647, MaxReceivedMessageSize = 2147483647, MaxBufferSize = 2147483647, TransferMode = TransferMode.Streamed }, "");
                    ServiceDebugBehavior stp = _host.Description.Behaviors.Find<ServiceDebugBehavior>();
                    stp.HttpHelpPageEnabled = false;
                }
                
                _host.Open();

                Log.Info("Service is live and listening on '{0}'.".FormatS(_host.BaseAddresses.First().AbsoluteUri));
            }
            catch (Exception e)
            {
                Log.Exception(e, "An error occurred during service start up");
            }
        }

        /// <summary>
        /// Closes the host.
        /// </summary>
        public static void StopService()
        {
            //Before we shut down the service, we need to flush the current config to the config file.
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "config.txt");

            File.WriteAllText(path, JsonConvert.SerializeObject(CurrentConfigState,
                    new JsonSerializerSettings
                    {
                        Converters = new List<JsonConverter> { new StringEnumConverter { CamelCaseText = false } },
                        ContractResolver = new AtwoodUtils.SerializationSettings.NHibernateContractResolver(),
                        Formatting = Formatting.Indented,
                        DateFormatString = "yyyy-MM-ddTHH:mm:ss.sssZ",
                        DateTimeZoneHandling = DateTimeZoneHandling.Utc
                    }));

            if (_host != null && _host.State != CommunicationState.Closed)
                _host.Close();
        }

        /// <summary>
        /// Scans the entire executing assembly for any methods that want to be run at service start up.
        /// </summary>
        /// <returns></returns>
        private static void RunStartupMethods(CLI.Options.LaunchOptions launchOptions)
        {
            Log.Info("Scanning for startup methods.");

            //Scan for all start up methods.
            var startupMethods = Assembly.GetExecutingAssembly().GetTypes()
                    .SelectMany(x => x.GetMethods(BindingFlags.NonPublic | BindingFlags.Static))
                    .Where(x => x.GetCustomAttribute<StartMethodAttribute>() != null)
                    .Select(x =>
                    {
                        //Make sure all start up methods follow the same pattern.
                        if (x.ReturnType != typeof(void) || x.GetParameters().Length != 1 || x.GetParameters()[0].ParameterType != typeof(CLI.Options.LaunchOptions))
                            throw new ArgumentException("The method, '{0}', in the type, '{1}', does not match the signature of a startup method!".FormatS(x.Name, x.DeclaringType.Name));

                        //Create the method's call and compile it.
                        var parameters = x.GetParameters()
                           .Select(p => Expression.Parameter(p.ParameterType, p.Name))
                           .ToArray();
                        var call = Expression.Call(null, x, parameters);
                        var startupMethod = (Action<CLI.Options.LaunchOptions>)Expression.Lambda(call, parameters).Compile();


                        var startupMethodAttribute = x.GetCustomAttribute<StartMethodAttribute>();

                        return new
                        {
                            Method = startupMethod,
                            Priority = startupMethodAttribute.Priority,
                            Name = x.Name
                        };

                    }).GroupBy(x => x.Priority).OrderByDescending(x => x.Key);

            //Now run them all in order.  We allow multiple of the same priority, in which case they will execute in a random order within that priority.
            Log.Info("Executing {0} startup method(s). ({1})".FormatS(startupMethods.Count(), String.Join(", ", startupMethods.Select(x => x.ToList().First().Priority))));

            //Write to the console here because the logging system hasn't been set up yet.
            Console.WriteLine("Executing {0} startup method(s). ({1})".FormatS(startupMethods.Count(), String.Join(", ", startupMethods.Select(x => x.ToList().First().Priority))));
            foreach (var group in startupMethods)
            {
                //We can say first because we know there's only one.
                var infos = group.ToList();

                Log.Info("Executing startup method(s) {0} with priority {1}.".FormatS(String.Join(", ", infos.Select(x => x.Name)), group.Key));
                
                infos.ForEach(x => x.Method(launchOptions));
            }
        }
    }
}
