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
    /// Provides methods for interacting with commands including a command class, the commands cache, and data access methods.
    /// </summary>
    public static class Commands
    {
        /// <summary>
        /// This local, readonly property is intended to standardize all methods in this class that access the database and allow easy maintenance.
        /// </summary>
        private static readonly string _tableName = "newcommands";

        /// <summary>
        /// The cache of in-memory, thread-safe commands.  Intended to be used by the service during operation.
        /// </summary>
        private static ConcurrentDictionary<string, Command> _commandsCache = new ConcurrentDictionary<string, Command>();

        /// <summary>
        /// Returns the count of the number of entries in the commands cache.
        /// </summary>
        public static int CurrentCommandsLoaded
        {
            get
            {
                return _commandsCache.Count;

            }
        }

        /// <summary>
        /// Returns the commands cache.
        /// </summary>
        internal static ConcurrentDictionary<string, Command> CommandsCache
        {
            get
            {
                return _commandsCache;
            }
        }

        /// <summary>
        /// Describes a single command including its properties and data access methods.
        /// </summary>
        public class Command
        {

            #region Properties

            /// <summary>
            /// The ID of the command.
            /// </summary>
            public string ID { get; set; }
            /// <summary>
            /// The command's name.
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// A short description of this command.
            /// </summary>
            public string Description { get; set; }
            /// <summary>
            /// The departments of the command
            /// </summary>
            public List<Department> Departments { get; set; }

            #endregion

            #region Helping Classes

            /// <summary>
            /// Describes a single department and also contains the definition for a division.
            /// </summary>
            public class Department
            {
                #region Properties

                /// <summary>
                /// The ID of this department.
                /// </summary>
                public string ID { get; set; }
                /// <summary>
                /// The Name of this department.
                /// </summary>
                public string Name { get; set; }
                /// <summary>
                /// A short description of this department.
                /// </summary>
                public string Description { get; set; }
                /// <summary>
                /// A list of those divisions that belong to this department.
                /// </summary>
                public List<Division> Divisions { get; set; }


                #endregion

                #region Helping Classes

                /// <summary>
                /// Describes a single division.
                /// </summary>
                public class Division
                {

                    #region Properties

                    /// <summary>
                    /// The ID of this division.
                    /// </summary>
                    public string ID { get; set; }
                    /// <summary>
                    /// The name of this division.
                    /// </summary>
                    public string Name { get; set; }
                    /// <summary>
                    /// A short descripion of this division.
                    /// </summary>
                    public string Description { get; set; }

                    #endregion
                    
                }

                #endregion
            }

            #endregion

            #region Data Access Methods

            /// <summary>
            /// Inserts a new command into the new commands table and optionally updates the cache with this new command.
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
                        command.CommandText = string.Format("INSERT INTO `{0}` (`ID`,`Name`,`Departments`,`Description`) VALUES (@ID, @Name, @Departments, @Description)", _tableName);

                        command.Parameters.AddWithValue("@ID", this.ID);
                        command.Parameters.AddWithValue("@Name", this.Name);
                        command.Parameters.AddWithValue("@Departments", this.Departments.Serialize());
                        command.Parameters.AddWithValue("@Description", this.Description);

                        await command.ExecuteNonQueryAsync();

                        if (updateCache)
                        {
                            _commandsCache.AddOrUpdate(this.ID, this, (key, value) =>
                            {
                                throw new Exception("There was an issue adding this command to the cache");
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
            /// Updates the current command instance by resetting all columns to the current instance and uses the ID to index.  The ID itself cannot be updated.
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
                        command.CommandText = string.Format("UPDATE `{0}` SET `Name` = @Name, `Departments` = @Departments, `Description` = @Description WHERE `ID` = @ID", _tableName);

                        command.Parameters.AddWithValue("@Name", this.Name);
                        command.Parameters.AddWithValue("@Departments", this.Departments.Serialize());
                        command.Parameters.AddWithValue("@Description", this.Description);
                        command.Parameters.AddWithValue("@ID", this.ID);

                        await command.ExecuteNonQueryAsync();

                        if (updateCache)
                        {
                            if (!_commandsCache.ContainsKey(this.ID))
                                throw new Exception("The cache does not have this command and so wasn't able to update it.");

                            _commandsCache[this.ID] = this;
                        }

                    }
                }
                catch
                {
                    throw;
                }
            }

            /// <summary>
            /// Deletes the current command instance from the database by using the current ID as the primary key.
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
                            Command temp;
                            if (!_commandsCache.TryRemove(this.ID, out temp))
                                throw new Exception("The cache does not contain this command and so wasn't able to delete it.");
                        }

                    }
                }
                catch
                {
                    throw;
                }
            }

            /// <summary>
            /// Returns a boolean indicating if the current instance exists in the database.  This is done by searching for the ID.
            /// </summary>
            /// <returns></returns>
            public async Task<bool> DBExists()
            {
                try
                {
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
        /// Loads all commands from the database.  This call will also optionally flush the current cache and reset it with these results.
        /// </summary>
        /// <returns></returns>
        public static async Task<List<Command>> DBLoadAll(bool updateCache)
        {
            try
            {
                List<Command> result = new List<Command>();
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
                                result.Add(new Command()
                                {
                                    ID = reader["ID"].ToString(),
                                    Name = reader["Name"].ToString(),
                                    Departments = reader["Departments"].ToString().Deserialize<List<Command.Department>>(),
                                    Description = reader["Description"].ToString()
                                });
                            }
                        }
                    }
                }

                if (updateCache)
                {
                    _commandsCache = new ConcurrentDictionary<string, Command>(result.Select(x => new KeyValuePair<string, Command>(x.ID, x)));
                }

                return result;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Releases the Commands cache's memory by clearing the cache.
        /// </summary>
        public static void ReleaseCache()
        {
            try
            {
                _commandsCache.Clear();
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
        /// Loads the commands from the database.
        /// <para />
        /// Options: 
        /// <para />
        /// acceptcachedresults : Instructs to the service to either return the commands from the cache or to load them new.  Default : true
        /// <para />
        /// name : The name of the command the client wants to load.  If not found or if it is empty, returns all of the commands.  Case sensitive.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<MessageTokens.MessageToken> LoadCommands_Client(MessageTokens.MessageToken token)
        {
            try
            {

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
                        token.Result = _commandsCache.Values.Where(x => x.Name.SafeEquals(name)).ToList();
                    }
                    else
                    {
                        token.Result = _commandsCache.Values.ToList();
                    }
                }
                else //The client wants the results.
                {
                    List<Command> result = new List<Command>();
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
                                    result.Add(new Command()
                                    {
                                        ID = reader["ID"].ToString(),
                                        Name = reader["Name"].ToString(),
                                        Departments = reader["Departments"].ToString().Deserialize<List<Command.Department>>(),
                                        Description = reader["Description"].ToString()
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

        

        #endregion


    }
}
