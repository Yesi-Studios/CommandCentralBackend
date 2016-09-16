using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace CCServ.CLI.Options
{
    /// <summary>
    /// The command that instructs the service to uninstall.
    /// </summary>
    public class UninstallOptions
    {
        /// <summary>
        /// The name of the service to uninstall.
        /// </summary>
        [Option('n', "servicename", HelpText = "The name of the service", Required = true)]
        public string ServiceName { get; set; }
    }
}
