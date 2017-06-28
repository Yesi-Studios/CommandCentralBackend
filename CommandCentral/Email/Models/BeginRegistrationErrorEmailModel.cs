using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Email.Models
{
    /// <summary>
    /// Contains the parameters for the model sent to the begin registration error email.
    /// </summary>
    public class BeginRegistrationErrorEmailModel
    {
        /// <summary>
        /// The person's friendly name.
        /// </summary>
        public string FriendlyName { get; set; }
    }
}
