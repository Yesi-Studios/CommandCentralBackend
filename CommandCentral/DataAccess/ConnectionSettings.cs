using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.DataAccess
{
    public class ConnectionSettings
    {
        public string Username { get; set; }

        public string Password { get; set; }

        public string Database { get; set; }

        public string Server { get; set; }

        public bool VerboseLogging { get; set; }

        public static Dictionary<string, ConnectionSettings> PredefinedConnectionSettings = new Dictionary<string, ConnectionSettings>()
        {
            { "Default NIPRNet Production", new ConnectionSettings
            {
                Database = "test_command_central",
                Password = "niocga",
                Server = "gord14ec204",
                Username = "niocga",
                VerboseLogging = false
            }},
            { "Default NIPRNet Development/Debugging", new ConnectionSettings
            {
                Database = "test_command_central_development",
                Password = "niocga",
                Server = "gord14ec204",
                Username = "niocga",
                VerboseLogging = false
            }},
            { "Atwood's Home Machine", new ConnectionSettings
            {
                Database = "test_db",
                Password = "douglas0678",
                Server = "localhost",
                Username = "xanneth",
                VerboseLogging = false
            }}
        };

        public static string CurrentSettingsKey { get; set; }
    }
}
