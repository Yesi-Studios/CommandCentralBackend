﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using AtwoodUtils;
using System.Reflection;

namespace CommandCentral.Authorization.Groups
{
    /// <summary>
    /// Builds a property group.
    /// </summary>
    public class PropertyGroupPart
    {
        /// <summary>
        /// Indicates for which access category this property group was made.
        /// </summary>
        public AccessCategories AccessCategory { get; set; }

        /// <summary>
        /// The list of the properties referenced by this property group.
        /// </summary>
        public List<MemberInfo> Properties { get; set; }

        /// <summary>
        /// The list of disjunctions held in this property group.
        /// </summary>
        public List<Rules.RuleDisjunction> Disjunctions { get; set; }

        /// <summary>
        /// The module this property gruop belongs to.
        /// </summary>
        public ModulePart ParentModule { get; set; }

        /// <summary>
        /// Creates a new property group.
        /// </summary>
        public PropertyGroupPart(ModulePart module)
        {
            ParentModule = module;
            Properties = new List<MemberInfo>();
            Disjunctions = new List<Rules.RuleDisjunction>();
        }

        /// <summary>
        /// Adds an IfSelf rule to the disjunctions.
        /// </summary>
        /// <returns></returns>
        public PropertyGroupPart IfSelf()
        {
            Disjunctions.Add(new Rules.RuleDisjunction() { Rules = new List<Rules.AuthorizationRuleBase> { new Rules.IfSelfRule() } });
            return this;
        }

        /// <summary>
        /// Adds an IfInChainOfCommand rule to the disjunctions.
        /// </summary>
        /// <returns></returns>
        public PropertyGroupPart IfInChainOfCommand()
        {
            Disjunctions.Add(new Rules.RuleDisjunction() { Rules = new List<Rules.AuthorizationRuleBase> { new Rules.IfInChainOfCommandRule() } });
            return this;
        }

        /// <summary>
        /// Steps the build back up to the module.
        /// </summary>
        /// <returns></returns>
        public ModulePart And
        {
            get
            {
                return this.ParentModule;
            }
        }

    }
}