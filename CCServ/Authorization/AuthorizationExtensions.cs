﻿using System;
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
        /// Resolves a list of permission groups into a resolved permission for the given person.
        /// </summary>
        /// <param name="person"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        public static ResolvedPermissions ResolvePermissions(this Entities.Person client, Entities.Person person)
        {
            //Ensure that the client isn't null.
            if (client == null)
                throw new ArgumentException("The client may not be null");

            var resolvedPermissions = new ResolvedPermissions()
            {
                TimeResolved = DateTime.UtcNow,
                ClientId = client.Id.ToString(),
                PersonId = person?.Id.ToString()
            };

            //Now we need to start iterating.
            foreach (var group in client.PermissionGroups)
            {
                //Add the names to the permission group names so the client knows who permission groups we used.
                resolvedPermissions.PermissionGroupNames.Add(group.GroupName);

                //Add the editable permission groups.
                foreach (var editPermissionGroup in group.GroupsCanEditMembershipOf)
                    if (!resolvedPermissions.EditablePermissionGroups.Contains(editPermissionGroup))
                        resolvedPermissions.EditablePermissionGroups.Add(editPermissionGroup);

                //And the accessible submodules.
                foreach (var accessibleSubmodule in group.AccessibleSubModules)
                    if (!resolvedPermissions.AccessibleSubmodules.Contains(accessibleSubmodule))
                        resolvedPermissions.AccessibleSubmodules.Add(accessibleSubmodule);

                foreach (var coc in group.ChainsOfCommand)
                {
                    //First the highest levels.
                    if (resolvedPermissions.HighestLevels[coc.ChainOfCommand] < group.AccessLevel)
                        resolvedPermissions.HighestLevels[coc.ChainOfCommand] = group.AccessLevel;

                    //Now let's go through this module and the types and see who passes the tests.  Editable first.
                    coc.PropertyGroups
                        .Where(x => x.AccessCategory == Groups.AccessCategories.Edit &&
                               x.Disjunctions
                                    .All(y => y.Rules
                                        .Any(z => z.AuthorizationOperation(new AuthorizationToken(client, person)))))
                        .SelectMany(x => x.Properties)
                        .ToList()
                        .ForEach(x =>
                        {
                            if (resolvedPermissions.EditableFields.TryGetValue(x.DeclaringType.Name, out List<string> fields))
                            {
                                fields.Add(x.Name);
                            }
                            else
                            {
                                resolvedPermissions.EditableFields[]
                            }

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

                    //And those fields we can return with stipulations at each of the levels.
                    //If the priv return fields doesn't have the module, then set it all up including empty lists for all of the access levels.
                    if (!resolvedPermissions.PrivelegedReturnableFields.ContainsKey(module.ModuleName))
                    {
                        resolvedPermissions.PrivelegedReturnableFields.Add(module.ModuleName, 
                            Enum.GetNames(typeof(ChainOfCommandLevels))
                            .Select(x => new KeyValuePair<string, List<string>>(x, new List<string>()))
                            .ToDictionary(x => x.Key, x => x.Value));
                    }

                    //Ok cool now we have this module and a list of all the access levels.  weeeeee.
                    //Now go through all the property groups in this module.  if the property group is return and either has no rules or its rules are all if in chain of command rules, then add it.
                    resolvedPermissions.PrivelegedReturnableFields[module.ModuleName][module.ParentPermissionGroup.AccessLevel.ToString()] = module.PropertyGroups
                        .Where(x => x.AccessCategory == Groups.AccessCategories.Return && (!x.Disjunctions.Any() || x.Disjunctions.All(y => y.Rules.All(z => z is Rules.IfInChainOfCommandRule))))
                        .SelectMany(x => x.Properties.Select(y => y.Name))
                        .ToList();

                    //Now we need to copy the fields to the level beneath them because of this assumption:
                    //Any field I can return at the command level, I can return at the division level.
                    resolvedPermissions.PrivelegedReturnableFields[module.ModuleName][ChainOfCommandLevels.Department.ToString()] =
                        resolvedPermissions.PrivelegedReturnableFields[module.ModuleName][ChainOfCommandLevels.Department.ToString()]
                        .Concat(resolvedPermissions.PrivelegedReturnableFields[module.ModuleName][ChainOfCommandLevels.Command.ToString()])
                        .Distinct().ToList();

                    resolvedPermissions.PrivelegedReturnableFields[module.ModuleName][ChainOfCommandLevels.Division.ToString()] =
                        resolvedPermissions.PrivelegedReturnableFields[module.ModuleName][ChainOfCommandLevels.Division.ToString()]
                        .Concat(resolvedPermissions.PrivelegedReturnableFields[module.ModuleName][ChainOfCommandLevels.Department.ToString()])
                        .Distinct().ToList();
                }
            }

            //Now let's do the chain of command determination.  If we're talking about the same person, then the answer is no.
            foreach (var highestLevel in resolvedPermissions.HighestLevels)
            {
                if (person == null)
                {
                    resolvedPermissions.ChainOfCommandByModule[highestLevel.Key] = false;
                }
                else
                {
                    switch (highestLevel.Value)
                    {
                        case ChainOfCommandLevels.Command:
                            {
                                resolvedPermissions.ChainOfCommandByModule[highestLevel.Key] = client.IsInSameCommandAs(person);
                                break;
                            }
                        case ChainOfCommandLevels.Department:
                            {
                                resolvedPermissions.ChainOfCommandByModule[highestLevel.Key] = client.IsInSameDepartmentAs(person);
                                break;
                            }
                        case ChainOfCommandLevels.Division:
                            {
                                resolvedPermissions.ChainOfCommandByModule[highestLevel.Key] = client.IsInSameDivisionAs(person);
                                break;
                            }
                        case ChainOfCommandLevels.Self:
                        case ChainOfCommandLevels.None:
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
