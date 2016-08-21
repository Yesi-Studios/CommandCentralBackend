using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            var resolvedPermissions = authToken.Client.PermissionGroups.Resolve(authToken.Client, authToken.PersonFromClient);

            var moduleName = this.ParentPropertyGroup.ParentModule.ModuleName;

            if (!resolvedPermissions.ChainOfCommandByModule.ContainsKey(moduleName))
                return false;

            return resolvedPermissions.ChainOfCommandByModule[moduleName];
        }
    }
}
