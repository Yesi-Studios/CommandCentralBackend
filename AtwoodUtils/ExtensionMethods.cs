using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AutoMapper;

namespace AtwoodUtils
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Does a shuffle using the Fisher-Yates shuffle algorithm.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static IList<T> Shuffle<T>(this IEnumerable<T> source)
        {
            var list = source.ToList();
            Random random = new Random(DateTime.Now.Millisecond);

            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);

                T t = list[k];
                list[k] = list[n];
                list[n] = t;
            }

            return list;
        }

        /// <summary>
        /// Maps the source object to the given type.  The source type is inferred from the source.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T MapTo<T>(this object source)
        {
            return Mapper.Map<T>(source);
        }

        /// <summary>
        /// Maps the source object to the given type.  The source type is inferred from the source.  Using the given options.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="opts"></param>
        /// <returns></returns>
        public static T MapTo<T>(this object source, Action<IMappingOperationOptions> opts)
        {
            return Mapper.Map<T>(source, opts);
        }

        public static IEnumerable<T> RepeatIndefinitely<T>(this IEnumerable<T> source)
        {
            var list = source.ToList();

            while (true)
            {
                foreach (var item in list)
                    yield return item;
            }
        }

        public static string Truncate(this string str, int length, string message = "|| MESSAGE TRUNCATED ||")
        {
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException("length", "Length must be greater than or equal to 0!");
            }

            if (str == null)
            {
                return null;
            }

            if (str.Length >= length)
            {
                str = str.Substring(0, length - message.Length);
                str += message;
            }

            return str;
        }

        /// <summary>
        /// Deep copies an object, copying value data only - not reference data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T Clone<T>(this object obj) where T : ISerializable
        {
            if (obj == null)
                return default(T);

            using (var stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, obj);
                stream.Seek(0, SeekOrigin.Begin);

                T result = (T)formatter.Deserialize(stream);

                return result;
            }
        }

        /// <summary>
        /// Writes a given string to the Console.
        /// </summary>
        /// <param name="str"></param>
        public static void WriteLine(this string str, params object[] args)
        {
            Console.WriteLine(str, args);
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
            if (value == null)
                return null;

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
        public static bool InsensitiveEquals(this string str1, string str)
        {
            return str != null && (str1.Equals(str, StringComparison.CurrentCultureIgnoreCase));
        }

        public static List<string> CreateList(this string str)
        {
            return new List<string> { str };
        }
    }
}
