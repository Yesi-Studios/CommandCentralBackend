using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;
using CCServ.Logging;

namespace CCServ.Authorization
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
        public static ResolvedPermissions Resolve(this IEnumerable<Groups.PermissionGroup> groups, Entities.Person client, Entities.Person person)
        {
            //Ensure that the client isn't null.
            if (client == null)
                throw new ArgumentException("The client may not be null");

            var resolvedPermissions = new ResolvedPermissions();
            resolvedPermissions.TimeResolved = DateTime.Now;
            resolvedPermissions.ClientId = client.Id.ToString();
            resolvedPermissions.PersonId = person == null ? null : person.Id.ToString();

            //Now we need to start iterating.
            foreach (var group in groups)
            {
                //Add the names to the permission group names so the client knows who permission groups we used.
                resolvedPermissions.PermissionGroupNames.Add(group.GroupName);

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
                        resolvedPermissions.HighestLevels.Add(module.ModuleName, group.AccessLevel);
                    else
                        if (resolvedPermissions.HighestLevels[module.ModuleName] < group.AccessLevel)
                            resolvedPermissions.HighestLevels[module.ModuleName] = group.AccessLevel;

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

                    //Now let's go through this module and the types and see who passes the tests.  Editable first.
                    module.PropertyGroups
                        .Where(x => x.AccessCategory == Groups.AccessCategories.Edit &&
                               x.Disjunctions
                                    .All(y => y.Rules
                                        .Any(z => z.AuthorizationOperation(new AuthorizationToken(client, person)))))
                        .SelectMany(x => x.Properties)
                        .ToList()
                        .ForEach(x =>
                        {
                            if (!editableFieldsByType.ContainsKey(x.DeclaringType.Name))
                                editableFieldsByType.Add(x.DeclaringType.Name, new List<string>() { x.Name });
                            else
                                if (!editableFieldsByType[x.DeclaringType.Name].Contains(x.Name))
                                    editableFieldsByType[x.DeclaringType.Name].Add(x.Name);
                        });

                    //And now the returnable fields.
                    Dictionary<string, List<string>> returnableFieldsByType;
                    if (resolvedPermissions.ReturnableFields.ContainsKey(module.ModuleName))
                    {
                        returnableFieldsByType = resolvedPermissions.ReturnableFields[module.ModuleName];
                    }
                    else
                    {
                        returnableFieldsByType = new Dictionary<string, List<string>>();
                        resolvedPermissions.ReturnableFields.Add(module.ModuleName, returnableFieldsByType);
                    }

                    //Now let's go through this module and the types and see who passes the tests.  Now returnable!
                    module.PropertyGroups
                        .Where(x => x.AccessCategory == Groups.AccessCategories.Return &&
                               x.Disjunctions
                                    .All(y => y.Rules
                                        .Any(z => z.AuthorizationOperation(new AuthorizationToken(client, person)))))
                        .SelectMany(x => x.Properties)
                        .ToList()
                        .ForEach(x =>
                        {
                            if (!returnableFieldsByType.ContainsKey(x.DeclaringType.Name))
                                returnableFieldsByType.Add(x.DeclaringType.Name, new List<string>() { x.Name });
                            else
                                if (!returnableFieldsByType[x.DeclaringType.Name].Contains(x.Name))
                                returnableFieldsByType[x.DeclaringType.Name].Add(x.Name);
                        });

                }

            }

            //Now let's do the chain of command determination.  If we're talking about the same person, then the answer is no.
            foreach (var highestLevel in resolvedPermissions.HighestLevels)
            {
                if (person == null || client.Id == person.Id)
                {
                    resolvedPermissions.ChainOfCommandByModule[highestLevel.Key] = false;
                }
                else
                {
                    switch (highestLevel.Value)
                    {
                        case Groups.PermissionGroupLevels.Command:
                            {
                                resolvedPermissions.ChainOfCommandByModule[highestLevel.Key] = client.IsInSameCommandAs(person);
                                break;
                            }
                        case Groups.PermissionGroupLevels.Department:
                            {
                                resolvedPermissions.ChainOfCommandByModule[highestLevel.Key] = client.IsInSameDepartmentAs(person);
                                break;
                            }
                        case Groups.PermissionGroupLevels.Division:
                            {
                                resolvedPermissions.ChainOfCommandByModule[highestLevel.Key] = client.IsInSameDivisionAs(person);
                                break;
                            }
                        case Groups.PermissionGroupLevels.Self:
                        case Groups.PermissionGroupLevels.None:
                            {
                                resolvedPermissions.ChainOfCommandByModule[highestLevel.Key] = false;
                                break;
                            }
                        default:
                            {
                                throw new NotImplementedException("In the switch between levels in the CoC determinations in Resolve().");
                            }
                    }
                }
            }

            return resolvedPermissions;
        }
    }
}
