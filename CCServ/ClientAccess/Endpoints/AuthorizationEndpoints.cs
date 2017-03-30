using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CCServ.ClientAccess;
using CCServ.Entities;
using AtwoodUtils;
using CCServ.Authorization;
using Humanizer;

namespace CCServ.ClientAccess.Endpoints
{
    static class AuthorizationEndpoints
    {
        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Returns all permission group definitions to the client.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = false)]
        private static void GetPermissionGroups(MessageToken token)
        {
            token.SetResult(Authorization.Groups.PermissionGroup.AllPermissionGroups.ToList());
        }

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Returns all permission group definitions to the client.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void LoadPermissionGroupsByPerson(MessageToken token)
        {
            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
                throw new CommandCentralException("You must be logged in to do that.", HttpStatusCodes.AuthenticationFailed);

            if (!token.Args.ContainsKey("personid"))
                throw new CommandCentralException("You failed to send a 'personid' parameter!", HttpStatusCodes.BadRequest);

            if (!Guid.TryParse(token.Args["personid"] as string, out Guid personId))
                throw new CommandCentralException("The person id you send was in the wrong format.", HttpStatusCodes.BadRequest);

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                var person = session.Get<Person>(personId) ??
                    throw new CommandCentralException("The person id you sent was not correct.", HttpStatusCodes.BadRequest);

                //Get the person's permissions and then add the defaults.
                var groups = Authorization.Groups.PermissionGroup.AllPermissionGroups.Where(x => person.PermissionGroupNames.Contains(x.GroupName))
                    .Concat(Authorization.Groups.PermissionGroup.AllPermissionGroups.Where(x => x.IsDefault));

                //The editable permissions are all those they can edit.
                var editableGroups = token.AuthenticationSession.Person.PermissionGroups.Resolve(token.AuthenticationSession.Person, person)
                    .EditablePermissionGroups;

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
            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
                throw new CommandCentralException("You must be logged in to do that.", HttpStatusCodes.AuthenticationFailed);

            token.Args.ContainsKeysOrThrow("personid", "permissiongroups");

            //Get the person's Id.
            if (!Guid.TryParse(token.Args["personid"] as string, out Guid personId))
                throw new CommandCentralException("Your person Id parameter was in the wrong format.", HttpStatusCodes.BadRequest);

            List<string> desiredPermissionGroups = null;

            //Get the list of permission group names.
            try
            {
                desiredPermissionGroups = token.Args["permissiongroups"].CastJToken<List<string>>();
            }
            catch
            {
                //If that cast failed.
                throw new CommandCentralException("Your 'permissiongroups' parameter was in the wrong format.", HttpStatusCodes.BadRequest);
            }

            //Now we load the person and begin the permissions edit.
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    //Get the person and check if the id was legit.
                    var person = session.Get<Person>(personId) ??
                        throw new CommandCentralException("Your person Id parameter was wrong. lol.", HttpStatusCodes.BadRequest);

                    //Get the current permission groups the person is a part of.
                    var currentGroups = person.PermissionGroupNames.Concat(Authorization.Groups.PermissionGroup.AllPermissionGroups.Where(x => x.IsDefault).Select(x => x.GroupName)).ToList();

                    //Now get the resolved permissions of our client.
                    var resolvedPermissions = token.AuthenticationSession.Person.PermissionGroups.Resolve(token.AuthenticationSession.Person, person);

                    //Now determine what permissions the client wants to change.
                    var changes = currentGroups.Concat(desiredPermissionGroups).GroupBy(x => x).Where(x => x.Count() == 1).Select(x => x.First()).ToList();

                    //Now go through all the requested changes and make sure the client can make them.
                    List<string> failures = new List<string>();
                    foreach (string groupName in changes)
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

                            var highestPermissionLevelInGroups = resolvedPermissions.HighestLevels
                                .Where(x => group.ChainsOfCommandMemberOf.Select(y => y.ToString()).Contains(x.Key, StringComparer.CurrentCultureIgnoreCase))
                                .Max(x => x.Value);

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
                        throw new CommandCentralException("You were not allowed to edit the membership of the following permission groups: {0}".FormatS(String.Join(", ", failures)), HttpStatusCodes.Unauthorized);

                    //Now make sure we don't try to save the default permissions.
                    person.PermissionGroupNames = Authorization.Groups.PermissionGroup.AllPermissionGroups.Where(x => desiredPermissionGroups.Contains(x.GroupName) && !x.IsDefault).Select(x => x.GroupName).ToList();

                    //We also need to add the changes to the person.
                    person.Changes.Add(new Change
                    {
                        Editee = person,
                        Editor = token.AuthenticationSession.Person,
                        Id = Guid.NewGuid(),
                        NewValue = String.Join(", ", person.PermissionGroupNames.Concat(Authorization.Groups.PermissionGroup.AllPermissionGroups.Where(x => x.IsDefault).Select(x => x.GroupName)).Select(x => x.Humanize(LetterCasing.Title))),
                        OldValue = String.Join(", ", currentGroups.Select(x => x.Humanize(LetterCasing.Title))),
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
