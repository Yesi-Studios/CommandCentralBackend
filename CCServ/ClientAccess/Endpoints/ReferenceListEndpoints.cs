using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CCServ.ClientAccess;
using CCServ.Authorization;
using AtwoodUtils;
using NHibernate.Metadata;
using CCServ.Entities.ReferenceLists;

namespace CCServ.ClientAccess.Endpoints
{
    static class ReferenceListEndpoints
    {
        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Loads reference lists.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = false)]
        private static void LoadReferenceLists(MessageToken token, DTOs.ReferenceListEndpoints.LoadReferenceLists dto)
        {
            if (dto.Editable)
            {
                var names = DataAccess.NHibernateHelper.GetAllEntityMetadata()
                    .Where(x => (typeof(EditableReferenceListItemBase)).IsAssignableFrom(x.Value.GetMappedClass(NHibernate.EntityMode.Poco)))
                    .Select(x => x.Key);

                if (dto.Exclude)
                {
                    names = names.Where(x => !dto.EntityNames?.Contains(x, StringComparer.CurrentCultureIgnoreCase) ?? true);
                }
                else
                {
                    names = names.Where(x => dto.EntityNames?.Contains(x, StringComparer.CurrentCultureIgnoreCase) ?? true);
                }

                using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
                {
                    var result = session.QueryOver<EditableReferenceListItemBase>()
                        .Cacheable().CacheMode(NHibernate.CacheMode.Normal)
                        .List().GroupBy(x => x.GetType().Name)
                        .ToDictionary(x => x.Key, x => x.ToList());

                    foreach (var name in names)
                    {
                        if (!result.ContainsKey(name))
                        {
                            result.Add(name, new List<EditableReferenceListItemBase>());
                        }
                    }

                    token.SetResult(result);
                    return;
                }
            }
            else
            {
                var names = DataAccess.NHibernateHelper.GetAllEntityMetadata()
                    .Where(x => (typeof(ReferenceListItemBase)).IsAssignableFrom(x.Value.GetMappedClass(NHibernate.EntityMode.Poco)))
                    .Select(x => x.Key);

                if (dto.Exclude)
                {
                    names = names.Where(x => !dto.EntityNames?.Contains(x, StringComparer.CurrentCultureIgnoreCase) ?? true);
                }
                else
                {
                    names = names.Where(x => dto.EntityNames?.Contains(x, StringComparer.CurrentCultureIgnoreCase) ?? true);
                }

                using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
                {
                    var result = session.QueryOver<ReferenceListItemBase>()
                        .Cacheable().CacheMode(NHibernate.CacheMode.Normal)
                        .List().GroupBy(x => x.GetType().Name)
                        .ToDictionary(x => x.Key, x => x.ToList());

                    foreach (var name in names)
                    {
                        if (!result.ContainsKey(name))
                        {
                            result.Add(name, new List<ReferenceListItemBase>());
                        }
                    }

                    token.SetResult(result);
                    return;
                }
            }
        }

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Update or insert reference lists.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void UpdateOrInsertReferenceList(MessageToken token)
        {
            token.AssertLoggedIn();
            token.Args.AssertContainsKeys("entityname", "item");

            //You have permission?
            if (!token.AuthenticationSession.Person.PermissionGroups.Any(x => x.AccessibleSubModules.Contains(SubModules.AdminTools.ToString(), StringComparer.CurrentCultureIgnoreCase)))
                throw new CommandCentralException("You don't have permission to update or create reference lists.", ErrorTypes.Authorization);
            
            string entityName = token.Args["entityname"] as string;

            //Ok so we were given an entity name, let's make sure that it is both an editable reference list and a real entity.
            var metadata = DataAccess.NHibernateHelper.GetEntityMetadata(entityName);

            //Ok, now let's see if it's a reference list.
            if (!typeof(EditableReferenceListItemBase).IsAssignableFrom(metadata.GetMappedClass(NHibernate.EntityMode.Poco)))
                throw new CommandCentralException("That entity was not a valid editable reference list.", ErrorTypes.Validation);
            
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
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void DeleteReferenceList(MessageToken token)
        {
            token.AssertLoggedIn();
            token.Args.AssertContainsKeys("entityname", "id");

            //You have permission?
            if (!token.AuthenticationSession.Person.PermissionGroups.Any(x => x.AccessibleSubModules.Contains(SubModules.AdminTools.ToString(), StringComparer.CurrentCultureIgnoreCase)))
                throw new CommandCentralException("You don't have permission to update or create reference lists.", ErrorTypes.Authorization);

            string entityName = token.Args["entityname"] as string;

            //Ok so we were given an entity name, let's make sure that it is both an editable reference list and a real entity.
            var metadata = DataAccess.NHibernateHelper.GetEntityMetadata(entityName);

            //Ok, now let's see if it's a reference list.
            if (!typeof(EditableReferenceListItemBase).IsAssignableFrom(metadata.GetMappedClass(NHibernate.EntityMode.Poco)))
                throw new CommandCentralException("That entity was not a valid editable reference list.", ErrorTypes.Validation);

            if (!Guid.TryParse(token.Args["id"] as string, out Guid id))
                throw new CommandCentralException("Your id parameter was in the wrong format.", ErrorTypes.Validation);

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
