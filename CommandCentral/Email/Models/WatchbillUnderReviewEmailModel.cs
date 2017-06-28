using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Email.Models
{
    /// <summary>
    /// The email model that is sent to the watchbill under review template.
    /// </summary>
    public class WatchbillUnderReviewEmailModel
    {
        /// <summary>
        /// The name or title of the watchbill referenced in the email.
        /// </summary>
        public string Watchbill { get; set; }
    }
}
