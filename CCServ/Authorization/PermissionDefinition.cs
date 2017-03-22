using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.Authorization
{
    public class PermissionDefinition
    {

        public ChainOfCommand ChainOfCommand { get; set; }
        public ChainOfCommandLevels Level { get; set; }

    }
}
