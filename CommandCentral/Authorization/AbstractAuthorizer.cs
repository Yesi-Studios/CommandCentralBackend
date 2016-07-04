using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;

namespace CommandCentral.Authorization
{
    /// <summary>
    /// Manages authorization rules for a given type.  These rules are intended to be used in generic use cases to determine a person's ability to edit or return elements of another's profile.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AbstractAuthorizer<T>
    {
        private List<RuleGroup<T>> _ruleGroups = new List<RuleGroup<T>>();

        /// <summary>
        /// Create a new authorization rule builder and appends the given property to the properties list.
        /// </summary>
        /// <typeparam name="PropertyT"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public AuthorizationRuleBuilder<T> RulesFor<PropertyT>(Expression<Func<T, PropertyT>> expression)
        {
            string propertyName = expression.GetPropertyName();

            var ruleBuilder = new AuthorizationRuleBuilder<T>();

            RuleGroup<T> group = new RuleGroup<T>(propertyName, ruleBuilder);
            ruleBuilder.ParentRuleGroup = group;



            _ruleGroups.Add(group);

            return ruleBuilder;
        }

        /// <summary>
        /// Returns all properties of this type that have declared they will ignore edits to themselves.
        /// </summary>
        /// <returns></returns>
        public List<string> GetPropertiesThatIgnoreEdit()
        {
            return _ruleGroups.Where(x => x.RuleBuilder.IgnoresGenericEdits).SelectMany(x => x.PropertyNames).ToList();
        }

        /// <summary>
        /// Given a client and a person and a rule category, returns all properties the client can either edit or return.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="newPersonFromClient"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        public List<string> GetAuthorizedProperties(Entities.Person client, Entities.Person newPersonFromClient, AuthorizationRuleCategoryEnum category)
        {
            if (category == AuthorizationRuleCategoryEnum.Null)
                throw new ArgumentException("Category can't be null.");

            List<string> properties = new List<string>();

            AuthorizationToken authToken = new AuthorizationToken(client, newPersonFromClient);

            foreach (var ruleGroup in _ruleGroups.Where(x => x.RuleBuilder.Disjunctions.Exists(y => y.Rules.First().ForCategory == category)))
            {
                if (category != AuthorizationRuleCategoryEnum.Edit || !ruleGroup.RuleBuilder.IgnoresGenericEdits)
                {
                    var passed = ruleGroup.RuleBuilder.Disjunctions.Where(x => x.Rules.Exists(y => y.ForCategory == category)).All(x => x.Evaluate(authToken));

                    if (passed)
                        properties.AddRange(ruleGroup.PropertyNames);
                }
            }

            return properties;
        }
    }
}
