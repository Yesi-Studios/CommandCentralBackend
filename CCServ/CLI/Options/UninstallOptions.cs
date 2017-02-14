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
    /// The command that instructs the service to uninstall.
    /// </summary>
    public class UninstallOptions
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

        /// <summary>
        /// The name of the service to uninstall.
        /// </summary>
        [Option('n', "servicename", HelpText = "The name of the service", Required = true)]
        public string ServiceName { get; set; }
    }
}
