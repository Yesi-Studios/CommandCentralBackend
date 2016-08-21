﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;

namespace CCServ
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

            ServiceName = "CCSERV";
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
                Environment.Exit(CommandLine.Parser.DefaultExitCodeFail);
            }

            _launchOptions = options;

            ServiceManagement.ServiceManager.StartService(_launchOptions);
        }

        protected override void OnStop()
        {
            ServiceManagement.ServiceManager.StopService();
        }

        protected override void OnShutdown()
        {
            ServiceManagement.ServiceManager.StopService();
        }

        protected override void OnPause()
        {
            ServiceManagement.ServiceManager.StopService();
        }

        protected override void OnContinue()
        {
            ServiceManagement.ServiceManager.StartService(_launchOptions);
        }
    }
}

