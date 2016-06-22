using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Authorization
{
    public class ReturnableAttribute : Attribute
    {
        public IAuthorizationRule[] Rules { get; set; }

        public ReturnableAttribute(params Type[] rules)
        {


            //this.Rules = rules;
        }
    }
}
