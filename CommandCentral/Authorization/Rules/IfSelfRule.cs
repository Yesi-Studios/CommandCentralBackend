using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Authorization.Rules
{
    public class IfSelfRule : AuthorizationRuleBase
    {
        public IfSelfRule(AuthorizationRuleCategoryEnum category, List<string> propertyNames) : base(category, propertyNames)
        {
        }

        public override bool AuthorizationOperation(AuthorizationToken authToken)
        {
            return authToken.Client.Id == authToken.NewPersonFromClient.Id;
        }
    }
}
