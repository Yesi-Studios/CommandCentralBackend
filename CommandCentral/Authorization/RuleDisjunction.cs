using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Authorization
{
    public class RuleDisjunction
    {
        public List<AuthorizationRuleBase> Rules { get; set; }

        public RuleDisjunction()
        {
            Rules = new List<AuthorizationRuleBase>();
        }

        public IEnumerable<AuthorizationRuleResult> Evaluate(AuthorizationToken authToken)
        {
            return Rules.Select(x => x.AuthorizationOperation(authToken));
        }
    }
}
