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
    /// Describes members for dealing with a Change.  This should not be confused with the ChangeEvents class or the Change log from the main data class.
    /// <para />
    /// Because of the volume of changes likely to occur, we do not implement a cache for changes.  All calls occur directly against the database.
    /// <para />
    /// Changes are stored in their own table and are related through a common ID to the client they belong to.  Changes do not remember their own previous value, only the new value that was set on the user's profile.  
    /// Instead, when the changes are returned to the client, we just calculate the changes be using a change's new value to determine the next change's old value.  Old values are not inserted into the database.
    /// </summary>
    public static class Changes
    {
        /// <summary>
        /// This local, readonly property is intended to standardize all methods in this class that access the database and allow easy maintenance.
        /// </summary>
        private static readonly string _tableName = "changes";

        /// <summary>
        /// Describes a single change and provices methods for interacting with the database.
        /// </summary>
        public class Change
        {

            #region Properties

            /// <summary>
            /// The ID of this change.
            /// </summary>
            public string ID { get; set; }

            /// <summary>
            /// The ID of the client who initiated this change.
            /// </summary>
            public string EditorID { get; set; }

            /// <summary>
            /// The name of the object that was changed.
            /// </summary>
            public string ObjectName { get; set; }

            /// <summary>
            /// The ID of the object that was changed.
            /// </summary>
            public string ObjectID { get; set; }

            /// <summary>
            /// The variance that describes the change.
            /// </summary>
            public Variance Variance { get; set; }

            /// <summary>
            /// The time this change was made.
            /// </summary>
            public DateTime Time { get; set; }

            /// <summary>
            /// A free text field describing this change.
            /// </summary>
            public string Remarks { get; set; }

            #endregion

            #region Overrides

            /// <summary>
            /// Returns the Variance's .ToString() method.
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return Variance.ToString();
            }

            #endregion

            #region Data Access Methods

            /// <summary>
            /// Very simply, this method inserts a new change.
            /// </summary>
            /// <returns></returns>
            public async Task DBInsert()
            {
                try
                {
                    using (MySqlConnection connection = new MySqlConnection(Settings.ConnectionString))
                    {
                        await connection.OpenAsync();

                        MySqlCommand command = connection.CreateCommand();
                        command.CommandType = CommandType.Text;
                        command.CommandText = string.Format("INSERT INTO `{0}` (`ID`,`EditorID`,`ObjectName`,`ObjectID`,`PropertyName`,`OldValue`,`NewValue`,`Time`,`Remarks`) ", _tableName) +
                            "VALUES (@ID, @EditorID, @ObjectName, @ObjectID, @PropertyName, @OldValue, @NewValue, @Time, @Remarks)";

                        command.Parameters.AddWithValue("@ID", this.ID);
                        command.Parameters.AddWithValue("@EditorID", this.EditorID);
                        command.Parameters.AddWithValue("@ObjectName", this.ObjectName);
                        command.Parameters.AddWithValue("@ObjectID", this.ObjectID);
                        command.Parameters.AddWithValue("@PropertyName", this.Variance.PropertyName);
                        command.Parameters.AddWithValue("@OldValue", this.Variance.OldValue as string);
                        command.Parameters.AddWithValue("@NewValue", this.Variance.NewValue as string);
                        command.Parameters.AddWithValue("@Time", this.Time.ToMySqlDateTimeString());
                        command.Parameters.AddWithValue("@Time", this.Remarks);
                        
                        await command.ExecuteNonQueryAsync();
                    }
                }
                catch
                {
                    throw;
                }
            }

            /// <summary>
            /// Updates the current change instance by resetting all columns to the current instance and uses the ID to index.  The ID itself cannot be updated.
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
                        command.CommandText = string.Format("UPDATE `{0}` SET `EditorID` = @EditorID, `ObjectName` = @ObjectName, `ObjectID` = @ObjectID, ", _tableName) + //This thing got stupid long fast :(
                            "`PropertyName` = @PropertyName, `OldValue` = @OldValue, `NewValue` = @NewValue, `Time` = @Time, `Remarks` = @Remarks WHERE `ID` = @ID";

                        command.Parameters.AddWithValue("@EditorID", this.EditorID);
                        command.Parameters.AddWithValue("@ObjectName", this.ObjectName);
                        command.Parameters.AddWithValue("@ObjectID", this.ObjectID);
                        command.Parameters.AddWithValue("@PropertyName", this.Variance.PropertyName);
                        command.Parameters.AddWithValue("@OldValue", this.Variance.OldValue as string);
                        command.Parameters.AddWithValue("@NewValue", this.Variance.NewValue as string);
                        command.Parameters.AddWithValue("@Time", this.Time.ToMySqlDateTimeString());
                        command.Parameters.AddWithValue("@Remarks", this.Remarks);
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
            /// Deletes the current change instance from the database by using the current ID as the primary key.
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
            /// Returns a boolean indicating whether or not the current change instance exists in the database.  Uses the ID to do this comparison.
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
        /// Inserts all changes, launching each change's DBInsert method on a separate thread.  When all threads are completed, the aggregate exceptions are analyzed and handled if any are found.
        /// <para />
        /// This method does not block the thread but also does not rejoin the synchonization context.
        /// </summary>
        /// <param name="changes"></param>
        public static void DBInsertAll(List<Change> changes)
        {
            Task.Factory.StartNew(() =>
                {
                    try
                    {
                        ConcurrentQueue<Exception> exceptions = new ConcurrentQueue<Exception>();

                        Parallel.ForEach(changes, async change =>
                        {
                            try
                            {
                                await change.DBInsert();
                            }
                            catch (Exception e)
                            {
                                exceptions.Enqueue(e);
                            }
                        });

                        if (exceptions.Count > 0)
                            throw new AggregateException(exceptions).Flatten();
                    }
                    catch (Exception e)
                    {
                        //TODO Write exception handling code.  Remember we need to log it, send an email, and post it.
                    }
                });
        }

        #endregion

        #region Client Access Methods

        /// <summary>
        /// WARNING!  THIS IS A CLIENT METHOD.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Loads all changes that have been made to a user profile and redacts information that the user is not allowed to see.
        /// <para />
        /// Options: 
        /// <para />
        /// personid : The person whose changes the client wants to load.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<MessageTokens.MessageToken> LoadChangesByPersonAsync(MessageTokens.MessageToken token)
        {
            try
            {
                //First we need the id of the person for whom we want the changes.
                if (!token.Args.ContainsKey("personid"))
                    throw new ServiceException("In order to load the changes belonging to a person, you must send a person's id.  Duh?", ErrorTypes.Validation);
                string personID = token.Args["personid"] as string;

                //Let's do some basic validaiton on this shit.
                if (!ValidationMethods.IsValidGuid(personID))
                    throw new ServiceException("Welp, you managed to send a person id... but it doens't even look like a person ID should.  Bummer.  NO CHANGES FOR YOU.", ErrorTypes.Validation);

                //We'll use this variable to catch our changes.
                List<Change> changes = new List<Change>();

                //Now let's go do our load.
                using (MySqlConnection connection = new MySqlConnection(Properties.ConnectionString))
                {
                    await connection.OpenAsync();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = string.Format("SELECT * FROM `{0}` WHERE `PersonID` = @PersonID", _tableName);

                    command.Parameters.AddWithValue("@PersonID", personID);

                    using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            while (await reader.ReadAsync())
                            {
                                changes.Add(new Change()
                                {
                                    EditorID = reader["EditorID"] as string,
                                    ID = reader["ID"] as string,
                                    ObjectID = reader["ObjectID"] as string,
                                    ObjectName = reader["ObjectName"] as string,
                                    Remarks = reader["Remarks"] as string,
                                    Time = DateTime.Parse(reader["Time"] as string),
                                    Variance = new Variance()
                                    {
                                        NewValue = reader["NewValue"] as string,
                                        OldValue = reader["OldValue"] as string,
                                        PropertyName = reader["PropertyName"] as string
                                    }
                                });
                            }
                        }
                    }
                }

                //Ok we have all the changes, now we just need to make sure the requesting client is actually allowed to view all the changes.
                var clientPermissions = UnifiedServiceFramework.Authorization.Permissions.GetModelPermissionsForUser(token, "Person");

                changes.ForEach(x =>
                    {
                        //If the client doesn't have permission to return this field, then set the values to REDACTED.  Like they some classified shit.
                        if (!clientPermissions.ReturnableFields.Contains(x.Variance.PropertyName, StringComparer.CurrentCultureIgnoreCase))
                        {
                            x.Variance.NewValue = "REDACTED";
                            x.Variance.OldValue = "REDACTED";
                        }
                    });

                token.Result = changes;

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
