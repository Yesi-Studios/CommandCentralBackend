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
        public AuthorizationRuleCategoryEnum CurrentCategory { get; set; }

        public RuleDisjunction CurrentDisjunction { get; set; }

        public List<RuleDisjunction> Disjunctions { get; set; }

        public RuleGroup<T> ParentRuleGroup { get; set; }

        public AuthorizationRuleBuilder(string propertyName)
        {
            this.ParentRuleGroup.PropertyNames = new List<string> { propertyName };
            this.CurrentCategory = AuthorizationRuleCategoryEnum.Null;


            Disjunctions = new List<RuleDisjunction>();
            this.CurrentDisjunction = new RuleDisjunction();
            this.Disjunctions.Add(this.CurrentDisjunction);

        }

        public AuthorizationRuleBuilder<T> AndFor<PropertyT>(Expression<Func<T, PropertyT>> expression)
        {
            this.ParentRuleGroup.PropertyNames.Add(expression.GetPropertyName());

            return this;
        }
             

        public AuthorizationRuleBuilder<T> And()
        {
            CurrentDisjunction = new RuleDisjunction();
            Disjunctions.Add(CurrentDisjunction);

            return this;
        }

        public AuthorizationRuleBuilder<T> Or()
        {
            return this;
        }

        public AuthorizationRuleBuilder<T> Returnable()
        {
            CurrentCategory = AuthorizationRuleCategoryEnum.Return;

            CurrentDisjunction = new RuleDisjunction();
            Disjunctions.Add(CurrentDisjunction);
            return this;
        }

        public AuthorizationRuleBuilder<T> Editable()
        {
            CurrentCategory = AuthorizationRuleCategoryEnum.Edit;

            CurrentDisjunction = new RuleDisjunction();
            Disjunctions.Add(CurrentDisjunction);
            return this;
        }

        public AuthorizationRuleBuilder<T> IfSelf()
        {
            CurrentDisjunction.Rules.Add(new Rules.IfSelfRule(CurrentCategory, this.ParentRuleGroup.PropertyNames));
            return this;
        }

        public AuthorizationRuleBuilder<T> Never()
        {
            CurrentDisjunction.Rules.Add(new Rules.NeverRule(CurrentCategory, this.ParentRuleGroup.PropertyNames));
            return this;
        }

        public AuthorizationRuleBuilder<T> IfInChainOfCommand()
        {
            CurrentDisjunction.Rules.Add(new Rules.IfInChainOfCommandRule(CurrentCategory, this.ParentRuleGroup.PropertyNames));
            return this;
        }

        public AuthorizationRuleBuilder<T> ForEveryone()
        {
            CurrentDisjunction.Rules.Add(new Rules.ForEveryoneRule(CurrentCategory, this.ParentRuleGroup.PropertyNames));
            return this;
        }

        public AuthorizationRuleBuilder<T> IfGrantedByPermissionGroup()
        {
            CurrentDisjunction.Rules.Add(new Rules.IfGrantedByPermissionGroupRule(CurrentCategory, this.ParentRuleGroup.PropertyNames));
            return this;
        }

        public AuthorizationRuleBuilder<T> IfHasSpecialPermission(SpecialPermissions specialPermission)
        {
            CurrentDisjunction.Rules.Add(new Rules.IfHasSpecialPermissionRule(CurrentCategory, this.ParentRuleGroup.PropertyNames, specialPermission));
            return this;
        }

        public AuthorizationRuleBuilder<T> IfSatisfiesPermissionGroupRule()
        {
            CurrentDisjunction.Rules.Add(new Rules.PermissionGroupSpecialRule(CurrentCategory, this.ParentRuleGroup.PropertyNames));
            return this;
        }

    }
}
