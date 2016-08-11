using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

namespace CCServ.CLI.Options
{
    public class MainOptions
    {
        [VerbOption("launch", HelpText = "Launches the application.")]
        public LaunchOptions LaunchVerb { get; set; }

        [VerbOption("install", HelpText = "Installs the service.")]
        public InstallOptions InstallVerb { get; set; }

        [VerbOption("uninstall", HelpText = "Uninstalls the service.")]
        public UninstallOptions UninstallVerb { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpVerbOption]
        public string GetUsage(string verb)
        {
            var help = HelpText.AutoBuild(this, verb);
            help.Heading = new HeadingInfo("Command Central Service CLI", Config.Version.RELEASE_VERSION);
            help.Copyright = new CopyrightInfo(true, "U.S. Navy", 2016);
            help.AdditionalNewLineAfterOption = true;
            help.AddDashesToOption = true;

            help.AddPreOptionsLine("License: IDK.");
            
            return help;
        }
    }
}
