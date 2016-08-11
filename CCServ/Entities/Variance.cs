using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CCServ
{
    /// <summary>
    /// Describes a single variance in an object.
    /// </summary>
    public class Variance
    {
        /// <summary>
        /// The name of the property that is in variance.
        /// </summary>
        public string PropertyName { get; set; }
        /// <summary>
        /// The value of the property from the old object.
        /// </summary>
        public object OldValue { get; set; }
        /// <summary>
        /// The value of the property from the new object.
        /// </summary>
        public object NewValue { get; set; }
    }

    /// <summary>
    /// The class that contains the comparison method.
    /// </summary>
    public static class VarianceExtensions
    {
        /// <summary>
        /// Compares two objects and returns a list of variances.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="oldObject"></param>
        /// <param name="newObject"></param>
        /// <returns></returns>
        public static IEnumerable<Variance> DetailedCompare<T>(this T oldObject, T newObject)
        {
            PropertyInfo[] pi = oldObject.GetType().GetProperties();
            foreach (PropertyInfo p in pi)
            {
                Variance v = new Variance();
                v.PropertyName = p.Name;
                v.OldValue = p.GetValue(oldObject);
                v.NewValue = p.GetValue(newObject);
                if (!Equals(v.OldValue, v.NewValue))
                    yield return v;
            }
        }
    }
    
    
}
