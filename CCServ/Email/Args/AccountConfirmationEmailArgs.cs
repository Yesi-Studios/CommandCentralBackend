﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;

namespace CCServ.Email.Args
{
    /// <summary>
    /// Encapsulates the arguments to the account confirmation thing.
    /// </summary>
    public class AccountConfirmationEmailArgs : BaseEmailArgs
    {
        /// <summary>
        /// The Id of the confirmation.
        /// </summary>
        public Guid ConfirmationId { get; set; }

        /// <summary>
        /// The link the client should click to continue on to the next page.
        /// </summary>
        public string ConfirmEmailAddressLink { get; set; }

        /// <summary>
        /// Returns the full continue link.
        /// </summary>
        public string FullContinueLink
        {
            get
            {
                var replaceMe = "{{confirmationid}}";

                if (!ConfirmEmailAddressLink.ContainsInsensitive(replaceMe, CultureInfo.CurrentCulture))
                {
                    return ConfirmEmailAddressLink + ConfirmationId;
                }
                else
                {
                    return ConfirmEmailAddressLink.Replace(replaceMe, ConfirmationId.ToString());
                }

            }
        }
    }
}
