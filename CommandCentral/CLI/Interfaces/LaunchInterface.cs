using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;
using CommandCentral.ServiceManagement.Service;

namespace CommandCentral.CLI.Interfaces
{
    public static class LaunchInterface
    {
        private static Options.LaunchOptions _options = null;

        public static void Launch(Options.LaunchOptions launchOptions)
        {
            _options = launchOptions;

            //Let's determine if our given port is usable.
            //Make sure the port hasn't been claimed by any other application.
            if (!Utilities.IsPortAvailable(launchOptions.Port))
            {
                "It appears the port '{0}' is already in use.  Please try a different port.".FormatS(launchOptions.Port).WriteLine();
                Environment.Exit(0);
            }

            //Ok, so now we have a valid port.  Let's set up the service.
            using (WebServiceHost host = new WebServiceHost(typeof(CommandCentralService), new Uri("http://localhost:" + launchOptions.Port)))
            {
                host.AddServiceEndpoint(typeof(ICommandCentralService), new WebHttpBinding() { MaxBufferPoolSize = 2147483647, MaxReceivedMessageSize = 2147483647, MaxBufferSize = 2147483647, TransferMode = TransferMode.Streamed }, "");
                ServiceDebugBehavior stp = host.Description.Behaviors.Find<ServiceDebugBehavior>();
                stp.HttpHelpPageEnabled = false;

                //Tell the service to initialize itself.
                ServiceManagement.ServiceManager.InitializeService(launchOptions);

                host.Faulted += host_Faulted;

                host.Open();

                "Press enter to shutdown service...".WriteLine();
                Console.ReadLine();
            }

        }

        /// <summary>
        /// If the service faults for any reason.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void host_Faulted(object sender, EventArgs e)
        {
            CommandCentral.Communicator.PostMessage("The host has entered the faulted state.  Service re-initialization will now be started.", CommandCentral.Communicator.MessageTypes.Critical);
            Launch(_options);
        }
    }
}
