using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.Authorization.Groups.Definitions
{
    class CommandMuster : PermissionGroup
    {
        public CommandMuster()
        {
            CanEditMembershipOf(typeof(Users), typeof(DivisionLeadership));

            HasAccessLevel(PermissionGroupLevels.Command);

            CanAccessModule("Muster");
        }
    }
}
