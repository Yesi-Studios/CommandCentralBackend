using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Authorization.Rules
{
    public class IfInChainOfCommandRule : AuthorizationRuleBase
    {
        public IfInChainOfCommandRule(AuthorizationRuleCategoryEnum category, List<string> propertyNames) : base(category, propertyNames)
        {
        }

        public override bool AuthorizationOperation(AuthorizationToken authToken)
        {
            return authToken.Client.IsInChainOfCommandOf(authToken.OldPersonFromDB);
        }
    }
}
