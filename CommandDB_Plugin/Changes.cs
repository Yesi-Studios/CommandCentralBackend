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
                        command.Parameters.AddWithValue("@OldValue", (this.Variance.OldValue == null) ? null : this.Variance.OldValue.Serialize());
                        command.Parameters.AddWithValue("@NewValue", (this.Variance.NewValue == null) ? null : this.Variance.NewValue.Serialize());
                        command.Parameters.AddWithValue("@Time", this.Time.ToMySqlDateTimeString());
                        command.Parameters.AddWithValue("@Remarks", this.Remarks);
                        
                        await command.ExecuteNonQueryAsync();
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

        /// <summary>
        /// Loads all changes for a given object ID.  
        /// </summary>
        /// <param name="objectName"></param>
        /// <param name="objectID"></param>
        /// <returns></returns>
        public static async Task<List<Change>> DBLoadAllByObject(string objectID)
        {
            try
            {
                List<Change> results = new List<Change>();
                using (MySqlConnection connection = new MySqlConnection(Properties.ConnectionString))
                {
                    await connection.OpenAsync();

                    using (MySqlCommand command = new MySqlCommand("", connection))
                    {
                        command.CommandText = string.Format("SELECT * FROM `{0}` WHERE `ObjectID` = @ObjectID", _tableName);

                        command.Parameters.AddWithValue("@ObjectID", objectID);

                        using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                        {
                            if (reader.HasRows)
                            {
                                while (await reader.ReadAsync())
                                {
                                    results.Add(new Change()
                                    {
                                        EditorID = reader["EditorID"] as string,
                                        ID = reader["ID"] as string,
                                        ObjectID = reader["ObjectID"] as string,
                                        ObjectName = reader["ObjectName"] as string,
                                        Remarks = reader["Remarks"] as string,
                                        Time = DateTime.Parse(reader["Time"] as string),
                                        Variance = new Variance()
                                        {
                                            NewValue = (reader["NewValue"] as string).DeserializeToJObject(),
                                            OldValue = (reader["OldValue"] as string).DeserializeToJObject(),
                                            PropertyName = reader["PropertyName"] as string
                                        }
                                    });
                                }
                            }
                        }
                    }
                }
                return results;
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
        /// Loads all changes that have been made to a given object.  If the object matches a model, then non-returnable fields will have their values redacted.
        /// <para />
        /// Options: 
        /// <para />
        /// objectid : the ID of the object for which to load changes.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<MessageTokens.MessageToken> LoadChanges_Client(MessageTokens.MessageToken token)
        {
            try
            {
                //First we need the id of the person for whom we want the changes.
                if (!token.Args.ContainsKey("objectid"))
                    throw new ServiceException("In order to load the changes belonging to an object, you must send an object's id.  Duh?", ErrorTypes.Validation);
                string objectID = token.Args["objectid"] as string;

                //Let's do some basic validaiton on this shit.
                if (!ValidationMethods.IsValidGuid(objectID))
                    throw new ServiceException("Well, you managed to send an object id... but it doens't even look like a GUID should.  Bummer.  NO CHANGES FOR YOU.", ErrorTypes.Validation);

                //We'll use this variable to catch our changes.
                List<Change> changes = await Changes.DBLoadAllByObject(objectID);

                //If we got any changes we need to check if we should drop field values.
                if (changes.Any())
                {
                    //Ok we have all the changes, now we just need to make sure the requesting client is actually allowed to view all the changes.
                    //To do this, we need to know what object these changes came from.  Let's just make sure that all the object names are the same, for reasons.
                    if (changes.Select(x => x.ObjectName).Distinct().Count() != 1)
                        throw new Exception(string.Format("While loading changes for the object whose ID is '{0}', that object had multiple object names.", objectID));

                    //Since we know they're all the same, let's just pull one's object name.
                    string objectName = changes.First().ObjectName;

                    //Now get the client's model permissions
                    var modelPermissions = UnifiedServiceFramework.Authorization.Permissions.GetModelPermissionsForUser(token, objectName);

                    //If model permissions comes back null, it's because the object name doesn't match a model.  
                    //That's ok, in this case, we just give the client all the changes and we don't implement permissions on it.

                    if (modelPermissions != null)
                    {
                        changes.ForEach(x =>
                        {
                            //If the client doesn't have permission to return this field, then set the values to REDACTED.  Like they some classified shit.
                            if (!modelPermissions.ReturnableFields.Contains(x.Variance.PropertyName, StringComparer.CurrentCultureIgnoreCase))
                            {
                                x.Variance.NewValue = "REDACTED";
                                x.Variance.OldValue = "REDACTED";
                            }
                        });
                    }
                }
                else
                {
                    //If we got here, it means there are no changes, so just return it.
                    token.Result = changes;
                }

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
