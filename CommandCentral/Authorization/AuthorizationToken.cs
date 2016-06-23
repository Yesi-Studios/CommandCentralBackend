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
        /// The new version of the person the client sent us to be edited.
        /// </summary>
        public Entities.Person NewPersonFromClient { get; set; }

        /// <summary>
        /// The old version of the person as it currently looks in the database, prior to any update.
        /// </summary>
        public Entities.Person OldPersonFromDB { get; set; }
    }
}
