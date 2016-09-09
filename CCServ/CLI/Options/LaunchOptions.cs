using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace CCServ.CLI.Options
{
    /// <summary>
    /// The options for the CLI in the launch verb.
    /// </summary>
    public class LaunchOptions
    {
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

        [Option("secure", HelpText = "Instructs the service to run in secure mode.  In this mode, communication to clients (https) and the database (SSL certs) are secured.", DefaultValue = false)]
        public bool UseSecureMode { get; set; }

        [Option("dropfirst", HelpText = "Instructs the service to attempt to drop the targeted schema before running.", DefaultValue = false)]
        public bool DropFirst { get; set; }

        [Option("ingest", HelpText = "Instructs the service to ingest the old database.  This will only happen if the schema has to be created, so please combine this with --dropfirst.", DefaultValue = false)]
        public bool Ingest { get; set; }
    }
}
