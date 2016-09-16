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
    public class IndividualQueryToken<T>
    {
        public QueryOver<T, T> Query { get; set; }

        public IEnumerable<string> Terms { get; set; }

        public MemberInfo Member { get; set; }

        public List<string> Errors { get; set; }

        public bool HasErrors
        {
            get
            {
                return Errors.Any();
            }
        }
    }
}
