using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;

namespace CCServ.Authorization.Groups.Definitions
{
    /// <summary>
    /// The permission group that is responsible for the quarterdeck watchbill at the division level.
    /// </summary>
    public class DivisionQuarterdeckWatchbill : PermissionGroup
    {
        /// <summary>
        /// The permission group that is responsible for the quarterdeck watchbill at the division level.
        /// </summary>
        public DivisionQuarterdeckWatchbill()
        {
            CanEditMembershipOf(typeof(DivisionQuarterdeckWatchbill));

            HasAccessLevel(ChainOfCommandLevels.Division);

            InChainsOfCommand(ChainsOfCommand.QuarterdeckWatchbill);

            CanAccessModule("Watchbill");
        }
    }
}
