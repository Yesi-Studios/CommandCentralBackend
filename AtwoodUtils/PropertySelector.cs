using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Linq.Expressions;
using AtwoodUtils;

namespace AtwoodUtils
{
    /// <summary>
    /// A static class that cleans up property selection from a given type.
    /// </summary>
    public static class PropertySelector
    {
        /// <summary>
        /// Selects a number of properties from a given type that are all of the same given type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expressions"></param>
        /// <returns></returns>
        public static List<MemberInfo> SelectPropertiesFrom<T>(params Expression<Func<T, object>>[] expressions)
        {
            return expressions.Select(x => x.GetProperty()).ToList();
        }

        /// <summary>
        /// Returns the property that matches the expression.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static MemberInfo SelectPropertyFrom<T>(Expression<Func<T, object>> expression)
        {
            return expression.GetProperty();
        }

        /// <summary>
        /// Selects a number of properties from a given type that are all of the same given type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expressions"></param>
        /// <returns></returns>
        public static List<MemberInfo> SelectPropertiesFrom<T>(IEnumerable<string> propertyNames, StringComparison comparison)
        {
            return typeof(T).GetProperties().Where(x => propertyNames.Any(y => string.Equals(y, x.Name, comparison))).Cast<MemberInfo>().ToList();
        }

        /// <summary>
        /// Selects a number of properties from a given type that are all of the same given type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expressions"></param>
        /// <returns></returns>
        public static PropertyInfo SelectPropertyFrom<T>(string propertyName)
        {
            return typeof(T).GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
        }

        /// <summary>
        /// Selects a number of properties from a given type.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="propertyNames"></param>
        /// <param name="comparison"></param>
        /// <returns></returns>
        public static List<MemberInfo> SelectPropertiesFrom(Type type, IEnumerable<string> propertyNames, StringComparison comparison)
        {
            return type.GetProperties().Where(x => propertyNames.Any(y => string.Equals(y, x.Name, comparison))).Cast<MemberInfo>().ToList();
        }
    }
}
