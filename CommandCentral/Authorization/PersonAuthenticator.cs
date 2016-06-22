using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;

namespace CommandCentral.Authorization
{
    public class AbstractAuthorizor
    {
        private Dictionary<string, IAuthorizationRule[]> _rules = new Dictionary<string, IAuthorizationRule[]>();

        public void RulesFor(string propertyName, params IAuthorizationRule[] rules)
        {
            if (_rules.ContainsKey(propertyName))
                throw new ArgumentException("The property, '{0}', already has a rule defined for it.".FormatS(propertyName));

            _rules.Add(propertyName, rules);
        }
    }
}
