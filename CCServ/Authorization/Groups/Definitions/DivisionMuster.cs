using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;

namespace CCServ.Authorization.Groups.Definitions
{
    public class DivisionMuster : PermissionGroup
    {
        public DivisionMuster()
        {
            CanEditMembershipOf(typeof(DivisionMuster));

            HasAccessLevel(PermissionGroupLevels.Division);

            CanAccessModule("Muster");
        }
    }
}
