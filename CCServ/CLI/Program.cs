using System;
using AtwoodUtils;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Runtime.InteropServices;
using CCServ.CLI.Options;
using System.ServiceProcess;

namespace CCServ.CLI
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
            try
            {
                if (!Environment.UserInteractive)
                    // running as service
                    using (var service = new Service1())
                        ServiceBase.Run(service);
                else
                {
                    // running as console app
                    Start(args);

                    Console.WriteLine("Press enter key to stop...");
                    Console.ReadLine();

                    Stop();
                }
            }
            catch (Exception e)
            {
                e.ToString().WriteLine();

                Console.WriteLine("Press any key to close...");
                Console.ReadKey(true);
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
                "Bad things...".WriteLine();
                Console.ReadLine();
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
                case "install":
                    {
                        var installOptions = (InstallOptions)invokedVerbInstance;
                        ServiceInstaller.InstallService(System.Reflection.Assembly.GetExecutingAssembly().Location, installOptions.ServiceName, installOptions.ServiceDisplayName);
                        break;
                    }
                case "uninstall":
                    {
                        var uninstallOptions = (UninstallOptions)invokedVerbInstance;
                        ServiceInstaller.UnInstallService(uninstallOptions.ServiceName);
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
