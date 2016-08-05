using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Authorization
{
    /// <summary>
    /// A list of all the submodules in the main module.  
    /// <para />
    /// These are turned into strings whe nthey go into the permissions groups but at least here we have them strongly typed.
    /// </summary>
    public enum SubModules
    {
        CreatePerson,
        AdminTools,
        EditNews,
        Muster
    }
}
