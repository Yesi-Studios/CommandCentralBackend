using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.ClientAccess
{
    /// <summary>
    /// Defines a class as having endpoints within it.  This instructs the service that we should look within that type for all endpoints.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class HasEndpointsAttribute : Attribute
    {
    }
}
