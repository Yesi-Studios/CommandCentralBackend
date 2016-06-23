using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Authorization.Rules
{
    public class IfHasSpecialPermissionRule : AuthorizationRuleBase
    {
        private SpecialPermissions specialPermission { get; set; }

        public IfHasSpecialPermissionRule(AuthorizationRuleCategoryEnum category, List<string> propertyNames, SpecialPermissions specialPermission) : base(category, propertyNames)
        {
            this.specialPermission = specialPermission;
        }

        public override bool AuthorizationOperation(AuthorizationToken authToken)
        {
            return authToken.Client.PermissionGroups.SelectMany(x => x.SpecialPermissions).Contains(this.specialPermission);
        }
    }
}
