using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;

namespace CCServ.Authorization.Groups.Definitions
{
    class DivisionMuster : PermissionGroup
    {
        public DivisionMuster()
        {
            CanEditMembershipOf(typeof(Users), typeof(DivisionLeadership));

            HasAccessLevel(PermissionGroupLevels.Division);

            CanAccessModule("Muster");
        }
    }
}
