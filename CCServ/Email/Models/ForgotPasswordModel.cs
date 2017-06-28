using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Email.Models
{
    /// <summary>
    /// The model for the forgot password email.
    /// </summary>
    public class ForgotPasswordModel
    {
        /// <summary>
        /// The person's friendly name.
        /// </summary>
        public string FriendlyName { get; set; }

        /// <summary>
        /// The username to be sent.
        /// </summary>
        public string Username { get; set; }
    }
}
