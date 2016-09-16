using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NHibernate;

namespace CCServ.DataAccess
{
    public class PropertyGroupPart<T>
    {
        public QueryStrategy<T> ParentQueryStrategy { get; set; }

        public List<MemberInfo> Properties { get; set; }

        public Func<IEnumerable<string>, IQueryOver<T>, IQueryOver<T>> QueryProvider { get; set; }

        public void UseStrategy(Func<IEnumerable<string>, IQueryOver<T>, IQueryOver<T>> strategy)
        {
            QueryProvider = strategy;
        }
    }
}
