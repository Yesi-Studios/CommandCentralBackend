using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.ServiceModel.Web;
using System.Text;
using Newtonsoft.Json;
using System.Collections;

namespace AtwoodUtils
{
    public static class Utilities
    {
        //I have no idea what is going on down here... But it works.  Thank you Stack Overflow
        public static double HaversineDistance(LatitudeAndLongitude pos1, LatitudeAndLongitude pos2, DistanceUnit unit)
        {
            double r = (unit == DistanceUnit.Miles) ? 3960 : 6371;
            var lat = ConvertDegreesToRadians((pos2.Latitude - pos1.Latitude));
            var lng = ConvertDegreesToRadians((pos2.Longitude - pos1.Longitude));
            var h1 = Math.Sin(lat / 2) * Math.Sin(lat / 2) +
                          Math.Cos(ConvertDegreesToRadians(pos1.Latitude)) * Math.Cos(ConvertDegreesToRadians(pos2.Latitude)) *
                          Math.Sin(lng / 2) * Math.Sin(lng / 2);
            var h2 = 2 * Math.Asin(Math.Min(1, Math.Sqrt(h1)));
            return r * h2;
        }

        public enum DistanceUnit { Miles, Kilometers }

        public struct LatitudeAndLongitude
        {
            public double Latitude { get; set; }
            public double Longitude { get; set; }
        }

        private static double ConvertDegreesToRadians(double degrees)
        {
            double radians = (Math.PI / 180) * degrees;
            return (radians);
        }

        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string GenerateSSN(string delimiter = "")
        {
            int iThree = GetRandomNumber(132, 921);
            int iTwo = GetRandomNumber(12, 83);
            int iFour = GetRandomNumber(1423, 9211);
            return iThree.ToString() + delimiter + iTwo.ToString() + delimiter + iFour.ToString();
        }

        public static bool ScrambledEquals<T>(IEnumerable<T> list1, IEnumerable<T> list2)
        {
            var cnt = new Dictionary<T, int>();
            foreach (T s in list1)
            {
                if (cnt.ContainsKey(s))
                {
                    cnt[s]++;
                }
                else
                {
                    cnt.Add(s, 1);
                }
            }
            foreach (T s in list2)
            {
                if (cnt.ContainsKey(s))
                {
                    cnt[s]--;
                }
                else
                {
                    return false;
                }
            }
            return cnt.Values.All(c => c == 0);
        }

        public static string GenerateDoDId()
        {
            string result = "";
            for (int x = 0; x < 10; x++)
            {
                result += GetRandomNumber(1, 9).ToString();
            }
            return result;
        }

        //Function to get random number
        public static int GetRandomNumber(int min, int max)
        {
            return random.Next(min, max);
        }

        public static string FirstCharacterToUpper(string str)
        {
            if (String.IsNullOrEmpty(str))
                return null;

            return str.First().ToString().ToUpper() + str.Substring(1);
        }

        /// <summary>
        /// For the given property of the given type, returns the name of that property.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static string GetPropertyName<T>(this Expression<Func<T, object>> expression)
        {
            return GetProperty(expression).Name;
        }

        /// <summary>
        /// For the given property of a given type, returned the member info of that property.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static MemberInfo GetProperty<T>(this Expression<Func<T, object>> expression)
        {
            if (expression.Body is MemberExpression memberExp)
                return memberExp.Member;

            // for unary types like datetime or guid
            if (expression.Body is UnaryExpression unaryExp)
            {
                memberExp = unaryExp.Operand as MemberExpression;
                if (memberExp != null)
                    return memberExp.Member;
            }

            throw new ArgumentException("'expression' should be a member expression or a method call expression.", "expression");
        }

        /// <summary>
        /// Returns the base type of an enumerable.
        /// <para />
        /// Does not work with non-homogenous collections.
        /// </summary>
        /// <param name="enumerable"></param>
        /// <returns></returns>
        public static Type GetBaseTypeOfEnumerable(IEnumerable enumerable)
        {
            if (enumerable == null)
                throw new ArgumentException("enumerable must not be null.");

            var genericInterface = enumerable
                .GetType()
                .GetInterfaces()
                .FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>));

            if (genericInterface == null)
            {
                throw new Exception("enumerable appears to not implement a single base type.");
            }

