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
                token.AddErrorMessage("You must send a 'list' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
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
                    if (!type.IsAssignableFrom(typeof(ReferenceListItemBase)))
                    {
                        token.AddErrorMessage("That list name is not a reference list.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        return;
                    }
                    
                    //Okey dokey.  So we have the list the client wants to add to.  Now let's get the value and the description the client wants to add.


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
