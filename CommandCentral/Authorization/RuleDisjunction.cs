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

        public bool Evaluate(AuthorizationToken authToken)
        {
            return Rules.Any(x => x.AuthorizationOperation(authToken));
        }
    }
}
