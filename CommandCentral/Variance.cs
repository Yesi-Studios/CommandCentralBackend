using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral
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
        /// The value of the property from val1.
        /// </summary>
        public object val_obj1 { get; set; }
        /// <summary>
        /// The value of the property from val2.
        /// </summary>
        public object val_obj2 { get; set; }
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
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns></returns>
        public static IEnumerable<Variance> DetailedCompare<T>(this T obj1, T obj2)
        {
            PropertyInfo[] pi = obj1.GetType().GetProperties();
            foreach (PropertyInfo p in pi)
            {
                Variance v = new Variance();
                v.PropertyName = p.Name;
                v.val_obj1 = p.GetValue(obj1);
                v.val_obj2 = p.GetValue(obj2);
                if (!Equals(v.val_obj1, v.val_obj2))
                    yield return v;
            }
        }
    }
    
    
}
