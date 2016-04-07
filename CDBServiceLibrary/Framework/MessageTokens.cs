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
using UnifiedServiceFramework;
using AtwoodUtils;

namespace UnifiedServiceFramework.Framework
{
    /// <summary>
    /// Provides members that help interact with message tokens as well as the message token class itself and a cache.
    /// </summary>
    public static class MessageTokens
    {
        /// <summary>
        /// This local, readonly property is intended to standardize all methods in this class that access the database and allow easy maintenance.
        /// </summary>
        private static readonly string _tableName = "messages";

        /// <summary>
        /// The cache of in-memory, thread-safe message tokens.  Intended to be used by the service during operation.  The cache should be purged of Inactive or Failed Messages
        /// </summary>
        private static ConcurrentDictionary<string, MessageToken> _messageTokensCache = new ConcurrentDictionary<string, MessageToken>();

        /// <summary>
        /// Indicates for how long instances should exist in the database before being dropped.  This is used by Cron Operations.
        /// </summary>
        private static readonly TimeSpan _maxDBPersistanceTime = TimeSpan.FromDays(2);

        /// <summary>
        /// An enum describing the possible states a message can be in.
        /// </summary>
        public enum MessageStates
        {
            /// <summary>
            /// Indicates that the message is currently being handled.
            /// </summary>
            Active,
            /// <summary>
            /// Inidicates that the message has been handled.
            /// </summary>
            Handled,
            /// <summary>
            /// Indicates the the message failed for some reason.
            /// </summary>
            Failed
        }

        /// <summary>
        /// Describes a message token, including its Access Methods.  Intended to be used to track an interaction with the client.
        /// </summary>
        public class MessageToken
        {

            #region Properties

            /// <summary>
            /// The unique ID assigned to this message interaction
            /// </summary>
            public string ID { get; set; }
            /// <summary>
            /// The APIKey that the client used to access the API
            /// </summary>
            public string APIKey { get; set; }
            /// <summary>
            /// The time at which the client called the API.
            /// </summary>
            public DateTime CallTime { get; set; }
            /// <summary>
            /// The Arguments the client sent to the API.
            /// </summary>
            public Dictionary<string, object> Args { get; set; }
            /// <summary>
            /// The endpoint that was invoked by the client.
            /// </summary>
            public string Endpoint { get; set; }
            /// <summary>
            /// The response that was sent back to the client.
            /// </summary>
            public object Result { get; set; }
            /// <summary>
            /// The current state of the message interaction.
            /// </summary>
            public MessageStates State { get; set; }
            /// <summary>
            /// The time at which the message was handled.
            /// </summary>
            public DateTime HandledTime { get; set; }
            /// <summary>
            /// The session that was active when the message began.
            /// </summary>
            public Authentication.Sessions.Session Session { get; set; }

            #endregion

            #region Data Access Methods

