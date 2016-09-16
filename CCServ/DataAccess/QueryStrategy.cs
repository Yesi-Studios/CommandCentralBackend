using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NHibernate;
using AtwoodUtils;

namespace CCServ.DataAccess
{
    public abstract class QueryStrategy<T>
    {
        List<PropertyGroupPart<T>> PropertyGroups { get; set; }

        public PropertyGroupPart<T> ForProperties(params MemberInfo[] members)
        {
            PropertyGroupPart<T> part = new PropertyGroupPart<T>
            {
                ParentQueryStrategy = this,
                Properties = members.ToList(),
                QueryProvider = null //TODO maybe provider a default here.
            };

            return part;
        }

        public IQueryOver<T> CreateQueryFor(params MemberInfo[] members)
        {

        }

        public IQueryOver<T> CreateQueryFor(params string[] members)
        {
        }
    }
}
