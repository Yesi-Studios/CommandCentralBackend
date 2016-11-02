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
    public abstract class QueryStrategy<T>
    {
        List<PropertyGroupPart<T>> PropertyGroups { get; set; }

        /// <summary>
        /// Creates a new query strat and initializes the property groups to an empty collection.
        /// </summary>
        public QueryStrategy()
        {
            PropertyGroups = new List<PropertyGroupPart<T>>();
        }

        public PropertyGroupPart<T> ForProperties(IEnumerable<MemberInfo> members)
        {
            PropertyGroupPart<T> part = new PropertyGroupPart<T>
            {
                ParentQueryStrategy = this,
                Properties = members.ToList(),
                CriteriaProvider = null //TODO maybe provider a default here.
            };

            PropertyGroups.Add(part);

            return part;
        }

        public IEnumerable<MemberInfo> GetMembersThatAreUsedIn(QueryTypes type)
        {
            return PropertyGroups.Where(x => x.CanSearchIn(type)).SelectMany(x => x.Properties);
        }

        public IEnumerable<MemberInfo> GetIdentifiers()
        {
            return PropertyGroups.Where(x => x.AreIdintifiers).SelectMany(x => x.Properties);
        }

        public SearchDataTypes? GetSearchDataTypeForProperty(PropertyInfo property)
        {
            var group = PropertyGroups.FirstOrDefault(x => x.Properties.Contains(property));

            if (group == null)
                return null;

            return group.SearchType;
        }

        public QueryResultToken<T> CreateSimpleSearchQuery(object rawTerm)
        {
            QueryResultToken<T> result = new QueryResultToken<T> { Query = QueryOver.Of<T>(), SearchParameter = GetMembersThatAreUsedIn(QueryTypes.Simple).ToDictionary(x => x, x => rawTerm) };

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
    }
}
