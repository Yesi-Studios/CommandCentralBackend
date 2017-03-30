using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ
{
    /// <summary>
    /// This attribute is used to indicate what type of command central error corresponds with a given http status code.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class CorrespondingErrorTypeAttribute : Attribute
    {
        /// <summary>
        /// The type of error.
        /// </summary>
        public ClientAccess.ErrorTypes ErrorType { get; set; }
    }
}
