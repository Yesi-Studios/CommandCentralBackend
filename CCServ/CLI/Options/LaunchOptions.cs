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
    /// The options for the CLI in the launch verb.
    /// </summary>
    public class LaunchOptions
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

        [Option("printsql", HelpText = "Instructs the data providers to print the SQL they generate to the output stream.", DefaultValue = false)]
        public bool PrintSQL { get; set; }

        [Option('u', "username", HelpText = "The username to use to connect to the database.", Required = true)]
        public string Username { get; set; }

        [Option('p', "password", HelpText = "The password to use to connect to the database.", Required = true)]
        public string Password { get; set; }

        [Option('d', "database", HelpText = "The name of the database (schema) to connect to.", Required = true)]
        public string Database { get; set; }

        [Option('s', "server", HelpText = "The address (IP or FQDN) of the database server.", Required = true)]
        public string Server { get; set; }

        [Option("port", HelpText = "The port on which to launch the service.", DefaultValue = 1337)]
        public int Port { get; set; }

        [Option('c', "certpass", HelpText = "The certificate password to use if using ssl to connect to the database. Leave blank if not using the --secure option.", DefaultValue = "")]
        public string CertificatePassword { get; set; }

        [Option("securitymode", HelpText = "Indicates which security mode should be used by the service.", DefaultValue = SecurityModes.None)]
        public SecurityModes SecurityMode { get; set; }

        [Option("dropfirst", HelpText = "Instructs the service to attempt to drop the targeted schema before running.", DefaultValue = false)]
        public bool DropFirst { get; set; }

        [Option("ingest", HelpText = "Instructs the service to ingest the old database.  This will only happen if the schema has to be created, so please combine this with --dropfirst.", DefaultValue = false)]
        public bool Ingest { get; set; }

        [Option("gigo", HelpText = "Instructs the service to fill the database with x records filled with random data.", DefaultValue = 0)]
        public int GIGO { get; set; }

    }
}
