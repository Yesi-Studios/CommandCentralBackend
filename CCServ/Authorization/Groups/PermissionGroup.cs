using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;
using CCServ.Logging;
using NHibernate.Criterion;

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
        /// This permission group's access level.
        /// </summary>
        public ChainOfCommandLevels AccessLevel { get; set; }

        /// <summary>
        /// The chains of command included in this group.
        /// </summary>
        public List<ChainOfCommandPart> ChainsOfCommand { get; set; }

        /// <summary>
        /// Sets this to default, meaning all users should have it.
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// The list of sub modules this permission group entitles a user to access.
        /// </summary>
        public List<string> AccessibleSubModules { get; set; }

        /// <summary>
        /// A list of those permission groups this permission group can edit the membership of.
        /// </summary>
        public List<string> GroupsCanEditMembershipOf { get; set; }
        
        #endregion

        #region ctors

        /// <summary>
        /// Builds a new permission group, setting up the defaults.
        /// </summary>
        public PermissionGroup()
        {
            GroupName = this.GetType().Name;
            ChainsOfCommand = new List<ChainOfCommandPart>();
            AccessibleSubModules = new List<string>();
            GroupsCanEditMembershipOf = new List<string>();
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
        /// Sets the group's access level.
        /// </summary>
        /// <param name="level"></param>
        public void HasAccessLevel(ChainOfCommandLevels level)
        {
            AccessLevel = level;
        }

        /// <summary>
        /// Creates a new module permission and sets its name.
        /// </summary>
        /// <param name="chainOfCommand"></param>
        /// <returns></returns>
        public ChainOfCommandPart HasChainOfCommand(ChainsOfCommand chainOfCommand)
        {
            if (Modules.Any(x => x.ModuleName.SafeEquals(moduleName)))
                throw new Exception("You may not declare access to a module more than once for the same permission group.");

            Modules.Add(new ChainOfCommandPart(moduleName) { ParentPermissionGroup = this });
            return Modules.Last();
        }

        /// <summary>
        /// Declares optional submodules this permission group can access.
        /// </summary>
        /// <param name="subModules"></param>
        /// <returns></returns>
        public void CanAccessSubModules(params string[] subModules)
        {
            AccessibleSubModules.AddRange(subModules);
        }

        /// <summary>
        /// Declares optional submodules this permission group can access.
        /// </summary>
        /// <param name="subModules"></param>
        /// <returns></returns>
        public void CanAccessSubModules(params SubModules[] subModules)
        {
            AccessibleSubModules.AddRange(subModules.Select(x => x.ToString()));
        }

        /// <summary>
        /// Declares the permission groups whose membership this client can edit.
        /// </summary>
        /// <param name="permissionGroups"></param>
        /// <returns></returns>
        public void CanEditMembershipOf(params Type[] permissionGroups)
        {
            GroupsCanEditMembershipOf.AddRange(permissionGroups.Select(x => x.Name));
        }

        #endregion

        #region Startup Methods

        /// <summary>
        /// Scans the entire assembly looking for any types that implement permission groups, creates an instance of them, and then saves them.
        /// <para />
        /// Validates that no two permission groups have the same name.
        /// </summary>
        [ServiceManagement.StartMethod(Priority = 17)]
        private static void ScanPermissions(CLI.Options.LaunchOptions launchOptions)
        {
            Log.Info("Collecting permissions...");

            var groups = Assembly.GetExecutingAssembly().GetTypes()
                .Where(x => typeof(PermissionGroup).IsAssignableFrom(x) && x != typeof(PermissionGroup))
                .Select(x => (PermissionGroup)Activator.CreateInstance(x))
                .ToList();

            if (groups.GroupBy(x => x.GroupName, StringComparer.OrdinalIgnoreCase).Any(x => x.Count() > 1))
                throw new Exception("Atwood, you gave two groups the same name again.  Fix it; this is embarrassing.  I will not start up until you do.");

            AllPermissionGroups = new ConcurrentBag<PermissionGroup>(groups);

            Log.Info("Found {0} permission group(s): {1}".FormatS(AllPermissionGroups.Count, String.Join(", ", AllPermissionGroups.Select(x => x.GroupName))));
        }

        #endregion

    }
}
