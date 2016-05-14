using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.ClientAccess
{
    /// <summary>
    /// Provides members that describe an endpoint.  Intended to allow dynamic endpoint invocation.
    /// </summary>
    public class EndpointMethodAttribute : Attribute
    {
        /// <summary>
        /// The name of the endpoint.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Indicates whether or not an endpoint should require authentication.  All endpoints that require a session must have this set to true.
        /// </summary>
        public bool RequiresAuthentication { get; set; }

        /// <summary>
        /// Indicates whether or not the arguments the client submits to this endpoint should be logged.
        /// </summary>
        public bool AllowArgumentLogging { get; set; }

        /// <summary>
        /// Indicates whether or not the response this endpoint sends to the client should be logged.
        /// </summary>
        public bool AllowResponseLogging { get; set; }

        /// <summary>
        /// A description of the endpoint.  This is intended to be used such that clients can request a "manual" type of thing for endpoints.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// A list of all parameters that may be accessed in the arguments by this endpoint.
        /// </summary>
        public string[] Parameters { get; set; }

        /// <summary>
        /// Stores the descriptions of each of the parameters.
        /// </summary>
        public string[] ParameterDescriptions { get; set; }

        /// <summary>
        /// The list of all special permissions required to access this endpoint.
        /// </summary>
        public Authorization.SpecialPermissions[] RequiredSpecialPermissions { get; set; }
    }
}
