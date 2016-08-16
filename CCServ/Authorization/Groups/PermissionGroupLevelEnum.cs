using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.Authorization.Groups
{
    /// <summary>
    /// Describes the levels of a permissions.  Assists in determining chain of command.
    /// </summary>
    public enum PermissionGroupLevels
    {
        /// <summary>
        /// The default permission level indicating no permission to exercise the powers in this permission.
        /// </summary>
        None = 0,
        /// <summary>
        /// A permission with this level may be execrecised on the person's self only.
        /// </summary>
        Self = 1,
        /// <summary>
        /// A permission with this permission level may be exercised by a person on all other person's in his/her same division.
        /// </summary>
        Division = 2,
        /// <summary>
        /// A permission with this permission level may be exercised by a person on all other person's in his/her same department.
        /// </summary>
        Department = 3,
        /// <summary>
        /// A permission with this permission level may be exercised by a person on all other person's in his/her same command.
        /// </summary>
        Command = 4
    }
}