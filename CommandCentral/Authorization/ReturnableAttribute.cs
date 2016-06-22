using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Authorization
{
    public class ReturnableAttribute : Attribute
    {
        public List<IAuthorizationRule> Rules { get; set; }
    }
}
