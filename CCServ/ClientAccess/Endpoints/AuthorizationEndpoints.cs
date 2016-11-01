using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CCServ.ClientAccess;
using CCServ.Entities;
using AtwoodUtils;
using CCServ.Authorization;

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
        [EndpointMethod(EndpointName = "LoadPermissionGroups", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = false)]
        private static void EndpointMethod_GetPermissionGroups(MessageToken token)
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
        [EndpointMethod(EndpointName = "LoadPermissionGroupsByPerson", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void EndpointMethod_LoadPermissionGroupsByPerson(MessageToken token)
        {
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to do that.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Forbidden);
                return;
            }

            if (!token.Args.ContainsKey("personid"))
            {
                token.AddErrorMessage("You failed to send a 'personid' parameter!", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            Guid personId;
            if (!Guid.TryParse(token.Args["personid"] as string, out personId))
            {
                token.AddErrorMessage("The person id you send was in the wrong format.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                var person = session.Get<Person>(personId);

                if (person == null)
                {
                    token.AddErrorMessage("The person id you sent was not correct.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                    return;
                }

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
        [EndpointMethod(EndpointName = "UpdatePermissionGroupsByPerson", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void EndpointMethod_UpdatePermissionGroupsByPerson(MessageToken token)
        {
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to do that.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Forbidden);
                return;
            }

            //Get the person's Id.
            if (!token.Args.ContainsKey("personid"))
            {
                token.AddErrorMessage("You failed to send a 'personid' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            Guid personId;
            if (!Guid.TryParse(token.Args["personid"] as string, out personId))
            {
                token.AddErrorMessage("Your person Id parameter was in the wrong format.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            //What permission groups does the client want this person to be in?
            if (!token.Args.ContainsKey("permissiongroups"))
            {
                token.AddErrorMessage("You failed to send a 'permissiongroups' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            List<string> desiredPermissionGroups = null;

            //Get the list of permission group names.
            try
            {
                desiredPermissionGroups = token.Args["permissiongroups"].CastJToken<List<string>>();
            }
            catch
            {
                //If that cast failed.
                token.AddErrorMessage("Your 'permissiongroups' parameter was in the wrong format.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            //Now we load the person and begin the permissions edit.
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    //Get the person and check if the id was legit.
                    var person = session.Get<Person>(personId);

                    if (person == null)
                    {
                        token.AddErrorMessage("Your person Id parameter was wrong. lol.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        return;
                    }

                    //Get the current permission groups the person is a part of.
                    var currentGroups = person.PermissionGroupNames.Concat(Authorization.Groups.PermissionGroup.AllPermissionGroups.Where(x => x.IsDefault).Select(x => x.GroupName)).ToList();

                    //Now get the resolved permissions of our client.
                    var resolvedPermissions = token.AuthenticationSession.Person.PermissionGroups.Resolve(token.AuthenticationSession.Person, person);

                    //Now determine what permissions the client wants to change.
                    var changes = currentGroups.Concat(desiredPermissionGroups).GroupBy(x => x).Where(x => x.Count() == 1).Select(x => x.First());

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
                            //Ok so the client can edit that permission group, now we need to take into account access level.
                        }
                    }

                    if (failures.Any())
                    {
                        token.AddErrorMessage("You were not allowed to edit the membership of the following permission groups: {0}".FormatS(String.Join(", ", failures)), ErrorTypes.Authorization, System.Net.HttpStatusCode.Unauthorized);
                        return;
                    }

                    //Now make sure we don't try to save the default permissions.
                    person.PermissionGroupNames = Authorization.Groups.PermissionGroup.AllPermissionGroups.Where(x => desiredPermissionGroups.Contains(x.GroupName) && !x.IsDefault).Select(x => x.GroupName).ToList();

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
