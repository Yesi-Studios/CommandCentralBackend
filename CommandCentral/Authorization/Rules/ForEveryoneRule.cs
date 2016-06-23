using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Authorization.Rules
{
    public class ForEveryoneRule : AuthorizationRuleBase
    {
        public ForEveryoneRule(AuthorizationRuleCategoryEnum category, List<string> propertyNames) : base(category, propertyNames)
        {
        }

        public override bool AuthorizationOperation(AuthorizationToken authToken)
        {
            return true;
        }
    }
}
