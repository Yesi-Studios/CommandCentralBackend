using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

namespace CommandCentral.CLI.Options
{
    public class InstallOptions
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

        [Option('n', "servicename", HelpText = "The name of the service", Required = true)]
        public string ServiceName { get; set; }

        [Option('d', "servicedisplayname", HelpText = "The display name name of the service - usually the same as the service name.", Required = true)]
        public string ServiceDisplayName { get; set; }
    }
}
