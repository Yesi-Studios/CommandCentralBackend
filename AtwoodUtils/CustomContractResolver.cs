using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AtwoodUtils
{
    /// <summary>
    /// Provides custom contract resolution.  This allows us to make broad changes to how serialization occurs across all objects.
    /// </summary>
    public class CustomContractResolver : DefaultContractResolver
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
