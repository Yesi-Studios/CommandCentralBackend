using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;
using CommandCentral.Logging;
using NHibernate.Criterion;

namespace CommandCentral.Authorization.Groups
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

        static PermissionGroup()
        {
            AllPermissionGroups = new ConcurrentBag<PermissionGroup>(Assembly.GetExecutingAssembly().GetTypes()
                .Where(x => typeof(PermissionGroup).IsAssignableFrom(x) && x != typeof(PermissionGroup))
                .Select(x => (PermissionGroup)Activator.CreateInstance(x)));
        }

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
        public List<ChainOfCommandPart> ChainsOfCommandParts { get; set; }

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
            GroupName = GetType().Name;
            ChainsOfCommandParts = new List<ChainOfCommandPart>();
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
            GroupName = name;
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
        /// Creates a new chain of command permission.
        /// </summary>
        /// <param name="chainOfCommand"></param>
        /// <returns></returns>
        public ChainOfCommandPart HasChainOfCommand(ChainsOfCommand chainOfCommand)
        {
            if (ChainsOfCommandParts.Any(x => x.ChainOfCommand == chainOfCommand))
                throw new Exception("You may not declare access to a chain of command more than once for the same permission group.");

            ChainsOfCommandParts.Add(new ChainOfCommandPart(chainOfCommand) { ParentPermissionGroup = this });
            return ChainsOfCommandParts.Last();
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

    }
}
