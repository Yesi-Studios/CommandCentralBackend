﻿using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate;

namespace CommandCentral.DataAccess
{
    /// <summary>
    /// Provides DB access methods in which update, insert and delete are done against both the database and the cache.  Loads, however, occur only against the cache.
    /// <para />
    /// This is ideal for situations in which values will be read often but changed infrequently.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class CachedModel<T> where T : class, new()
    {
        private static ConcurrentDictionary<object, T> _cache = new ConcurrentDictionary<object, T>();

        /// <summary>
        /// Inserts or updates this object into the database as well as into the underlying cache.
        /// </summary>
        /// <returns></returns>
        public async Task DBInsertOrUpdate()
        {
            try
            {
                await Task.Run(() =>
                    {
                        using (var session = SessionProvider.CreateSession())
                        using (var transaction = session.BeginTransaction())
                        {
                            try
                            {
                                session.SaveOrUpdate(this as T);

                                object primaryKey = typeof(T).GetProperty(SessionProvider.GetEntityMetadata(typeof(T).Name).IdentifierPropertyName).GetValue(this);

                                _cache.AddOrUpdate(primaryKey, this as T, (key, value) =>
                                {
                                    return this as T;
                                });

                                transaction.Commit();
                            }
                            catch
                            {
                                transaction.Rollback();
                                throw;
                            }
                        }
                    });
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Deletes this object from the cache and the database.
        /// </summary>
        /// <returns></returns>
        public async Task DBDelete()
        {
            try
            {
                await Task.Run(() =>
                    {
                        using (var session = SessionProvider.CreateSession())
                        using (var transaction = session.BeginTransaction())
                        {
                            try
                            {

                                session.Delete(this as T);

                                object primaryKey = typeof(T).GetProperty(SessionProvider.GetEntityMetadata(typeof(T).Name).IdentifierPropertyName).GetValue(this);

                                T _;
                                if (!_cache.TryRemove(primaryKey, out _))
                                    throw new Exception(string.Format("The object with the ID, '{0}', was not found in the cache during a delete.", primaryKey));

                                transaction.Commit();
                            }
                            catch
                            {
                                transaction.Rollback();
                                throw;
                            }
                        }
                    });
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Gets a single instance for the given ID from the cache or returns null if none exists.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public static T GetOne(object id)
        {
            try
            {
                T value;
                if (!_cache.TryGetValue(id, out value))
                    return null;

                return value;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Gets all types into a list from the cache.  
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<T> GetAll()
        {
            try
            {
                return _cache.Values.ToList();
            }
            catch
            {
                throw;
            }
        }
    }
}
