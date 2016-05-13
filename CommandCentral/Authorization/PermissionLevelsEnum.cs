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
        Command,
        Department,
        Division,
        None
    }
}
