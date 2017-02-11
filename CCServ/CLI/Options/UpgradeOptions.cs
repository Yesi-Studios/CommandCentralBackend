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

    }
}
