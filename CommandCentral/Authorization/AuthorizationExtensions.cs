using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;

namespace CommandCentral.Authorization
{
    /// <summary>
    /// Extensions that help with authorization stuff.  Mostly operating on multiple permission groups.
    /// </summary>
    public static class AuthorizationExtensions
    {
        /// <summary>
        /// Returns a boolean indicating if the given permission groups allow a person to access all of the given submodules.
        /// <para />
        /// Case insensitive.
        /// </summary>
        /// <param name="groups"></param>
        /// <param name="submodules"></param>
        /// <returns></returns>
        public static bool CanAccessSubmodules(this IEnumerable<Groups.PermissionGroup> groups, params string[] submodules)
        {
            return groups.SelectMany(x => x.AccessibleSubModules).Intersect(submodules, StringComparer.CurrentCultureIgnoreCase).Any();
        }

        /// <summary>
        /// Resolves a list of permission groups into a resolved permission for the give person.
        /// </summary>
        /// <param name="person"></param>
        /// <param name="client"></param>
        /// <param name="groups"></param>
        /// <returns></returns>
        public static ResolvedPermissions Resolve(this List<Groups.PermissionGroup> groups, Entities.Person client, Entities.Person person)
        {
            var resolvedPermissions = new ResolvedPermissions();
            resolvedPermissions.TimeResolved = DateTime.Now;
            resolvedPermissions.ClientId = client.Id;
            resolvedPermissions.PersonId = person.Id;

            //Now we need to start iterating.
            foreach (var group in groups)
            {
                //Add the editable permission groups.
                foreach (var editPermissionGroup in group.GroupsCanEditMembershipOf)
                    if (!resolvedPermissions.EditablePermissionGroups.Contains(editPermissionGroup.GroupName))
                        resolvedPermissions.EditablePermissionGroups.Add(editPermissionGroup.GroupName);

                //And the accessible submodules.
                foreach (var accessibleSubmodule in group.AccessibleSubModules)
                    if (!resolvedPermissions.AccessibleSubmodules.Contains(accessibleSubmodule))
                        resolvedPermissions.AccessibleSubmodules.Add(accessibleSubmodule);

                foreach (var module in group.Modules)
                {
                    //First the highest level.
                    if (!resolvedPermissions.HighestLevels.ContainsKey(module.ModuleName))
                        resolvedPermissions.HighestLevels.Add(module.ModuleName, module.Level);
                    else
                        if (resolvedPermissions.HighestLevels[module.ModuleName] < module.Level)
                            resolvedPermissions.HighestLevels[module.ModuleName] = module.Level;

                    //And now for editable fields.  First get or add them.
                    Dictionary<string, List<string>> editableFieldsByType;
                    if (resolvedPermissions.EditableFields.ContainsKey(module.ModuleName))
                    {
                        editableFieldsByType = resolvedPermissions.EditableFields[module.ModuleName];
                    }
                    else
                    {
                        editableFieldsByType = new Dictionary<string, List<string>>();
                        resolvedPermissions.EditableFields.Add(module.ModuleName, editableFieldsByType);
                    }

                    //Now let's go through this module and the types and see who passes the tests.


                }

            }

            



            return resolvedPermissions;
        }
    }
}
