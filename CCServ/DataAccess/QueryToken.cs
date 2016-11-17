using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NHibernate;
using NHibernate.Criterion;

namespace CCServ.DataAccess
{
    /// <summary>
    /// Stores the current state of a query as it is built before being submitted to the database.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class QueryToken<T>
    {
        /// <summary>
        /// The underlying query, detached from a session.
        /// </summary>
        public QueryOver<T, T> Query { get; set; }

        /// <summary>
        /// Contains references to all those aliases that have been used by the application.  Required plumbing for NHibernate.
        /// 
        /// The key is the name of the type, while the object is the type itself.
        /// </summary>
        public Dictionary<Guid, object> Aliases { get; set; }

        private KeyValuePair<MemberInfo, object> _searchParameter;
        /// <summary>
        /// The search parameter.
        /// </summary>
        public KeyValuePair<MemberInfo, object> SearchParameter
        {
            get
            {
                return _searchParameter;
            }
            set
            {
                if (value.Value == null)
                {
                    throw new ArgumentException("You may not search for a null value.");
                }

                _searchParameter = value;
            }
        }

        /// <summary>
        /// Any errors that may have occurred.
        /// </summary>
        public List<string> Errors { get; set; }

        /// <summary>
        /// Are there any errors?
        /// </summary>
        public bool HasErrors
        {
            get
            {
                return Errors.Any();
            }
        }

        /// <summary>
        /// Creates a new query token, initializing the collections.
        /// </summary>
        public QueryToken()
        {
            Errors = new List<string>();
            Aliases = new Dictionary<Guid, object>();
        }
    }
}
