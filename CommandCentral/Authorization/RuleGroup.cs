using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Authorization
{
    public class RuleGroup<T>
    {
        public List<string> PropertyNames { get; set; }

        public AuthorizationRuleBuilder<T> RuleBuilder { get; set; }

        public RuleGroup(List<string> propertyNames, AuthorizationRuleBuilder<T> ruleBuilder)
        {
            this.PropertyNames = propertyNames;
            this.RuleBuilder = ruleBuilder;
        }
    }
}
