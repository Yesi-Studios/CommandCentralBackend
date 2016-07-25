using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Authorization.Groups
{
    public class PermissionGroupBuilder
    {
        private PermissionGroup _permissionGroup = new PermissionGroup();

        private string _currentModule = "";

        /// <summary>
        /// This method starts off the builder chain.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static PermissionGroupBuilder NewPermissionGroup(string name)
        {
            var builder = new PermissionGroupBuilder();
            builder._permissionGroup.Name = name;
            return builder;
        }

        public PermissionGroupBuilder CanAccessModule(string moduleName)
        {
            _currentModule = moduleName;
        }


    }
}
