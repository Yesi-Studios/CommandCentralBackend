using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Authorization.Rules
{
    public class IfCoCAndInPermissionGroupRule : AuthorizationRuleBase
    {
        public IfCoCAndInPermissionGroupRule(AuthorizationRuleCategoryEnum category, List<string> propertyNames)
            : base(category, propertyNames)
        {
        }

        public override bool AuthorizationOperation(AuthorizationToken authToken)
        {
            return new IfGrantedByPermissionGroupRule(ForCategory, PropertyNames).AuthorizationOperation(authToken) &&
                new IfInChainOfCommandRule(ForCategory, PropertyNames).AuthorizationOperation(authToken);
        }
    }
}
