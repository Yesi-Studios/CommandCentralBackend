using System;
using System.Collections.Generic;

namespace CommandCentral.ClientAccess
{
    /// <summary>
    /// Provides members that describe an endpoint.  Intended to allow dynamic endpoint invocation.
    /// </summary>
    public class EndpointDescription
    {
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
        /// The data method that this endpoint will use to retrieve its data.  All data methods must take and return a message token.
        /// </summary>
        public Func<MessageToken, MessageToken> DataMethod { get; set; }

        /// <summary>
        /// A description of the endpoint.  This is intended to be used such that clients can request a "manual" type of thing for endpoints.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The parameters that the endpoint requires.  Intended to be used for a "manual" style thing that the client can call.
        /// </summary>
        public List<string> RequiredParameters { get; set; }

        /// <summary>
        /// The parameters that are optional for the endpoint.  Intended to be used for a "manual" style thing that the client can call.
        /// </summary>
        public List<string> OptionalParameters { get; set; }

        /// <summary>
        /// Indicates whether or not calls to the endpoint should be allowed.  This will allow us to disable endpoints for maintenance if needed.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// A method that, when run, produces a string that mimics expected output from the service for this endpoint.
        /// </summary>
        public Func<string> ExampleOutput { get; set; }

        /// <summary>
        /// A free text field that describes shortly what authorization takes place on this endpoint.
        /// </summary>
        public string AuthorizationNote { get; set; }

        /// <summary>
        /// Indicates what additional permissions this endpoint requires that a client have.
        /// </summary>
        public List<string> RequiredSpecialPermissions { get; set; }

    }
}
