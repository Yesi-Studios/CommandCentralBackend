using NHibernate;
using NHibernate.Engine;
using NHibernate.Persister.Entity;
using NHibernate.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.DataAccess
{
    public static class SessionExtensions
    {

        public static Boolean IsDirtyEntity(this ISession session, Object entity)
        {

            ISessionImplementor sessionImpl = session.GetSessionImplementation();

            IPersistenceContext persistenceContext = sessionImpl.PersistenceContext;

            EntityEntry oldEntry = persistenceContext.GetEntry(entity);

            String className = oldEntry.EntityName;

            IEntityPersister persister = sessionImpl.Factory.GetEntityPersister(className);



            if ((oldEntry == null) && (entity is INHibernateProxy))
            {

                INHibernateProxy proxy = entity as INHibernateProxy;

                Object obj = sessionImpl.PersistenceContext.Unproxy(proxy);

                oldEntry = sessionImpl.PersistenceContext.GetEntry(obj);

            }



            Object[] oldState = oldEntry.LoadedState;

            Object[] currentState = persister.GetPropertyValues(entity, sessionImpl.EntityMode);




            Int32[] dirtyProps = oldState.Select((o, i) => (oldState[i] == currentState[i]) ? -1 : i).Where(x => x >= 0).ToArray();



            return (dirtyProps != null);

        }



        public static Boolean IsDirtyProperty(this ISession session, Object entity, String propertyName)
        {

            ISessionImplementor sessionImpl = session.GetSessionImplementation();

            IPersistenceContext persistenceContext = sessionImpl.PersistenceContext;

            EntityEntry oldEntry = persistenceContext.GetEntry(entity);

            String className = oldEntry.EntityName;

            IEntityPersister persister = sessionImpl.Factory.GetEntityPersister(className);




            if ((oldEntry == null) && (entity is INHibernateProxy))
            {

                INHibernateProxy proxy = entity as INHibernateProxy;

                Object obj = sessionImpl.PersistenceContext.Unproxy(proxy);

                oldEntry = sessionImpl.PersistenceContext.GetEntry(obj);

            }



            Object[] oldState = oldEntry.LoadedState;

            Object[] currentState = persister.GetPropertyValues(entity, sessionImpl.EntityMode);

            Int32[] dirtyProps = persister.FindDirty(currentState, oldState, entity, sessionImpl);

            Int32 index = Array.IndexOf(persister.PropertyNames, propertyName);



            Boolean isDirty = (dirtyProps != null) ? (Array.IndexOf(dirtyProps, index) != -1) : false;



            return (isDirty);

        }

        public static IEnumerable<Variance> GetDirtyProperties(this ISession session, Object entity)
        {
            ISessionImplementor sessionImpl = session.GetSessionImplementation();

            IPersistenceContext persistenceContext = sessionImpl.PersistenceContext;

            EntityEntry oldEntry = persistenceContext.GetEntry(entity);

            String className = NHibernateHelper.GetEntityMetadata("Person").EntityName;

            IEntityPersister persister = sessionImpl.Factory.GetEntityPersister(className);



            if ((oldEntry == null) && (entity is INHibernateProxy))
            {

                INHibernateProxy proxy = entity as INHibernateProxy;

                Object obj = sessionImpl.PersistenceContext.Unproxy(proxy);

                oldEntry = sessionImpl.PersistenceContext.GetEntry(obj);

            }



            Object[] oldState = oldEntry.LoadedState;

            Object[] currentState = persister.GetPropertyValues(entity, sessionImpl.EntityMode);

            Int32[] dirtyProps = persister.FindDirty(currentState, oldState, entity, sessionImpl);

            if (dirtyProps == null)
                yield break;

            foreach (var propIndex in dirtyProps)
            {
                yield return new Variance { NewValue = currentState[propIndex], OldValue = oldState[propIndex], PropertyName = persister.PropertyNames[propIndex] };
            }
        }



        public static Object GetOriginalEntityProperty(this ISession session, Object entity, String propertyName)
        {

            ISessionImplementor sessionImpl = session.GetSessionImplementation();

            IPersistenceContext persistenceContext = sessionImpl.PersistenceContext;

            EntityEntry oldEntry = persistenceContext.GetEntry(entity);

            String className = oldEntry.EntityName;

            IEntityPersister persister = sessionImpl.Factory.GetEntityPersister(className);



            if ((oldEntry == null) && (entity is INHibernateProxy))
            {

                INHibernateProxy proxy = entity as INHibernateProxy;

                Object obj = sessionImpl.PersistenceContext.Unproxy(proxy);

                oldEntry = sessionImpl.PersistenceContext.GetEntry(obj);

            }



            Object[] oldState = oldEntry.LoadedState;

            Object[] currentState = persister.GetPropertyValues(entity, sessionImpl.EntityMode);

            Int32[] dirtyProps = persister.FindDirty(currentState, oldState, entity, sessionImpl);

            Int32 index = Array.IndexOf(persister.PropertyNames, propertyName);



            Boolean isDirty = (dirtyProps != null) ? (Array.IndexOf(dirtyProps, index) != -1) : false;



            return ((isDirty == true) ? oldState[index] : currentState[index]);

        }

    }
}
