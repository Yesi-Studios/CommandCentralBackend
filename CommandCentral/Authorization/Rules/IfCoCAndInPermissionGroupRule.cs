using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Authorization.Rules
{
    /// <summary>
    /// A rule that is a conjunction between permission group and chain of command.
    /// </summary>
    public class IfCoCAndInPermissionGroupRule : AuthorizationRuleBase
    {
        /// <summary>
        /// Ze constructor
        /// </summary>
        /// <param name="category"></param>
        /// <param name="propertyNames"></param>
        public IfCoCAndInPermissionGroupRule(AuthorizationRuleCategoryEnum category, List<string> propertyNames)
            : base(category, propertyNames)
        {
        }

        /// <summary>
        /// Creates a Coc and PG rule and then ands their operations together.
        /// </summary>
        /// <param name="authToken"></param>
        /// <returns></returns>
        public override bool AuthorizationOperation(AuthorizationToken authToken)
        {
            return new IfGrantedByPermissionGroupRule(ForCategory, PropertyNames).AuthorizationOperation(authToken) &&
                new IfInChainOfCommandRule(ForCategory, PropertyNames).AuthorizationOperation(authToken);
        }
    }
}
