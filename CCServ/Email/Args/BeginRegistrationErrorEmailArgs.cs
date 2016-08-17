using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.Email.Args
{
    public class BeginRegistrationErrorEmailArgs : BaseEmailArgs
    {
        /// <summary>
        /// The person Id.
        /// </summary>
        public Guid PersonID { get; set; }
    }
}
