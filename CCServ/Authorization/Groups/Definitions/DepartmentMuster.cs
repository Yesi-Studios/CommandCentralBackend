using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.Authorization.Groups.Definitions
{
    class DepartmentMuster : PermissionGroup
    {
        public DepartmentMuster()
        {
            CanEditMembershipOf(typeof(Users), typeof(DivisionLeadership));

            HasAccessLevel(PermissionGroupLevels.Department);

            CanAccessModule("Muster");
        }
    }
}
