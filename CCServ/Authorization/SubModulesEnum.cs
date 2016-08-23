using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.Authorization
{
    /// <summary>
    /// A list of all the submodules in the main module.  
    /// <para />
    /// These are turned into strings when they go into the permissions groups but at least here we have them strongly typed.
    /// </summary>
    public enum SubModules
    {
        /// <summary>
        /// The submodule for creating persons.  This is separate from the Admin Tools because a client might need to create users but not edit other things.
        /// </summary>
        CreatePerson,
        /// <summary>
        /// The admin tools, such as the list editors.
        /// </summary>
        AdminTools,
        /// <summary>
        /// The permission needed to edit the news.
        /// </summary>
        EditNews
    }
}
