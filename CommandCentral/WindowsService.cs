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

namespace CommandCentral
{
    public class WindowsService : ServiceBase
    {
        CLI.Options.LaunchOptions _launchOptions;

        public WindowsService()
        {
            ServiceName = "CCServ Windows Service v{0}".FormatS(Config.Version.RELEASE_VERSION);
        }

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
