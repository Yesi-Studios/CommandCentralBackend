using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NHibernate;
using NHibernate.Criterion;
using System.Linq.Expressions;

namespace CommandCentral.DataAccess
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

        private KeyValuePair<Expression<Func<T, object>>, object> _searchParameter;

        /// <summary>
        /// The search parameter.
        /// </summary>
        public KeyValuePair<Expression<Func<T, object>>, object> SearchParameter
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
        /// Creates a new query token.
        /// </summary>
        public QueryToken(QueryOver<T, T> query, KeyValuePair<Expression<Func<T, object>>, object> searchParameter)
        {
            Query = query ?? throw new ArgumentNullException("query");
            SearchParameter = searchParameter;
        }
    }
}