            /// <summary>
            /// Inserts the message token into the database and optionally updates the cache, redacts argument information and response information.
            /// </summary>
            /// <returns></returns>
            public async Task DBInsert(bool updateCache, bool allowArgumentLogging, bool allowResponseLogging)
            {
                try
                {
                    using (MySqlConnection connection = new MySqlConnection(Framework.Settings.ConnectionString))
                    {
                        await connection.OpenAsync();

                        MySqlCommand command = connection.CreateCommand();
                        command.CommandType = CommandType.Text;
                        command.CommandText = string.Format("INSERT INTO `{0}` (`ID`,`APIKey`,`CallTime`,`Args`,`Endpoint`,`Response`,`State`,`HandledTime`,`SessionID`) ", _tableName) +
                            "VALUES (@ID, @APIKey, @CallTime, @Args, @Endpoint, @Response, @State, @HandledTime, @SessionID)";

                        //I use a "fake" dictionary to store the redacted string so that if the message were loaded, it would map back into the Args field without an issue.
                        string args = (this.Args == null) ? null : (allowArgumentLogging) ? this.Args.Serialize() : new Dictionary<string, string>() { { "REDACTED", "REDACTED" } }.Serialize();
                        string response = (this.Result == null) ? null : (allowResponseLogging) ? this.Result.Serialize() : "REDACTED";

                        command.Parameters.AddWithValue("@ID", this.ID);
                        command.Parameters.AddWithValue("@APIKey", this.APIKey);
                        command.Parameters.AddWithValue("@CallTime", this.CallTime.ToMySqlDateTimeString());
                        command.Parameters.AddWithValue("@Args", args);
                        command.Parameters.AddWithValue("@Endpoint", this.Endpoint);
                        command.Parameters.AddWithValue("@Response", response.Truncate(1000));
                        command.Parameters.AddWithValue("@State", this.State.Serialize());
                        command.Parameters.AddWithValue("@HandledTime", this.HandledTime.ToMySqlDateTimeString());
                        command.Parameters.AddWithValue("@SessionID", (this.Session == null) ? null : this.Session.ID); 

                        await command.ExecuteNonQueryAsync();

                        if (updateCache)
                        {
                            _messageTokensCache.AddOrUpdate(this.ID, this, (key, value) =>
                            {
                                throw new Exception("There was an issue adding this message token to the cache");
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
            /// Updates the current message token instance by resetting all columns to the current instance and uses the ID to index.  The ID itself cannot be updated.
            /// <para />
            /// Optionally, redacts the argument or response information.
            /// </summary>
            /// <returns></returns>
            public async Task DBUpdate(bool updateCache, bool allowArgumentLogging, bool allowResponseLogging)
            {
                try
                {
                    using (MySqlConnection connection = new MySqlConnection(Settings.ConnectionString))
                    {
                        await connection.OpenAsync();

                        MySqlCommand command = connection.CreateCommand();
                        command.CommandType = CommandType.Text;
                        command.CommandText = string.Format("UPDATE `{0}` SET `APIKey` = @APIKey, `CallTime` = @CallTime, `Args` = @Args, `Endpoint` = @Endpoint, `Response` = @Response, `State` = @State, `HandledTime` = @HandledTime, `SessionID` = @SessionID WHERE `ID` = @ID", _tableName);

                        string args = (this.Args == null) ? null : (allowArgumentLogging) ? this.Args.Serialize() : new Dictionary<string, string>() { { "REDACTED", "REDACTED" } }.Serialize();
                        string response = (this.Result == null) ? null : (allowResponseLogging) ? this.Result.Serialize() : "REDACTED";

                        command.Parameters.AddWithValue("@APIKey", this.APIKey);
                        command.Parameters.AddWithValue("@CallTime", this.CallTime.ToMySqlDateTimeString());
                        command.Parameters.AddWithValue("@Args", args);
                        command.Parameters.AddWithValue("@Endpoint", this.Endpoint);
                        command.Parameters.AddWithValue("@Response", response.Truncate(1000));
                        command.Parameters.AddWithValue("@State", this.State.Serialize());
                        command.Parameters.AddWithValue("@HandledTime", this.HandledTime.ToMySqlDateTimeString());
                        command.Parameters.AddWithValue("@SessionID", (this.Session == null) ? "No Session" : this.Session.ID);

                        command.Parameters.AddWithValue("@ID", this.ID);

                        await command.ExecuteNonQueryAsync();

                        if (updateCache)
                        {
                            if (!_messageTokensCache.ContainsKey(this.ID))
                                throw new Exception("The cache does not have this message token and so wasn't able to update it.");

                            _messageTokensCache[this.ID] = this;
                        }

                    }
                }
                catch
                {
                    throw;
                }
            }

            /// <summary>
            /// Deletes the current message token instance from the database by using the current ID as the primary key.
            /// </summary>
            /// <returns></returns>
            public async Task DBDelete(bool updateCache)
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
                            MessageToken temp;
                            if (!_messageTokensCache.TryRemove(this.ID, out temp))
                                throw new Exception("The cache does not contain this message token and so wasn't able to delete it.");
                        }
                    }
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

        #region Static Access Methods

        /// <summary>
        /// Used to update a message token if the message fails and falls into the catch block.  This method can't be asynchrounous and if it fails we're fucked.  
        /// <para />
        /// Also, in this method we can't have access to a message token because that's built in the try block of most requests.
        /// </summary>
        /// <param name="messageID"></param>
        /// <param name="errorMessage"></param>
        public static void UpdateOnFatalError(string messageID, string errorMessage)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(Settings.ConnectionString))
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = string.Format("UPDATE {0} SET `HandledTime` = @HandledTime, `State` = @State, `Response` = @Response WHERE `ID` = @ID", _tableName);

                    command.Parameters.AddWithValue("@HandledTime", DateTime.Now.ToMySqlDateTimeString());
                    command.Parameters.AddWithValue("@State", MessageStates.Failed.Serialize());
                    command.Parameters.AddWithValue("@Response", errorMessage);
                    command.Parameters.AddWithValue("@ID", messageID);

                    command.ExecuteNonQuery();
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Used to update a message token if the message fails and falls into the catch block.  This method can't be asynchrounous and if it fails we're fucked.  
        /// <para />
        /// Also, in this method we can't have access to a message token because that's built in the try block of most requests.
        /// </summary>
        /// <param name="messageID"></param>
        /// <param name="errorMessage"></param>
        public static void UpdateOnExpectedError(string messageID, string errorMessage)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(Settings.ConnectionString))
                {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = string.Format("UPDATE {0} SET `HandledTime` = @HandledTime, `State` = @State, `Response` = @Response WHERE `ID` = @ID", _tableName);

                    command.Parameters.AddWithValue("@HandledTime", DateTime.Now.ToMySqlDateTimeString());
                    command.Parameters.AddWithValue("@State", MessageStates.Handled.Serialize());
                    command.Parameters.AddWithValue("@Response", errorMessage);
                    command.Parameters.AddWithValue("@ID", messageID);

                    command.ExecuteNonQuery();
                }
            }
            catch
            {
                throw;
            }
        }


