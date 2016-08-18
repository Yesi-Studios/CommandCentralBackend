using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

        /// <summary>
        /// Writes a given string to the Console.
        /// </summary>
        /// <param name="str"></param>
        public static void WriteLine(this string str)
        {
            Console.WriteLine(str);
        }

        public static string FormatS(this string str, params object[] args)
        {
            return string.Format(str, args);
        }

        public static bool InclusiveBetween(this IComparable a, IComparable b, IComparable c)
        {
            return a.CompareTo(b) >= 0 && a.CompareTo(c) <= 0;
        }

        public static bool ExclusiveBetween(this IComparable a, IComparable b, IComparable c)
        {
            return a.CompareTo(b) > 0 && a.CompareTo(c) < 0;
        }

        public static bool SqlBetween(this IComparable a, IComparable b, IComparable c)
        {
            return a.InclusiveBetween(b, c);
        }

        /// <summary>
        /// A plural version of the dictionary's ContainsKey.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dict"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public static bool ContainsKeys<TKey, TValue>(this IDictionary<TKey, TValue> dict, params TKey[] keys)
        {
            return keys.All(x => dict.ContainsKey(x));
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

        public static bool ContainsInsensitive(this string str, string other, CultureInfo culture)
        {
            return culture.CompareInfo.IndexOf(str, other, CompareOptions.IgnoreCase) >= 0;
        }

        public static string Serialize(this object obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.None, SerializationSettings.StandardSettings);
        }

        public static T Deserialize<T>(this string json)
        {
            return (string.IsNullOrWhiteSpace(json) || !Utilities.IsValidJson(json)) ? default(T) : JsonConvert.DeserializeObject<T>(json);
        }

        public static JObject DeserializeToJObject(this string json)
        {
            return JObject.Parse(json);
        }

        /// <summary>
        /// Casts the object (which is expected to be a JObject) into the requested class.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T CastJToken<T>(this object value) where T : class, new()
        {
            if (!(value is JToken))
                throw new Exception("The value could not be cast to a JToken.");


            return ((JToken) value).ToObject<T>();
        }

        /// <summary>
        /// Casts the object (which is expected to be a JObject) into the requested class.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static JToken CastJToken(this object value)
        {
            if (!(value is JToken))
                throw new Exception("The value could not be cast to a JToken.");


            return ((JToken)value);
        }

        /// <summary>
        /// Performs a case insensitive, current culture string comparison.  Handles nulls.
        /// </summary>
        /// <param name="str1"></param>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool SafeEquals(this string str1, string str)
        {
            return str != null && (str1.Equals(str, StringComparison.CurrentCultureIgnoreCase));
        }

        public static List<string> CreateList(this string str)
        {
            return new List<string> { str };
        }
    }
}
