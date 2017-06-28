using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandCentral.Authorization.Groups;

namespace CommandCentral.Authorization.Rules
{
    /// <summary>
    /// The base class for all authorization rules.  All authorization rules that inherit from this type are responsible for declaring their own operation by overriding the AuthorizationOperation method.
    /// </summary>
    public abstract class AuthorizationRuleBase
    {
        /// <summary>
        /// The parents of this rule.  This is where you can go to get the property names and the category.
        /// </summary>
        public PropertyGroupPart ParentPropertyGroup { get; set; }

        /// <summary>
        /// The method that should be called to determine if anything violated this rule.
        /// </summary>
        /// <param name="authToken"></param>
        /// <returns></returns>
        public abstract bool AuthorizationOperation(AuthorizationToken authToken);

        /// <summary>
        /// Creates a new AuthorizationRuleBase.
        /// </summary>
        public AuthorizationRuleBase()
        {
        }
    }
}
