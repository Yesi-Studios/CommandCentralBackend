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
using CCServ.Entities;

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
        public static IEnumerable<Change> GetVariantProperties(this ISession session, Object entity)
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
                        foreach (var obj in notInCurrent)
                        {
                            yield return new Change
                            {
                                Remarks = "The item was removed"
                            };
                        }
                    }
                }
                else
                {
                    var previousValue = previousState[x];
                    var currentValue = currentState[x];

                    if (!Object.Equals(previousValue, currentValue))
                    {
                        yield return new Change
                        {
                            Id = Guid.NewGuid(),
                            NewValue = currentValue.ToString(),
                            OldValue = previousValue.ToString(),
                            PropertyName = propertyName
                        };
                    }
                }
            }

            
        }



        

    }
}
