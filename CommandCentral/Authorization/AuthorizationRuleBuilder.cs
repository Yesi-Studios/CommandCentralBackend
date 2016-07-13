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
    /// The generic rule builder for the given type.  Contains the Fluent-style chain.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AuthorizationRuleBuilder<T>
    {
        public AuthorizationRuleCategoryEnum CurrentCategory { get; set; }

        public RuleDisjunction CurrentDisjunction { get; set; }

        public List<RuleDisjunction> Disjunctions { get; set; }

        public RuleGroup<T> ParentRuleGroup { get; set; }

        public bool IgnoresGenericEdits { get; set; }

        public AuthorizationRuleBuilder()
        {
            this.CurrentCategory = AuthorizationRuleCategoryEnum.Null;

            this.IgnoresGenericEdits = false;


            Disjunctions = new List<RuleDisjunction>();
        }

        /// <summary>
        /// Adds a new property to the rule's property names collection.
        /// </summary>
        /// <typeparam name="PropertyT"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public AuthorizationRuleBuilder<T> AndFor<PropertyT>(Expression<Func<T, PropertyT>> expression)
        {
            this.ParentRuleGroup.PropertyNames.Add(expression.GetPropertyName());

            return this;
        }
             
        /// <summary>
        /// Creates a new disjunction and adds it to the disjunctions.  THe current disjunction becomes this new one.
        /// </summary>
        /// <returns></returns>
        public AuthorizationRuleBuilder<T> And()
        {
            CurrentDisjunction = new RuleDisjunction();
            Disjunctions.Add(CurrentDisjunction);

            return this;
        }

        /// <summary>
        /// Does nothing.  Syntatic sugar only.
        /// </summary>
        /// <returns></returns>
        public AuthorizationRuleBuilder<T> Or()
        {
            return this;
        }

        /// <summary>
        /// Indicates that properties in this rule should not be updated even if asked to be updated.  
        /// <para />
        /// If this rule is set, then a manual update is the only way to update these properties.
        /// <para />
        /// This rule element is most often paired with Editable().Never()
        /// </summary>
        /// <returns></returns>
        public AuthorizationRuleBuilder<T> MakeIgnoreGenericEdits()
        {
            IgnoresGenericEdits = true;
            return this;
        }

        /// <summary>
        /// Switches the current category to Return such that all rules coming after this call are rules about who can return a field.
        /// </summary>
        /// <returns></returns>
        public AuthorizationRuleBuilder<T> Returnable()
        {
            CurrentCategory = AuthorizationRuleCategoryEnum.Return;

            CurrentDisjunction = new RuleDisjunction();
            Disjunctions.Add(CurrentDisjunction);
            return this;
        }

        /// <summary>
        /// Switches the current category to Edit such that all rules coming after this call are rules about who can edit a field.
        /// </summary>
        /// <returns></returns>
        public AuthorizationRuleBuilder<T> Editable()
        {
            CurrentCategory = AuthorizationRuleCategoryEnum.Edit;

            CurrentDisjunction = new RuleDisjunction();
            Disjunctions.Add(CurrentDisjunction);
            return this;
        }

        /// <summary>
        /// Adds if self rule to the current disjunction, indicating a person can only return or edit a field if it is their own.
        /// </summary>
        /// <returns></returns>
        public AuthorizationRuleBuilder<T> IfSelf()
        {
            CurrentDisjunction.Rules.Add(new Rules.IfSelfRule(CurrentCategory, this.ParentRuleGroup.PropertyNames));
            return this;
        }

        /// <summary>
        /// Adds a Never rule to the current disjunction which returns false at all times.
        /// </summary>
        /// <returns></returns>
        public AuthorizationRuleBuilder<T> Never()
        {
            CurrentDisjunction.Rules.Add(new Rules.NeverRule(CurrentCategory, this.ParentRuleGroup.PropertyNames));
            return this;
        }

        /// <summary>
        /// Adds the if in coc rule to the current disjunction.
        /// </summary>
        /// <returns></returns>
        public AuthorizationRuleBuilder<T> IfInChainOfCommand()
        {
            CurrentDisjunction.Rules.Add(new Rules.IfInChainOfCommandRule(CurrentCategory, this.ParentRuleGroup.PropertyNames));
            return this;
        }

        /// <summary>
        /// Adds the for everyone rule to the current disjunction.  This returns true at all times.
        /// </summary>
        /// <returns></returns>
        public AuthorizationRuleBuilder<T> ForEveryone()
        {
            CurrentDisjunction.Rules.Add(new Rules.ForEveryoneRule(CurrentCategory, this.ParentRuleGroup.PropertyNames));
            return this;
        }

        /// <summary>
        /// Adds the if in PG rule to the current disjunction.
        /// </summary>
        /// <returns></returns>
        public AuthorizationRuleBuilder<T> IfGrantedByPermissionGroup()
        {
            CurrentDisjunction.Rules.Add(new Rules.IfGrantedByPermissionGroupRule(CurrentCategory, this.ParentRuleGroup.PropertyNames));
            return this;
        }

        /// <summary>
        /// Adds the special permission rule to the current disjunction for the given special permission.
        /// </summary>
        /// <param name="specialPermission"></param>
        /// <returns></returns>
        public AuthorizationRuleBuilder<T> IfHasSpecialPermission(SpecialPermissions specialPermission)
        {
            CurrentDisjunction.Rules.Add(new Rules.IfHasSpecialPermissionRule(CurrentCategory, this.ParentRuleGroup.PropertyNames, specialPermission));
            return this;
        }

        /// <summary>
        /// Adds the If in coc and permission group rules as a conjunction to the current disjunction.
        /// </summary>
        /// <returns></returns>
        public AuthorizationRuleBuilder<T> IfInCoCAndInPermissionGroup()
        {
            CurrentDisjunction.Rules.Add(new Rules.IfCoCAndInPermissionGroupRule(CurrentCategory, this.ParentRuleGroup.PropertyNames));
            return this;
        }

    }
}
