using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Authorization.Rules
{
    public class NeverRule : AuthorizationRuleBase
    {
        public NeverRule(AuthorizationRuleCategoryEnum category, List<string> propertyNames) : base(category, propertyNames)
        {
        }

        public override bool AuthorizationOperation(AuthorizationToken authToken)
        {
            return false;
        }
    }
}
