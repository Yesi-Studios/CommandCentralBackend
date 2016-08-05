using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.DataAccess
{
    /// <summary>
    /// Declares a single connection's settings as well as a static dictionary of some pre-defined settings.
    /// </summary>
    public class ConnectionSettings
    {
        /// <summary>
        /// The username to use to connect to the database.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// The password to use for connection
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// The database within the server to connect to (the schema)
        /// </summary>
        public string Database { get; set; }

        /// <summary>
        /// The database server's address
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        /// Whether or not to use verbose logging.  This will tell NHIbernate to print all SQL.
        /// </summary>
        public bool VerboseLogging { get; set; }

        /// <summary>
        /// Some pre-defined connection settings.
        /// </summary>
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
            { "Atwood's Home Machine Production", new ConnectionSettings
            {
                Database = "test_db",
                Password = "douglas0678",
                Server = "localhost",
                Username = "xanneth",
                VerboseLogging = false
            }},
            { "Atwood's Home Machine Development", new ConnectionSettings
            {
                Database = "command_central_development",
                Password = "douglas0678",
                Server = "localhost",
                Username = "xanneth",
                VerboseLogging = false
            }},
            { "McLean's Home Machine", new ConnectionSettings
            {
                Database = "command_central",
                Password = "password",
                Server = "localhost",
                Username = "anguslmm",
                VerboseLogging = false
            }}
        };

        /// <summary>
        /// The current settings object being used by NHibernate.
        /// </summary>
        public static string CurrentSettingsKey { get; set; }
    }
}
