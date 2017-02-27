using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NHibernate;
using AtwoodUtils;
using NHibernate.Criterion;

namespace CCServ.DataAccess
{
    /// <summary>
    /// A query strategy instructs the application as to how to query for a specific group of properties.
    /// <para />
    /// For example, query for first name and last name by doing a LIKE CASE-I search.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class QueryStrategy<T>
    {
        #region Properties

        /// <summary>
        /// The list of all property groups included in this query strategy.
        /// </summary>
        private List<PropertyGroupPart<T>> PropertyGroups { get; set; }

        #endregion

        #region ctors

        /// <summary>
        /// Creates a new query strat and initializes the property groups to an empty collection.
        /// </summary>
        public QueryStrategy()
        {
            PropertyGroups = new List<PropertyGroupPart<T>>();
        }

        #endregion

        #region Fluent Methods

        /// <summary>
        /// Used to select a number of memers from T and begin a property group selection.
        /// </summary>
        /// <param name="members"></param>
        /// <returns></returns>
        public PropertyGroupPart<T> ForProperties(IEnumerable<MemberInfo> members)
        {
            PropertyGroupPart<T> part = new PropertyGroupPart<T>
            {
                ParentQueryStrategy = this,
                Properties = members.ToList(),
                CriteriaProvider = null
            };

            PropertyGroups.Add(part);

            return part;
        }

        /// <summary>
        /// From this query strat, select all members that are used/can be used in a specific type of query.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public IEnumerable<MemberInfo> GetMembersThatAreUsedIn(QueryTypes type)
        {
            return PropertyGroups.Where(x => x.CanSearchIn(type)).SelectMany(x => x.Properties);
        }

        /// <summary>
        /// Gets all members from this query strat that have been marked as identifiers.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<MemberInfo> GetIdentifiers()
        {
            return PropertyGroups.Where(x => x.AreIdintifiers).SelectMany(x => x.Properties);
        }

        /// <summary>
        /// Given a property, returns the data type a property should be queried as.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public SearchDataTypes? GetSearchDataTypeForProperty(PropertyInfo property)
        {
            var group = PropertyGroups.FirstOrDefault(x => x.Properties.Contains(property));

            if (group == null)
                return null;

            return group.SearchType;
        }

        /// <summary>
        /// Given a query term, builds a simple search query and returns the query token.
        /// <para />
        /// Uses the simple search algorithm and uses all those properties that were marked to be used in a simple search.
        /// </summary>
        /// <param name="rawTerm"></param>
        /// <returns></returns>
        public QueryResultToken<T> CreateSimpleSearchQuery(object rawTerm)
        {
            QueryResultToken<T> result = new QueryResultToken<T> { Query = QueryOver.Of<T>(), SearchParameter = GetMembersThatAreUsedIn(QueryTypes.Simple).ToDictionary(x => x, x => rawTerm) };

            //First, we're going to split the raw term
            foreach (var term in (rawTerm as string).Split(null))
            {
                var disjunction = Restrictions.Disjunction();

                foreach (var member in result.SearchParameter.Keys)
                {
                    var propertyGroup = PropertyGroups.FirstOrDefault(x => x.Properties.Any(y => y.Equals(member)));

                    if (propertyGroup == null)
                    {
                        result.Errors.Add("The member, {0}, declared no search strategy!  This is most likely because it is not searchable.  If you believe this is in error, please contact us.".FormatS(member.Name));
                    }
                    else
                    {
                        var token = new QueryToken<T> { Query = result.Query, SearchParameter = new KeyValuePair<MemberInfo, object>(member, term) };

                        var criteria = propertyGroup.CriteriaProvider(token);

                        if (token.HasErrors)
                        {
                            result.Errors.AddRange(token.Errors);
                            return null;
                        }

                        disjunction.Add(criteria);

                    }
                }

                result.Query.Where(disjunction);
            }

            return result;
        }

        public QueryResultToken<T> CreateAdvancedQueryFor(Dictionary<MemberInfo, object> filters, IQueryOver<T, T> query = null)
        {
            QueryResultToken<T> result = new QueryResultToken<T> { Query = query == null ? QueryOver.Of<T>() : (QueryOver<T, T>)query, SearchParameter = filters };

            foreach (var filter in filters)
            {
                var propertyGroup = PropertyGroups.FirstOrDefault(x => x.Properties.Any(y => y.Equals(filter.Key)));

                if (propertyGroup == null)
                {
                    result.Errors.Add("The member, {0}, declared no search strategy!  This is most likely because it is not searchable.  If you believe this is in error, please contact us.".FormatS(filter.Key.Name));
                }
                else
                {
                    var disjunction = Restrictions.Disjunction();

                    List<object> values;

                    switch (propertyGroup.SearchType)
                    {
                        case SearchDataTypes.String:
                            {
                                var rawValue = (filter.Value as string);
                                if (string.IsNullOrWhiteSpace(rawValue))
                                {
                                    result.Errors.Add("Your search value must not be blank.");
                                }

                                values = rawValue.Split(null).Cast<object>().ToList();

                                break;
                            }
                        case SearchDataTypes.DateTime:
                            {
                                values = filter.Value.CastJToken<List<Dictionary<string, DateTime?>>>().Cast<object>().ToList();

                                break;
                            }
                        default:
                            {
                                throw new NotImplementedException("An unrecognized query type was used in the switch in the CreateAdvancedQueryFor method.");
                            }
                    }

                    foreach (var value in values)
                    {
                        var token = new QueryToken<T> { Query = result.Query, SearchParameter = new KeyValuePair<MemberInfo, object>(filter.Key, value) };

                        var criteria = propertyGroup.CriteriaProvider(token);

                        if (token.HasErrors)
                        {
                            result.Errors.AddRange(token.Errors);
                            return result;
                        }

                        disjunction.Add(criteria);
                    }

                    result.Query.Where(disjunction);
                }
            }

            return result;
        }

        #endregion
    }
}
