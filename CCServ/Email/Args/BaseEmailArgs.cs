using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.Email.Args
{
    public abstract class BaseEmailArgs
    {
        List<string> ToList { get; set; }

        public BaseEmailArgs()
        {
            ToList = new List<string>();
        }
    }
}
