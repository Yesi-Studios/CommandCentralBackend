﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.Email.Models
{
    public class FatalErrorEmailModel
    {
        /// <summary>
        /// The original message.
        /// </summary>
        public string OriginalMessage { get; set; }

        /// <summary>
        /// The exception to send.
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// The token...
        /// </summary>
        public ClientAccess.MessageToken Token { get; set; }

        public string ClientPermissionNames
        {
            get
            {
                if (Token.AuthenticationSession == null)
                    return "null";

                return String.Join(", ", Token.AuthenticationSession.Person.PermissionGroupNames);
            }
        }
    }
}
