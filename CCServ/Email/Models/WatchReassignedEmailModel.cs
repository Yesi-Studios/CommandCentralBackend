using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Email.Models
{
    /// <summary>
    /// The email model that should be sent to the watch reassigned email templates.
    /// </summary>
    public class WatchReassignedEmailModel
    {
        /// <summary>
        /// The name of the person to whom this email will be sent.
        /// </summary>
        public string FriendlyName { get; set; }

        /// <summary>
        /// The name or title of the watchbill this email relates to.
        /// </summary>
        public string Watchbill { get; set; }

        /// <summary>
        /// The watch assignment that is referred to in this email.
        /// </summary>
        public Entities.Watchbill.WatchAssignment WatchAssignment { get; set; }
    }
}
