using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;

namespace CommandCentral.Email.Models
{
    public class BeginPasswordResetEmailModel
    {
        public Guid PasswordResetId { get; set; }

        public string FriendlyName { get; set; }

        /// <summary>
        /// The link the client should click to continue on to the next page.
        /// </summary>
        public string PasswordResetLink { get; set; }

        /// <summary>
        /// Returns the full continue link.
        /// </summary>
        public string FullContinueLink
        {
            get
            {
                var replaceMe = "{{passwordresetid}}";

                if (!PasswordResetLink.ContainsInsensitive(replaceMe, CultureInfo.CurrentCulture))
                {
                    return PasswordResetLink + PasswordResetId;
                }
                else
                {
                    return PasswordResetLink.Replace(replaceMe, PasswordResetId.ToString());
                }

            }
        }
    }
}
