using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.Email.Args
{
    public class CompletedAccountRegistrationEmailArgs : BaseEmailArgs
    {
        /// <summary>
        /// The friendly name of the client who registered with us.
        /// </summary>
        public string FriendlyName { get; set; }
    }
}
