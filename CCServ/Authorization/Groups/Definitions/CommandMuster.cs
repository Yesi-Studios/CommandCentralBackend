using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.Authorization.Groups.Definitions
{
    public class CommandMuster : PermissionGroup
    {
        public CommandMuster()
        {
            CanEditMembershipOf(typeof(CommandMuster), typeof(DepartmentMuster), typeof(DivisionMuster));

            HasAccessLevel(ChainOfCommandLevels.Command);

            HasChainOfCommand(ChainsOfCommand.Muster);
        }
    }
}
