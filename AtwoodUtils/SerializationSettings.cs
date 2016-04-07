using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AtwoodUtils
{
    public static class SerializationSettings
    {
        public static readonly JsonSerializerSettings StandardSettings = new JsonSerializerSettings()
        {
            Converters = new List<JsonConverter>() { new Newtonsoft.Json.Converters.StringEnumConverter { CamelCaseText = false } }
        };
    }
}
