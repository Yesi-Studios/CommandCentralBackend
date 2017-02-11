using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

namespace CCServ.CLI.Options
{
    /// <summary>
    /// The CLI options declarations for the "Upgrade" verb.
    /// </summary>
    public class UpgradeOptions
    {
        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpVerbOption]
        public string GetUsage(string verb)
        {
            var help = HelpText.AutoBuild(this);
            help.Heading = new HeadingInfo("Command Central Service CLI", "1.0.0");
            help.Copyright = new CopyrightInfo(true, "U.S. Navy", 2016);
            help.AdditionalNewLineAfterOption = true;
            help.AddDashesToOption = true;

            help.AddPreOptionsLine("License: IDK.");

            return help;
        }

        [Option('n', "servicename", HelpText = "The name of the service.  This will also be used for the service's display name.", DefaultValue = "ccserv")]
        public string ServiceName { get; set; }

        [Option("productionbranch", HelpText = "The name of the Git production branch.", DefaultValue = "production")]
        public string ProductionBranchName { get; set; }

        [Option("betabranch", HelpText = "The name of the Git beta branch.", DefaultValue = "beta")]
        public string BetaBranchName { get; set; }

        [Option('u', "username", HelpText = "The username of the Git user.", Required = true)]
        public string GitUsername { get; set; }

        [Option('p', "password", HelpText = "The password of the Git user.", Required = true)]
        public string GitPassword { get; set; }

        [Option("giturl", HelpText = "The local or remote url of the Git repository.", DefaultValue = @"https://github.com/Yesi-Studios/CommandCentralBackend.git")]
        public string GitURL { get; set; }

        [Option("nugeturl", HelpText = "The url path to the nuget.exe to use during the build.", DefaultValue = @"https://dist.nuget.org/win-x86-commandline/v3.5.0/nuget.exe")]
        public string NugetURL { get; set; }

        [Option("msbuildpath", HelpText = "The url path to the nuget.exe to use during the build.", DefaultValue = @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild")]
        public string MSBuildPath { get; set; }

        [Option("port", HelpText = "The port on which to launch the service.", DefaultValue = 1113)]
        public int Port { get; set; }

        [Option("securitymode", HelpText = "Indicates which security mode should be used by the service.", DefaultValue = SecurityModes.None)]
        public SecurityModes SecurityMode { get; set; }
    }
}
