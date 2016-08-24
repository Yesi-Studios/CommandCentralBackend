using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;

namespace CCServ.Authorization.Rules
{
    /// <summary>
    /// Returns true if the client is in the person's chain of command.
    /// </summary>
    public class IfInChainOfCommandRule : AuthorizationRuleBase
    {
        /// <summary>
        /// Returns true if the client is in the person's chain of command.
        /// </summary>
        /// <param name="authToken"></param>
        /// <returns></returns>
        public override bool AuthorizationOperation(AuthorizationToken authToken)
        {
            if (authToken.PersonFromClient == null)
                return false;

            var moduleName = this.ParentPropertyGroup.ParentModule.ModuleName;

            //First find the person's highest level in this module.
            var highestLevel = (Groups.PermissionGroupLevels)authToken.Client.PermissionGroups.SelectMany(x => x.Modules).Where(x => x.ModuleName.SafeEquals(moduleName)).Max(x => x.ParentPermissionGroup.AccessLevel);

            switch (highestLevel)
            {
                case Groups.PermissionGroupLevels.Command:
                    {
                        return authToken.Client.IsInSameCommandAs(authToken.PersonFromClient);
                    }
                case Groups.PermissionGroupLevels.Department:
                    {
                        return authToken.Client.IsInSameDepartmentAs(authToken.PersonFromClient);
                    }
                case Groups.PermissionGroupLevels.Division:
                    {
                        return authToken.Client.IsInSameDivisionAs(authToken.PersonFromClient);
                    }
                case Groups.PermissionGroupLevels.Self:
                case Groups.PermissionGroupLevels.None:
                    {
                        return false;
                    }
                default:
                    {
                        throw new NotImplementedException("In the switch between levels in the CoC determinations in Resolve().");
                    }
            }
        }
    }
}
