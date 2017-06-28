using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Email.Models
{
    /// <summary>
    /// The email model to be used with the like named email template.
    /// </summary>
    public class WatchInputRequirementsEmailModel
    {
        /// <summary>
        /// The person to whom we're sending this email.
        /// </summary>
        public Entities.Person Person { get; set; }

        /// <summary>
        /// The watchbill referenced in the email we're sending.
        /// </summary>
        public Entities.Watchbill.Watchbill Watchbill { get; set; }

        /// <summary>
        /// The list of personnel who do not have inputs.
        /// </summary>
        public IEnumerable<Entities.Person> PersonsWithoutInputs { get; set; }
    }
}
