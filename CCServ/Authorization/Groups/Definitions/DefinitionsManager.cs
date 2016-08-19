using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.Authorization.Groups.Definitions
{
    public static class DefinitionsManager
    {
        public static PermissionGroup Users = new Definitions.Users();
        public static PermissionGroup DivisionLeadership = new Definitions.DivisionLeadership();
        public static PermissionGroup Developers = new Definitions.Developers();
        public static PermissionGroup DepartmentLeadership = new Definitions.DepartmentLeadership();
        public static PermissionGroup CommandLeadership = new Definitions.CommandLeadership();
        public static PermissionGroup Admin = new Definitions.CommandLeadership();
    }
}
