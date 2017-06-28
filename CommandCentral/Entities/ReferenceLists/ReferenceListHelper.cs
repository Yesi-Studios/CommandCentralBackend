using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Criterion;
using AtwoodUtils;

namespace CommandCentral.Entities.ReferenceLists
{
    /// <summary>
    /// Provides generalized methods for helping with reference lists.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class ReferenceListHelper<T> where T : ReferenceListItemBase
    {
        /// <summary>
        /// Returns true or false if the list exists.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool Exists(string value)
        {
            using (var session = DataAccess.DataProvider.CreateStatefulSession())
            {
                return session.QueryOver<T>().Where(x => x.Value.IsInsensitiveLike(value)).RowCount() != 0;
            }
        }

        /// <summary>
        /// Returns true or false indicating if all values represent a reference list.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static bool AllExist(params string[] values)
        {
            using (var session = DataAccess.DataProvider.CreateStatefulSession())
            {
                return session.QueryOver<T>().Where(x => x.Value.IsIn(values)).RowCount() == values.Length;
            }
        }

        /// <summary>
        /// Returns true or false indicating if all values represent a reference list.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static bool AllExist(IEnumerable<string> values)
        {
            var array = values.ToArray();
            using (var session = DataAccess.DataProvider.CreateStatefulSession())
            {
                return session.QueryOver<T>().Where(x => x.Value.IsIn(array)).RowCount() == array.Length;
            }
        }

        /// <summary>
        /// Returns all reference lists of the given type.
        /// </summary>
        /// <returns></returns>
        public static List<T> All()
        {
            using (var session = DataAccess.DataProvider.CreateStatefulSession())
            {
                return (List<T>)session.QueryOver<T>().List();
            }
        }

        /// <summary>
        /// Returns a reference list whose value is the requested value or throws an exception if none are found.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T Find(string value)
        {
            using (var session = DataAccess.DataProvider.CreateStatefulSession())
            {
                return session.QueryOver<T>().Where(x => x.Value.IsInsensitiveLike(value))
                    .Cacheable()
                    .SingleOrDefault() ??
                    throw new Exception("Failed to find reference list {0} of type {1}".With(value, typeof(T).Name));
            }
        }

        /// <summary>
        /// Gets a reference list with the given id or returns null.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static T Get(string id)
        {
            if (!Guid.TryParse(id, out Guid result))
                return null;

            using (var session = DataAccess.DataProvider.CreateStatefulSession())
            {
                return session.Get<T>(result);
            }
        }

        /// <summary>
        /// Gets a reference list with the given id or returns null.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static T Get(Guid id)
        {
            using (var session = DataAccess.DataProvider.CreateStatefulSession())
            {
                return session.Get<T>(id);
            }
        }
    }
}
