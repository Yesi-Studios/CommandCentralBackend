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
    public class QueryToken<T>
    {
        public QueryOver<T, T> Query { get; set; }

        private KeyValuePair<MemberInfo, object> _searchParameter;
        public KeyValuePair<MemberInfo, object> SearchParameter
        {
            get
            {
                return _searchParameter;
            }
            set
            {
                if (value.Value == null)
                {
                    throw new ArgumentException("You may not search for a null value.");
                }

                _searchParameter = value;
            }
        }

        public List<string> Errors { get; set; }

        public bool HasErrors
        {
            get
            {
                return Errors.Any();
            }
        }

        public QueryToken()
        {
            Errors = new List<string>();
        }
    }
}
