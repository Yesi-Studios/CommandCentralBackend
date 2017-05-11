using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.ClientAccess.DTOs
{
    /// <summary>
    /// Indicates that a property is optional.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class OptionalAttribute : Attribute
    {
        /// <summary>
        /// The default value of this proeprty.
        /// </summary>
        public object DefaultValue { get; set; }

        public OptionalAttribute(object defaultValue)
        {
            this.DefaultValue = defaultValue;
        }

    }
}
