using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using MySql.Data.Common;

namespace UnifiedServiceFramework
{
    /// <summary>
    /// Provides methods that are intended to enable diagnostics of the service.  These methods may, or may not, be exposed to the client.
    /// </summary>
    public static class Diagnostics
    {
        /// <summary>
        /// Tests the connection to the database.  If an issue occurs, the error that caused it will be rethrown.
        /// </summary>
        /// <returns></returns>
        public static async Task TestDBConnection(string connectionString)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                }
            }
            catch
            {
                throw;
            }
        }
    }
}
