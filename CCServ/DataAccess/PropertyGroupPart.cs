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
    public class PropertyGroupPart<T>
    {
        public QueryStrategy<T> ParentQueryStrategy { get; set; }

        public List<MemberInfo> Properties { get; set; }

        public Func<QueryToken<T>, ICriterion> CriteriaProvider { get; set; }

        public SearchDataTypes SearchType { get; set; }

        public List<QueryTypes> QueryTypesUsedIn { get; set; }

        public bool AreIdintifiers { get; set; }

        public bool CanSearchIn(QueryTypes type)
        {
            return QueryTypesUsedIn.Contains(type);
        }

        public PropertyGroupPart<T> UsingStrategy(Func<QueryToken<T>, ICriterion> strat)
        {
            CriteriaProvider = strat;
            return this;
        }

        public PropertyGroupPart<T> CanBeUsedIn(params QueryTypes[] usedIn)
        {
            QueryTypesUsedIn = usedIn.ToList();
            return this;
        }

        public PropertyGroupPart<T> AsType(SearchDataTypes type)
        {
            SearchType = type;
            return this;
        }

        public PropertyGroupPart<T> UsedAsIdentifiers()
        {
            AreIdintifiers = true;
            return this;
        }

        public PropertyGroupPart()
        {
            QueryTypesUsedIn = new List<QueryTypes>();
        }
    }
}
