using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Authorization
{
    public abstract class AuthorizationRuleBase
    {
        public List<string> PropertyNames { get; set; }

        public AuthorizationRuleCategoryEnum ForCategory { get; set; }

        public abstract bool AuthorizationOperation(AuthorizationToken authToken);

        public AuthorizationRuleBase(AuthorizationRuleCategoryEnum category, List<string> propertyNames)
        {
            if (category == AuthorizationRuleCategoryEnum.Null)
                throw new ArgumentException("Category mustn't be null.");

            this.ForCategory = category;
            this.PropertyNames = propertyNames.ToList();
        }
    }
}
