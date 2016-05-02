using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Globalization;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System.Reflection;

namespace AtwoodUtils
{
    public static class ExtensionMethods
    {
        public static string Truncate(this string str, int length)
        {
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException("length", "Length must be greater than or equal to 0!");
            }

            if (str == null)
            {
                return null;
            }

            int maxLength = Math.Min(str.Length, length);
            return str.Substring(0, maxLength);
        }

        public static string ToMySqlDateTimeString(this DateTime date)
        {
            return date.ToString(CultureInfo.InvariantCulture.DateTimeFormat.SortableDateTimePattern);
        }

        public static string ToMySqlDateTimeString(this string str)
        {
            DateTime date;
            if (!DateTime.TryParse(str, out date))
                throw new Exception("Input not in a valid DateTime pattern.");

            return date.ToString(CultureInfo.InvariantCulture.DateTimeFormat.SortableDateTimePattern);
        }

       /// <summary>
       /// Returns a list of variances describing every property that differed between the two and the two values.
       /// <para />
       /// NOTE: This method only compares properties, not fields, in the target Type.
       /// </summary>
       /// <typeparam name="T"></typeparam>
       /// <param name="objectA"></param>
       /// <param name="objectB"></param>
       /// <returns></returns>
        public static List<Variance> DetermineVariances<T>(this T newObject, T oldObject) where T : class
        {
            List<Variance> variances = new List<Variance>();
            List<PropertyInfo> props = typeof(T).GetProperties().ToList();
            props.ForEach(x =>
                {
                    Variance variance = new Variance();
                    variance.PropertyName = x.Name;
                    variance.NewValue = x.GetValue(newObject);
                    variance.OldValue = x.GetValue(oldObject);

                    //At this point we need to check if we need to go recursive.  If the objects' types implemenet IEnumerable, then we need to step through the pieces seperately. 
                    //We also need to explicitly ignore "string" because it implements IEnumerable but can be compared without this recursion.
                    if (x.PropertyType.IsGenericType && x.PropertyType.GetInterfaces().Any(y => y.Name.SafeEquals("IList")))
                    {
                        //At this point, we know that we have a collection of some sort, so let's iterate through these.  If we find any variance in it, then add the whole object as a variance.
                        //We don't care about the individual variances.
                        //We're going to use dyamic types for run time type inference.  We know from above that this type must implement IList.
                        dynamic dynOldList = variance.OldValue;
                        dynamic dynNewList = variance.NewValue;

                        //Make sure they're not both null
                        if (!(dynNewList == null && dynOldList == null))
                        {
                            //Ok if they're both not null, does one equal null while the other doesn't?
                            if ((dynNewList == null && dynOldList != null) || (dynNewList != null && dynOldList == null))
                                variances.Add(variance);
                            else
                            {
                                //Ok, neither item is null, let's do some comparisons.

                                //If the lists aren't of the same size, then add the variance.
                                if (dynOldList.Count != dynNewList.Count)
                                    variances.Add(variance);
                                else
                                {
                                    //They have the same count, so let's iterate through them.
                                    //We're going to go through the old list and ask if the new list has an item that matches the current old list item.
                                    //This will help us ignore list order.
                                    for (int y = 0; y < dynOldList.Count; y++)
                                    {
                                        //does the new list contain an element that equals this one?
                                        bool exists = false;
                                        for (int z = 0; z < dynNewList.Count; z++)
                                        {
                                            if (((dynamic)dynNewList[z]).Equals((dynamic)dynOldList[y]))
                                            {
                                                //If it exists, then let's short circuit the loop.
                                                exists = true;
                                                break;
                                            }
                                        }

                                        //If we didn't find any elements in the new list for this old list item, then the whole object can be considered to be variant.  
                                        //At this point, we can add the variance and break.
                                        if (!exists)
                                        {
                                            variances.Add(variance);
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        //Ok, we don't have a collection, so we can just compare the two objects.  Let's ask if one or the other is null first.
                        if (!(variance.NewValue == null && variance.OldValue == null))
                        {
                            //If one or the other is null, then that's a variance.
                            if ((variance.NewValue == null && variance.OldValue != null) || (variance.NewValue != null && variance.OldValue == null))
                                variances.Add(variance);
                            else //Both aren't null, so let's do comparison
                                if (!((dynamic)variance.NewValue).Equals((dynamic)variance.OldValue))
                                    variances.Add(variance);
                        }
                    }
                    
                });
            return variances;
        }

        public static void WL(this string str)
        {
            Console.WriteLine(str);
        }

        public static string F(this string str, params object[] args)
        {
            return String.Format(str, args);
        }

        /// <summary>
        /// Returns the number of elements in an enumerable.
        /// </summary>
        /// <param name="enumerable"></param>
        /// <returns></returns>
        public static int Count(this IEnumerable enumerable)
        {
            int count = 0;
            foreach (var element in enumerable)
            {
                count++;
            }
            return count;
        }


        /// <summary>
        /// A plural version of ContainsKey
        /// </summary>
        /// <param name="dick"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public static bool ContainsKeys(this Dictionary<string, object> dick, string[] keys)
        {
            foreach (string key in keys)
            {
                if (!dick.ContainsKey(key))
                    return false;
            }
            return true;
        }

        public static bool ContainsAll(this IEnumerable<object> list, IEnumerable<object> objs)
        {
            foreach (object obj in objs)
            {
                if (!list.Contains(obj))
                    return false;
            }

            return true;
        }

        public static bool ContainsAll(this IEnumerable<string> list, IEnumerable<string> objs, StringComparer comparer)
        {
            foreach (string obj in objs)
            {
                if (!list.Contains(obj, comparer))
                    return false;
            }

            return true;
        }


        public static bool ContainsOnlyAll(this IEnumerable<object> list, IEnumerable<object> objs)
        {
            if (list.Count() != objs.Count())
                return false;

            foreach (object obj in objs)
            {
                if (!list.Contains(obj))
                    return false;
            }

            return true;
        }

        public static bool ContainsOnlyAll(this IEnumerable<string> list, IEnumerable<string> objs, StringComparer comparer)
        {
            if (list.Count() != objs.Count())
                return false;

            foreach (string obj in objs)
            {
                if (!list.Contains(obj, comparer))
                    return false;
            }

            return true;
        }

        public static bool ContainsAny(this IEnumerable<object> list, IEnumerable<object> objs)
        {
            foreach (object obj in list)
            {
                if (objs.Contains(obj))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns a boolean that indicates whether or not a list contains any duplicates.  This method uses hashsets for duplicate comparison and will fail at the first duplicate.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static bool ContainsDuplicates<T>(this IEnumerable<T> list)
        {
            var hashset = new HashSet<T>();
            return list.Any(x => !hashset.Add(x));
        }

        /// <summary>
        /// Determines whether the character is one of a letter, a digit, or a symbol.
        /// </summary>
        /// <param name="c">The character to test.</param>
        /// <returns>
        ///   <c>true</c> if the character is a letter, a digit, or a symbol; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsLetterOrDigitOrSymbol(this char c)
        {
            return (char.IsLetterOrDigit(c) || char.IsSymbol(c));
        }

        public static string Serialize(this object obj)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented, SerializationSettings.StandardSettings);
        }

        public static T Deserialize<T>(this string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return default(T);

            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
        }

        public static JObject DeserializeToJObject(this string json)
        {
            return Newtonsoft.Json.Linq.JObject.Parse(json);
        }

        /// <summary>
        /// Casts the object (which is expected to be a JObject) into the requested class.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T CastJToken<T>(this object value) where T : class, new()
        {
            try
            {
                if (value as JToken == null)
                    throw new Exception("The value could not be cast to a JToken.");


                return (value as JToken).ToObject<T>();
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Performs a case insensitive, current culture string comparison.  Handles nulls.
        /// </summary>
        /// <param name="str1"></param>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool SafeEquals(this string str1, string str)
        {
            if (str == null)
                return false;

            return (str1.Equals(str, StringComparison.CurrentCultureIgnoreCase));
        }

        public static IEnumerable<T> SelectAllThatAreNot<T>(this IEnumerable<T> list, IEnumerable<T> exclusions)
        {
            try
            {
                return list.Where(x => !exclusions.Contains(x));
            }
            catch
            {

                throw;
            }
        }

        public static List<string> CreateList(this string str)
        {
            return new List<string>() { str };
        }
    }
}
