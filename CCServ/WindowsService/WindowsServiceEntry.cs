using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;

namespace CommandCentral
{
    /// <summary>
    /// Contains the windows service entry.  This is akin to the Program class for the interactive version.
    /// </summary>
    public partial class WindowsServiceEntry : ServiceBase
    {
        /// <summary>
        /// The options with which the service was launched.  Important so that we can do restarts.
        /// </summary>
        private static CLI.Options.LaunchOptions _launchOptions;

        /// <summary>
        /// Initializes the windows service.  This is called when the system first creates our service.
        /// </summary>
        public WindowsServiceEntry()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Invoked by the system when the service is started.
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            var options = new CLI.Options.LaunchOptions();

            if (args == null || !args.Any() || !CommandLine.Parser.Default.ParseArguments(args, options))
            {
                //In the event that parsing fails we need to throw an exception, because we'll be in the non-interactive state.
                throw new Exception("Failed to parse arguments...");
                    
            }

            _launchOptions = options;

            ServiceManagement.ServiceManager.StartService(_launchOptions);
        }

        /// <summary>
        /// Fires when the service manager instructs our service to stop.
        /// </summary>
        protected override void OnStop()
        {
            ServiceManagement.ServiceManager.StopService();
        }

        /// <summary>
        /// When the service manager instructs the service to shutdown.
        /// </summary>
        protected override void OnShutdown()
        {
            ServiceManagement.ServiceManager.StopService();
        }

        /// <summary>
        /// When our service is paused.
        /// </summary>
        protected override void OnPause()
        {
            ServiceManagement.ServiceManager.StopService();
        }

        /// <summary>
        /// The service was paused... and then continued, this fires.
        /// </summary>
        protected override void OnContinue()
        {
            ServiceManagement.ServiceManager.StartService(_launchOptions);
        }
    }
}

