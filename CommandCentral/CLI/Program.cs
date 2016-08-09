using System;
using AtwoodUtils;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Runtime.InteropServices;
using CommandCentral.CLI.Options;
using CommandCentral.CLI.Interfaces;

namespace CommandCentralHost
{
    /// <summary>
    /// Main Class
    /// </summary>
    class Program
    {
        [STAThread]
        static void Main(string[] args)
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
                        LaunchInterface.Launch((LaunchOptions)invokedVerbInstance);
                        break;
                    }
                default:
                    {
                        "Fell to default statement in verb switch.".WriteLine();
                        break;
                    }
            }
        }
    }
}
