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
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class EndpointMethodAttribute : Attribute
    {
        /// <summary>
        /// The name of the endpoint.  If left blank, the name of the method itself should be used.
        /// </summary>
        public string EndpointName { get; set; }

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
        
    }
}
