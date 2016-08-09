using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

namespace CommandCentral.CLI.Options
{
    public class MainOptions
    {
        [VerbOption("launch", HelpText = "Launches the application.")]
        public LaunchOptions LaunchVerb { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpVerbOption]
        public string GetUsage(string verb)
        {
            var help = HelpText.AutoBuild(this, verb);
            help.Heading = new HeadingInfo("Command Central Service CLI", CommandCentral.Config.Version.RELEASE_VERSION);
            help.Copyright = new CopyrightInfo(true, "U.S. Navy", 2016);
            help.AdditionalNewLineAfterOption = true;
            help.AddDashesToOption = true;

            help.AddPreOptionsLine("License: IDK.  Ask LT Rawls.");
            
            return help;
        }
    }
}
