﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CCServ.ClientAccess;
using CCServ.Authorization;
using AtwoodUtils;

namespace CCServ.Entities.ReferenceLists
{
    static class ReferenceListEndpoints
    {
        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Loads reference lists.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(EndpointName = "LoadReferenceLists", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = false)]
        private static void EndpointMethod_LoadReferenceLists(MessageToken token)
        {
            List<string> entityNames = new List<string>();
            if (token.Args.ContainsKey("entitynames"))
            {
                entityNames = token.Args["entitynames"].CastJToken<List<string>>();
            }

            //Does the client want editable lists?
            bool editableOnly = false;
            if (token.Args.ContainsKey("editable"))
            {
                editableOnly = (bool)token.Args["editable"];
            }

            //If the client gives no entity names, give back all lists.
            if (!entityNames.Any())
            {
                if (editableOnly)
                {
                    using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
                    {
                        token.SetResult(session.QueryOver<EditableReferenceListItemBase>().List().GroupBy(x => x.GetType().Name)
                            .ToDictionary(x => x.Key, x => x.ToList()));
                        return;
                    }
                }
                else
                {
                    using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
                    {
                        token.SetResult(session.QueryOver<ReferenceListItemBase>().List().GroupBy(x => x.GetType().Name)
                            .ToDictionary(x => x.Key, x => x.ToList()));
                        return;
                    }
                }
            }

            
            //Ok so we were given an entity name, let's make sure that it is both a reference list and a real entity.
            var metadataWithEntityNames = entityNames.Select(x => new { Metadata = DataAccess.NHibernateHelper.GetEntityMetadata(x), Name = x }).ToList();

            if (editableOnly)
            {
                //Ok, now let's see if it's all reference lists or editable lists - depending on the flag.
                if (!metadataWithEntityNames.All(x => typeof(EditableReferenceListItemBase).IsAssignableFrom(x.Metadata.GetMappedClass(NHibernate.EntityMode.Poco))))
                {
                    token.AddErrorMessage("One or more of your entity names were not valid editable reference lists.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                    return;
                }
            }
            else
            {
                //Ok, now let's see if it's all reference lists or editable lists - depending on the flag.
                if (!metadataWithEntityNames.All(x => typeof(ReferenceListItemBase).IsAssignableFrom(x.Metadata.GetMappedClass(NHibernate.EntityMode.Poco))))
                {
                    token.AddErrorMessage("One or more of your entity names were not valid reference lists.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                    return;
                }
            }

            //Now let's get the Id of the item the client wants.  This can be null, in which case, set the Guid to default and we'll go get multiple lists.
            //If this isn't default afterwards, then the client may only request a single list.
            Guid id = default(Guid);
            if (token.Args.ContainsKey("id"))
            {
                if (!Guid.TryParse(token.Args["id"] as string, out id))
                {
                    token.AddErrorMessage("Your id was not in the right format.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                    return;
                }
            }

            if (id != default(Guid) && entityNames.Count != 1)
            {
                token.AddErrorMessage("If you include an Id in your request, then you must only specify a single list from which to load.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            List<ReferenceListItemBase> results = new List<ReferenceListItemBase>();
            //Cool we have a real item and an Id.  Now let's call its loader.
            foreach (var metadata in metadataWithEntityNames)
            {
                var lists = (Activator.CreateInstance(metadata.Metadata.GetMappedClass(NHibernate.EntityMode.Poco)) as ReferenceListItemBase).Load(id, token);

                if (token.HasError)
                    return;

                results.AddRange(lists);
            }

            token.SetResult(results.GroupBy(x => x.GetType().Name).ToDictionary(x => x.Key, x => x.ToList()));
        }

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Update or insert reference lists.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(EndpointName = "UpdateOrInsertReferenceList", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void EndpointMethod_UpdateOrInsertList(MessageToken token)
        {
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to update or insert reference lists.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            //You have permission?
            if (!token.AuthenticationSession.Person.PermissionGroups.Any(x => x.AccessibleSubModules.Contains(SubModules.AdminTools.ToString(), StringComparer.CurrentCultureIgnoreCase)))
            {
                token.AddErrorMessage("You don't have permission to update or create reference lists.", ErrorTypes.Authorization, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            if (!token.Args.ContainsKey("entityname"))
            {
                token.AddErrorMessage("You failed to send an entityname parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }
            string entityName = token.Args["entityname"] as string;

            //Ok so we were given an entity name, let's make sure that it is both an editable reference list and a real entity.
            var metadata = DataAccess.NHibernateHelper.GetEntityMetadata(entityName);

            //Ok, now let's see if it's a reference list.
            if (!typeof(EditableReferenceListItemBase).IsAssignableFrom(metadata.GetMappedClass(NHibernate.EntityMode.Poco)))
            {
                token.AddErrorMessage("That entity was not a valid editable reference list.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            if (!token.Args.ContainsKey("item"))
            {
                token.AddErrorMessage("You failed to send an item parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }
            var item = token.Args["item"].CastJToken();

            (Activator.CreateInstance(metadata.GetMappedClass(NHibernate.EntityMode.Poco)) as EditableReferenceListItemBase)
                .UpdateOrInsert(item, token);
        }

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Update or insert reference lists.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(EndpointName = "DeleteReferenceList", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void EndpointMethod_DeleteReferenceList(MessageToken token)
        {
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to update or insert reference lists.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            //You have permission?
            if (!token.AuthenticationSession.Person.PermissionGroups.Any(x => x.AccessibleSubModules.Contains(SubModules.AdminTools.ToString(), StringComparer.CurrentCultureIgnoreCase)))
            {
                token.AddErrorMessage("You don't have permission to update or create reference lists.", ErrorTypes.Authorization, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            if (!token.Args.ContainsKey("entityname"))
            {
                token.AddErrorMessage("You failed to send an entityname parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }
            string entityName = token.Args["entityname"] as string;

            //Ok so we were given an entity name, let's make sure that it is both an editable reference list and a real entity.
            var metadata = DataAccess.NHibernateHelper.GetEntityMetadata(entityName);

            //Ok, now let's see if it's a reference list.
            if (!typeof(EditableReferenceListItemBase).IsAssignableFrom(metadata.GetMappedClass(NHibernate.EntityMode.Poco)))
            {
                token.AddErrorMessage("That entity was not a valid editable reference list.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            if (!token.Args.ContainsKey("id"))
            {
                token.AddErrorMessage("You failed to send an id parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            Guid id;
            if (!Guid.TryParse(token.Args["id"] as string, out id))
            {
                token.AddErrorMessage("Your id parameter was in the wrong format.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            bool forceDelete = false;
            if (token.Args.ContainsKey("forcedelete"))
            {
                forceDelete = (bool)token.Args["forcedelete"];
            }

            (Activator.CreateInstance(metadata.GetMappedClass(NHibernate.EntityMode.Poco)) as EditableReferenceListItemBase)
                .Delete(id, forceDelete, token);
        }

    }
}