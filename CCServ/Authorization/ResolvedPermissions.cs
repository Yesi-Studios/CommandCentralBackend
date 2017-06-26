using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.Authorization
{
    /// <summary>
    /// Defines a class that stores a permission group after it has been refined.
    /// </summary>
    public class ResolvedPermissions
    {
        /// <summary>
        /// The list of permission groups' names that went into this resolved permission.
        /// </summary>
        public List<string> PermissionGroupNames { get; set; } = new List<string>();

        /// <summary>
        /// The time at which this resolved permission was built.
        /// </summary>
        public DateTime TimeResolved { get; set; }

        /// <summary>
        /// The Id of the client for whom this resolved permission was made.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// The person against which these permissions were resolved.
        /// </summary>
        public string PersonId { get; set; }

        /// <summary>
        /// The list of editable fields, broken down by what type they belong to.  The key is case insensitive.
        /// </summary>
        public Dictionary<string, List<string>> EditableFields { get; set; } = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// The list of returnable fields, broken down by what module and type they belong to.  The key is case insensitive.
        /// </summary>
        public Dictionary<string, List<string>> ReturnableFields { get; set; } = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// The list of fields that the client can return, with stipulations.
        /// </summary>
        public Dictionary<string, List<string>> PrivelegedReturnableFields { get; set; } = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// The highest levels in each of the chains of command that this client has.  The key is case insensitive.
        /// </summary>
        public Dictionary<ChainsOfCommand, ChainOfCommandLevels> HighestLevels { get; set; } = ((ChainsOfCommand[])Enum.GetValues(typeof(ChainsOfCommand))).ToDictionary(x => x, x => ChainOfCommandLevels.None);

        /// <summary>
        /// A dictionary where the key is a chain of command and the boolean indicates if the client is in that chain of command for the given person.
        /// <para/>
        /// Guarenteed to contain all chains of command.
        /// </summary>
        public Dictionary<ChainsOfCommand, bool> ChainOfCommandByModule { get; set; } = ((ChainsOfCommand[])Enum.GetValues(typeof(ChainsOfCommand))).ToDictionary(x => x, x => false);

        /// <summary>
        /// The list of those permission groups' names that this client can edit the membership of.
        /// </summary>
        public List<string> EditablePermissionGroups { get; set; } = new List<string>();

        /// <summary>
        /// The list of all submodules this client can access.
        /// </summary>
        public List<string> AccessibleSubmodules { get; set; } = new List<string>();

    }
}
