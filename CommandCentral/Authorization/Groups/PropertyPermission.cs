using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Authorization.Groups
{
    public class PropertyPermission
    {
        public ModulePermission ParentModulePermission { get; set; }

        public Type Type { get; set; }

        public string PropertyName { get; set; }
    }
}
