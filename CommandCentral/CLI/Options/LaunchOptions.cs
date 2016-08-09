using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace CommandCentral.CLI.Options
{
    /// <summary>
    /// The options for the CLI in the launch verb.
    /// </summary>
    public class LaunchOptions
    {
        [Option('v', "verbose", HelpText = "Instructs the service to log 'Informational' messages.", DefaultValue = false)]
        public bool Verbose { get; set; }

        [Option("printsql", HelpText = "Instructs the data providers to print the SQL they generate to the output stream.", DefaultValue = false)]
        public bool PrintSQL { get; set; }

        [Option('u', "username", HelpText = "The username to use to connect to the database.", Required = true)]
        public string Username { get; set; }

        [Option('p', "password", HelpText = "The password to use to connect to the database.", Required = true)]
        public string Password { get; set; }

        [Option('d', "database", HelpText = "The name of the database (schema) to connect to.", Required = true)]
        public string Database { get; set; }

        [Option('s', "server", HelpText = "The address (IP or FQDN) of the database server.", DefaultValue = "GORD14EC204")]
        public string Server { get; set; }

        [Option('e', "emailserver", HelpText = "The address (IP or FQDN) of the email/SMTP server to which the service should send emails.", Required = false)]
        public string SMTPServer { get; set; }

        [Option('l', "log", HelpText = "The relative or absolute path to the directory to which the logs should be written. NOT IMPLEMENTED", DefaultValue = "CONSOLE")]
        public string LogFileDirectoryPath { get; set; }

        /// <summary>
        /// Indicates if the log file directory path = CONSOLE - telling us to print to the console and not to a file.
        /// </summary>
        public bool LogToConsole
        {
            get
            {
                return LogFileDirectoryPath == "CONSOLE";
            }
        }

        [Option("port", HelpText = "The port on which to launch the service.", DefaultValue = 1113)]
        public int Port { get; set; }
    }
}
