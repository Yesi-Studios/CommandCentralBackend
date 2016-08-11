using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.Config
{
    /// <summary>
    /// Defines configuration stuff for contact details for the developers.  This will carry the developers' distro, for example.
    /// </summary>
    public static class ContactDetails
    {
        /// <summary>
        /// The email address to which emails should be sent to communicate with the developers.
        /// </summary>
        public const string DEV_DISTRO = "usn.gordon.inscom.list.nsag-nioc-ga-webmaster@mail.mil";

        /// <summary>
        /// The devs' phone number.
        /// </summary>
        public const string DEV_PHONE_NUMBER = "505-401-7252";
    }
}
