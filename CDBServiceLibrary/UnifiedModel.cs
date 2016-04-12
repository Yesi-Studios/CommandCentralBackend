using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using AtwoodUtils;
using System.Collections.Concurrent;

namespace UnifiedServiceFramework
{
    /// <summary>
    /// The base for a unified model, provides methods for creating data interactive models.
    /// </summary>
    public abstract class UnifiedModel
    {

        /// <summary>
        /// The cache of all of the unified properties in this class.
        /// </summary>
        private ConcurrentBag<UnifiedProperties.UnifiedProperty> _unifiedPropertiesCache = new ConcurrentBag<UnifiedProperties.UnifiedProperty>();

        /// <summary>
        /// Returns the value of a property.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        private object GetValue(UnifiedProperties.UnifiedProperty property)
        {
            if (this.GetType() != property.DeclaringType)
                throw new Exception("Mismatch in type value property selection.");

            PropertyInfo propInfo = property.DeclaringType.GetProperty(property.PropertyName);

            if (propInfo == null)
                throw new Exception(string.Format("The property, '{0}', does not exist on the type, '{1}'.", property.PropertyName, this.GetType().Name));

            return propInfo.GetValue(this);
        }

        /// <summary>
        /// Sets the value of a UProperty on this object to the given value.
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        private void SetValue(UnifiedProperties.UnifiedProperty property, object value)
        {
            if (this.GetType() != property.DeclaringType)
                throw new Exception("Mismatch in type value property selection.");

            PropertyInfo propInfo = property.DeclaringType.GetProperty(property.PropertyName);

            if (propInfo == null)
                throw new Exception(string.Format("The property, '{0}', does not exist on the type, '{1}'.", property.PropertyName, this.GetType().Name));

            propInfo.SetValue(this, value);
        }

        /// <summary>
        /// Registers a unified property for this model.
        /// </summary>
        /// <param name="property"></param>
        private void RegisterProperty(UnifiedProperties.UnifiedProperty property)
        {
            try
            {
                _unifiedPropertiesCache.Add(property);
            }
            catch
            {
                throw;
            }
        }


    }
}
