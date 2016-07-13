using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;

namespace CommandCentral.Authorization.Rules
{
    /// <summary>
    /// A rule to determine if the property names of this rule are editable/returnable by the client.
    /// </summary>
    public class IfGrantedByPermissionGroupRule : AuthorizationRuleBase
    {
        /// <summary>
        /// Die Condtruuctor
        /// </summary>
        /// <param name="category"></param>
        /// <param name="propertyNames"></param>
        public IfGrantedByPermissionGroupRule(AuthorizationRuleCategoryEnum category, List<string> propertyNames) 
            : base(category, propertyNames)
        {
        }

        /// <summary>
        /// A rule to determine if the property names of this rule are editable/returnable by the client.
        /// </summary>
        /// <param name="authToken"></param>
        /// <returns></returns>
        public override bool AuthorizationOperation(AuthorizationToken authToken)
        {
            var fields = new List<string>();

            switch (this.ForCategory)
            {
                case AuthorizationRuleCategoryEnum.Edit:
                    {
                        fields = authToken.Client.PermissionGroups.SelectMany(x => x.ModelPermissions).SelectMany(x => x.EditableFields).ToList();
                        break;
                    }
                case AuthorizationRuleCategoryEnum.Return:
                    {
                        fields = authToken.Client.PermissionGroups.SelectMany(x => x.ModelPermissions).SelectMany(x => x.ReturnableFields).ToList();
                        break;
                    }
                default:
                    {
                        throw new NotImplementedException("An invalid case made it to the category switch in the if granted by permission group rule auth operation method.  The case was: '{0}'".FormatS(this.ForCategory.ToString()));
                    }
            }

            return PropertyNames.All(x => fields.Contains(x));
        }
    }
}
