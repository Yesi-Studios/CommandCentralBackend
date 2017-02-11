using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

namespace CCServ.CLI.Options
{
    class ServiceLaunchOptions
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

        [Option('u', "username", HelpText = "The username to use to connect to the database.", Required = true)]
        public string Username { get; set; }

        [Option('p', "password", HelpText = "The password to use to connect to the database.", Required = true)]
        public string Password { get; set; }

        [Option("proddb", HelpText = "The name of the production database (schema) to connect to.", Required = true)]
        public string ProdDatabase { get; set; }

        [Option("betadb", HelpText = "The name of the beta database (schema) to connect to.", Required = true)]
        public string BetaDatabase { get; set; }

        [Option('s', "server", HelpText = "The address (IP or FQDN) of the database server.", Required = true)]
        public string Server { get; set; }

        public int BetaPort { get; set; }

        public int ProdPort { get; set; }

        public SecurityModes SecurityMode { get; set; }
    }
}
