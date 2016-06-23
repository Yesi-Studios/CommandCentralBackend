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

        public AuthorizationRuleBuilder<T> RulesFor<PropertyT>(Expression<Func<T, PropertyT>> expression)
        {
            string propertyName = expression.GetPropertyName();

            var ruleBuilder = new AuthorizationRuleBuilder<T>(propertyName);

            RuleGroup<T> group = new RuleGroup<T>(propertyName, ruleBuilder);

            _ruleGroups.Add(group);

            return ruleBuilder;
        }
    }
}
