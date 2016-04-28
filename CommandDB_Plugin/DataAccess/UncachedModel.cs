using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.DataAccess
{
    public abstract class UncachedModel<T> where T : class, new()
    {

        /// <summary>
        /// Inserts or updates this object into the database.
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
        /// Deletes this object from the database.
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
        /// Loads a single instance for the given ID from the database or returns null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<T> DBLoadOne(object id)
        {
            try
            {
                var task = Task.Run<T>(() =>
                    {
                        using (var session = SessionProvider.CreateSession())
                        {
                            return session.Load<T>(id);
                        }
                    });

                return await task;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Loads all types from the database.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static async Task<List<T>> DBLoadAll()
        {
            try
            {
                var task = Task.Run<List<T>>(() =>
                    {
                        using (var session = SessionProvider.CreateSession())
                        {
                            var criteria = session.CreateCriteria<T>();
                            return criteria.List<T>().ToList();
                        }
                    });

                return await task;
            }
            catch
            {
                throw;
            }
        }

    }
}
