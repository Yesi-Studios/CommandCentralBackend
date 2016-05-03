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

        public static bool ContainsAll(this IList<object> list, IList<object> objs)
        {
            foreach (object obj in objs)
            {
                if (!list.Contains(obj))
                    return false;
            }

            return true;
        }

        public static bool ContainsAll(this IList<string> list, IList<string> objs, StringComparer comparer)
        {
            foreach (string obj in objs)
            {
                if (!list.Contains(obj, comparer))
                    return false;
            }

            return true;
        }


        public static bool ContainsOnlyAll(this IList<object> list, IList<object> objs)
        {
            if (list.Count != objs.Count)
                return false;

            foreach (object obj in objs)
            {
                if (!list.Contains(obj))
                    return false;
            }

            return true;
        }

        public static bool ContainsOnlyAll(this IList<string> list, IList<string> objs, StringComparer comparer)
        {
            if (list.Count != objs.Count)
                return false;

            foreach (string obj in objs)
            {
                if (!list.Contains(obj, comparer))
                    return false;
            }

            return true;
        }

        public static bool ContainsAny(this IList<object> list, IList<object> objs)
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
            return JsonConvert.SerializeObject(obj, Formatting.Indented, SerializationSettings.StandardSettings);
        }

        public static T Deserialize<T>(this string json)
        {
            return string.IsNullOrWhiteSpace(json) ? default(T) : JsonConvert.DeserializeObject<T>(json);
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
        /// Performs a case insensitive, current culture string comparison.  Handles nulls.
        /// </summary>
        /// <param name="str1"></param>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool SafeEquals(this string str1, string str)
        {
            return str != null && (str1.Equals(str, StringComparison.CurrentCultureIgnoreCase));
        }

        public static IEnumerable<T> SelectAllThatAreNot<T>(this IEnumerable<T> list, IEnumerable<T> exclusions)
        {
            return list.Where(x => !exclusions.Contains(x));
        }

        public static List<string> CreateList(this string str)
        {
            return new List<string> { str };
        }
    }
}
