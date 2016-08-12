using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;

namespace CCServ.Authorization.Groups
{
    /// <summary>
    /// All permission groups must inherit from this in order to be included into the permissions system.
    /// </summary>
    public abstract class PermissionGroup
    {

        /// <summary>
        /// All of the permission groups.
        /// </summary>
        public static ConcurrentBag<PermissionGroup> AllPermissionGroups { get; set; }

        #region Properties

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

        #endregion

        #region ctors

        /// <summary>
        /// Builds a new permission group, setting up the defaults.
        /// </summary>
        public PermissionGroup()
        {
            GroupName = this.GetType().Name;
            Modules = new List<ModulePart>();
            AccessibleSubModules = new List<string>();
            GroupsCanEditMembershipOf = new List<PermissionGroup>();
        }

        #endregion

        #region Fluent Methods

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

        #endregion

        #region Startup Methods

        /// <summary>
        /// Scans the entire assembly looking for any types that implement permission groups, creates an instance of them, and then saves them.
        /// <para />
        /// Validates that no two permission groups have the same name.
        /// </summary>
        [ServiceManagement.StartMethod(Priority = 3)]
        private static void ScanPermissions(CLI.Options.LaunchOptions launchOptions)
        {
            Communicator.PostMessage("Scanning for permissions.", Communicator.MessageTypes.Informational);

            var groups = Assembly.GetExecutingAssembly().GetTypes()
                    .Where(x => x.IsSubclassOf(typeof(PermissionGroup)))
                        .Select(x => (PermissionGroup)Activator.CreateInstance(x));

            if (groups.GroupBy(x => x.GroupName, StringComparer.OrdinalIgnoreCase).Any(x => x.Count() > 1))
                throw new Exception("No two groups may have the same name.");

            AllPermissionGroups = new ConcurrentBag<PermissionGroup>(groups);

            Communicator.PostMessage("Found {0} permission group(s): {1}".FormatS(AllPermissionGroups.Count, String.Join(",", AllPermissionGroups.Select(x => x.GroupName))), Communicator.MessageTypes.Informational);
        }

        #endregion

    }
}
