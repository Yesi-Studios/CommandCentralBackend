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
        private List<RuleGroup<T>> _ruleGroups = new List<RuleGroup<T>>();

        public AuthorizationRuleBuilder<T> RulesFor<PropertyT>(params Expression<Func<T, PropertyT>>[] expressions)
        {
            List<string> propertyNames = expressions.Select(x => x.GetPropertyName()).ToList();

            foreach (var propertyName in propertyNames)
            {
                if (_ruleGroups.Any(y => y.PropertyNames.Contains(propertyName)))
                    throw new ArgumentException("The property, '{0}', already has a rule defined for it.".FormatS(propertyName));
            }

            var ruleBuilder = new AuthorizationRuleBuilder<T>(propertyNames);

            RuleGroup<T> group = new RuleGroup<T>(propertyNames, ruleBuilder);

            _ruleGroups.Add(group);

            return ruleBuilder;
        }
    }
}
