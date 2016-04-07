using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Collections.Concurrent;
using System.Reflection;
using MySql.Data.MySqlClient;
using MySql.Data.Common;
using UnifiedServiceFramework.Framework;
using AtwoodUtils;
using System.Net;
using System.IO;

namespace CommandDB_Plugin.Scripts
{
    public static class Scripts
    {
        public static async Task FixDatabase()
        {
            try
            {

                DataTable mainTable = await LoadAllMain();

                DataTable contactTable = await LoadAllContactInfo();

                foreach (DataRow row in contactTable.AsEnumerable().ToList())
                {

                    if (!String.IsNullOrWhiteSpace(row["SecondaryAddresses"] as string))
                    {
                        string raw = row["SecondaryAddresses"] as string;
                        List<string> addresses = raw.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();

                        if (addresses.Count > 0)
                        {

                            foreach (string address in addresses)
                            {
                                bool keepLooping = true;

                                while (keepLooping)
                                {
                                    try
                                    {

                                        string baseURL = @"http://maps.googleapis.com/maps/api/geocode/json?address={0}&sensor=true";

                                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(string.Format(baseURL, address));
                                        request.Timeout = (int)TimeSpan.FromMinutes(1).TotalMilliseconds;

                                        HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();

                                        Stream responseStream = response.GetResponseStream();

                                        using (StreamReader reader = new StreamReader(responseStream))
                                        {
                                            string json = reader.ReadToEnd();

                                            if (string.IsNullOrEmpty(json))
                                                throw new Exception("no response?");

                                            dynamic formattedAddress = Utilities.GetAddressFromGoogleAPIResponse(json.DeserializeToJObject());

                                            if (formattedAddress != null)
                                            {
                                                await new PhysicalAddresses.PhysicalAddress()
                                                {
                                                    City = formattedAddress.GetType().GetProperty("City").GetValue(formattedAddress, null),
                                                    Country = formattedAddress.GetType().GetProperty("Country").GetValue(formattedAddress, null),
                                                    ID = Guid.NewGuid().ToString(),
                                                    IsHomeAddress = false,
                                                    Latitude = (float?)formattedAddress.GetType().GetProperty("Latitude").GetValue(formattedAddress, null),
                                                    Longitude = (float?)formattedAddress.GetType().GetProperty("Longitude").GetValue(formattedAddress, null),
                                                    State = formattedAddress.GetType().GetProperty("State").GetValue(formattedAddress, null),
                                                    StreetNumber = formattedAddress.GetType().GetProperty("StreetNumber").GetValue(formattedAddress, null),
                                                    Route = formattedAddress.GetType().GetProperty("Route").GetValue(formattedAddress, null),
                                                    ZipCode = formattedAddress.GetType().GetProperty("ZipCode").GetValue(formattedAddress, null),
                                                    OwnerID = row["ID"] as string
                                                }.DBInsert();
                                            }


                                        }

                                        keepLooping = false;
                                    }
                                    catch
                                    {
                                    }
                                }
                            }
                        }
                    }
                }


            }
            catch
            {
                throw;
            }
        }

        public static async Task<DataTable> LoadAllMain()
        {
            using (MySqlConnection connection = new MySqlConnection(Properties.ConnectionString))
            {
                await connection.OpenAsync();

                MySqlCommand command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = "SELECT * FROM `persons_main`";

                using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                {
                    if (reader.HasRows)
                    {
                        DataTable table = new DataTable();
                        table.Load(reader);
                        return table;
                    }
                    else
                        throw new Exception("no rows?");
                }

            }
        }

        public static async Task<DataTable> LoadAllContactInfo()
        {
            using (MySqlConnection connection = new MySqlConnection(Properties.ConnectionString))
            {
                await connection.OpenAsync();

                MySqlCommand command = connection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = "SELECT * FROM `persons_contactinfo`";

                using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                {
                    if (reader.HasRows)
                    {
                        DataTable table = new DataTable();
                        table.Load(reader);
                        return table;
                    }
                    else
                        throw new Exception("no rows?");
                }

            }
        }

    }
}
