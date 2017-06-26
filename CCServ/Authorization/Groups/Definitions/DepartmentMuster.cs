using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.Authorization.Groups.Definitions
{
    /// <summary>
    /// Defines the department level group that can muster personnel and nothing else.
    /// </summary>
    public class DepartmentMuster : PermissionGroup
    {
        /// <summary>
        /// Defines the department level group that can muster personnel and nothing else.
        /// </summary>
        public DepartmentMuster()
        {
            CanEditMembershipOf(typeof(DepartmentMuster), typeof(DivisionMuster));

            HasAccessLevel(ChainOfCommandLevels.Department);

            HasChainOfCommand(ChainsOfCommand.Muster);
        }
    }
}
