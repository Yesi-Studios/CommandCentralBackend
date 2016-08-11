using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CCServ.ClientAccess;
using CCServ.Entities;

namespace CCServ.Authorization
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
            token.SetResult(Groups.PermissionGroup.AllPermissionGroups.ToList());
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
                var groups = Groups.PermissionGroup.AllPermissionGroups.Where(x => person.PermissionGroupNames.Contains(x.GroupName))
                    .Concat(Groups.PermissionGroup.AllPermissionGroups.Where(x => x.IsDefault));

                //The editable permissions are all those they can edit.
                var editableGroups = token.AuthenticationSession.Person.PermissionGroups.Resolve(token.AuthenticationSession.Person, person)
                    .EditablePermissionGroups;

                token.SetResult(new
                {
                    CurrentPermissionGroups = groups,
                    EditablePermissionGroups = editableGroups,
                    FriendlyName = person.ToString(),
                    AllPermissionGroups = Groups.PermissionGroup.AllPermissionGroups.Select(x => x.GroupName).ToList()
                });
            }
        }
    }
}
