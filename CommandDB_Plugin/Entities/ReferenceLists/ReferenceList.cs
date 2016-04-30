using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Entities.ReferenceLists
{
    /// <summary>
    /// Provides abstracted access to a reference list such as Ranks or Rates.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ReferenceListItem<T> where T : class
    {
        /// <summary>
        /// The ID of this reference item.
        /// </summary>
        public virtual string ID { get; set; }

        /// <summary>
        /// The value of this item.
        /// </summary>
        public virtual string Value { get; set; }

        /// <summary>
        /// A description of this item.
        /// </summary>
        public virtual string Description { get; set; }

        /// <summary>
        /// Loads all items from the database, and allows them to be cached.
        /// </summary>
        /// <returns></returns>
        public List<T> LoadAll()
        {
            using (var session = DataAccess.SessionProvider.CreateSession())
            {
                return session.CreateCriteria<T>()
                    .SetCacheable(true)
                    .SetCacheMode(NHibernate.CacheMode.Normal)
                    .List<T>().ToList();
            }
        }
    }
}
