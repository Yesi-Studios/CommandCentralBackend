﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.ServiceModel.Description;
using System.Threading;
using System.ServiceModel.Activation;
using AtwoodUtils;

namespace CommandCentralHost
{
    /// <summary>
    /// Exposes members that assist the hosting of the service.  Namely, the actual WebServiceHost object is stored here along with methods to start and stop it.
    /// </summary>
    public static class ServiceManager
    {
        /// <summary>
        /// This is the WebServiceHost.  It is through this host that the service is exposed.
        /// </summary>
        private static WebServiceHost _host;

        /// <summary>
        /// An accessor that enforces readonly access to the host from outside of this service.
        /// </summary>
        public static WebServiceHost Host
        {
            get
            {
                return _host;
            }
        }

        /// <summary>
        /// Initializes the service host.
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public static void InitializeService()
        {
            bool keepLooping = true;

            while (keepLooping)
            {
                //First the client needs to tell us on what port we're working.
                "On what port would you like to host the service?  Enter a blank line for port 1113...".WL();

                int port;
                string input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                    port = 1113;
                else
                    if (!Int32.TryParse(input, out port))
                    {
                        "That was not a valid port.  Press any key to try again...".WL();
                        Console.ReadKey();
                        Console.Clear();
                        continue;
                    }

                //Make sure the port hasn't been claimed by any other application.
                if (!Utilities.IsPortAvailable(port))
                {
                    "It appears the port '{0}' is already in use.  Would you like to try again (y) or would you like to cancel service start up (any other key)?".F(port).WL();
                    if (Console.ReadLine().ToLower() != "y")
                        keepLooping = false;
                    else
                        Console.Clear();

                    continue;
                }

                //Ok, so now we have a valid port.  Let's set up the service.
                _host = new WebServiceHost(typeof(CommandCentral.ClientAccess.Service.CommandCentralService), new Uri("http://localhost:" + port));
                ServiceEndpoint ep = _host.AddServiceEndpoint(typeof(CommandCentral.ClientAccess.Service.ICommandCentralService), new WebHttpBinding(), "");
                ServiceDebugBehavior stp = _host.Description.Behaviors.Find<ServiceDebugBehavior>();
                stp.HttpHelpPageEnabled = false;

                //Cool, that's done.  Let's also register with the communicator.
                CommandCentral.Communicator.InitializeCommunicator(Console.Out);
                "Communicator Initialized.  Listening to these message priorities:\n\t{0}".F(String.Join(",", CommandCentral.Communicator.ListeningPriorities.Select(x => x.ToString()))).WL();

                //Register a faulted event listener with the host.
                _host.Faulted += _host_Faulted;

                //Tell the client we're done.
                "Service initialized.  Base address is '{0}'.".F(_host.BaseAddresses.First().AbsoluteUri).WL();

                keepLooping = false;
            }
        }

        /// <summary>
        /// If the service faults for any reason.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void _host_Faulted(object sender, EventArgs e)
        {
            "The host has entered the faulted state.  Service re-initialization will now be started.  Press any key to continue...".WL();
            Console.ReadKey();
            _host = null;
            InitializeService();
        }

        /// <summary>
        /// Releases the service and all of its dependencies.
        /// </summary>
        public static void ReleaseService()
        {
            if (_host != null)
            {
                if (_host.State != CommunicationState.Closed)
                {
                    "The host is currently active and listening.  Please stop the service first.".WL();
                }
                else
                {
                    _host = null;
                    CommandCentral.Communicator.ReleaseCommunicator();
                    "The host has been released along with any used memory.".WL();
                }
            }
            else
            {
                "The host has not yet been initialized and therefore can not be released.".WL();
            }
        }

        /// <summary>
        /// Start the service listening.
        /// </summary>
        public static void StartService()
        {
            //If the host has been initialized, open it and then tell the client where we're listening.
            if (_host != null)
            {
                if (_host.State == CommunicationState.Opened)
                    "The host is already open and listening on port, '{0}'.".F(_host.BaseAddresses.First().AbsoluteUri).WL();
                else
                {
                    _host.Open();
                    "Service opened.  Base address is '{0}'.".F(_host.BaseAddresses.First().AbsoluteUri).WL();
                }
            }
            else
            {
                "The host has not yet been initialized. Please consider doing that first.".WL();
            }
        }

        /// <summary>
        /// Stops the service.
        /// </summary>
        public static void StopService()
        {
            if (_host == null)
            {
                if (_host.State == CommunicationState.Closed)
                {
                    "The host has already been closed.".WL();
                }
                else
                {
                    _host.Close();
                    "Host has been closed.".WL();
                }
            }
            else
            {
                "You can't stop the host because it hasn't even been initialized yet!".WL();
            }
        }
    }
}
