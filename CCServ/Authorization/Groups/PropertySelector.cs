using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Linq.Expressions;
using AtwoodUtils;

namespace CCServ.Authorization.Groups
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
        /// <typeparam name="PropertyT"></typeparam>
        /// <param name="expressions"></param>
        /// <returns></returns>
        public static List<MemberInfo> SelectPropertiesFrom<T>(params Expression<Func<T, object>>[] expressions)
        {
            return expressions.Select(x => x.GetProperty()).ToList();
        }
    }
}
