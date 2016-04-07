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
    /// Contains members for dealing with a change event subscription, which is a modeling of the change event subscriptions table which relates change events to a user.
    /// </summary>
    public static class ChangeEventSubscriptions
    {

        /// <summary>
        /// This local, readonly property is intended to standardize all methods in this class that access the database and allow easy maintenance.
        /// </summary>
        private static readonly string _tableName = "changeeventsubscriptions";

        /// <summary>
        /// Describes a single change event.
        /// </summary>
        public class ChangeEventSubscription
        {

            #region Properties

            private string _id = null;
            /// <summary>
            /// The ID of the change event subscription.
            /// </summary>
            public string ID
            {
                get
                {
                    return _id;
                }
                set
                {
                    _id = value;
                }
            }

            private string _ownerID = null;
            /// <summary>
            /// The ID of the person to which this change event subscription belongs.
            /// </summary>
            public string OwnerID
            {
                get
                {
                    return _ownerID;
                }
                set
                {
                    _ownerID = value;
                }
            }

            private string _changeEventID = null;
            /// <summary>
            /// The ID of the change event.
            /// </summary>
            public string ChangeEventID
            {
                get
                {
                    return _changeEventID;
                }
                set
                {
                    _changeEventID = value;
                }
            }

            #endregion

            #region Data Access Methods

            /// <summary>
            /// Inserts the change event subscription, effectively subscribing the person to the change event.
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
                        command.CommandText = string.Format("INSERT INTO `{0}` (`ID`, `OwnerID`, `ChangeEventID`)", _tableName) +
                            " VALUES (@ID, @OwnerID, @ChangeEventID)";

                        command.Parameters.AddWithValue("@ID", this.ID);
                        command.Parameters.AddWithValue("@OwnerID", this.OwnerID);
                        command.Parameters.AddWithValue("@ChangeEventID", this.ChangeEventID);

                        await command.ExecuteNonQueryAsync();
                    }
                }
                catch
                {
                    throw;
                }
            }

            /// <summary>
            /// Inserts the change event subscription, effectively subscribing the person to the change event.
            /// <para />
            /// This version uses the transaction for DB Interaction.
            /// </summary>
            /// <returns></returns>
            public async Task DBInsert(MySqlTransaction transaction)
            {
                try
                {

                    using (MySqlCommand command = new MySqlCommand("", transaction.Connection, transaction))
                    {
                        command.CommandText = string.Format("INSERT INTO `{0}` (`ID`, `OwnerID`, `ChangeEventID`)", _tableName) +
                            " VALUES (@ID, @OwnerID, @ChangeEventID)";

                        command.Parameters.AddWithValue("@ID", this.ID);
                        command.Parameters.AddWithValue("@OwnerID", this.OwnerID);
                        command.Parameters.AddWithValue("@ChangeEventID", this.ChangeEventID);

                        await command.ExecuteNonQueryAsync();
                    }

                }
                catch
                {
                    throw;
                }
            }

            /// <summary>
            /// Deletes the current change event subscription, effectively unsubscribing the person from the event. Uses the ID field to do this deletion.
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
            /// Deletes the current change event subscription, effectively unsubscribing the person from the event. Uses the ID field to do this deletion.
            /// <para />
            /// This version uses the transaction as the DB interaction object.
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
            /// Returns a boolean indicating whether or not the current change event subscription exists in the database.  Uses the ID to do this comparison.
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

            #region Overloads

            public override bool Equals(object obj)
            {
                return false;
            }

            public override int GetHashCode()
            {
                return this.ChangeEventID.GetHashCode() ^ this.ID.GetHashCode() ^ this.OwnerID.GetHashCode();
            }

            #endregion

        }

        #region Static Data Access Methods

        /// <summary>
        /// Loads all change event subscriptions from the database.
        /// </summary>
        /// <returns></returns>
        public static async Task<List<ChangeEventSubscription>> DBLoadAll()
        {
            try
            {
                List<ChangeEventSubscription> result = new List<ChangeEventSubscription>();
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
                                result.Add(new ChangeEventSubscription()
                                {
                                    ChangeEventID = reader["ChangeEventID"] as string,
                                    ID = reader["ID"] as string,
                                    OwnerID = reader["OwnerID"] as string
                                });
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
        /// Loads all change event subscriptions for a given person.  This uses that person's id.
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        public static async Task<List<ChangeEventSubscription>> DBLoadAllByPerson(Persons.Person person)
        {
            try
            {
                List<ChangeEventSubscription> result = new List<ChangeEventSubscription>();
                using (MySqlConnection connection = new MySqlConnection(Properties.ConnectionString))
                {
                    await connection.OpenAsync();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = string.Format("SELECT * FROM `{0}` WHERE `OwnerID` = @OwnerID", _tableName);

                    command.Parameters.AddWithValue("@OwnerID", person.ID);

                    using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            while (await reader.ReadAsync())
                            {
                                result.Add(new ChangeEventSubscription()
                                {
                                    ChangeEventID = reader["ChangeEventID"] as string,
                                    ID = reader["ID"] as string,
                                    OwnerID = reader["OwnerID"] as string
                                });
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
        /// Loads all change event subscriptions for a given person id.
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        public static async Task<List<ChangeEventSubscription>> DBLoadAllByPerson(string personID)
        {
            try
            {
                List<ChangeEventSubscription> result = new List<ChangeEventSubscription>();
                using (MySqlConnection connection = new MySqlConnection(Properties.ConnectionString))
                {
                    await connection.OpenAsync();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = string.Format("SELECT * FROM `{0}` WHERE `OwnerID` = @OwnerID", _tableName);

                    command.Parameters.AddWithValue("@OwnerID", personID);

                    using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            while (await reader.ReadAsync())
                            {
                                result.Add(new ChangeEventSubscription()
                                {
                                    ChangeEventID = reader["ChangeEventID"] as string,
                                    ID = reader["ID"] as string,
                                    OwnerID = reader["OwnerID"] as string
                                });
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
        /// Loads all change event subscriptions for a given change event id.
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        public static async Task<List<ChangeEventSubscription>> DBLoadAllByEvent(string changeEventID)
        {
            try
            {
                List<ChangeEventSubscription> result = new List<ChangeEventSubscription>();
                using (MySqlConnection connection = new MySqlConnection(Properties.ConnectionString))
                {
                    await connection.OpenAsync();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = string.Format("SELECT * FROM `{0}` WHERE `ChangeEventID` = @OwnerChangeEventIDID", _tableName);

                    command.Parameters.AddWithValue("@ChangeEventID", changeEventID);

                    using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            while (await reader.ReadAsync())
                            {
                                result.Add(new ChangeEventSubscription()
                                {
                                    ChangeEventID = reader["ChangeEventID"] as string,
                                    ID = reader["ID"] as string,
                                    OwnerID = reader["OwnerID"] as string
                                });
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

        


    }
}
