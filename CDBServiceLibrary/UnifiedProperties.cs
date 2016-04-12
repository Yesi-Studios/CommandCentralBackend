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
