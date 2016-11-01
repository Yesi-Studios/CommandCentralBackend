using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Criterion;

namespace CCServ.DataAccess
{
    public class QueryResultToken<T>
    {
        public QueryOver<T, T> Query { get; set; }

        public Dictionary<MemberInfo, object> SearchParameter { get; set; }

        public List<string> Errors { get; set; }

        public bool HasErrors
        {
            get
            {
                return Errors.Any();
            }
        }

        public QueryResultToken()
        {
            Errors = new List<string>();
            SearchParameter = new Dictionary<MemberInfo, object>();
        }
    }
}
