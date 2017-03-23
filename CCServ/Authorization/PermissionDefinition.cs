using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.Authorization
{
    /// <summary>
    /// TODO
    /// </summary>
    /// <returns></returns>
    public class PermissionDefinition
    {
        /// <summary>
        /// TODO
        /// </summary>
        /// <returns></returns>
        public ChainOfCommand ChainOfCommand { get; set; }

        /// <summary>
        /// TODO
        /// </summary>
        /// <returns></returns>
        public ChainOfCommandLevels Level { get; set; }

    }
}
