using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NHibernate;
using AtwoodUtils;
using NHibernate.Criterion;
using System.Linq.Expressions;
using CommandCentral.ClientAccess;

namespace CommandCentral.DataAccess
{
    /// <summary>
    /// A query strategy instructs the application as to how to query for a specific group of properties.
    /// <para />
    /// For example, query for first name and last name by doing a LIKE CASE-I search.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class QueryStrategyProvider<T>
    {
        #region Properties

        /// <summary>
        /// The list of all property groups included in this query strategy.
        /// </summary>
        private List<PropertyGroupPart<T>> PropertyGroups { get; set; } = new List<PropertyGroupPart<T>>();

        #endregion

        #region ctors

        /// <summary>
        /// Creates a new query strat.
        /// </summary>
        public QueryStrategyProvider()
        {
        }

        #endregion

        #region Fluent Methods

        /// <summary>
        /// Used to select a number of memers from T and begin a property group selection.
        /// </summary>
        /// <param name="expressions"></param>
        /// <returns></returns>
        public PropertyGroupPart<T> ForProperties(params Expression<Func<T, object>>[] expressions)
        {
            PropertyGroups.Add(new PropertyGroupPart<T>(this, expressions));
            return PropertyGroups.Last();
        }

        /// <summary>
        /// From this query strat, select all members that are used/can be used in a specific type of query.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public IEnumerable<Expression<Func<T, object>>> GetMembersThatAreUsedIn(QueryTypes type)
        {
            return PropertyGroups.Where(x => x.CanSearchIn(type)).SelectMany(x => x.Expressions);
        }

        /// <summary>
        /// Given a property, returns the data type a property should be queried as.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public SearchDataTypes? GetSearchDataTypeForProperty(PropertyInfo property)
        {
            var group = PropertyGroups.FirstOrDefault(x => x.Expressions.Any(y => y.GetPropertyName().Equals(property.Name)));

            if (group == null)
                return null;

            return group.SearchType;
        }

        /// <summary>
        /// Routes the search data to the necessary underlying implementation of the desired search type.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="searchData"></param>
        /// <returns></returns>
        public QueryOver<T, T> CreateQuery(QueryTypes type, object searchData)
        {
            switch (type)
            {
                case QueryTypes.Advanced:
                    {
                        var filters = searchData as Dictionary<Expression<Func<T, object>>, object> ??
                            throw new CommandCentralException("Your search data must be in the format of a dicionary of filters.", ErrorTypes.Validation);

                        return CreateAdvancedQuery(filters);
                    }
                case QueryTypes.Simple:
                    {
                        var searchTerms = searchData as string ??
                            throw new CommandCentralException("Your search terms must be a string and not be null.", ErrorTypes.Validation);

                        return CreateSimpleSearchQuery(searchTerms, GetMembersThatAreUsedIn(QueryTypes.Simple));
                    }
                default:
                    {
                        throw new NotImplementedException("Hit default case in query type switch.");
                    }
            }
        }

        /// <summary>
        /// Given a query term, builds a simple search query and returns the query token.
        /// <para />
        /// Uses the simple search algorithm and uses all those properties that were marked to be used in a simple search.
        /// </summary>
        /// <param name="rawTerm"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        private QueryOver<T, T> CreateSimpleSearchQuery(string rawTerm, IEnumerable<Expression<Func<T, object>>> properties)
        {
            var searchParameters = properties.ToDictionary(x => x, x => rawTerm);

            QueryOver<T, T> result = QueryOver.Of<T>();

            //First, we're going to split the raw term
            foreach (var term in (rawTerm as string).Split(null))
            {
                var disjunction = Restrictions.Disjunction();

                foreach (var propertyExpression in searchParameters.Keys)
                {
                    var propertyGroup = PropertyGroups.FirstOrDefault(x => x.Expressions.Any(y => String.Equals(y.GetPropertyName(), propertyExpression.GetPropertyName(), StringComparison.CurrentCultureIgnoreCase)));

                    if (propertyGroup == null)
                    {
                        throw new CommandCentralException("The member, {0}, declared no search strategy!  ".FormatS(propertyExpression.GetPropertyName()) +
                            "This is most likely because it is not searchable.  " +
                            "If you believe this is in error, please contact us.", ErrorTypes.Validation);
                    }
                    else
                    {
                        var token = new QueryToken<T>(result, new KeyValuePair<Expression<Func<T, object>>, object>(propertyExpression, term));

                        disjunction.Add(propertyGroup.CriteriaProvider(token));
                    }
                }

                result.Where(disjunction);
            }

            return result;
        }

        /// <summary>
        /// Creates an advanced query by submitting each filter to the corresponding query provider and appending them all in a conjunction.
        /// </summary>
        /// <param name="filters"></param>
        /// <returns></returns>
        private QueryOver<T, T> CreateAdvancedQuery(Dictionary<Expression<Func<T, object>>, object> filters)
        {
            QueryOver<T, T> result = QueryOver.Of<T>();

            foreach (var filter in filters)
            {
                var propertyGroup = PropertyGroups.FirstOrDefault(x => x.Expressions.Any(y => String.Equals(y.GetPropertyName(), filter.Key.GetPropertyName(), StringComparison.CurrentCultureIgnoreCase)));

                if (propertyGroup == null)
                {
                    throw new CommandCentralException("The member, {0}, declared no search strategy!  ".FormatS(filter.Key.Name) +
                            "This is most likely because it is not searchable.  " +
                            "If you believe this is in error, please contact us.", ErrorTypes.Validation);
                }
                else
                {
                    var disjunction = Restrictions.Disjunction();

                    var token = new QueryToken<T>(result, filter);

                    disjunction.Add(propertyGroup.CriteriaProvider(token));
                    
                    result.Where(disjunction);
                }
            }

            return result;
        }

        #endregion
    }
}
