using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandDB_Plugin
{
    /// <summary>
    /// Describes custom permissions that are intended to supplement what the unified service gives us.
    /// </summary>
    public enum CustomPermissionTypes
    {
        Add_New_User,
        Search_Users,
        Edit_Users,
        Division_Leadership,
        Department_Leadership,
        Command_Leadership,
        Can_Muster_Self,
        Can_Muster_Division,
        Can_Muster_Department,
        Can_Muster_Command,
        Developer,
        Manpower_Admin,
        Conduct_Muster,
        Manage_News,
        Super_User
    }


}
