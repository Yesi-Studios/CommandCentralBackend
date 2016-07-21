using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using CommandCentral.ClientAccess;
using AtwoodUtils;

namespace CommandCentral
{
    /// <summary>
    /// Provides abstracted access to a reference list such as Ranks or Rates.
    /// </summary>
    public abstract class ReferenceListItemBase
    {
        #region Properties

        /// <summary>
        /// The Id of this reference item.
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The value of this item.
        /// </summary>
        public virtual string Value { get; set; }

        /// <summary>
        /// A description of this item.
        /// </summary>
        public virtual string Description { get; set; }

        #endregion

        #region Overrides

        /// <summary>
        /// Returns the Value.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Value;
        }

        /// <summary>
        /// Compares this reference list to another reference list.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {

            if (obj as ReferenceListItemBase == null)
                return false;

            var other = (ReferenceListItemBase)obj;

            return this.Id == other.Id && this.Value == other.Value && this.Description == other.Description;
        }

        /// <summary>
        /// Gets the hashcode of this object. Seeds are 17 and 23
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;

                hash = hash * 23 + Id.GetHashCode();
                hash = hash * 23 + (string.IsNullOrEmpty(Value) ? "".GetHashCode() : Value.GetHashCode());
                hash = hash * 23 + (string.IsNullOrEmpty(Description) ? "".GetHashCode() : Description.GetHashCode());

                return hash;
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Attempts to get a list item of the given type with a given value.  
        /// <para/>
        /// This is the preferred method for getting lists by value as it'll help us catch errors in lists that we assume to be preset, but are not.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        public static T GetListItemWithValue<T>(string value, NHibernate.ISession session) where T : ReferenceListItemBase
        {
            T item = session.QueryOver<T>().WhereRestrictionOn(x => x.Value).IsInsensitiveLike(value).SingleOrDefault();
            if (item == null)
                throw new Exception("An attempt to get the value, '{1}', from the list of type, '{0}', failed because the value could not be found.".FormatS(typeof(T).Name, value));
            return item;
        }

        /// <summary>
        /// A validation method that all consumers must implement.  Returns a validation result from FluentValidation.
        /// </summary>
        /// <returns></returns>
        public abstract FluentValidation.Results.ValidationResult Validate();

        #endregion

        #region Client Access Methods

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// </summary>
        /// Returns all reference lists to the client.  Reference lists are ordered by their type.
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(EndpointName = "LoadLists", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = false)]
        private static void EndpointName_LoadReferenceLists(MessageToken token)
        {
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                var result = new Dictionary<string, object>();

                //Very easily we're just going to throw back all the lists.  Easy day.  We're going to group the lists by name so that it looks nice for the client.
                result = session.QueryOver<ReferenceListItemBase>().CacheMode(NHibernate.CacheMode.Get)
                    .List<ReferenceListItemBase>().GroupBy(x => x.GetType().Name).Select(x =>
                    {
                        return new KeyValuePair<string, List<ReferenceListItemBase>>(x.Key, x.ToList());
                    }).ToDictionary(x => x.Key, x => (object)x.Value);
                //TODO REVIEW go through the enum namespace
                result.Add("ChangeEventLevels", Enum.GetNames(typeof(ChangeEventLevels)));
                result.Add("DutyStatuses", Enum.GetNames(typeof(DutyStatuses)));
                result.Add("MusterStatuses", Enum.GetNames(typeof(MusterStatuses)));
                result.Add("Paygrades", Enum.GetNames(typeof(Paygrades)));
                result.Add("PhoneNumberTypes", Enum.GetNames(typeof(PhoneNumberTypes)));
                result.Add("Sexes", Enum.GetNames(typeof(Sexes)));

                token.SetResult(result);
            }
        }

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// </summary>
        /// Adds a list item to the given list for a given value and description and then runs that list's validation.
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(EndpointName = "AddListItem", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void EndpointName_AddListItem(MessageToken token)
        {

            //First we need to know if the client is logged in and is a client.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to add a list item.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Forbidden);
                return;
            }

            if (!token.AuthenticationSession.Person.HasSpecialPermissions(Authorization.SpecialPermissions.Developer))
            {
                token.AddErrorMessage("Only developers may add list items.", ErrorTypes.Authorization, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            //Now we need to know to which list the client wants to add a list item.
            if (!token.Args.ContainsKey("listname"))
            {
                token.AddErrorMessage("You must send a 'listname' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    string listName = token.Args["listname"] as string;

                    if (!DataAccess.NHibernateHelper.GetAllEntityMetadata().ContainsKey(listName))
                    {
                        token.AddErrorMessage("That list name is not a reference list.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        return;
                    }

                    var type = DataAccess.NHibernateHelper.GetEntityMetadata(listName).GetMappedClass(NHibernate.EntityMode.Poco);

                    //Now let's make sure the type the client is asking about is actually a reference list.
                    if (!type.IsSubclassOf(typeof(ReferenceListItemBase)))
                    {
                        token.AddErrorMessage("That list name is not a reference list.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        return;
                    }
                    
                    //Okey dokey.  So we have the list the client wants to add to.  Now let's get the value and the description the client wants to add.
                    if (!token.Args.ContainsKey("value"))
                    {
                        token.AddErrorMessage("You didn't send a 'value' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        return;
                    }
                    string value = token.Args["value"] as string;

                    //Now we need the description from the client.  It is optional.
                    string description = "";
                    if (token.Args.ContainsKey("description"))
                        description = token.Args["description"] as string;

                    //Now put it in the object and then validate it.
                    var listItem = Activator.CreateInstance(type) as ReferenceListItemBase;
                    listItem.Description = description;
                    listItem.Value = value;

                    //Validate it.
                    var validationResult = listItem.Validate();

                    if (validationResult.Errors.Any())
                    {
                        token.AddErrorMessages(validationResult.Errors.Select(x => x.ErrorMessage), ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        return;
                    }

                    //Are we about to try to create a duplicate list?
                    if (session.CreateCriteria(type.Name).Add(NHibernate.Criterion.Expression.Like("Value", listItem.Value)).List<ReferenceListItemBase>().Any())
                    {
                        token.AddErrorMessage("A list item with that value already exists in this list.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        return;
                    }

                    //The list item is now valid. We can insert it.  It doens't need an Id because the mappings should handle that.
                    session.Save(listItem);

                    //Now we need all list items for this type to give back to the client.
                    token.SetResult(session.CreateCriteria(type.Name).List());

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// </summary>
        /// Edits a given line item with the give value and description assuming both pass validation.
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(EndpointName = "EditListItem", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void EndpointMethod_EditListItem(MessageToken token)
        {
            //First we need to know if the client is logged in and is a client.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to edit a list item.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Forbidden);
                return;
            }

            if (!token.AuthenticationSession.Person.HasSpecialPermissions(Authorization.SpecialPermissions.Developer))
            {
                token.AddErrorMessage("Only developers may edit list items.", ErrorTypes.Authorization, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            //Now we need the params from the client.  First up is the Id.
            if (!token.Args.ContainsKey("listitemid"))
            {
                token.AddErrorMessage("You didn't send a 'listitemid' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            Guid listItemId;
            if (!Guid.TryParse(token.Args["listitemid"] as string, out listItemId))
            {
                token.AddErrorMessage("The list item Id you provided was in the wrong format.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            //Let's load the list item and make sure it's real.
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    var listItem = session.Get<ReferenceListItemBase>(listItemId);

                    if (listItem == null)
                    {
                        token.AddErrorMessage("The list item id that you provided did not resolve to a real list.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        return;
                    }

                    //Ok, so we have the list.  Now let's put the client's values in and ask if they're valid.
                    if (token.Args.ContainsKey("value"))
                        listItem.Value = token.Args["value"] as string;
                    if (token.Args.ContainsKey("description"))
                        listItem.Description = token.Args["description"] as string;

                    //Validation
                    var result = listItem.Validate();

                    if (!result.IsValid)
                    {
                        token.AddErrorMessages(result.Errors.Select(x => x.ErrorMessage), ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        return;
                    }

                    //Ok that's all good.  Let's update the list.
                    session.Update(listItem);

                    //Then, we're going to set the result to the list of all list items from this list. List.
                    token.SetResult(session.CreateCriteria(listItem.GetType().Name).List());

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// </summary>
        /// Deletes a given list item given an Id.
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(EndpointName = "DeleteListItem", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void EndpointMethod_DeleteListItem(MessageToken token)
        {
            //First we need to know if the client is logged in and is a client.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to delete a list item.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Forbidden);
                return;
            }

            if (!token.AuthenticationSession.Person.HasSpecialPermissions(Authorization.SpecialPermissions.Developer))
            {
                token.AddErrorMessage("Only developers may delete list items.", ErrorTypes.Authorization, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            //Now we need the params from the client.  First up is the Id.
            if (!token.Args.ContainsKey("listitemid"))
            {
                token.AddErrorMessage("You didn't send a 'listitemid' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            Guid listItemId;
            if (!Guid.TryParse(token.Args["listitemid"] as string, out listItemId))
            {
                token.AddErrorMessage("The list item Id you provided was in the wrong format.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            //Now we delete it.
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {

                    var listItem = session.QueryOver<ReferenceListItemBase>().Where(x => x.Id == listItemId).SingleOrDefault();

                    if (listItem == null)
                    {
                        token.AddErrorMessage("The list item id provided matched no list items.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        return;
                    }

                    //Since we found a list item let's go ahead and delete it.
                    session.Delete(listItem);

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        #endregion
    }
}
