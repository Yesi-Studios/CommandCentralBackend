using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtwoodUtils
{
    /// <summary>
    /// Instructs the serializers to ignore this property during serialization but NOT during deserialization
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ConditionalJsonIgnoreAttribute : Attribute
    {

    }
}
