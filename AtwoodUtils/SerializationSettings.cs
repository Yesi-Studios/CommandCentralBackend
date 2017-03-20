using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace AtwoodUtils
{
    public static class SerializationSettings
    {
        public static readonly JsonSerializerSettings StandardSettings = new JsonSerializerSettings
        {
            Converters = new List<JsonConverter> { new StringEnumConverter { CamelCaseText = false } },
            ContractResolver = new NHibernateContractResolver(),
            PreserveReferencesHandling = PreserveReferencesHandling.All,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Formatting = Formatting.Indented,
            DateFormatString = "yyyy-MM-ddTHH:mm:ss.fffZ",
            DateTimeZoneHandling = DateTimeZoneHandling.Utc
        };

        public class NHibernateContractResolver : DefaultContractResolver
        {
            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                JsonProperty property = base.CreateProperty(member, memberSerialization);

                var atts = property.AttributeProvider.GetAttributes(typeof(ConditionalJsonIgnoreAttribute), true);

                if (!atts.Any())
                    return property;

                property.ShouldSerialize = x => false;

                return property;
            }

            protected override JsonContract CreateContract(System.Type objectType)
            {
                if (typeof(NHibernate.Proxy.INHibernateProxy).IsAssignableFrom(objectType))
                    return base.CreateContract(objectType.BaseType);
                else
                    return base.CreateContract(objectType);
            }
        }
    }
}
