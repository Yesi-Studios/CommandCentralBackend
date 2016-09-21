using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.Authorization.Groups.Definitions
{
    public class DepartmentMuster : PermissionGroup
    {
        public DepartmentMuster()
        {
            CanEditMembershipOf(typeof(DepartmentMuster), typeof(DivisionMuster));

            HasAccessLevel(PermissionGroupLevels.Department);

            CanAccessModule("Muster");
        }
    }
}
