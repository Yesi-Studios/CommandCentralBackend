using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandCentral.ClientAccess;
using CommandCentral.Entities;
using AtwoodUtils;
using CommandCentral.Authorization;

namespace CommandCentral.ClientAccess.Endpoints
{
    static class AuthorizationEndpoints
    {
        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Returns all permission group definitions to the client.
        /// </summary>
        /// <param name="token"></param>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = false)]
        private static void LoadPermissionGroups(MessageToken token)
        {
            token.SetResult(Authorization.Groups.PermissionGroup.AllPermissionGroups.ToList());
        }

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Returns all permission group definitions to the client for the specific client.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void LoadPermissionGroupsByPerson(MessageToken token)
        {
            token.AssertLoggedIn();
            token.Args.AssertContainsKeys("personid");

            if (!Guid.TryParse(token.Args["personid"] as string, out var personId))
                throw new CommandCentralException("The person id you send was in the wrong format.", ErrorTypes.Validation);

            using (var session = DataAccess.DataProvider.CreateStatefulSession())
            {
                var person = session.Get<Person>(personId) ??
                    throw new CommandCentralException("The person id you sent was not correct.", ErrorTypes.Validation);

                //Get the person's permissions and then add the defaults.
                var groups = Authorization.Groups.PermissionGroup.AllPermissionGroups.Where(x => person.PermissionGroupNames.Contains(x.GroupName))
                    .Concat(Authorization.Groups.PermissionGroup.AllPermissionGroups.Where(x => x.IsDefault));

                //The editable permissions are all those they can edit.
                var editableGroups = token.AuthenticationSession.Person.ResolvePermissions(person).EditablePermissionGroups;

                token.SetResult(new
                {
                    CurrentPermissionGroups = groups.Select(x => x.GroupName),
                    EditablePermissionGroups = editableGroups,
                    FriendlyName = person.ToString(),
                    AllPermissionGroups = Authorization.Groups.PermissionGroup.AllPermissionGroups.Select(x => x.GroupName).ToList()
                });
            }
        }

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Edits the permission groups a person is a part of.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void UpdatePermissionGroupsByPerson(MessageToken token)
        {
            token.AssertLoggedIn();
            token.Args.AssertContainsKeys("personid", "permissiongroups");

            //Get the person's Id.
            if (!Guid.TryParse(token.Args["personid"] as string, out var personId))
                throw new CommandCentralException("Your person Id parameter was in the wrong format.", ErrorTypes.Validation);

            List<string> desiredPermissionGroups = null;

            //Get the list of permission group names.
            try
            {
                desiredPermissionGroups = token.Args["permissiongroups"].CastJToken<List<string>>();
            }
            catch
            {
                //If that cast failed.
                throw new CommandCentralException("Your 'permissiongroups' parameter was in the wrong format.", ErrorTypes.Validation);
            }

            //Now we load the person and begin the permissions edit.
            using (var session = DataAccess.DataProvider.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    //Get the person and check if the id was legit.
                    var person = session.Get<Person>(personId) ??
                        throw new CommandCentralException("Your person Id parameter was wrong. lol.", ErrorTypes.Validation);

                    //Get the current permission groups the person is a part of.
                    var currentGroups = person.PermissionGroupNames.Concat(Authorization.Groups.PermissionGroup.AllPermissionGroups.Where(x => x.IsDefault).Select(x => x.GroupName)).ToList();

                    //Now get the resolved permissions of our client.
                    var resolvedPermissions = token.AuthenticationSession.Person.ResolvePermissions(person);

                    //Now determine what permissions the client wants to change.
                    var changes = currentGroups.Concat(desiredPermissionGroups).GroupBy(x => x).Where(x => x.Count() == 1).Select(x => x.First()).ToList();

                    //Now go through all the requested changes and make sure the client can make them.
                    var failures = new List<string>();
                    foreach (var groupName in changes)
                    {
                        //First is the easy one.  Can client the edit the permission group's membership at all?
                        if (!resolvedPermissions.EditablePermissionGroups.Contains(groupName))
                        {
                            failures.Add(groupName);
                        }
                        else
                        {
                            //Here we get the group the client is trying to edit.  We know the client is allowed to edit its membership at this point.
                            var group = Authorization.Groups.PermissionGroup.AllPermissionGroups.First(x => x.GroupName.SafeEquals(groupName));
                             
                            var highestPermissionLevelInGroups = ChainOfCommandLevels.None;
                            var selection = resolvedPermissions.HighestLevels
                                .Where(x => group.ChainsOfCommandParts.Select(y => y.ChainOfCommand).Contains(x.Key));

                            if (selection.Any())
                                highestPermissionLevelInGroups = selection.Max(x => x.Value);

                            //Now, we need to know if the client has the right access level (chain of command) to edit it.
                            switch (group.AccessLevel)
                            {
                                case ChainOfCommandLevels.Command:
                                    {
                                        if (!person.IsInSameCommandAs(token.AuthenticationSession.Person))
                                            failures.Add(groupName);

                                        break;
                                    }
                                case ChainOfCommandLevels.Department:
                                    {
                                        switch (highestPermissionLevelInGroups)
                                        {
                                            case ChainOfCommandLevels.Command:
                                                {
                                                    if (!person.IsInSameCommandAs(token.AuthenticationSession.Person))
                                                        failures.Add(groupName);

                                                    break;
                                                }
                                            case ChainOfCommandLevels.Department:
                                                {
                                                    if (!person.IsInSameDepartmentAs(token.AuthenticationSession.Person))
                                                        failures.Add(groupName);

                                                    break;
                                                }
                                            case ChainOfCommandLevels.Division:
                                                {
                                                    throw new Exception("This case shouldn't be met...");
                                                }
                                            default:
                                                {
                                                    throw new Exception("Fell to default case.");
                                                }
                                        }

                                        break;
                                    }
                                case ChainOfCommandLevels.Division:
                                    {
                                        switch (highestPermissionLevelInGroups)
                                        {
                                            case ChainOfCommandLevels.Command:
                                                {
                                                    if (!person.IsInSameCommandAs(token.AuthenticationSession.Person))
                                                        failures.Add(groupName);

                                                    break;
                                                }
                                            case ChainOfCommandLevels.Department:
                                                {
                                                    if (!person.IsInSameDepartmentAs(token.AuthenticationSession.Person))
                                                        failures.Add(groupName);

                                                    break;
                                                }
                                            case ChainOfCommandLevels.Division:
                                                {
                                                    if (!person.IsInSameDivisionAs(token.AuthenticationSession.Person))
                                                        failures.Add(groupName);

                                                    break;
                                                }
                                            default:
                                                {
                                                    throw new Exception("Fell to default case.");
                                                }
                                        }

                                        break;
                                    }
                                case ChainOfCommandLevels.None:
                                    {
                                        failures.Add(groupName);

                                        break;
                                    }
                                case ChainOfCommandLevels.Self:
                                    {
                                        if (!person.Id.Equals(token.AuthenticationSession.Person.Id))
                                            failures.Add(groupName);

                                        break;
                                    }
                                default:
                                    {
                                        throw new Exception("In the access level switch of the edit permission groups endpoint.");
                                    }
                            }
                        }
                    }

                    if (failures.Any())
                        throw new CommandCentralException("You were not allowed to edit the membership of the following permission groups: {0}".With(String.Join(", ", failures)), ErrorTypes.Authorization);

                    //Now make sure we don't try to save the default permissions.
                    person.PermissionGroupNames = Authorization.Groups.PermissionGroup.AllPermissionGroups.Where(x => desiredPermissionGroups.Contains(x.GroupName) && !x.IsDefault).Select(x => x.GroupName).ToList();

                    //We also need to add the changes to the person.
                    person.Changes.Add(new Change
                    {
                        Editee = person,
                        Editor = token.AuthenticationSession.Person,
                        Id = Guid.NewGuid(),
                        NewValue = String.Join(", ", person.PermissionGroupNames.Concat(Authorization.Groups.PermissionGroup.AllPermissionGroups.Where(x => x.IsDefault).Select(x => x.GroupName))),
                        OldValue = String.Join(", ", currentGroups),
                        PropertyName = PropertySelector.SelectPropertyFrom<Person>(x => x.PermissionGroups).Name,
                        Time = token.CallTime
                    });

                    session.Update(person);

                    //Finally tell the client if they updated themselves.
                    token.SetResult(new { WasSelf = token.AuthenticationSession.Person.Id == person.Id });

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
    }
}
