using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.ServiceManagement
{
    public class StartMethodAttribute : Attribute
    {
        public int Priority { get; set; }
    }
}
