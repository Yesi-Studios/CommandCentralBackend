using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.Authorization.Groups.Definitions
{
    /// <summary>
    /// The permission group that is responsible for the quarterdeck watchbill at the command level.
    /// </summary>
    public class CommandQuarterdeckWatchbill : PermissionGroup
    {
        /// <summary>
        /// The permission group that is responsible for the quarterdeck watchbill at the command level.
        /// </summary>
        public CommandQuarterdeckWatchbill()
        {
            CanEditMembershipOf(typeof(DivisionQuarterdeckWatchbill), typeof(DepartmentQuarterdeckWatchbill), typeof(CommandQuarterdeckWatchbill));

            HasAccessLevel(ChainOfCommandLevels.Command);

            InChainsOfCommand(ChainsOfCommand.QuarterdeckWatchbill);

            CanAccessModule("Watchbill");

            CanAccessSubModules(SubModules.CreateWatchbill);
        }

    }
}
