using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Authorization.Rules
{
    /// <summary>
    /// Indicates a rule disjunction.
    /// </summary>
    public class RuleDisjunction
    {
        /// <summary>
        /// The rules that are included in this Disjunction.
        /// </summary>
        public List<AuthorizationRuleBase> Rules { get; set; }

        /// <summary>
        /// Creates a new disjunction.
        /// </summary>
        public RuleDisjunction()
        {
            Rules = new List<AuthorizationRuleBase>();
        }

        /// <summary>
        /// Executes the Authorization Operation on each of the rules in this disjunction.
        /// </summary>
        /// <param name="authToken"></param>
        /// <returns></returns>
        public bool Evaluate(AuthorizationToken authToken)
        {
            return Rules.Any(x => x.AuthorizationOperation(authToken));
        }
    }
}
