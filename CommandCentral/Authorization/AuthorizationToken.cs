using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Authorization
{
    /// <summary>
    /// Provides an object for wrapping two persons together in an authorization context.
    /// </summary>
    public class AuthorizationToken
    {
        /// <summary>
        /// The client - the owner of the current session who initiated the edit request.
        /// </summary>
        public Entities.Person Client { get; set; }

        /// <summary>
        /// The new version of the person the client sent us to be edited.
        /// </summary>
        public Entities.Person PersonFromClient { get; set; }

        /// <summary>
        /// Creates a new Auth token setting the client and the person from client to the corresponding values.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="newPersonFromClient"></param>
        public AuthorizationToken(Entities.Person client, Entities.Person newPersonFromClient)
        {
            this.Client = client;
            this.PersonFromClient = newPersonFromClient;
        }
    }
}
