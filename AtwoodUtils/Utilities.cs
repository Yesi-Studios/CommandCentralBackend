using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.ServiceModel.Web;
using System.Text;
using Newtonsoft.Json;

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
        

        /// <summary>
        /// Gets the calling method name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetCallingMethodName([CallerMemberName] string name = "")
        {
            return name;
        }

        /// <summary>
        /// Adds the CORS headers to the outgoing response to enable cross domain requests.
        /// </summary>
        /// <param name="current"></param>
        public static void AddCorsHeadersToResponse(WebOperationContext current)
        {
            current.OutgoingResponse.Headers.Add("Access-Control-Allow-Origin", "*");
            current.OutgoingResponse.Headers.Add("Access-Control-Allow-Methods", "POST");
            current.OutgoingResponse.Headers.Add("Access-Control-Allow-Headers", "Content-Type,Accept");
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
