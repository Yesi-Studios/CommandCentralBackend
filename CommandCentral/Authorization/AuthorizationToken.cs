using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Authorization
{
    public class AuthorizationToken
    {
        
        /// <summary>
        /// The client - the owner of the current session who initiated the edit request.
        /// </summary>
        public Entities.Person Client { get; set; }

        /// <summary>
        /// The person who the client is trying to edit.
        /// </summary>
        public Entities.Person EditedPerson { get; set; }

        /// <summary>
        /// Indicates that the client is editing him or herself.
        /// </summary>
        public bool IsClientEditingSelf
        {
            get
            {
                return Client.Id == EditedPerson.Id;
            }
        }

        


    }
}
