using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Authorization
{
    /// <summary>
    /// The base class for all authorization rules.  All authorization rules that inherit from this type are responsible for declaring their own operation by overriding the AuthorizationOperation method.
    /// </summary>
    public abstract class AuthorizationRuleBase
    {
        /// <summary>
        /// The list of properties to which this rulre will apply.
        /// </summary>
        public List<string> PropertyNames { get; set; }

        /// <summary>
        /// For what category is this rule created?
        /// </summary>
        public AuthorizationRuleCategoryEnum ForCategory { get; set; }

        /// <summary>
        /// The method that should be called to determine if anything violated this rule.
        /// </summary>
        /// <param name="authToken"></param>
        /// <returns></returns>
        public abstract bool AuthorizationOperation(AuthorizationToken authToken);

        /// <summary>
        /// Creates a new AuthorizationRuleBase with the given category and the list of property names.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="propertyNames"></param>
        public AuthorizationRuleBase(AuthorizationRuleCategoryEnum category, List<string> propertyNames)
        {
            if (category == AuthorizationRuleCategoryEnum.Null)
                throw new ArgumentException("Category mustn't be null.");

            this.ForCategory = category;
            this.PropertyNames = propertyNames.ToList();
        }
    }
}
