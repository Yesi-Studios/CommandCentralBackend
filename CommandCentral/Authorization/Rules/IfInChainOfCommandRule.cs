using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;

namespace CommandCentral.Authorization.Rules
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

            var coc = this.ParentPropertyGroup.ParentCoC.ChainOfCommand;

            //First find the person's highest level in this module.
            var highestLevel = (ChainOfCommandLevels)authToken.Client.PermissionGroups.SelectMany(x => x.ChainsOfCommandParts).Where(x => x.ChainOfCommand == coc).Max(x => x.ParentPermissionGroup.AccessLevel);

            switch (highestLevel)
            {
                case ChainOfCommandLevels.Command:
                    {
                        return authToken.Client.IsInSameCommandAs(authToken.PersonFromClient);
                    }
                case ChainOfCommandLevels.Department:
                    {
                        return authToken.Client.IsInSameDepartmentAs(authToken.PersonFromClient);
                    }
                case ChainOfCommandLevels.Division:
                    {
                        return authToken.Client.IsInSameDivisionAs(authToken.PersonFromClient);
                    }
                case ChainOfCommandLevels.Self:
                case ChainOfCommandLevels.None:
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
