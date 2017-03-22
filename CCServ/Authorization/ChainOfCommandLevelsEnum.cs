using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.Authorization
{
    /// <summary>
    /// Describes the levels of chain of command.
    /// </summary>
    public enum ChainOfCommandLevels
    {
        /// <summary>
        /// The smallest grouping of personnel.
        /// </summary>
        Division = 0,
        /// <summary>
        /// Above a division and below a department.
        /// </summary>
        Department = 1,
        /// <summary>
        /// The largest grouping.
        /// </summary>
        Command = 2
    }
}