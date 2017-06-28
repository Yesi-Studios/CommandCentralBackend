using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Email.Models
{
    /// <summary>
    /// The model intended for the finish email template.
    /// </summary>
    public class FinishPasswordResetEmailModel
    {
        /// <summary>
        /// The person's .ToString() name.
        /// </summary>
        public string FriendlyName { get; set; }
    }
}
