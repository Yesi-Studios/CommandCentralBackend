using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace AtwoodUtils
{
    /// <summary>
    /// Declares global serialization settings which should be used in all serializations.
    /// </summary>
    public static class SerializationSettings
    {
        /// <summary>
        /// The settings themselves.
        /// </summary>
        public static readonly JsonSerializerSettings StandardSettings = new JsonSerializerSettings
        {
            Converters = new List<JsonConverter> { new StringEnumConverter { CamelCaseText = false } },
            ContractResolver = new CustomContractResolver(),
            PreserveReferencesHandling = PreserveReferencesHandling.All,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            Formatting = Formatting.Indented,
            DateFormatString = "yyyy-MM-ddTHH:mm:ss.fffZ",
            DateTimeZoneHandling = DateTimeZoneHandling.Utc
        };
    }
}
