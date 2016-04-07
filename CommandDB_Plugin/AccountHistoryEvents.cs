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
    /// Describes the different account history types.
    /// </summary>
    public enum AccountHistoryEventTypes
    {
        Login,
        Failed_Login,
        Password_Reset,
        Registration_Started,
        Registration_Completed,
        Password_Reset_Initiated,
        Password_Reset_Completed
    }

    /// <summary>
    /// Provides members and methods for tracking account history events.  These are events such as logins, password resets, failed logins, and other account history events that could indicate a compromise of an account.
    /// </summary>
    public static class AccountHistoryEvents
    {

        /// <summary>
        /// This local, readonly property is intended to standardize all methods in this class that access the database and allow easy maintenance.
        /// </summary>
        private static readonly string _tableName = "accounthistoryevents";

        /// <summary>
        /// Describes a single account history event and its data access methods.
        /// </summary>
        public class AccountHistoryEvent
        {

            #region Properties

            /// <summary>
            /// The unique GUID of this account history event.
            /// </summary>
            public string ID { get; set; }

            /// <summary>
            /// The unique GUID of the person on whose account this event occurred.
            /// </summary>
            public string PersonID { get; set; }

            /// <summary>
            /// The type of history event that occurred.
            /// </summary>
            public AccountHistoryEventTypes AccountHistoryEventType { get; set; }

            /// <summary>
            /// The time at which this event occurred.
            /// </summary>
            public DateTime EventTime { get; set; }

            #endregion

            #region Data Access Methods

            /// <summary>
            /// Inserts the account history event into the database.
            /// </summary>
            /// <returns></returns>
            public async Task DBInsert()
            {
                try
                {
                    using (MySqlConnection connection = new MySqlConnection(Properties.ConnectionString))
                    {
                        await connection.OpenAsync();

                        MySqlCommand command = connection.CreateCommand();
                        command.CommandType = CommandType.Text;
                        command.CommandText = string.Format("INSERT INTO `{0}` (`ID`,`PersonID`,`AccountHistoryEventType`,`EventTime`)", _tableName) +
                            " VALUES (@ID, @PersonID, @AccountHistoryEventType, @EventTime)";

                        command.Parameters.AddWithValue("@ID", this.ID);
                        command.Parameters.AddWithValue("@PersonID", this.PersonID);
                        command.Parameters.AddWithValue("@AccountHistoryEventType", this.AccountHistoryEventType.Serialize());
                        command.Parameters.AddWithValue("@EventTime", this.EventTime.ToMySqlDateTimeString());

                        await command.ExecuteNonQueryAsync();

                    }
                }
                catch
                {
                    throw;
                }
            }

            /// <summary>
            /// Inserts the account history event into the database.
            /// <para />
            /// This version uses the transaction that was made by someone else.
            /// </summary>
            /// <returns></returns>
            public async Task DBInsert(MySqlTransaction transaction)
            {
                try
                {
                    using (MySqlCommand command = new MySqlCommand("", transaction.Connection, transaction))
                    {
                        command.CommandText = string.Format("INSERT INTO `{0}` (`ID`,`PersonID`,`AccountHistoryEventType`,`EventTime`)", _tableName) +
                            " VALUES (@ID, @PersonID, @AccountHistoryEventType, @EventTime)";

                        command.Parameters.AddWithValue("@ID", this.ID);
                        command.Parameters.AddWithValue("@PersonID", this.PersonID);
                        command.Parameters.AddWithValue("@AccountHistoryEventType", this.AccountHistoryEventType.Serialize());
                        command.Parameters.AddWithValue("@EventTime", this.EventTime.ToMySqlDateTimeString());

                        await command.ExecuteNonQueryAsync();
                    }
                }
                catch
                {
                    throw;
                }
            }

            /// <summary>
            /// Updates the current account history event instance by resetting all columns to the current instance and uses the ID to index.  The ID itself cannot be updated.
            /// </summary>
            /// <returns></returns>
            public async Task DBUpdate()
            {
                try
                {
                    using (MySqlConnection connection = new MySqlConnection(Properties.ConnectionString))
                    {
                        await connection.OpenAsync();

                        MySqlCommand command = connection.CreateCommand();
                        command.CommandType = CommandType.Text;
                        command.CommandText = string.Format("UPDATE `{0}` SET `PersonID` = @PersonID, `AccountHistoryEventType` = @AccountHistoryEventType, `EventTime` = @EventTime WHERE `ID` = @ID ", _tableName);

                        command.Parameters.AddWithValue("@PersonID", this.PersonID);
                        command.Parameters.AddWithValue("@AccountHistoryEventType", this.AccountHistoryEventType.Serialize());
                        command.Parameters.AddWithValue("@EventTime", this.EventTime.ToMySqlDateTimeString());

                        command.Parameters.AddWithValue("@ID", this.ID);

                        await command.ExecuteNonQueryAsync();
                    }
                }
                catch
                {
                    throw;
                }
            }

            /// <summary>
            /// Updates the current account history event instance by resetting all columns to the current instance and uses the ID to index.  The ID itself cannot be updated.
            /// <para />
            /// This version uses a transaction that has already been created to do the update.
            /// </summary>
            /// <returns></returns>
            public async Task DBUpdate(MySqlTransaction transaction)
            {
                try
                {
                    using (MySqlCommand command = new MySqlCommand("", transaction.Connection, transaction))
                    {
                        command.CommandText = string.Format("UPDATE `{0}` SET `PersonID` = @PersonID, `AccountHistoryEventType` = @AccountHistoryEventType, `EventTime` = @EventTime WHERE `ID` = @ID ", _tableName);

                        command.Parameters.AddWithValue("@PersonID", this.PersonID);
                        command.Parameters.AddWithValue("@AccountHistoryEventType", this.AccountHistoryEventType.Serialize());
                        command.Parameters.AddWithValue("@EventTime", this.EventTime.ToMySqlDateTimeString());

                        command.Parameters.AddWithValue("@ID", this.ID);

                        await command.ExecuteNonQueryAsync();
                    }
                }
                catch
                {
                    throw;
                }
            }

            /// <summary>
            /// Deletes the current account history event instance from the database by using the current ID as the primary key.
            /// </summary>
            /// <returns></returns>
            public async Task DBDelete()
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
                    }
                }
                catch
                {
                    throw;
                }
            }

            /// <summary>
            /// Deletes the current account history event instance from the database by using the current ID as the primary key.
            /// <para />
            /// This version uses an already-made transaction.
            /// </summary>
            /// <returns></returns>
            public async Task DBDelete(MySqlTransaction transaction)
            {
                try
                {
                    using (MySqlCommand command = new MySqlCommand("", transaction.Connection, transaction))
                    {
                        command.CommandText = string.Format("DELETE FROM `{0}` WHERE `ID` = @ID", _tableName);

                        command.Parameters.AddWithValue("@ID", this.ID);

                        await command.ExecuteNonQueryAsync();
                    }
                }
                catch
                {
                    throw;
                }
            }

            /// <summary>
            /// Returns a boolean indicating whether or not the current account history event exists in the database.  Uses the ID to do this comparison.
            /// </summary>
            /// <param name="useCache"></param>
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

        #region Static Data Access Methods

        /// <summary>
        /// Loads all account history event objects.
        /// </summary>
        /// <returns></returns>
        public static async Task<List<AccountHistoryEvent>> DBLoadAll()
        {
            try
            {
                List<AccountHistoryEvent> result = new List<AccountHistoryEvent>();
                using (MySqlConnection connection = new MySqlConnection(Properties.ConnectionString))
                {
                    await connection.OpenAsync();

                    using (MySqlCommand command = new MySqlCommand("", connection))
                    {
                        command.CommandText = string.Format("SELECT * FROM `{0}`", _tableName);

                        using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                        {
                            if (reader.HasRows)
                            {
                                while (await reader.ReadAsync())
                                {
                                    result.Add(new AccountHistoryEvent()
                                    {
                                        AccountHistoryEventType = (reader["AccountHistoryEventType"] as string).Deserialize<AccountHistoryEventTypes>(),
                                        EventTime = reader.GetDateTime("EventTime"),
                                        ID = reader["ID"] as string,
                                        PersonID = reader["PersonID"] as string
                                    });
                                }
                            }
                        }
                    }
                }
                return result;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Loads all account history event objects for a given person, using a given limit.
        /// </summary>
        /// <param name="personID"></param>
        /// <param name="limit">If the limit is null, all records are returned.  If the limit is less than or equal to zero, an error is thrown., </param>
        /// <returns></returns>
        public static async Task<List<AccountHistoryEvent>> DBLoadByPerson(string personID, int? limit)
        {
            try
            {
                //Validate the limit
                if (limit.HasValue && limit.Value <= 0)
                    throw new ServiceException(string.Format("The value, '{0}', was not valid for the limit when loading account history for a person.  It must be greater than zero.", limit.Value), ErrorTypes.Validation);

                List<AccountHistoryEvent> result = new List<AccountHistoryEvent>();
                using (MySqlConnection connection = new MySqlConnection(Properties.ConnectionString))
                {
                    await connection.OpenAsync();

                    using (MySqlCommand command = new MySqlCommand("", connection))
                    {
                        if (limit.HasValue)
                            //I'm going to order by EventTime so that the limit gives us the most recent events.
                            command.CommandText = string.Format("SELECT * FROM `{0}` WHERE `PersonID` = @PersonID ORDER BY `EventTime` DESC LIMIT {1}", _tableName, limit);
                        else
                            command.CommandText = string.Format("SELECT * FROM `{0}` WHERE `PersonID` = @PersonID", _tableName);

                        command.Parameters.AddWithValue("@PersonID", personID);

                        using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                        {
                            if (reader.HasRows)
                            {
                                while (await reader.ReadAsync())
                                {
                                    result.Add(new AccountHistoryEvent()
                                    {
                                        AccountHistoryEventType = (reader["AccountHistoryEventType"] as string).Deserialize<AccountHistoryEventTypes>(),
                                        EventTime = reader.GetDateTime("EventTime"),
                                        ID = reader["ID"] as string,
                                        PersonID = reader["PersonID"] as string
                                    });
                                }
                            }
                        }
                    }
                }
                return result;
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
        /// Loads the account history for a given person and limits the results by the given limit.  Results are ordered by EventTime, so the limit will return the most recent 'x' results.
        /// <para />
        /// Options: 
        /// <para />
        /// personid - The person for whom the client wants to load account history events.
        /// limit - The first 'x' results the client wants to return.  Optional.  If omitted, all account histories are returned.  If the value is less than or equal to zero, an error is thrown.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<MessageTokens.MessageToken> LoadAccountHistoryByPerson_Client(MessageTokens.MessageToken token)
        {
            try
            {
                //First, let's get the person ID that we're loading for.
                if (!token.Args.ContainsKey("personid"))
                    throw new ServiceException("You must send a personid in order to indicate for whom you want to load account history.", ErrorTypes.Validation);
                string personID = token.Args["personid"] as string;

                //Is the person ID legit in format?
                if (!ValidationMethods.IsValidGuid(personID))
                    throw new ServiceException(string.Format("The value, '{0}', was not a valid GUID for a person ID.", personID), ErrorTypes.Validation);

                //And now, get the limit
                int? limit = null;
                if (token.Args.ContainsKey("limit"))
                    limit = Convert.ToInt32(token.Args["limit"]);

                //Now we need the client's permissions.  We need to make sure the client can load account histories.
                var modelPermission = UnifiedServiceFramework.Authorization.Permissions.GetModelPermissionsForUser(token, "Person");

                //And then ask if they do or don't.
                if (!modelPermission.ReturnableFields.Contains("AccountHistory"))
                    throw new ServiceException("You do not have permission to request a person's account history.", ErrorTypes.Authorization);

                //We should be good to go now.  Let's load those account histories and return them.
                token.Result = await AccountHistoryEvents.DBLoadByPerson(personID, limit);

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