            var elementType = genericInterface.GetGenericArguments()[0];
            return elementType.IsGenericType && elementType.GetGenericTypeDefinition() == typeof(Nullable<>)
                ? elementType.GetGenericArguments()[0]
                : elementType;
        }

        /// <summary>
        /// Determines if two sets are equal (if both are null, that is considered to be equal), regardless of element order, in O(N) time.
        /// <para/>
        /// NOTE: T must override GetHashCode and Equals.  If you're not doing this and this method causes an error, that's your own damn fault, Atwood, you know you're supposed to be overriding those.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool SetEquals<T>(IEnumerable<T> left, IEnumerable<T> right)
        {
            /*
             * So this algorithm is fancy and simple.  Go through every object and count how many times we find it in the left collection.
             * Then, go through the right collection subtracting from the same counter every time we find the same object.  
             * If all the counts equal 0 at the end, then we have two equal collections regardless of their order.
             *
             * Also, return false if the right collection doesn't contain something from the left.
             */

            //Get some null checking up in here.
            if (left == null && right == null)
                return true;

            if (left == null || right == null)
                return false;

            Dictionary<T, int> dict = new Dictionary<T, int>();

            foreach (var member in left)
            {
                if (!dict.ContainsKey(member))
                {
                    dict[member] = 1;
                }
                else
                {
                    dict[member]++;
                }
            }

            foreach (var member in right)
            {
                if (!dict.ContainsKey(member))
                {
                    return false;
                }
                else
                {
                    dict[member]--;
                }
            }

            return dict.All(x => x.Value == 0);
        }

        /// <summary>
        /// Determines the differences between two sets.  Does not support duplicate objects.
        /// </summary>
        /// <param name="left">The left set.</param>
        /// <param name="right">The right set.</param>
        /// <param name="notInLeft">All those items that exist in the right set but not in the left.</param>
        /// <param name="notInRight">All those items that exist in the left set but not in the right.</param>
        /// <param name="inBothButChanged">All those items that exist in both sets, but that were changed.  This change detection is based on the property passed to be used as the object Id.  T1 is the left value, T2 is the right value.</param>
        /// <param name="keyPropertyName">The name of the property that should exist on the objects within the sets to use to identify an object.  Enable change detection.</param>
        /// <returns></returns>
        public static bool GetSetDifferences(List<object> left, List<object> right, out List<object> notInLeft, out List<object> notInRight, out List<Tuple<object, object>> inBothButChanged, string keyPropertyName = "id")
        {
            notInLeft = new List<object>();
            notInRight = new List<object>();
            inBothButChanged = new List<Tuple<object, object>>();

            if (left == null && right == null)
                return true;

            if (left == null || right == null)
                return false;

            if (left.Count == 0 && right.Count == 0)
                return true;

            Type elementType;
            
            if (left.FirstOrDefault() != null)
            {
                elementType = left.First().GetType();
            }
            else if (right.FirstOrDefault() != null)
            {
                elementType = right.First().GetType();
            }
            else
            {
                throw new Exception("How did you get here, cotton eye joe?");
            }

            var idProperty = elementType.GetProperties().FirstOrDefault(x => x.Name.SafeEquals(keyPropertyName));
            HashSet<object> foundIds = new HashSet<object>();

            foreach (var leftValue in left)
            {
                if (idProperty == null)
                {
                    if (!right.Contains(leftValue))
                    {
                        notInRight.Add(leftValue);
                    }
                }
                else
                {
                    var leftId = idProperty.GetValue(leftValue);
                    var rightValue = right.FirstOrDefault(x => idProperty.GetValue(x).Equals(leftId));

                    if (rightValue == null)
                    {
                        notInRight.Add(leftValue);
                    }
                    else if (!rightValue.Equals(leftValue))
                    {
                        inBothButChanged.Add(new Tuple<object, object>(leftValue, rightValue));
                        foundIds.Add(leftId);
                    }
                }
            }

            foreach (var rightValue in right)
            {
                if (idProperty == null)
                {
                    if (!left.Contains(rightValue))
                    {
                        notInLeft.Add(rightValue);
                    }
                }
                else
                {
                    var rightId = idProperty.GetValue(rightValue);
                    if (!foundIds.Contains(rightId))
                    {
                        var leftValue = left.FirstOrDefault(x => idProperty.GetValue(x).Equals(rightId));

                        if (leftValue == null)
                        {
                            notInLeft.Add(rightValue);
                        }
                    }
                }
            }

            return notInLeft.Count == 0 && notInRight.Count == 0 && inBothButChanged.Count == 0;
        } 

        /// <summary>
        /// Determines whether or not a type is assignable to a generic type.
        /// </summary>
        /// <param name="givenType"></param>
        /// <param name="genericType"></param>
        /// <returns></returns>
        public static bool IsAssignableToGenericType(Type givenType, Type genericType)
        {
            var interfaceTypes = givenType.GetInterfaces();

            foreach (var it in interfaceTypes)
            {
                if (it.IsGenericType && it.GetGenericTypeDefinition() == genericType)
                    return true;
            }

            if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
                return true;

            Type baseType = givenType.BaseType;
            if (baseType == null) return false;

            return IsAssignableToGenericType(baseType, genericType);
        }

        /// <summary>
        /// Gets all types that have a attribute, T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assembly"></param>
        /// <returns></returns>
        static IEnumerable<Type> GetTypesWithAttribute<T>(Assembly assembly) where T : Attribute
        {
            foreach (Type type in assembly.GetTypes())
            {
                if (type.GetCustomAttributes(typeof(T), true).Length > 0)
                {
                    yield return type;
                }
            }
        }
        
        /// <summary>
        /// Reads all text from a stream.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string ConvertStreamToString(Stream data)
        {
            using (StreamReader reader = new StreamReader(data))
                return reader.ReadToEnd();
        }

        /// <summary>
        /// A null safe method for getting an object's hashcode.  Returns 0 if the object is null.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static int GetSafeHashCode(object obj)
        {
            if (obj == null)
                return 0;

            return obj.GetHashCode();
        }

        /// <summary>
        /// Returns the .ToString() of the given object unless it is null, in which case the nullString is returned.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="nullString"></param>
        /// <returns></returns>
        public static string ToSafeString(object obj, string nullString = "null")
        {
            return (obj == null) ? nullString : obj.ToString();
        }

        /// <summary>
        /// Determines whether or not a given string starts and ends with either {} or [].
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static bool IsValidJson(string json)
        {
            json = json.Trim();

            return (json.StartsWith("{") && json.EndsWith("}")) || (json.StartsWith("[") && json.EndsWith("]"));
        }

        
        /// <summary>
        /// Pads elements in a nice grid.
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="padding"></param>
        /// <returns></returns>
        public static string PadElementsInLines(List<string[]> lines, int padding = 1)
        {

            var numElements = lines[0].Length;
            var maxValues = new int[numElements];
            for (int x = 0; x < numElements; x++)
            {
                maxValues[x] = lines.Max(y => y[x].Length) + padding;
            }

            StringBuilder sb = new StringBuilder();

            bool isFirst = true;
            foreach (var line in lines)
            {
                if (!isFirst)
                {
                    sb.AppendLine();
                }
                isFirst = false;
                for (int x = 0; x < line.Length; x++)
                {
                    var value = line[x];
                    sb.Append(value.PadRight(maxValues[x]));
                }
            }
            return sb.ToString();


        }

        public static void DeleteDirectory(string path, bool recursive)
        {
            // Delete all files and sub-folders?
            if (recursive)
            {
                // Yep... Let's do this
                var subfolders = Directory.GetDirectories(path);
                foreach (var s in subfolders)
                {
                    DeleteDirectory(s, recursive);
                }
            }

            // Get all files of the folder
            var files = Directory.GetFiles(path);
            foreach (var f in files)
            {
                // Get the attributes of the file
                var attr = File.GetAttributes(f);

                // Is this file marked as 'read-only'?
                if ((attr & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    // Yes... Remove the 'read-only' attribute, then
                    File.SetAttributes(f, attr ^ FileAttributes.ReadOnly);
                }

                // Delete the file
                File.Delete(f);
            }

            // When we get here, all the files of the folder were
            // already deleted, so we just delete the empty folder
            Directory.Delete(path);
        }

        /// <summary>
        /// Gets the calling method name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetCallingMethodName([CallerMemberName] string name = "")
        {
            return name;
        }

        public static string BuildJoinStatement(IEnumerable<string> tableNames, string primaryKeyName)
        {
            var names = tableNames as IList<string> ?? tableNames.ToList();
            if (tableNames == null || !names.Any())
                throw new ArgumentException("The table names argument may not be empty or null.");

            string str = "";
            for (int x = 0; x < names.Count; x++)
            {
                if (x == 0)
                    str += string.Format("`{0}` ", names.ElementAt(x));
                else
                    str += string.Format("JOIN `{0}` USING (`{1}`) ", names.ElementAt(x), primaryKeyName);
            }
            return str;
        }

        /// <summary>
        /// Evaluate current system tcp connections. This is the same information provided
        /// by the netstat command line application, just in .Net strongly-typed object
        /// form.  We will look through the list, and if our port we would like to use
        /// in our TcpClient is occupied, we will return false.
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public static bool IsPortAvailable(int port)
        {
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();

            return !ipGlobalProperties.GetActiveTcpConnections().ToList().Exists(x => x.LocalEndPoint.Port == port)
                && !ipGlobalProperties.GetActiveTcpListeners().ToList().Exists(x => x.Port == port);
            
            
        }


    }
}
