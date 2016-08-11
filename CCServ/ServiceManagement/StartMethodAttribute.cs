using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.ServiceManagement
{
    /// <summary>
    /// This attribute indicates that the following method should be run when the service launches with the given priority.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class StartMethodAttribute : Attribute
    {
        /// <summary>
        /// The priority with which the given method should run.  Higher priorities run first.
        /// </summary>
        public int Priority { get; set; }
    }
}
