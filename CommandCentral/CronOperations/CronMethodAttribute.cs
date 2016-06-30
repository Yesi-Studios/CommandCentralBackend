using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.CronOperations
{
    /// <summary>
    /// Indicates that a method is a cron method and should be registered with the cron method manager.
    /// <para />
    /// Note: Cron methods are responsible for their own scheduling.
    /// </summary>
    public class CronMethodAttribute : Attribute
    {
        /// <summary>
        /// The name of the cron method, if not set, is the name of the method itself.
        /// </summary>
        public string Name { get; set; }
    }
}
