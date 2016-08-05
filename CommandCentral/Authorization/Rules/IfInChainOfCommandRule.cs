using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Authorization.Rules
{
    /// <summary>
    /// Returns true if the client is in the person's chain of command.
    /// </summary>
    /// <typeparam name="T"></typeparam>
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

            return authToken.Client.IsInChainOfCommandOf(authToken.PersonFromClient, this.ParentPropertyGroup.ParentModule.ModuleName);
        }
    }
}
