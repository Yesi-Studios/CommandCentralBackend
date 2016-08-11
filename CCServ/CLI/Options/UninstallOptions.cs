using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace CCServ.CLI.Options
{
    public class UninstallOptions
    {
        [Option('n', "servicename", HelpText = "The name of the service", Required = true)]
        public string ServiceName { get; set; }
    }
}
