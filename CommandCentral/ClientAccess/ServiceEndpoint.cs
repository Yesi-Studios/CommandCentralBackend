using System;
using System.Collections.Generic;

namespace CommandCentral.ClientAccess
{
    /// <summary>
    /// Provides members that describe an endpoint.  Intended to allow dynamic endpoint invocation.
    /// </summary>
    public class ServiceEndpoint
    {
        /// <summary>
        /// The data method that this endpoint will use to retrieve its data.  All data methods must take a message token.
        /// </summary>
        public Action<MessageToken> EndpointMethod { get; set; }

        /// <summary>
        /// Indicates whether or not calls to the endpoint should be allowed.  This will allow us to disable endpoints for maintenance if needed.
        /// </summary>
        public bool IsActive { get; set; }
        
        /// <summary>
        /// The endpoint method's attribute which describes information about it.
        /// </summary>
        public EndpointMethodAttribute EndpointMethodAttribute { get; set; }
    }
}
