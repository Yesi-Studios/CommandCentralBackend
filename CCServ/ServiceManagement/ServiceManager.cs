﻿using System;
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

namespace CCServ.ServiceManagement
{
    public static class ServiceManager
    {
        private static WebServiceHost _host = null;

        private static CLI.Options.LaunchOptions _options = null;

        public static void StartService(CLI.Options.LaunchOptions launchOptions)
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
                Log.Warning("It appears the port '{0}' is already in use. We cannot continue from this.");
                Environment.Exit(0);
            }

            //Ok, so now we have a valid port.  Let's set up the service.
            _host = new WebServiceHost(typeof(CommandCentralService), new Uri("http://localhost:" + launchOptions.Port));
            _host.AddServiceEndpoint(typeof(ICommandCentralService), new WebHttpBinding() { MaxBufferPoolSize = 2147483647, MaxReceivedMessageSize = 2147483647, MaxBufferSize = 2147483647, TransferMode = TransferMode.Streamed }, "");
            ServiceDebugBehavior stp = _host.Description.Behaviors.Find<ServiceDebugBehavior>();
            stp.HttpHelpPageEnabled = false;

            _host.Faulted += host_Faulted;

            _host.Open();

            Log.Info("Service is live and listening on '{0}'.".FormatS(_host.BaseAddresses.First().AbsoluteUri));
        }

        public static void StopService()
        {
            _host.Close();
        }

        /// <summary>
        /// If the service faults for any reason.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void host_Faulted(object sender, EventArgs e)
        {
            Log.Warning("The host has entered the faulted state.  Service re-initialization will now be started.");
            StartService(_options);
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

            //Make sure no methods share the same priority
            if (startupMethods.Any(x => x.Count() > 1))
            {
                throw new Exception("One or more start up methods share the same startup priority.");
            }

            //Now run them all in order.
            Log.Info("Executing {0} startup method(s).".FormatS(startupMethods.Count()));
            foreach (var group in startupMethods)
            {
                //We can say first because we know there's only one.
                var info = group.ToList().First();

                Log.Info("Executing startup method {0} with priority {1}.".FormatS(info.Name, info.Priority));
                info.Method(launchOptions);
            }
        }
    }
}