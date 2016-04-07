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

namespace CommandDB_Plugin
{
    /// <summary>
    /// Provides methods for interacting with lists including a list class, the lists cache, and data access methods.
    /// </summary>
    public static class CDBLists
    {
        /// <summary>
        /// This local, readonly property is intended to standardize all methods in this class that access the database and allow easy maintenance.
        /// </summary>
        private static readonly string _tableName = "lists";

        /// <summary>
        /// The cache of in-memory, thread-safe lists.  Intended to be used by the service during operation.
        /// </summary>
        private static ConcurrentDictionary<string, CDBList> _cdbListsCache = new ConcurrentDictionary<string, CDBList>();

        /// <summary>
        /// Returns the current number of lists in the cache.
        /// </summary>
        public static int CurrentListsLoaded
        {
            get
            {
                return _cdbListsCache.Count;
            }
        }

        /// <summary>
        /// Describes a single list item including its ID, name, and values and data access methods.
        /// </summary>
        public class CDBList
        {

            #region Properties

            /// <summary>
            /// The ID of the list.
            /// </summary>
            public string ID { get; set; }
            /// <summary>
            /// SUPRISE!  It's the name of the list... ok, no surprise.
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// The values... if this isn't self documenting then I don't know what is.
            /// </summary>
            public List<string> Values { get; set; }

            #endregion

            #region Data Access Methods

