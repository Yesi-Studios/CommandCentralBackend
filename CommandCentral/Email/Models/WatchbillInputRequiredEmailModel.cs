using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Email.Models
{
    /// <summary>
    /// The email model that should be sent to the watch input required email template.
    /// </summary>
    public class WatchbillInputRequiredEmailModel
    {
        /// <summary>
        /// The name of the person to whom this email will be sent.
        /// </summary>
        public string FriendlyName { get; set; }

        /// <summary>
        /// The name or title of the watchbill this email relates to.
        /// </summary>
        public string Watchbill { get; set; }
    }
}
