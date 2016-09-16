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

        public PropertyGroupPart<T> ForProperties(IEnumerable<MemberInfo> members)
        {
            PropertyGroupPart<T> part = new PropertyGroupPart<T>
            {
                ParentQueryStrategy = this,
                Properties = members.ToList(),
                QueryProvider = null //TODO maybe provider a default here.
            };

            return part;
        }

        public IEnumerable<MemberInfo> GetSimpleSearchableMembers()
        {
            return PropertyGroups.Where(x => x.IsSimpleSearchable).SelectMany(x => x.Properties);
        }

        public QueryResultToken<T> CreateSimpleSearchQuery(IEnumerable<string> terms)
        {
            return CreateQueryFor(terms, PropertyGroups.Where(x => x.IsSimpleSearchable).SelectMany(x => x.Properties));
        }

        public QueryResultToken<T> CreateQueryFor(IEnumerable<string> terms, params MemberInfo[] members)
        {
            return CreateQueryFor(terms, members);
        }

        public QueryResultToken<T> CreateQueryFor(IEnumerable<string> terms, params string[] members)
        {
            return CreateQueryFor(terms, PropertySelector.SelectPropertiesFrom<T>(members, StringComparison.CurrentCultureIgnoreCase));
        }

        public QueryResultToken<T> CreateQueryFor(IEnumerable<string> terms, IEnumerable<MemberInfo> members)
        {
            QueryResultToken<T> result = new QueryResultToken<T> { Query = QueryOver.Of<T>(), MembersToSearch = members.ToList()};

            foreach (var member in result.MembersToSearch)
            {
                var propertyGroup = PropertyGroups.FirstOrDefault(x => x.Properties.Any(y => y.Equals(member)));

                if (propertyGroup == null)
                {
                    result.Errors.Add("The member, {0}, declared no search strategy!  This is most likely because it is not searchable.  If you believe this is in error, please contact us.".FormatS(member.Name));
                }
                else
                {
                    var token = new IndividualQueryToken<T> { Member = member, Query = result.Query, Terms = terms };

                    propertyGroup.QueryProvider(token);

                    if (token.HasErrors)
                        result.Errors.AddRange(token.Errors);
                }
            }

            return result;
        }

        public QueryResultToken<T> CreateQueryFor(Dictionary<MemberInfo, List<string>> filters, IQueryOver<T, T> query)
        {
            QueryResultToken<T> result = new QueryResultToken<T> { Query = (QueryOver<T, T>)query, MembersToSearch = filters.Keys.ToList() };

            foreach (var filter in filters)
            {
                var propertyGroup = PropertyGroups.FirstOrDefault(x => x.Properties.Any(y => y.Equals(filter.Key)));

                if (propertyGroup == null)
                {
                    result.Errors.Add("The member, {0}, declared no search strategy!  This is most likely because it is not searchable.  If you believe this is in error, please contact us.".FormatS(filter.Key.Name));
                }
                else
                {
                    var token = new IndividualQueryToken<T> { Member = filter.Key, Query = result.Query, Terms = filter.Value };

                    propertyGroup.QueryProvider(token);

                    if (token.HasErrors)
                        result.Errors.AddRange(token.Errors);
                }
            }

            return result;
        }
    }
}