        /// <summary>
        /// Loads all Message Tokens from the database.  During normal operations, this should never be needed.  Optionally, resets the entire cache with the results.
        /// <para />
        /// The second update parameter is used in a child call to Authentication.Sessions.DBLoadOne
        /// </summary>
        /// <returns></returns>
        public static async Task<List<MessageToken>> DBLoadAll(bool updateCache, bool updateSessionsCache)
        {
            try
            {
                List<MessageToken> result = new List<MessageToken>();
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
                                result.Add(new MessageToken()
                                {
                                    APIKey = reader["APIKey"].ToString(),
                                    Args = reader["Args"].ToString().Deserialize<Dictionary<string, object>>(),
                                    CallTime = Convert.ToDateTime(reader["CallTime"]),
                                    Endpoint = reader["Endpoint"].ToString(),
                                    HandledTime = Convert.ToDateTime(reader["HandledTime"]),
                                    ID = reader["ID"].ToString(),
                                    Result = reader["Response"].ToString(),
                                    Session = await Authentication.Sessions.DBLoadOne(reader["SessionID"].ToString(), updateSessionsCache),
                                    State = (reader["State"] as string).Deserialize<MessageStates>()
                                });
                            }
                        }
                    }
                }

                if (updateCache)
                {
                    _messageTokensCache = new ConcurrentDictionary<string, MessageToken>(result.Select(x => new KeyValuePair<string, MessageToken>(x.ID, x)));
                }

                return result;
            }
            catch
            {
                throw;
            }
        }


        /// <summary>
        /// Released the Message Tokens cache's memory by clearing the cache.
        /// </summary>
        public static void ReleaseCache()
        {
            try
            {
                _messageTokensCache.Clear();
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region Cron Operations

        /// <summary>
        /// Scrubs the messages table, removing old message token from the database.
        /// </summary>
        public static void ScrubMessages()
        {
            try
            {
                //We're going to load all of the message tokens from the database and then delete those that are older than the persistance time and are not in the cache.
                var messageTokens = MessageTokens.DBLoadAll(false, false).Result;

                Parallel.ForEach<MessageTokens.MessageToken>(messageTokens, (token) =>
                    {
                        if (DateTime.Now.Subtract(token.CallTime) > _maxDBPersistanceTime)
                        {
                            if (_messageTokensCache.ContainsKey(token.ID))
                                token.DBDelete(true).Wait();
                            else
                                token.DBDelete(false).Wait();
                        }

                    });
            }
            catch
            {
                throw;
            }
        }

        #endregion



    }
}
