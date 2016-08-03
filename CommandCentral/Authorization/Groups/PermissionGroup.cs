using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;

namespace CommandCentral.Authorization.Groups
{
    /// <summary>
    /// All permission grups must inherit from this in order to be included into the permissions system.
    /// </summary>
    public abstract class PermissionGroup
    {
        /// <summary>
        /// The name of the permission group. Default : the class's name.
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// The modules included in this group.
        /// </summary>
        public List<ModulePart> Modules { get; set; }

        /// <summary>
        /// Sets this to default, meaning all users should have it.
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// The list of sub modules this permission gruop entitles a user to access.
        /// </summary>
        public List<string> AccessibleSubModules { get; set; }

        /// <summary>
        /// A list of those permission groups this permission group can edit the membership of.
        /// </summary>
        public List<PermissionGroup> GroupsCanEditMembershipOf { get; set; }

        /// <summary>
        /// Builds a new permission group, setting up the defaults.
        /// </summary>
        public PermissionGroup()
        {
            GroupName = this.GetType().Name;
        }

        /// <summary>
        /// Assigns the given name to the current permission group.
        /// </summary>
        /// <param name="name"></param>
        public void Name(string name)
        {
            this.GroupName = name;
        }

        /// <summary>
        /// Sets the resulting permission group to be default.
        /// </summary>
        /// <returns></returns>
        public void Default()
        {
            IsDefault = true;
        }

        /// <summary>
        /// Creates a new module permission and sets its name.
        /// </summary>
        /// <param name="moduleName"></param>
        /// <returns></returns>
        public ModulePart CanAccessModule(string moduleName)
        {
            if (Modules.Any(x => x.ModuleName.SafeEquals(moduleName)))
                throw new Exception("You may not declare access to a module more than once for the same permission group.");

            Modules.Add(new ModulePart(moduleName));
            return Modules.Last();
        }

        /// <summary>
        /// Declares optional submodules this permission group can access.
        /// </summary>
        /// <param name="subModules"></param>
        /// <returns></returns>
        public void CanAccessSubModules(params string[] subModules)
        {
            AccessibleSubModules = subModules.ToList();
        }

        /// <summary>
        /// Declares the permission groups whose memership this client can edit.
        /// </summary>
        /// <param name="permissionGroups"></param>
        /// <returns></returns>
        public void CanEditMembershipOf(params PermissionGroup[] permissionGroups)
        {
            GroupsCanEditMembershipOf = permissionGroups.ToList();
        }
    }
}
