using CommandCentral.Entities.Watchbill;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Email.Models
{
    /// <summary>
    /// The email model that is sent to the watchbill open for inputs template.
    /// </summary>
    public class WatchbillOpenForInputsEmailModel
    {
        /// <summary>
        /// The name or title of the watchbill referenced in the email.
        /// </summary>
        public string WatchbillTitle { get; set; }

        /// <summary>
        /// The collection of those people who are not qualified for any watches on this watchbill.
        /// </summary>
        public List<string> NotQualledPersonsFriendlyNames { get; set; }
    }
}
