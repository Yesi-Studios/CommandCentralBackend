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

namespace UnifiedServiceFramework.Authentication
{
    /// <summary>
    /// Provides methods for interacting with apikeys including an apikey class, the apikeys cache, and data access methods.
    /// </summary>
    internal static class APIKeys
    {
        /// <summary>
        /// This local, readonly property is intended to standardize all methods in this class that access the database and allow easy maintenance.
        /// </summary>
        private static readonly string _tableName = "apikeys";

        /// <summary>
        /// The cache of in-memory, thread-safe apikeys.  Intended to be used by the service during operation.
        /// </summary>
        private static ConcurrentDictionary<string, APIKey> _apiKeysCache = new ConcurrentDictionary<string, APIKey>();

        /// <summary>
        /// Describes a single apikey and provides methods for DB interaction.
        /// </summary>
        internal class APIKey
        {

            #region Properties

            /// <summary>
            /// The unique ID of this api key.
            /// </summary>
            internal string ID { get; set; }

            /// <summary>
            /// The actual api key itself.
            /// </summary>
            internal string Key { get; set; }

            #endregion

            #region Data Access Methods

            /// <summary>
            /// Inserts a new apikey into the apikeys table and optionally updates the cache with this new apikey.
            /// </summary>
            /// <param name="updateCache"></param>
            /// <returns></returns>
            internal async Task DBInsert(bool updateCache)
            {
                try
                {
                    using (MySqlConnection connection = new MySqlConnection(Framework.Settings.ConnectionString))
                    {
                        await connection.OpenAsync();

                        MySqlCommand command = connection.CreateCommand();
                        command.CommandType = CommandType.Text;
                        command.CommandText = string.Format("INSERT INTO `{0}` VALUES (@ID, @APIKey)", _tableName);

                        command.Parameters.AddWithValue("@ID", this.ID);
                        command.Parameters.AddWithValue("@APIKey", this.Key);

                        await command.ExecuteNonQueryAsync();

                        if (updateCache)
                        {
                            _apiKeysCache.AddOrUpdate(this.ID, this, (key, value) =>
                            {
                                throw new Exception("There was an issue adding this apikey to the cache");
                            });
                        }
                    }
                }
                catch
                {
                    throw;
                }
            }

            /// <summary>
            /// Updates the current apikey instance by resetting all columns to the current instance and uses the ID to index.  The ID itself cannot be updated.
            /// </summary>
            /// <returns></returns>
            internal async Task DBUpdate(bool updateCache)
            {
                try
                {
                    using (MySqlConnection connection = new MySqlConnection(Settings.ConnectionString))
                    {
                        await connection.OpenAsync();

                        MySqlCommand command = connection.CreateCommand();
                        command.CommandType = CommandType.Text;
                        command.CommandText = string.Format("UPDATE `{0}` SET `Key` = @Key WHERE `ID` = @ID", _tableName);

                        command.Parameters.AddWithValue("@Key", this.Key);
                        command.Parameters.AddWithValue("@ID", this.ID);

                        await command.ExecuteNonQueryAsync();

                        if (updateCache)
                        {
                            if (!_apiKeysCache.ContainsKey(this.ID))
                                throw new Exception("The cache does not have this api key and so wasn't able to update it.");

                            _apiKeysCache[this.ID] = this;
                        }

                    }
                }
                catch
                {
                    throw;
                }
            }

            /// <summary>
            /// Deletes the current apikey instance from the database by using the current ID as the primary key.
            /// </summary>
            /// <returns></returns>
            internal async Task DBDelete(bool updateCache)
            {
                try
                {
                    using (MySqlConnection connection = new MySqlConnection(Settings.ConnectionString))
                    {
                        await connection.OpenAsync();

                        MySqlCommand command = connection.CreateCommand();
                        command.CommandType = CommandType.Text;
                        command.CommandText = string.Format("DELETE FROM `{0}` WHERE `ID` = @ID", _tableName);

                        command.Parameters.AddWithValue("@ID", this.ID);

                        await command.ExecuteNonQueryAsync();

                        if (updateCache)
                        {
                            APIKey temp;
                            if (!_apiKeysCache.TryRemove(this.ID, out temp))
                                throw new Exception("The cache does not contain this api key and so wasn't able to delete it.");
                        }

                    }
                }
                catch
                {
                    throw;
                }
            }

            #endregion

        }

        #region Static Data Access Methods

        /// <summary>
        /// Loads all APIKeys from the database.  This call will also optionally flush the current cache and reset it with these results.
        /// </summary>
        /// <returns></returns>
        internal static async Task<List<APIKey>> DBLoadAll(bool updateCache)
        {
            try
            {
                List<APIKey> result = new List<APIKey>();
                using (MySqlConnection connection = new MySqlConnection(Settings.ConnectionString))
                {
                    await connection.OpenAsync();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = string.Format("SELECT * FROM `{0}`", _tableName);

                    using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            while (await reader.ReadAsync())
                            {
                                result.Add(new APIKey()
                                {
                                    ID = reader["ID"].ToString(),
                                    Key = reader["Key"].ToString()
                                });
                            }
                        }
                    }
                }

                if (updateCache)
                {
                    _apiKeysCache = new ConcurrentDictionary<string, APIKey>(result.Select(x => new KeyValuePair<string, APIKey>(x.ID, x)));
                }

                return result;
            }
            catch
            {
                throw;
            }
        }

        internal static void ReleaseCache()
        {
            try
            {
                _apiKeysCache.Clear();
            }
            catch
            {
                throw;
            }
        }


        #endregion

        #region Other Methods

        internal static bool IsAPIKeyValid(string apikey)
        {
            if (string.IsNullOrEmpty(apikey))
                return false;

            if (!_apiKeysCache.Values.ToList().Exists(x => x.Key.SafeEquals(apikey)))
                return false;

            return true;
        }



        #endregion




    }
}
