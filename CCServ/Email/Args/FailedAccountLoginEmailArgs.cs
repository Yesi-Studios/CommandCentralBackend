using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.Email.Args
{
    public class FailedAccountLoginEmailArgs : BaseEmailArgs
    {
        /// <summary>
        /// The friendly name of the person.
        /// </summary>
        public string FriendlyName { get; set; }
    }
}
