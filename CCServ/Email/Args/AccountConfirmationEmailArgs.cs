using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.Email.Args
{
    /// <summary>
    /// Encapsulates the arguments to the account confirmation thing.
    /// </summary>
    public class AccountConfirmationEmailArgs
    {
        /// <summary>
        /// The Id of the confirmation.
        /// </summary>
        public Guid ConfirmationId { get; set; }

        /// <summary>
        /// The person's SSN.
        /// </summary>
        public string SSN { get; set; }
    }
}
