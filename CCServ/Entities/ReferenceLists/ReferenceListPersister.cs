using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.Entities.ReferenceLists
{
    /// <summary>
    /// This class contains the method that is responsible for ensuring that all non-editable reference lists exist in the database as they are entered in the code.
    /// <para />   
    /// If a discrepency occurs, the persister should deal with it.  
    /// </summary>
    static class ReferenceListPersister
    {
        [ServiceManagement.StartMethod(Priority = 11)]
        static void PersistNonEditableReferenceLists(CLI.Options.LaunchOptions options)
        {
            Logging.Log.Info("Begin non-editable reference list persistence...");
        }
    }
}
