using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedServiceFramework.Framework
{
    /// <summary>
    /// Describes settings that help the service react dynamically to its consumer.
    /// </summary>
    public static class Settings
    {
        /// <summary>
        /// The connection string to the database the consumer wants to use.
        /// </summary>
        public static string ConnectionString { get; set; }
    }
}
