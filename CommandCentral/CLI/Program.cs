﻿using System;
using AtwoodUtils;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Runtime.InteropServices;
using CommandCentral.CLI.Options;
using System.ServiceProcess;

namespace CommandCentral.CLI
{
    /// <summary>
    /// Main Class
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Entry point.
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            if (!Environment.UserInteractive)
                // running as service
                using (var service = new WindowsService())
                    ServiceBase.Run(service);
            else
            {
                // running as console app
                Start(args);

                Console.WriteLine("Press any key to stop...");
                Console.ReadKey(true);

                Stop();
            }
        }

        private static void Start(string[] args)
        {
            var options = new MainOptions();

            string invokedVerb = null;
            object invokedVerbInstance = null;
            if (args == null || !args.Any() || !CommandLine.Parser.Default.ParseArguments(args, options,
                (verb, subOptions) =>
                {
                    //If parsing succeeds the verb name and correct instance
                    //will be passed to onVerbCommand delegate (string,object)
                    invokedVerb = verb;
                    invokedVerbInstance = subOptions;
                }))
            {
                Environment.Exit(CommandLine.Parser.DefaultExitCodeFail);
            }

            //Ok parsing succeeded, let's find out what verb we have.
            switch (invokedVerb)
            {
                case "launch":
                    {
                        ServiceManagement.ServiceManager.StartService((LaunchOptions)invokedVerbInstance);
                        break;
                    }
                default:
                    {
                        "Fell to default statement in verb switch.".WriteLine();
                        break;
                    }
            }
        }

        private static void Stop()
        {
        }

        public static void ParseAndRouteArguments(string[] args)
        {
            
        }
    }

}
