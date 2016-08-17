using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.Email.Args
{
    /// <summary>
    /// The base of all email args.
    /// </summary>
    public abstract class BaseEmailArgs
    {
        /// <summary>
        /// The list of people to send the email to.  The Address.
        /// </summary>
        public List<string> ToAddressList { get; set; }

        /// <summary>
        /// The email's subject.
        /// </summary>
        public string Subject { get; set; }

        public DateTime DateTime { get; set; }

        /// <summary>
        /// Makes a new base email args.
        /// </summary>
        public BaseEmailArgs()
        {
            ToAddressList = new List<string>();
            DateTime = DateTime.Now;
        }
    }
}
