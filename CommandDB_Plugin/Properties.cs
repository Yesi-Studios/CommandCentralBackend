using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral
{
    /// <summary>
    /// Contains settings-like properties that help to standardize access to the database.
    /// </summary>
    public static class Properties
    {
        /// <summary>
        /// The connection string to the database, including the password and username.
        /// </summary>
        public static string ConnectionString
        {
            get
            {
                return @"server=147.51.62.100;uid=niocga;pwd=niocga;database=test_tdb";
            }
        }

        /// <summary>
        /// The return limit.
        /// </summary>
        public static int GlobalReturnLimit
        {
            get
            {
                return 100;
            }
        }

    }
}
