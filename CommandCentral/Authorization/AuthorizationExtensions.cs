using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;
using CommandCentral.Logging;

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

            //Let's do some work here to get either the permission groups or the permission group names and turn them into groups.
            List<Groups.PermissionGroup> groups = client.PermissionGroups;

            if ((groups == null || !groups.Any()) && client.PermissionGroupNames.Any())
            {
                groups = Groups.PermissionGroup.AllPermissionGroups.Where(x => client.PermissionGroupNames.Contains(x.GroupName, StringComparer.CurrentCultureIgnoreCase)).ToList();
            }

            if (groups == null)
                groups = Groups.PermissionGroup.AllPermissionGroups.Where(x => x.IsDefault).ToList();
            else
                groups.AddRange(Groups.PermissionGroup.AllPermissionGroups.Where(x => x.IsDefault));

            //Now we need to start iterating.
            foreach (var group in groups)
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

                foreach (var coc in group.ChainsOfCommandParts)
                {
                    //First the highest levels.
                    if (resolvedPermissions.HighestLevels[coc.ChainOfCommand] < group.AccessLevel)
                        resolvedPermissions.HighestLevels[coc.ChainOfCommand] = group.AccessLevel;

                    //Now let's go through this coc and the types and see who passes the tests.  Editable first.
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
                                if (!fields.Contains(x.Name))
                                    fields.Add(x.Name);
                            }
                            else
                            {
                                resolvedPermissions.EditableFields[x.DeclaringType.Name] = new List<string> { x.Name };
                            }
                        });

                    //Now let's go through this coc and the types and see who passes the tests.  Now returnable!
                    coc.PropertyGroups
                        .Where(x => x.AccessCategory == Groups.AccessCategories.Return &&
                               x.Disjunctions
                                    .All(y => y.Rules
                                        .Any(z => z.AuthorizationOperation(new AuthorizationToken(client, person)))))
                        .SelectMany(x => x.Properties)
                        .ToList()
                        .ForEach(x =>
                        {
                            if (resolvedPermissions.ReturnableFields.TryGetValue(x.DeclaringType.Name, out List<string> fields))
                            {
                                if (!fields.Contains(x.Name))
                                    fields.Add(x.Name);
                            }
                            else
                            {
                                resolvedPermissions.ReturnableFields[x.DeclaringType.Name] = new List<string> { x.Name };
                            }
                        });

                    coc.PropertyGroups
                        .Where(x => x.AccessCategory == Groups.AccessCategories.Return && (!x.Disjunctions.Any() || x.Disjunctions.All(y => y.Rules.All(z => z is Rules.IfInChainOfCommandRule))))
                        .SelectMany(x => x.Properties)
                        .ToList()
                        .ForEach(x =>
                        {
                            if (resolvedPermissions.PrivelegedReturnableFields.TryGetValue(group.AccessLevel, out Dictionary<string, List<string>> fieldsByType))
                            {
                                if (fieldsByType.TryGetValue(x.DeclaringType.Name, out List<string> fields))
                                {
                                    if (!fields.Contains(x.Name))
                                        fields.Add(x.Name);
                                }
                                else
                                {
                                    fieldsByType[x.DeclaringType.Name] = new List<string> { x.Name };
                                }
                            }
                            else
                            {
                                resolvedPermissions.PrivelegedReturnableFields[group.AccessLevel] = new Dictionary<string, List<string>> { { x.DeclaringType.Name, new List<string> { x.Name } } };
                            }
                        });
                }
            }

            //Now we need to copy the fields to the level beneath them because of this assumption:
            //Any field I can return at the command level, I can return at the division level.
            foreach (var pair in resolvedPermissions.PrivelegedReturnableFields[ChainOfCommandLevels.Command])
            {
                if (resolvedPermissions.PrivelegedReturnableFields[ChainOfCommandLevels.Department].TryGetValue(pair.Key, out List<string> fields))
                {
                    fields = fields.Concat(pair.Value).Distinct().ToList();
                }
                else
                {
                    resolvedPermissions.PrivelegedReturnableFields[ChainOfCommandLevels.Department] = new Dictionary<string, List<string>> { { pair.Key, pair.Value } };
                }
            }

            foreach (var pair in resolvedPermissions.PrivelegedReturnableFields[ChainOfCommandLevels.Department])
            {
                if (resolvedPermissions.PrivelegedReturnableFields[ChainOfCommandLevels.Division].TryGetValue(pair.Key, out List<string> fields))
                {
                    fields = fields.Concat(pair.Value).Distinct().ToList();
                }
                else
                {
                    resolvedPermissions.PrivelegedReturnableFields[ChainOfCommandLevels.Division] = new Dictionary<string, List<string>> { { pair.Key, pair.Value } };
                }
            }

            //Now let's do the chain of command determination.  If we're talking about the same person, then the answer is no.
            foreach (var highestLevel in resolvedPermissions.HighestLevels)
            {
                if (person == null)
                {
                    resolvedPermissions.IsInChainOfCommand[highestLevel.Key] = false;
                }
                else
                {
                    switch (highestLevel.Value)
                    {
                        case ChainOfCommandLevels.Command:
                            {
                                resolvedPermissions.IsInChainOfCommand[highestLevel.Key] = client.IsInSameCommandAs(person);
                                break;
                            }
                        case ChainOfCommandLevels.Department:
                            {
                                resolvedPermissions.IsInChainOfCommand[highestLevel.Key] = client.IsInSameDepartmentAs(person);
                                break;
                            }
                        case ChainOfCommandLevels.Division:
                            {
                                resolvedPermissions.IsInChainOfCommand[highestLevel.Key] = client.IsInSameDivisionAs(person);
                                break;
                            }
                        case ChainOfCommandLevels.Self:
                        case ChainOfCommandLevels.None:
                            {
                                resolvedPermissions.IsInChainOfCommand[highestLevel.Key] = false;
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
