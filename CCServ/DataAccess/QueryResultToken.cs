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

        public List<MemberInfo> MembersToSearch { get; set; }

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
            MembersToSearch = new List<MemberInfo>();

        }
    }
}
