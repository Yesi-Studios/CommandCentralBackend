using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedServiceFramework
{
    public static class UnifiedProperties
    {

        private static ConcurrentBag<UnifiedProperty> _unifiedPropertiesCache = new ConcurrentBag<UnifiedProperty>();

        public class UnifiedProperty
        {
            public string PropertyName { get; set; }

            public string DatabaseName { get; set; }

            public string TableName { get; set; }

            public Type DeclaringType { get; set; }

            /// <summary>
            /// Gets the value of an object given this unified property.
            /// </summary>
            /// <param name="obj"></param>
            /// <returns></returns>
            public object GetValue(object obj)
            {
                //Make sure the object is of the right type.
                if (obj.GetType() != this.DeclaringType)
                    throw new Exception(string.Format("You tried to get the value of the '{0}' property from the object whose type is '{1}'; however, the property is from the type '{2}'.",
                        this.PropertyName, obj.GetType().FullName, this.DeclaringType.FullName));

                //Now try to get the field.
                var prop = obj.GetType().GetProperty(this.PropertyName);

                //Make sure we got a property.
                if (prop == null)
                    throw new Exception(string.Format("On the object of type, '{0}', no property named '{1}' exists.", obj.GetType().FullName, this.PropertyName));

                //And then get the value
                return prop.GetValue(obj);
            }

            /// <summary>
            /// Sets a value on a given object.
            /// </summary>
            /// <param name="obj"></param>
            /// <param name="property"></param>
            /// <param name="value"></param>
            public void SetValue(object obj, object value)
            {
                try
                {
                    //Make sure the object is of the right type.
                    if (obj.GetType() != this.DeclaringType)
                        throw new Exception(string.Format("You tried to get the value of the '{0}' property from the object whose type is '{1}'; however, the property is from the type '{2}'.",
                            this.PropertyName, obj.GetType().FullName, this.DeclaringType.FullName));

                    //Now try to get the field.
                    var prop = obj.GetType().GetProperty(this.PropertyName);

                    //Make sure we got a property.
                    if (prop == null)
                        throw new Exception(string.Format("On the object of type, '{0}', no property named '{1}' exists.", obj.GetType().FullName, this.PropertyName));

                    //Now we need to make sure the type of this value matches the type of the property.
                    if (value.GetType() != prop.PropertyType)
                        throw new Exception("The type of the value was not the saem as this property.");

                    prop.SetValue(obj, value);
                }
                catch
                {
                    throw;
                }
            }
        
        }

        public static void RegisterProperty(UnifiedProperty property)
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

        
        /// <summary>
        /// Gets a unified property that represents the given property on the requested object.  Returns null if no property is present.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static UnifiedProperty GetProperty(Type type, string propertyName)
        {
            try
            {
                return _unifiedPropertiesCache.FirstOrDefault(x => x.PropertyName == propertyName && x.DeclaringType == type);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Returns all properties assigned to a given type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<UnifiedProperty> GetProperties(Type type)
        {
            try
            {
                return _unifiedPropertiesCache.Where(x => x.DeclaringType == type).ToList();
            }
            catch
            {
                
                throw;
            }
        }

        /// <summary>
        /// Returns all properties for a given type filtered by a given set of property names.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="propertyNames"></param>
        /// <returns></returns>
        public static List<UnifiedProperty> GetProperties(Type type, IEnumerable<string> propertyNames)
        {
            try
            {
                return _unifiedPropertiesCache.Where(x => x.DeclaringType == type && propertyNames.Contains(x.PropertyName)).ToList();
            }
            catch
            {
                throw;
            }
        }

        


        

    }
}
