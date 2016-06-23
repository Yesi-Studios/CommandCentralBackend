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

        public List<string> GetAuthorizedFields(Entities.Person client, Entities.Person newPersonFromClient, AuthorizationRuleCategoryEnum category)
        {
            if (category == AuthorizationRuleCategoryEnum.Null)
                throw new ArgumentException("Category can't be null.");

            List<string> properties = new List<string>();

            AuthorizationToken authToken = new AuthorizationToken(client, newPersonFromClient);

            foreach (var ruleGroup in _ruleGroups.Where(x => x.RuleBuilder.Disjunctions.Exists(y => y.Rules.First().ForCategory == category)))
            {
                if (category != AuthorizationRuleCategoryEnum.Edit || !ruleGroup.RuleBuilder.IgnoresGenericEdits)
                {
                    var passed = ruleGroup.RuleBuilder.Disjunctions.All(x => x.Evaluate(authToken));

                    if (passed)
                        properties.AddRange(ruleGroup.PropertyNames);
                }
            }

            return properties;
        }
    }
}
