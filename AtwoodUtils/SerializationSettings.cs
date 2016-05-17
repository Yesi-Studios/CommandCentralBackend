using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AtwoodUtils
{
    public static class SerializationSettings
    {
        public static readonly JsonSerializerSettings StandardSettings = new JsonSerializerSettings
        {
            Converters = new List<JsonConverter> { new StringEnumConverter { CamelCaseText = false } },
            ContractResolver = new NHibernateContractResolver(),
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize
        };

        public class NHibernateContractResolver : Newtonsoft.Json.Serialization.DefaultContractResolver
        {
            protected override Newtonsoft.Json.Serialization.JsonContract CreateContract(System.Type objectType)
            {
                if (typeof(NHibernate.Proxy.INHibernateProxy).IsAssignableFrom(objectType))
                    return base.CreateContract(objectType.BaseType);
                else
                    return base.CreateContract(objectType);
            }
        }
    }
}
