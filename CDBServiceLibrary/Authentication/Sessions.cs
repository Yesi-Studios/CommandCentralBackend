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
    /// Provides members for handling authentication sessions for clients.  Includes a cache, a Session class, and data access methods for managing the sessions in the database.
    /// </summary>
    public static class Sessions
    {
        /// <summary>
        /// This local, readonly property is intended to standardize all methods in this class that access the database and allow easy maintenance.
        /// </summary>
        private static readonly string _tableName = "sessions";

        /// <summary>
        /// Informs the service the maximum amount of time a session can go between making service calls before it becomes inactive.
        /// </summary>
        private static readonly TimeSpan _maxInactivityTime = TimeSpan.FromMinutes(30);

        /// <summary>
        /// An in-memory cache that allows for quickly handling sessions.
        /// </summary>
        private static ConcurrentDictionary<string, Session> _sessionsCache = new ConcurrentDictionary<string, Session>();

        /// <summary>
        /// Indicates for how long instances should exist in the database before being dropped.  This is used by Cron Operations.
        /// </summary>
        private static readonly TimeSpan _maxDBPersistanceTime = TimeSpan.FromDays(2);

        /// <summary>
        /// Describes a single session and provides members for interacting with that session.
        /// </summary>
        public class Session
        {

            #region Properties

            /// <summary>
            /// The ID of the session.  This ID should also be used as the authentication token by the client.
            /// </summary>
            public string ID { get; set; }

            /// <summary>
            /// The time this session was created which is the time the client logged in.
            /// </summary>
            public DateTime LoginTime { get; set; }

            /// <summary>
            /// The ID of the person to whom this session belongs.
            /// </summary>
            public string PersonID { get; set; }

            /// <summary>
            /// The time at which the client logged out, thus invalidating this session.
            /// </summary>
            public DateTime LogoutTime { get; set; }

            /// <summary>
            /// Indicates where or not the session is valid.
            /// </summary>
            public bool IsActive { get; set; }

            /// <summary>
            /// The permission IDs of the user to whom this session belongs.  The session carries these IDs with it so that we don't have to go grab them everytime we need to authorize an action by the client.  The permission IDs will be updated everytime the client logs in if they have changed.
            /// </summary>
            public List<string> PermissionIDs { get; set; }

            /// <summary>
            /// The last time this session was used, not counting this current time.
            /// </summary>
            public DateTime LastUsedTime { get; set; }

            #endregion

            #region Data Access Methods

            /// <summary>
            /// Deletes the current session instance from the database by using the current ID as the primary key.
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
                            Session temp;
                            if (!_sessionsCache.TryRemove(this.ID, out temp))
                                throw new Exception("The cache does not contain this session and so wasn't able to delete it.");
                        }
                    }
                }
                catch
                {
                    throw;
                }
            }
            #endregion

            #region Overrides

            /// <summary>
            /// Compares equality between two sessions.  They are equal if their IDs are equal.  This is necessary for the TryTake method of the cache.
            /// </summary>
            /// <param name="obj"></param>
            /// <returns></returns>
            public override bool Equals(object obj)
            {
                if (obj == null)
                    return false;

                Session other = obj as Session;
                if (other == null)
                    return false;

                if (other.ID.Equals(this.ID))
                    return true;
                else
                    return false;
            }

            /// <summary>
            /// Returns the hash code of the ID.
            /// </summary>
            /// <returns></returns>
            public override int GetHashCode()
            {
                return this.ID.GetHashCode();
            }

            /// <summary>
            /// Returns "ID | LoginTime | IsActive"
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return string.Format("{0} | {1} | {2}", this.ID, this.LoginTime, this.IsActive);
            }

            #endregion
        }

        #region Static Data Access Methods

        /// <summary>
        /// Loads all Sessions from the database.  Optionally, this can also flush the entire sessions cache and reset it with these results.
        /// </summary>
        /// <returns></returns>
        public static async Task<List<Session>> DBLoadAll(bool updateCache)
        {
            try
            {
                List<Session> result = new List<Session>();
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
                                result.Add(new Session()
                                {
                                    ID = reader["ID"].ToString(),
                                    IsActive = reader.GetBoolean("IsActive"),
                                    LoginTime = reader.GetDateTime("LoginTime"),
                                    LogoutTime = reader.GetDateTime("LogoutTime"),
                                    PersonID = reader["PersonID"].ToString(),
                                    PermissionIDs = reader["PermissionIDs"].ToString().Deserialize<List<string>>(),
                                    LastUsedTime = reader.GetDateTime("LastUsedTime")
                                });
                            }
                        }
                    }
                }

                if (updateCache)
                {
                    _sessionsCache = new ConcurrentDictionary<string, Session>(result.Select(x => new KeyValuePair<string, Session>(x.ID, x)));
                }

                return result;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Loads a single session from the database.  Optionally, if a record is found from the database, the cache will be updated with this record by either updating or adding it.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="updateCache"></param>
        /// <returns></returns>
        public static async Task<Session> DBLoadOne(string id, bool updateCache)
        {
            try
            {
                List<Session> result = new List<Session>();
                using (MySqlConnection connection = new MySqlConnection(Settings.ConnectionString))
                {
                    await connection.OpenAsync();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = string.Format("SELECT * FROM `{0}` WHERE `ID` = @ID", _tableName);

                    command.Parameters.AddWithValue("@ID", id);

                    using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            while (await reader.ReadAsync())
                            {
                                result.Add(new Session()
                                {
                                    ID = reader["ID"].ToString(),
                                    IsActive = reader.GetBoolean("IsActive"),
                                    LoginTime = reader.GetDateTime("LoginTime"),
                                    LogoutTime = reader.GetDateTime("LogoutTime"),
                                    PersonID = reader["PersonID"].ToString(),
                                    PermissionIDs = reader["PermissionIDs"].ToString().Deserialize<List<string>>(),
                                    LastUsedTime = reader.GetDateTime("LastUsedTime")
                                });
                            }
                        }
                    }
                }

                if (result.Count > 1)
                    throw new Exception("Somehow more than one session has the same ID.  That's not good.");

                Session finalResult = result.FirstOrDefault();

                if (updateCache && finalResult != null)
                {
                    _sessionsCache.AddOrUpdate(finalResult.ID, finalResult, (key, value) => finalResult);
                }

                return finalResult;

            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Releases the sessions cache by clearing it of all data.
        /// </summary>
        public static void ReleaseCache()
        {
            try
            {
                _sessionsCache.Clear();
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region Other Methods

        /// <summary>
        /// Returns the session that is associated with the authentication token or null if there is none.
        /// </summary>
        /// <param name="authenticationToken"></param>
        /// <returns></returns>
        public static Session GetSession(string authenticationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(authenticationToken))
                    return null;

                Session session;
                _sessionsCache.TryGetValue(authenticationToken, out session);
                
                return session;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Returns a boolean indicating whether or not the has become inactive.  Uses the private variable in this static class to determine that time out age.
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        public static bool IsSessionInactive(Session session)
        {
            try
            {
                if (DateTime.Now.Subtract(session.LastUsedTime) > _maxInactivityTime)
                    return true;

                return false;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Creates a new session with all default values except for person id and permission ids and return the session's new ID.
        /// <para />
        /// Inserts the new session into the cache and into the database.
        /// </summary>
        /// <param name="personID"></param>
        /// <param name="permissionIDs"></param>
        /// <returns></returns>
        public static async Task<string> CreateNewSession(string personID, List<string> permissionIDs)
        {
            try
            {
                //Create the session
                Session session = new Session()
                {
                    ID = Guid.NewGuid().ToString(),
                    IsActive = true,
                    LoginTime = DateTime.Now,
                    PermissionIDs = permissionIDs,
                    PersonID = personID,
                    LastUsedTime = DateTime.Now
                };

                //Add the session to the cache.
                _sessionsCache.AddOrUpdate(session.ID, session, (key, value) =>
                {
                    throw new Exception("An error occurred while trying to add the session to the cache.  Login failed.");
                });

                //Now let's add it to the database.
                using (MySqlConnection connection = new MySqlConnection(Settings.ConnectionString))
                {
                    await connection.OpenAsync();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = string.Format("INSERT INTO `{0}` (`ID`,`IsActive`,`LoginTime`,`LogoutTime`,`PersonID`, `PermissionIDs`, `LastUsedTime`) VALUES (@ID, @IsActive, @LoginTime, @LogoutTime, @PersonID, @PermissionIDs, @LastUsedTime)", _tableName);

                    command.Parameters.AddWithValue("@ID", session.ID);
                    command.Parameters.AddWithValue("@IsActive", session.IsActive);
                    command.Parameters.AddWithValue("@LoginTime", session.LoginTime.ToMySqlDateTimeString());
                    command.Parameters.AddWithValue("@LogoutTime", session.LogoutTime.ToMySqlDateTimeString());
                    command.Parameters.AddWithValue("@PersonID", session.PersonID);
                    command.Parameters.AddWithValue("@PermissionIDs", session.PermissionIDs.Serialize());
                    command.Parameters.AddWithValue("@LastUsedTime", session.LastUsedTime.ToMySqlDateTimeString());

                    //If the affected rows don't equal 1, then somehow something went wrong during the insert.  We should alert the devs.
                    if (await command.ExecuteNonQueryAsync() != 1)
                        throw new Exception("An error occurred during login that resulted in the session not being added or more than one row being added; however, it was added to the cache.  Recommend restarting the sessions provider.");
                }

                return session.ID;

            }
            catch (Exception)
            {
                
                throw;
            }
        }

        //TODO: Make the below method use transactions and rollback if there's an error.
        /// <summary>
        /// Invalidates a session by removing it from the cache and then updating it in the database.  If at any point something goes wrong, we throw all the errors.
        /// </summary>
        /// <param name="sessionID"></param>
        /// <returns></returns>
        public static async Task InvalidateSession(string sessionID)
        {
            try
            {
                //Make sure the session is actually in the cache.  If it's not, somehow we got out of sync and we should alert the devs.
                if (!_sessionsCache.Keys.Contains(sessionID))
                    throw new Exception(string.Format("During invalidate session, we attempted to remove a session from the cache; however, it wasn't found. Session ID: {0}", sessionID));

                //Ok, since it's in the cache, let's remove it.
                Sessions.Session session;
                if (!_sessionsCache.TryRemove(sessionID, out session))
                    throw new Exception(String.Format("Some issue caused the session not to be removed from the cache; however, we know the session id is in the cache.  Session ID: {0}", sessionID));

                //Now that it's out of the cache, we need to invalidate it in the database.
                using (MySqlConnection connection = new MySqlConnection(Settings.ConnectionString))
                {
                    await connection.OpenAsync();

                    using (MySqlTransaction transaction = await connection.BeginTransactionAsync())
                    {
                        try
                        {

                            using (MySqlCommand command = new MySqlCommand("", connection, transaction))
                            {
                                command.CommandText = string.Format("UPDATE `{0}` SET `IsActive` = @IsActive, `LogoutTime` = @LogoutTime WHERE `ID` = @ID", "sessions");

                                command.Parameters.AddWithValue("@IsActive", false);
                                command.Parameters.AddWithValue("@LogoutTime", DateTime.Now.ToMySqlDateTimeString());
                                command.Parameters.AddWithValue("@ID", sessionID);

                                int rowsAffected = await command.ExecuteNonQueryAsync();

                                if (rowsAffected != 1)
                                {
                                    if (rowsAffected == 0)
                                        throw new Exception(string.Format("We successfully removed a session from the cache but we were unable to remove it from the database.  Session ID: {0}", sessionID));

                                    if (rowsAffected > 1)
                                        throw new Exception(string.Format("During invalidate session, we somehow updated multiple sessions for one session key. Session ID: {0}", sessionID));
                                }
                            }

                            transaction.Commit();
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                    
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Updates the Last Used Time to DateTime.Now for the given session ID and throws an error if that session ID does not correspond to an actual session.
        /// <para /> 
        /// Also updates the cache optionally.
        /// </summary>
        /// <param name="sessionID"></param>
        /// <returns></returns>
        public static async Task RefreshSession(string sessionID, bool updateCache)
        {
            try
            {

                var now = DateTime.Now;

                using (MySqlConnection connection = new MySqlConnection(Settings.ConnectionString))
                {
                    await connection.OpenAsync();

                    using (MySqlTransaction transaction = await connection.BeginTransactionAsync())
                    {
                        try
                        {
                            using (MySqlCommand command = new MySqlCommand("", connection, transaction))
                            {
                                command.CommandText = string.Format("UPDATE `{0}` SET `LastUsedTime` = @LastUsedTime WHERE `ID` = @ID", _tableName);

                                command.Parameters.AddWithValue("@LastUsedTIme", now.ToMySqlDateTimeString());
                                command.Parameters.AddWithValue("@ID", sessionID);

                                int rowsAffected = await command.ExecuteNonQueryAsync();

                                if (rowsAffected != 1)
                                    throw new Exception(string.Format("While attempting to refresh the session whose id was '{0}', '{1}' sessions were updated!", sessionID, rowsAffected));
                            }

                            transaction.Commit();
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }

                if (updateCache)
                {
                    if (!_sessionsCache.ContainsKey(sessionID))
                        throw new Exception(string.Format("While attempting to refresh the session whose ID was '{0}', no session was found in the cache.", sessionID));
                    _sessionsCache[sessionID].LastUsedTime = now;
                }
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region Cron Operations

        /// <summary>
        /// Scrubs the sessions table, removing old sessions from the database.
        /// </summary>
        public static void ScrubSessions()
        {
            try
            {
                //We're going to load all of the sessions from the database and then delete those that were last used longer ago than the max db persistance time permits.
                var sessions = Sessions.DBLoadAll(false).Result;

                Parallel.ForEach<Sessions.Session>(sessions, (session) =>
                {
                    if (DateTime.Now.Subtract(session.LastUsedTime) > _maxDBPersistanceTime)
                    {
                        if (_sessionsCache.ContainsKey(session.ID))
                            session.DBDelete(true).Wait();
                        else
                            session.DBDelete(false).Wait();
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
