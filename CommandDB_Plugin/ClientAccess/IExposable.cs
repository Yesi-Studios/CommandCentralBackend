using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.ClientAccess
{
    /// <summary>
    /// Allows a class to volunteer methods to be exposed to the client.
    /// </summary>
    public interface IExposable
    {
        /// <summary>
        /// Describes the endpoints that should be exposed to the client.
        /// </summary>
        public Dictionary<string, EndpointDescription> EndpointDescriptions { get; }
    }
}
