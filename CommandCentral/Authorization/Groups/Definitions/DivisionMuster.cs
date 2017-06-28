using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;

namespace CommandCentral.Authorization.Groups.Definitions
{
    /// <summary>
    /// The division level muster group.
    /// </summary>
    public class DivisionMuster : PermissionGroup
    {
        /// <summary>
        /// The division level muster group.
        /// </summary>
        public DivisionMuster()
        {
            CanEditMembershipOf(typeof(DivisionMuster));

            HasAccessLevel(ChainOfCommandLevels.Division);

            HasChainOfCommand(ChainsOfCommand.Muster);
        }
    }
}
