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
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = false)]
        private static void LoadReferenceLists(MessageToken token)
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

            bool exclude = false;
            if (token.Args.ContainsKey("exclude"))
            {
                exclude = (bool)token.Args["exclude"];
            }

            //If the client gives no entity names, give back all lists.
            if (!entityNames.Any())
            {
                if (editableOnly)
                {
                    var names = DataAccess.NHibernateHelper.GetAllEntityMetadata()
                        .Where(x => (typeof(EditableReferenceListItemBase)).IsAssignableFrom(x.Value.GetMappedClass(NHibernate.EntityMode.Poco)))
                        .Select(x => x.Key);

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

            var metadataWithEntityNames = new Dictionary<string, IClassMetadata>();

            if (exclude)
            {
                //Here the client wants all reference list types that aren't the given ones.
                if (editableOnly)
                {
                    var metadatas = DataAccess.NHibernateHelper.GetAllEntityMetadata()
                        .Where(x => typeof(EditableReferenceListItemBase).IsAssignableFrom(x.Value.GetMappedClass(NHibernate.EntityMode.Poco)) && !entityNames.Contains(x.Key, StringComparer.CurrentCultureIgnoreCase));

                    foreach (var metadata in metadatas)
                    {
                        metadataWithEntityNames.Add(metadata.Key, metadata.Value);
                    }
                }
                else
                {
                    var metadatas = DataAccess.NHibernateHelper.GetAllEntityMetadata()
                        .Where(x => typeof(ReferenceListItemBase).IsAssignableFrom(x.Value.GetMappedClass(NHibernate.EntityMode.Poco)) && !entityNames.Contains(x.Key, StringComparer.CurrentCultureIgnoreCase));

                    foreach (var metadata in metadatas)
                    {
                        metadataWithEntityNames.Add(metadata.Key, metadata.Value);
                    }
                }
            }
            else
            {
                if (editableOnly)
                {
                    var metadatas = DataAccess.NHibernateHelper.GetAllEntityMetadata()
                        .Where(x => typeof(EditableReferenceListItemBase).IsAssignableFrom(x.Value.GetMappedClass(NHibernate.EntityMode.Poco)) && entityNames.Contains(x.Key, StringComparer.CurrentCultureIgnoreCase));

                    foreach (var metadata in metadatas)
                    {
                        metadataWithEntityNames.Add(metadata.Key, metadata.Value);
                    }
                }
                else
                {
                    var metadatas = DataAccess.NHibernateHelper.GetAllEntityMetadata()
                        .Where(x => typeof(ReferenceListItemBase).IsAssignableFrom(x.Value.GetMappedClass(NHibernate.EntityMode.Poco)) && entityNames.Contains(x.Key, StringComparer.CurrentCultureIgnoreCase));

                    foreach (var metadata in metadatas)
                    {
                        metadataWithEntityNames.Add(metadata.Key, metadata.Value);
                    }
                }
            }

            //Now let's get the Id of the item the client wants.  This can be null, in which case, set the Guid to default and we'll go get multiple lists.
            //If this isn't default afterwards, then the client may only request a single list.
            Guid id = default(Guid);
            if (token.Args.ContainsKey("id"))
            {
                if (!Guid.TryParse(token.Args["id"] as string, out id))
                {
                    throw new CommandCentralException("Your id was not in the right format.", HttpStatusCodes.BadRequest);
                }
            }

            if (id != default(Guid) && entityNames.Count != 1)
                throw new CommandCentralException("If you include an Id in your request, then you must only specify a single list from which to load.", HttpStatusCodes.BadRequest);

            Dictionary<string, List<ReferenceListItemBase>> results = new Dictionary<string, List<ReferenceListItemBase>>();
            //Cool we have a real item and an Id.  Now let's call its loader.
            foreach (var metadata in metadataWithEntityNames)
            {
                var lists = (Activator.CreateInstance(metadata.Value.GetMappedClass(NHibernate.EntityMode.Poco)) as ReferenceListItemBase).Load(id, token);

                lists.ForEach(x => NHibernate.NHibernateUtil.Initialize(x));

                results.Add(metadata.Key, lists);
            }

            token.SetResult(results);
        }

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Update or insert reference lists.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void UpdateOrInsertList(MessageToken token)
        {
            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
                throw new CommandCentralException("You must be logged in to do that.", HttpStatusCodes.AuthenticationFailed);

            token.Args.AssertContainsKeys("entityname", "item");

            //You have permission?
            if (!token.AuthenticationSession.Person.PermissionGroups.Any(x => x.AccessibleSubModules.Contains(SubModules.AdminTools.ToString(), StringComparer.CurrentCultureIgnoreCase)))
                throw new CommandCentralException("You don't have permission to update or create reference lists.", HttpStatusCodes.Unauthorized);
            
            string entityName = token.Args["entityname"] as string;

            //Ok so we were given an entity name, let's make sure that it is both an editable reference list and a real entity.
            var metadata = DataAccess.NHibernateHelper.GetEntityMetadata(entityName);

            //Ok, now let's see if it's a reference list.
            if (!typeof(EditableReferenceListItemBase).IsAssignableFrom(metadata.GetMappedClass(NHibernate.EntityMode.Poco)))
                throw new CommandCentralException("That entity was not a valid editable reference list.", HttpStatusCodes.BadRequest);
            
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
            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
                throw new CommandCentralException("You must be logged in to do that.", HttpStatusCodes.AuthenticationFailed);

            token.Args.AssertContainsKeys("entityname", "id");

            //You have permission?
            if (!token.AuthenticationSession.Person.PermissionGroups.Any(x => x.AccessibleSubModules.Contains(SubModules.AdminTools.ToString(), StringComparer.CurrentCultureIgnoreCase)))
                throw new CommandCentralException("You don't have permission to update or create reference lists.", HttpStatusCodes.Unauthorized);

            string entityName = token.Args["entityname"] as string;

            //Ok so we were given an entity name, let's make sure that it is both an editable reference list and a real entity.
            var metadata = DataAccess.NHibernateHelper.GetEntityMetadata(entityName);

            //Ok, now let's see if it's a reference list.
            if (!typeof(EditableReferenceListItemBase).IsAssignableFrom(metadata.GetMappedClass(NHibernate.EntityMode.Poco)))
                throw new CommandCentralException("That entity was not a valid editable reference list.", HttpStatusCodes.BadRequest);

            if (!Guid.TryParse(token.Args["id"] as string, out Guid id))
                throw new CommandCentralException("Your id parameter was in the wrong format.", HttpStatusCodes.BadRequest);

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
