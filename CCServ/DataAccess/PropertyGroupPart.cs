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

        public Action<IndividualQueryToken<T>> QueryProvider { get; set; }

        public SearchDataTypesEnum SearchType { get; set; }

        public bool IsSimpleSearchable { get; set; }

        public PropertyGroupPart<T> UseStrategy(Action<IndividualQueryToken<T>> strategy)
        {
            QueryProvider = strategy;
            return this;
        }

        public PropertyGroupPart<T> AsType(SearchDataTypesEnum type)
        {
            SearchType = type;
            return this;
        }

        public PropertyGroupPart<T> SimpleSearchable()
        {
            IsSimpleSearchable = true;
            return this;
        }
    }
}
