using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;

namespace CommandCentral.Authorization
{
    public class AbstractAuthorizor<T>
    {
        private Dictionary<string, IAuthorizationRule[]> _rules = new Dictionary<string, IAuthorizationRule[]>(StringComparer.OrdinalIgnoreCase);

        public void RulesFor(string propertyName, params IAuthorizationRule[] rules)
        {
            if (_rules.ContainsKey(propertyName))
                throw new ArgumentException("The property, '{0}', already has a rule defined for it.".FormatS(propertyName));

            _rules.Add(propertyName, rules);
        }

        private string GetPropertyName<T, TReturn>(this Expression<Func<T, TReturn>> expression)
        {
            return ((MemberExpression)expression.Body).Member.Name;
        }
    }
}
