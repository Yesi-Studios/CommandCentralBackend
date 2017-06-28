using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Authorization.Groups.Definitions
{
    /// <summary>
    /// The permission group that is responsible for the quarterdeck watchbill at the department level.
    /// </summary>
    public class DepartmentQuarterdeckWatchbill : PermissionGroup
    {
        /// <summary>
        /// The permission group that is responsible for the quarterdeck watchbill at the department level.
        /// </summary>
        public DepartmentQuarterdeckWatchbill()
        {
            CanEditMembershipOf(typeof(DivisionQuarterdeckWatchbill), typeof(DepartmentQuarterdeckWatchbill));

            HasAccessLevel(ChainOfCommandLevels.Department);

            HasChainOfCommand(ChainsOfCommand.QuarterdeckWatchbill);
        }

    }
}