            /// <summary>
            /// Inserts a new list into the lists table and optionally updates the cache with this new list.
            /// </summary>
            /// <param name="apikey"></param>
            /// <returns></returns>
            public async Task DBInsert(bool updateCache)
            {
                try
                {
                    using (MySqlConnection connection = new MySqlConnection(Properties.ConnectionString))
                    {
                        await connection.OpenAsync();

                        MySqlCommand command = connection.CreateCommand();
                        command.CommandType = CommandType.Text;
                        command.CommandText = string.Format("INSERT INTO `{0}` (`ID`,`Name`,`Values`) VALUES (@ID, @Name, @Values)", _tableName);

                        command.Parameters.AddWithValue("@ID", this.ID);
                        command.Parameters.AddWithValue("@Name", this.Name);
                        command.Parameters.AddWithValue("@Values", this.Values.Serialize());

                        await command.ExecuteNonQueryAsync();

                        if (updateCache)
                        {
                            _cdbListsCache.AddOrUpdate(this.ID, this, (key, value) =>
                            {
                                throw new Exception("There was an issue adding this list to the cache!");
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
            /// Updates the current list instance by resetting all columns to the current instance and uses the ID to index.  The ID itself cannot be updated.
            /// </summary>
            /// <returns></returns>
            public async Task DBUpdate(bool updateCache)
            {
                try
                {
                    using (MySqlConnection connection = new MySqlConnection(Properties.ConnectionString))
                    {
                        await connection.OpenAsync();

                        MySqlCommand command = connection.CreateCommand();
                        command.CommandType = CommandType.Text;
                        command.CommandText = string.Format("UPDATE `{0}` SET `Name` = @Name, `Values` = @Values WHERE `ID` = @ID", _tableName);

                        command.Parameters.AddWithValue("@Name", this.Name);
                        command.Parameters.AddWithValue("@Values", this.Values.Serialize());
                        command.Parameters.AddWithValue("@ID", this.ID);

                        await command.ExecuteNonQueryAsync();

                        if (updateCache)
                        {
                            if (!_cdbListsCache.ContainsKey(this.ID))
                                throw new Exception("The cache does not have this list and so wasn't able to update it.");

                            _cdbListsCache[this.ID] = this;
                        }

                    }
                }
                catch
                {
                    throw;
                }
            }

            /// <summary>
            /// Deletes the current list instance from the database by using the current ID as the primary key.
            /// </summary>
            /// <returns></returns>
            public async Task DBDelete(bool updateCache)
            {
                try
                {
                    using (MySqlConnection connection = new MySqlConnection(Properties.ConnectionString))
                    {
                        await connection.OpenAsync();

                        MySqlCommand command = connection.CreateCommand();
                        command.CommandType = CommandType.Text;
                        command.CommandText = string.Format("DELETE FROM `{0}` WHERE `ID` = @ID", _tableName);

                        command.Parameters.AddWithValue("@ID", this.ID);

                        await command.ExecuteNonQueryAsync();

                        if (updateCache)
                        {
                            CDBList temp;
                            if (!_cdbListsCache.TryRemove(this.ID, out temp))
                                throw new Exception("The cache does not contain this list and so wasn't able to delete it.");
                        }

                    }
                }
                catch
                {
                    throw;
                }
            }

            /// <summary>
            /// Returns a boolean indicating whether or not the current list instance exists in the database.  Uses the ID to do this comparison.
            /// </summary>
            /// <returns></returns>
            public async Task<bool> DBExists(bool useCache)
            {
                try
                {
                    if (useCache)
                        return _cdbListsCache.ContainsKey(this.ID);

                    using (MySqlConnection connection = new MySqlConnection(Properties.ConnectionString))
                    {
                        await connection.OpenAsync();

                        MySqlCommand command = connection.CreateCommand();
                        command.CommandType = CommandType.Text;
                        command.CommandText = string.Format("SELECT CASE WHEN EXISTS(SELECT * FROM `{0}` WHERE `ID` = @ID) THEN 1 ELSE 0 END", _tableName);

                        command.Parameters.AddWithValue("@ID", this.ID);

                        return Convert.ToBoolean(await command.ExecuteScalarAsync());
                    }
                }
                catch
                {
                    throw;
                }
            }

            #endregion

        }

        #region Static Access Methods

        /// <summary>
        /// Loads all lists from the database.  This call will also optionally flush the current cache and reset it with these results.
        /// </summary>
        /// <returns></returns>
        public static async Task<List<CDBList>> DBLoadAll(bool updateCache)
        {
            try
            {
                List<CDBList> result = new List<CDBList>();
                using (MySqlConnection connection = new MySqlConnection(Properties.ConnectionString))
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
                                result.Add(new CDBList()
                                {
                                    ID = reader["ID"].ToString(),
                                    Name = reader["Name"].ToString(),
                                    Values = reader["Values"].ToString().Deserialize<List<string>>()
                                });
                            }
                        }
                    }
                }

                if (updateCache)
                {
                    _cdbListsCache = new ConcurrentDictionary<string, CDBList>(result.Select(x => new KeyValuePair<string, CDBList>(x.ID, x)));
                }

                return result;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Released the CDB Lists cache's memory by clearing the cache.
        /// </summary>
        public static void ReleaseCache()
        {
            try
            {
                _cdbListsCache.Clear();
            }
            catch
            {
                throw;
            }
        }

        #endregion

        //The methods in this region are intended to be exposed to the client through the service interface.  
        //All inputs should first be validated prior to any database operations!
        #region Client Access Methods

        /// <summary>
        /// WARNING!  THIS IS A CLIENT METHOD.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Loads the lists from the database.  Lists are meant to power the drop down boxes on the front end, mainly.
        /// <para />
        /// Options: 
        /// <para />
        /// acceptcachedresults : Instructs the service to returns all lists from either the database or the cache.  Default : true
        /// <para />
        /// name : The name of the list the client wants to load.  If not found or if it is empty, returns all of the lists.  Not Case sensitive.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<MessageTokens.MessageToken> LoadLists_Client(MessageTokens.MessageToken token)
        {
            try
            {
                //default to true
                bool acceptCachedResults = true;
                if (token.Args.ContainsKey("acceptcachedresults"))
                    acceptCachedResults = Convert.ToBoolean(token.Args["acceptcachedresults"]);

                string name = null;
                if (token.Args.ContainsKey("name"))
                    name = token.Args["name"] as string;

                if (acceptCachedResults) //The client is ok with cached results.
                {
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        token.Result = _cdbListsCache.Values.Where(x => x.Name.SafeEquals(name)).ToList();
                    }
                    else
                    {
                        token.Result = _cdbListsCache.Values.ToList(); 
                    }
                }
                else //The client wants new results.
                {
                    List<CDBList> result = new List<CDBList>();
                    using (MySqlConnection connection = new MySqlConnection(Properties.ConnectionString))
                    {
                        await connection.OpenAsync();

                        MySqlCommand command = connection.CreateCommand();
                        command.CommandType = CommandType.Text;
                        command.CommandText = string.Format("SELECT * FROM `{0}` ", _tableName);

                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            command.CommandText += "WHERE `Name` LIKE @Name";

                            command.Parameters.AddWithValue("@Name", name);
                        }

                        using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                        {
                            if (reader.HasRows)
                            {
                                while (await reader.ReadAsync())
                                {
                                    result.Add(new CDBList()
                                    {
                                        ID = reader["ID"].ToString(),
                                        Name = reader["Name"].ToString(),
                                        Values = reader["Values"].ToString().Deserialize<List<string>>()
                                    });
                                }
                            }
                        }
                    }
                    token.Result = result;
                }

                return token;
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region Other Methods

        /// <summary>
        /// Retrieves a list from the cache.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static CDBList GetList(string name)
        {
            return _cdbListsCache.Values.FirstOrDefault(x => x.Name.SafeEquals(name));
        }

        #endregion



    }
}
