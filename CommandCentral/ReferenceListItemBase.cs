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
        public new virtual string ToString()
        {
            return Value;
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
        [EndpointMethod(EndpointName = "LoadReferenceLists", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = false)]
        private static void EndpointName_LoadReferenceLists(MessageToken token)
        {
            //Very easily we're just going to throw back all the lists.  Easy day.  We're going to group the lists by name so that it looks nice for the client.
            token.SetResult(token.CommunicationSession.QueryOver<ReferenceListItemBase>()
                .List<ReferenceListItemBase>().GroupBy(x => x.GetType().Name).Select(x =>
                {
                    return new KeyValuePair<string, List<ReferenceListItemBase>>(x.Key, x.ToList());
                }).ToDictionary(x => x.Key, x => x.Value));
        }

        #endregion

    }
}
