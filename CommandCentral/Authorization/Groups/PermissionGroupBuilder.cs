using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;
using System.Linq.Expressions;

namespace CommandCentral.Authorization.Groups
{
    /// <summary>
    /// Builds the permission groups.
    /// </summary>
    public class PermissionGroupBuilder
    {
        public string GroupName { get; set; }


        /// <summary>
        /// This method starts off the builder chain.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static PermissionGroupBuilder NewPermissionGroup(string name)
        {
            return new PermissionGroupBuilder(name);
        }

        public PermissionGroupBuilder(string name)
        {
            GroupName = name;
        }

        

        

        


    }
}
