using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;

namespace CommandCentral.Authorization
{
    public class AuthorizationRuleBuilder<T>
    {

        private List<string> propertyNames { get; set; }

        private AuthorizationRuleCategoryEnum currentCategory { get; set; }

        private RuleDisjunction currentDisjunction { get; set; }

        private List<RuleDisjunction> disjunctions { get; set; }

        public AuthorizationRuleBuilder(List<string> propertyNames)
        {
            this.propertyNames = propertyNames;
            this.currentCategory = AuthorizationRuleCategoryEnum.Null;


            disjunctions = new List<RuleDisjunction>();
            this.currentDisjunction = new RuleDisjunction();
            this.disjunctions.Add(this.currentDisjunction);

        }

        public AuthorizationRuleBuilder<T> And()
        {
            currentDisjunction = new RuleDisjunction();
            disjunctions.Add(currentDisjunction);

            return this;
        }

        public AuthorizationRuleBuilder<T> Or()
        {
            return this;
        }

        public AuthorizationRuleBuilder<T> Returnable()
        {
            currentCategory = AuthorizationRuleCategoryEnum.Return;
            return this;
        }

        public AuthorizationRuleBuilder<T> Editable()
        {
            currentCategory = AuthorizationRuleCategoryEnum.Edit;
            return this;
        }

        public AuthorizationRuleBuilder<T> IfSelf()
        {
            currentDisjunction.Rules.Add(new Rules.IfSelfRule(currentCategory, propertyNames));
            return this;
        }

        public AuthorizationRuleBuilder<T> Never()
        {
            currentDisjunction.Rules.Add(new Rules.NeverRule(currentCategory, propertyNames));
            return this;
        }

        public AuthorizationRuleBuilder<T> IfInChainOfCommand()
        {
            currentDisjunction.Rules.Add(new Rules.IfInChainOfCommandRule(currentCategory, propertyNames));
            return this;
        }

        public AuthorizationRuleBuilder<T> ForEveryone()
        {
            currentDisjunction.Rules.Add(new Rules.ForEveryoneRule(currentCategory, propertyNames));
            return this;
        }

        public AuthorizationRuleBuilder<T> IfGrantedByPermissionGroupRule()
        {
            currentDisjunction.Rules.Add(new Rules.IfGrantedByPermissionGroupRule(currentCategory, propertyNames));
            return this;
        }

    }
}
