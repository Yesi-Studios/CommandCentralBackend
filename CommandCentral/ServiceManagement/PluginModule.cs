using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace CommandCentral.ServiceManagement
{
    public class PluginModule
    {
        public Assembly Assembly { get; set; }

        public string Name { get; set; }

        #region Static Access

        public static ConcurrentDictionary<Guid, PluginModule> PluginModules { get; private set; }

        #endregion
    }
}
