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

        public static ConnectionSettings CurrentConnection { get; set; }

        #region Properties

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

        #endregion

    }
}
