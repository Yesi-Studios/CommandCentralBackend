using NHibernate;
using NHibernate.Engine;
using NHibernate.Persister.Entity;
using NHibernate.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;
using System.Collections;

namespace CCServ.DataAccess
{
    /// <summary>
    /// A collection of extensions to NHibernate's session object.
    /// </summary>
    public static class SessionExtensions
    {

        /// <summary>
        /// Returns a collection of properties that are dirty along with their new and old values.
        /// <para />
        /// This only work on the person object.
        /// </summary>
        /// <param name="session"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static IEnumerable<Variance> GetVariantProperties(this ISession session, Object entity)
        {
            //This is all the information about the session and its implementation from the underlying NHibernate set up.
            ISessionImplementor sessionImpl = session.GetSessionImplementation();
            IPersistenceContext persistenceContext = sessionImpl.PersistenceContext;
            EntityEntry oldEntry = persistenceContext.GetEntry(entity);
            String className = NHibernateHelper.GetEntityMetadata("Person").EntityName;
            IEntityPersister persister = sessionImpl.Factory.GetEntityPersister(className);

            //If the old entry is null, then that means we're creating something, so just set the old to the new.
            if ((oldEntry == null) && (entity is INHibernateProxy))
            {
                INHibernateProxy proxy = entity as INHibernateProxy;
                Object obj = sessionImpl.PersistenceContext.Unproxy(proxy);
                oldEntry = sessionImpl.PersistenceContext.GetEntry(obj);
            }

            Object[] previousState = oldEntry.LoadedState;

            Object[] currentState = persister.GetPropertyValues(entity, sessionImpl.EntityMode);

            //First, we need to know which properties changed.  We can't rely on NHibernate to tell us this because we need to go deeper than it will go.
            for (int x = 0; x < currentState.Length; x++)
            {
                var propertyName = persister.PropertyNames[x];

                if (persister.PropertyTypes[x].IsCollectionType)
                {
                    var currentCollection = ((IEnumerable)currentState[x]).Cast<object>().ToList();
                    var previousCollection = ((IEnumerable)previousState[x]).Cast<object>().ToList();

                    var notInCurrent = new List<object>();
                    var notInPrevious = new List<object>();
                    var changes = new List<Tuple<object, object>>();

                    if (!Utilities.GetSetDifferences(currentCollection, previousCollection, out notInCurrent, out notInPrevious, out changes))
                    {
                        int i = 0;
                    }
                }
                else
                {
                }
            }


            //Returns all property indices that are considered to be dirty, given a current/old state and an entity to compare against.
            Int32[] dirtyPropIndices = persister.FindDirty(currentState, previousState, entity, sessionImpl);

            //There were no dirty properties.
            if (dirtyPropIndices == null)
                yield break;

            //Walk across all firty properties
            foreach (var propIndex in dirtyPropIndices)
            {
                yield return new Variance 
                { 
                    NewValue = currentState[propIndex], 
                    OldValue = previousState[propIndex], 
                    PropertyName = persister.PropertyNames[propIndex] 
                };
            }
        }



        

    }
}
