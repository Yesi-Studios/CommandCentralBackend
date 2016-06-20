using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Authorization
{
    /// <summary>
    /// Describes the levels of a permissions.  Assists in determining chain of command.
    /// </summary>
    public enum PermissionLevels
    {
        /// <summary>
        /// A permission with this permission level may be exercised by a person on all other person's in his/her same command.
        /// </summary>
        Command,
        Department,
        Division,
        None
    }
}