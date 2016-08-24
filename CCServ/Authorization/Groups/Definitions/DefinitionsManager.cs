using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.Authorization.Groups.Definitions
{
    public static class DefinitionsManager
    {
        static DefinitionsManager()
        {
            Users = new Groups.Definitions.Users();
            DivisionLeadership = new Groups.Definitions.DivisionLeadership();
            DepartmentLeadership = new Groups.Definitions.DepartmentLeadership();
            Admin = new Groups.Definitions.Admin();
            CommandLeadership = new Groups.Definitions.CommandLeadership();
            Developers = new Groups.Definitions.Developers();
        }

        public static PermissionGroup Users;
        public static PermissionGroup DivisionLeadership;
        public static PermissionGroup DepartmentLeadership;
        public static PermissionGroup CommandLeadership;
        public static PermissionGroup Admin;
        public static PermissionGroup Developers;
    }
}
