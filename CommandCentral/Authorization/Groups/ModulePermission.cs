using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;

namespace CommandCentral.Authorization.Groups
{
    /// <summary>
    /// Declares permissions for an inidividual module.
    /// </summary>
    public class ModulePermission
    {
        /// <summary>
        /// The name of the module for which this object declares permissions.
        /// </summary>
        public string ModuleName { get; set; }



        public ModulePermission(string name)
        {
            ModuleName = name;
        }
    }
}
