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
    /// Contains members for handling MainData, which includes the change log, known issues, current version number, and other items.
    /// </summary>
    public static class MainData
    {
        /// <summary>
        /// This local, readonly property is intended to standardize all methods in this class that access the database and allow easy maintenance.
        /// </summary>
        private static readonly string _tableName = "maindata";

        /// <summary>
        /// The cache of in-memory, thread-safe main data.  Intended to be used by the service during operation.
        /// </summary>
        private static MainDataItem _currentMainData = new MainDataItem();

        /// <summary>
        /// returns the most recent main data item.
        /// </summary>
        public static MainDataItem CurrentMainData
        {
            get
            {
                return _currentMainData;
            }
        }

        /// <summary>
        /// Describes the main data of the application, which reprents dynamically loaded content.
        /// </summary>
        public class MainDataItem
        {

            #region Properties

            /// <summary>
            /// The ID of this main data object.
            /// </summary>
            public string ID { get; set; }
            /// <summary>
            /// The change log of the application
            /// </summary>
            public List<ChangeLogItem> ChangeLog { get; set; }
            /// <summary>
            /// A list of currently known issues.
            /// </summary>
            public List<string> KnownIssues { get; set; }
            /// <summary>
            /// The current version of the application.
            /// </summary>
            public string Version { get; set; }
            /// <summary>
            /// The time this main data was made.  
            /// </summary>
            public DateTime Time { get; set; }

            #endregion

            #region ctors

            public MainDataItem()
            {
                this.ChangeLog = new List<ChangeLogItem>();
                this.ID = string.Empty;
                this.KnownIssues = new List<string>();
                this.Time = DateTime.MinValue;
                this.Version = string.Empty;
            }

            #endregion

            #region Helper Classes

            /// <summary>
            /// Describes a single change log item.
            /// </summary>
            public class ChangeLogItem
            {

                #region Properties

                /// <summary>
                /// The ID of this change log item.
                /// </summary>
                public string ID { get; set; }
                /// <summary>
                /// The version of the application at the time of this change log.
                /// </summary>
                public string Version { get; set; }
                /// <summary>
                /// The time this change log item was made.
                /// </summary>
                public DateTime Time { get; set; }
                /// <summary>
                /// A list of all changes made during this change.
                /// </summary>
                public List<string> Changes { get; set; }

                #endregion

                #region ctors

                public ChangeLogItem()
                {
                    this.ID = string.Empty;
                    this.Version = string.Empty;
                    this.Time = DateTime.MinValue;
                    this.Changes = new List<string>();
                }

                #endregion

            }

            #endregion

            #region Data Access Methods

            /// <summary>
            /// Very simply, this methods inserts a new main data.
            /// </summary>
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
                        command.CommandText = string.Format("INSERT INTO `{0}` (`ID`,`ChangeLog`,`KnownIssues`,`Version`,`Time`) VALUES (@ID, @ChangeLog, @KnownIssues, @Version, @Time)", _tableName);

                        command.Parameters.AddWithValue("@ID", this.ID);
                        command.Parameters.AddWithValue("@ChangeLog", this.ChangeLog.Serialize());
                        command.Parameters.AddWithValue("@KnownIssues", this.KnownIssues.Serialize());
                        command.Parameters.AddWithValue("@Version", this.Version);
                        command.Parameters.AddWithValue("@Time", this.Time.ToMySqlDateTimeString());

                        await command.ExecuteNonQueryAsync();

                        if (updateCache)
                        {
                            _currentMainData = this;
                        }
                    }
                }
                catch
                {
                    throw;
                }
            }

            /// <summary>
            /// Updates this Main Data instance and optionally updates the cache.
            /// </summary>
            /// <param name="updateCache"></param>
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
                        command.CommandText = string.Format("UPDATE `{0}` SET `ChangeLog` = @ChangeLog, `KnownIssues` = @KnownIssues, `Version` = @Version, `Time` = @Time WHERE `ID` = @ID", _tableName);

                        command.Parameters.AddWithValue("@ChangeLog", this.ChangeLog.Serialize());
                        command.Parameters.AddWithValue("@KnownIssues", this.KnownIssues.Serialize());
                        command.Parameters.AddWithValue("@Version", this.Version);
                        command.Parameters.AddWithValue("@Time", this.Time.ToMySqlDateTimeString());
                        command.Parameters.AddWithValue("@ID", this.ID);

                        await command.ExecuteNonQueryAsync();

                        if (updateCache)
                        {
                            _currentMainData = this;
                        }

                    }
                }
                catch
                {
                    throw;
                }
            }

            /// <summary>
            /// Deletes the current main data instance from the database by using the current ID as the primary key.
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

                        if (updateCache && _currentMainData.ID.SafeEquals(this.ID))
                        {
                            _currentMainData = new MainDataItem();
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
        /// Loads all main data from the database.  This call will also optionally flush the current cache and reset it with the most recent result.
        /// </summary>
        /// <returns></returns>
        public static async Task<List<MainDataItem>> DBLoadAll(bool updateCache)
        {
            try
            {
                List<MainDataItem> result = new List<MainDataItem>();
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
                                result.Add(new MainDataItem()
                                {
                                    ID = reader["ID"].ToString(),
                                    ChangeLog = reader["ChangeLog"].ToString().Deserialize<List<MainDataItem.ChangeLogItem>>(),
                                    KnownIssues = reader["KnownIssues"].ToString().Deserialize<List<string>>(),
                                    Time = DateTime.Parse(reader["Time"].ToString()),
                                    Version = reader["Version"].ToString()
                                });
                            }
                        }
                    }
                }

                if (updateCache)
                {
                    _currentMainData = result.OrderByDescending(x => x.Time).FirstOrDefault() ?? new MainDataItem();
                }

                return result;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Releases the Main Data cache's memory by clearing the cache.
        /// </summary>
        public static void ReleaseCache()
        {
            try
            {
                _currentMainData = new MainDataItem();
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region Client Access Methods

        /// <summary>
        /// WARNING!  THIS IS A CLIENT METHOD.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Loads the most recent main data.
        /// <para />
        /// Options: 
        /// <para />
        /// acceptcachedresults : Instructs the service to return the most recent main data from either the cache or from the database. Default : true
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<MessageTokens.MessageToken> LoadMostRecentMainData(MessageTokens.MessageToken token)
        {
            try
            {
                //default to true
                bool acceptCachedResults = true;
                if (token.Args.ContainsKey("acceptcachedresults"))
                    acceptCachedResults = Convert.ToBoolean(token.Args["acceptcachedresults"]);

                if (acceptCachedResults) //Client is ok with cached results.
                {
                    token.Result = _currentMainData;
                }
                else //Client wants new results... so just reload the cache and then give the client the cache, lol.
                {
                    await DBLoadAll(true);
                    token.Result = _currentMainData;
                }

                return token;

            }
            catch
            {
                
                throw;
            }
        }

        #endregion


    }
}
