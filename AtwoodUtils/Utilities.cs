using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.IO;
using System.Reflection;

namespace AtwoodUtils
{
    public static class Utilities
    {
        //I have no idea what is going on down here... But it works.  Thank you Stack Overflow
        public static double HaversineDistance(LatitudeAndLongitude pos1, LatitudeAndLongitude pos2, DistanceUnit unit)
        {
            double R = (unit == DistanceUnit.Miles) ? 3960 : 6371;
            var lat = ConvertDegreesToRadians((pos2.Latitude - pos1.Latitude));
            var lng = ConvertDegreesToRadians((pos2.Longitude - pos1.Longitude));
            var h1 = Math.Sin(lat / 2) * Math.Sin(lat / 2) +
                          Math.Cos(ConvertDegreesToRadians(pos1.Latitude)) * Math.Cos(ConvertDegreesToRadians(pos2.Latitude)) *
                          Math.Sin(lng / 2) * Math.Sin(lng / 2);
            var h2 = 2 * Math.Asin(Math.Min(1, Math.Sqrt(h1)));
            return R * h2;
        }

        public enum DistanceUnit { Miles, Kilometers };

        public struct LatitudeAndLongitude
        {
            public double Latitude { get; set; }
            public double Longitude { get; set; }
        }

        public static double ConvertDegreesToRadians(double degrees)
        {
            double radians = (Math.PI / 180) * degrees;
            return (radians);
        }

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

        public static Dictionary<string, object> ConvertPostDataToDict(Stream data)
        {
            try
            {
                Dictionary<string, object> args = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                using (StreamReader reader = new StreamReader(data))
                {
                    string json = reader.ReadToEnd();

                    if (string.IsNullOrEmpty(json))
                        return new Dictionary<string, object>();

                    Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(json).AsEnumerable().ToList().ForEach(x =>
                    {
                        args.Add(x.Key.ToLower(), x.Value);
                    });

                }
                return args;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Determines whether or not a given string starts and ends with either {} or [].
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static bool IsValidJSON(string json)
        {
            json = json.Trim();

            return (json.StartsWith("{") && json.EndsWith("}")) || (json.StartsWith("[") && json.EndsWith("]"));
        }

        

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

        public static string GetAuthTokenFromArgs(Dictionary<string, object> args)
        {
            try
            {
                if (!args.ContainsKey("authenticationtoken"))
                    return null;

                return args["authenticationtoken"].ToString();
            }
            catch
            {
                throw;
            }
        }

        public static string GetOrderByFromArgs(Dictionary<string, object> dict)
        {
            try
            {
                if (!dict.ContainsKey("orderby"))
                    return null;
                return dict["orderby"].ToString();
            }
            catch
            {
                
                throw;
            }
        }

        public static int GetLimitFromArgs(Dictionary<string, object> dict, int returnLimit)
        {
            try
            {
                if (!dict.ContainsKey("limit"))
                    return returnLimit;

                int limit = -1;
                if (!Int32.TryParse(dict["limit"].ToString(), out limit) || limit < 0 || limit > returnLimit)
                    return returnLimit;

                return limit;
            }
            catch
            {
                
                throw;
            }
        }

        public static string GetAPIKeyFromArgs(Dictionary<string, object> args)
        {
            if (!args.ContainsKey("apikey"))
                return null;

            return args["apikey"].ToString();
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

        public static void AddCORSHeadersToResponse(System.ServiceModel.Web.WebOperationContext current)
        {
            current.OutgoingResponse.Headers.Add("Access-Control-Allow-Origin", "*");
            current.OutgoingResponse.Headers.Add("Access-Control-Allow-Methods", "POST");
            current.OutgoingResponse.Headers.Add("Access-Control-Allow-Headers", "Content-Type,Accept");
        }

        public static string BuildJoinStatement(IEnumerable<string> tableNames, string primaryKeyName)
        {
            if (tableNames == null || tableNames.Count() == 0)
                throw new ArgumentException("The table names argument may not be empty or null.");

            string str = "";
            for (int x = 0; x < tableNames.Count(); x++)
            {
                if (x == 0)
                    str += string.Format("`{0}` ", tableNames.ElementAt(x));
                else
                    str += string.Format("JOIN `{0}` USING (`{1}`) ", tableNames.ElementAt(x), primaryKeyName);
            }
            return str;
        }

        public static dynamic GetAddressFromGoogleAPIResponse(Newtonsoft.Json.Linq.JObject obj)
        {
            var results = (Newtonsoft.Json.Linq.JArray)obj.SelectToken("results");

            if (results.Count == 0)
                return null;

            var result = results.First;
            var addressComponents = result.SelectToken("address_components");
            var lat = result.SelectToken("geometry").SelectToken("location").Value<double>("lat");
            var lng = result.SelectToken("geometry").SelectToken("location").Value<double>("lng");

            var address = new 
            { 
                StreetNumber = (addressComponents.Where(x => ((Newtonsoft.Json.Linq.JArray)x.SelectToken("types")).ToObject<List<string>>().Contains("street_number")).FirstOrDefault() ?? Newtonsoft.Json.Linq.JToken.FromObject( new { long_name = (string)null })).Value<string>("long_name"),
                Route = (addressComponents.Where(x => ((Newtonsoft.Json.Linq.JArray)x.SelectToken("types")).ToObject<List<string>>().Contains("route")).FirstOrDefault() ?? Newtonsoft.Json.Linq.JToken.FromObject(new { long_name = (string)null })).Value<string>("long_name"),
                City = (addressComponents.Where(x => ((Newtonsoft.Json.Linq.JArray)x.SelectToken("types")).ToObject<List<string>>().Contains("locality")).FirstOrDefault() ?? Newtonsoft.Json.Linq.JToken.FromObject(new { long_name = (string)null })).Value<string>("long_name"),
                County = (addressComponents.Where(x => ((Newtonsoft.Json.Linq.JArray)x.SelectToken("types")).ToObject<List<string>>().Contains("administrative_area_level_2")).FirstOrDefault() ?? Newtonsoft.Json.Linq.JToken.FromObject(new { long_name = (string)null })).Value<string>("long_name"),
                State = (addressComponents.Where(x => ((Newtonsoft.Json.Linq.JArray)x.SelectToken("types")).ToObject<List<string>>().Contains("administrative_area_level_1")).FirstOrDefault() ?? Newtonsoft.Json.Linq.JToken.FromObject(new { long_name = (string)null })).Value<string>("long_name"),
                Country = (addressComponents.Where(x => ((Newtonsoft.Json.Linq.JArray)x.SelectToken("types")).ToObject<List<string>>().Contains("country")).FirstOrDefault() ?? Newtonsoft.Json.Linq.JToken.FromObject(new { long_name = (string)null })).Value<string>("long_name"),
                ZipCode = (addressComponents.Where(x => ((Newtonsoft.Json.Linq.JArray)x.SelectToken("types")).ToObject<List<string>>().Contains("postal_code")).FirstOrDefault() ?? Newtonsoft.Json.Linq.JToken.FromObject(new { long_name = (string)null })).Value<string>("long_name"),
                Latitude = lat,
                Longitude = lng
            };

            return address;
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
            try
            {
                System.Net.NetworkInformation.IPGlobalProperties ipGlobalProperties = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties();

                return !ipGlobalProperties.GetActiveTcpConnections().ToList().Exists(x => x.LocalEndPoint.Port == port) 
                    && !ipGlobalProperties.GetActiveTcpListeners().ToList().Exists(x => x.Port == port);
            }
            catch
            {
                throw;
            }
        }


    }
}
