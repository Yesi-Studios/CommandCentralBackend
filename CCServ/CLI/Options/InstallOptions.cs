using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace CCServ.CLI.Options
{
    public class InstallOptions
    {
        [Option('n', "servicename", HelpText = "The name of the service", Required = true)]
        public string ServiceName { get; set; }

        [Option('d', "servicedisplayname", HelpText = "The display name name of the service - usually the same as the service name.", Required = true)]
        public string ServiceDisplayName { get; set; }
    }
}
