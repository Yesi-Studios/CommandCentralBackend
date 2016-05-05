using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.ClientAccess
{
    /// <summary>
    /// Declares that a field or property contains definitions for endpoints.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class EndpointsAttribute : Attribute
    {
    }
}
