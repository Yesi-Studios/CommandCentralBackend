using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Email.Models
{
    /// <summary>
    /// The email model sent along with the password changed email.
    /// </summary>
    public class PasswordChangedEmailModel
    {
        /// <summary>
        /// The name of the user whose password changed.
        /// </summary>
        public string FriendlyName { get; set; } 
    }
}
