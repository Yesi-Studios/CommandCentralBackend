using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral
{
    /// <summary>
    /// Describes the levels of chain of command.
    /// </summary>
    public enum ChainOfCommandLevels
    {
        /// <summary>
        /// The default level indicating no presence in a chain of command.
        /// </summary>
        None = 0,
        /// <summary>
        /// A person with this is considered to be in its own chain of command with control over itself only.
        /// </summary>
        Self = 1,
        /// <summary>
        /// The smallest grouping of personnel.
        /// </summary>
        Division = 2,
        /// <summary>
        /// Above a division and below a department.
        /// </summary>
        Department = 3,
        /// <summary>
        /// The largest grouping.
        /// </summary>
        Command = 4
    }
}