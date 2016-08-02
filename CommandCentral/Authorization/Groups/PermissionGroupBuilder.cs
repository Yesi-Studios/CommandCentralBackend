using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Authorization.Groups
{
    /// <summary>
    /// Builds the permission groups.
    /// </summary>
    public class PermissionGroupBuilder
    {
        private PermissionGroup _permissionGroup = new PermissionGroup();

        private ModulePermission _currentModule;

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

        /// <summary>
        /// Sets the resulting permission group to be default.
        /// </summary>
        /// <returns></returns>
        public PermissionGroupBuilder IsDefault()
        {
            _permissionGroup.IsDefault = true;
            return this;
        }

        public PermissionGroupBuilder CanAccessModule(string moduleName)
        {
            _currentModule = new ModulePermission(moduleName);
            _permissionGroup.ModulePermissions.Add(_currentModule);
            return this;
        }


    }
}
