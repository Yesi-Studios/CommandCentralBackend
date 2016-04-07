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
    /// Provides members designed for assigning a billet or billets to a person.
    /// </summary>
    public static class BilletAssignments
    {

        /// <summary>
        /// This local, readonly property is intended to standardize all methods in this class that access the database and allow easy maintenance.
        /// </summary>
        private static readonly string _tableName = "billetassignments";

        /// <summary>
        /// Describes a single billet assignment.
        /// </summary>
        public class BilletAssignment
        {

            #region Properties

            /// <summary>
            /// The ID of this billet assignment.
            /// </summary>
            public string ID { get; set; }

            /// <summary>
            /// The ID of the person who has been assigned to this billet.
            /// </summary>
            public string PersonID { get; set; }

            /// <summary>
            /// The ID of the billet that is assigned to a given person.
            /// </summary>
            public string BilletID { get; set; }

            #endregion

            #region Data Access Methods

            /// <summary>
            /// Inserts the billet assignement, assigning the person to the billet.
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
                        command.CommandText = string.Format("INSERT INTO `{0}` (`ID`, `PersonID`, `BilletID`)", _tableName) +
                            " VALUES (@ID, @PersonID, @BilletID)";

                        command.Parameters.AddWithValue("@ID", this.ID);
                        command.Parameters.AddWithValue("@PersonID", this.PersonID);
                        command.Parameters.AddWithValue("@BilletID", this.BilletID);

                        await command.ExecuteNonQueryAsync();
                    }
                }
                catch
                {
                    throw;
                }
            }

            /// <summary>
            /// Inserts the billet assignement, assigning the person to the billet.
            /// <para />
            /// Uses an already defined transaction to do this insert.
            /// </summary>
            /// <param name="transaction"></param>
            /// <returns></returns>
            public async Task DBInsert(MySqlTransaction transaction)
            {
                try
                {
                    using (MySqlCommand command = new MySqlCommand("", transaction.Connection, transaction))
                    {
                        command.CommandText = string.Format("INSERT INTO `{0}` (`ID`, `PersonID`, `BilletID`)", _tableName) +
                            " VALUES (@ID, @PersonID, @BilletID)";

                        command.Parameters.AddWithValue("@ID", this.ID);
                        command.Parameters.AddWithValue("@PersonID", this.PersonID);
                        command.Parameters.AddWithValue("@BilletID", this.BilletID);

                        await command.ExecuteNonQueryAsync();
                    }
                }
                catch
                {
                    throw;
                }
            }

            /// <summary>
            /// Deletes the current billet assignment, unassigning the person from the billet. Uses the ID field to do this deletion.
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
            /// Deletes the current billet assignment, unassigning the person from the billet. Uses the ID field to do this deletion.
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
            /// Returns a boolean indicating whether or not the current billet assignment exists in the database.  Uses the ID to do this comparison.
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

        #region Other Methods

        /// <summary>
        /// Validates a billet assignment and returns a list of errors that indicates which properties had issues.
        /// <para />
        /// Returns null if no errors were found.
        /// </summary>
        /// <param name="billetAssignment"></param>
        /// <returns></returns>
        public static async Task<List<string>> ValidateBilletAssignment(BilletAssignment billetAssignment)
        {
            try
            {
                List<string> errors = new List<string>();
                var props = typeof(BilletAssignment).GetProperties().ToList();

                foreach (var prop in props)
                {
                    var error = await BilletAssignments.ValidateProperty(prop.Name, prop.GetValue(billetAssignment));

                    if (error != null)
                        errors.Add(error);
                }

                if (errors.Count == 0)
                    return null;

                return errors;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Validates a property of a billet assignment
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static async Task<string> ValidateProperty(string propertyName, object value)
        {
            try
            {
                switch (propertyName.ToLower())
                {
                    case "id":
                        {
                            if (!ValidationMethods.IsValidGuid(value))
                                return string.Format("The value, '{0}', was not valid for the ID field of a billet assignment; it must be a GUID.", value);

                            break;
                        }
                    case "personid":
                        {
                            if (!(await Persons.DoesPersonIDExist(value as string)))
                                return string.Format("The value, '{0}', was not valid for the Person ID field of a billet assignment; it must be a GUID and belong to an actual person.", value);

                            break;
                        }
                    case "billetid":
                        {
                            if (!await Billets.DoesBilletIDExist(value as string, true))
                                return string.Format("The value, '{0}', was not valid for the Billet ID field of a billet assignment; it must be a GUID and belong to an actual billet.", value);

                            break;
                        }
                    default:
                        {
                            throw new NotImplementedException(string.Format("While performing validaton on a billet assignment, no validation rules were found for the property '{0}'!", propertyName));
                        }
                }

                return null;
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region Static Data Access Methods

        /// <summary>
        /// Loads all billet assignments from the database.
        /// </summary>
        /// <returns></returns>
        public static async Task<List<BilletAssignment>> DBLoadAll()
        {
            try
            {
                List<BilletAssignment> result = new List<BilletAssignment>();
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
                                result.Add(new BilletAssignment()
                                {
                                    BilletID = reader["BilletID"] as string,
                                    ID = reader["ID"] as string,
                                    PersonID = reader["PersonID"] as string
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
        /// Loads the billet assignment for a person, returns null if none is found and throws an error if more than one is found.
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        public static async Task<BilletAssignment> DBLoadOneByPerson(string personID)
        {
            try
            {
                BilletAssignment result = new BilletAssignment();
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
                            DataTable table = new DataTable();
                            table.Load(reader);
                            if (table.Rows.Count > 1)
                                throw new Exception(string.Format("While loading the billet assignments for the person whose ID is '{0}', more than one billet assignment was returned.", personID));

                            return new BilletAssignment()
                            {
                                BilletID = table.Rows[0]["BilletID"] as string,
                                ID = table.Rows[0]["ID"] as string,
                                PersonID = table.Rows[0]["PersonID"] as string
                            };
                        }
                        else
                        {
                            return null;
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
        /// Loads all billet assignments for a given billet's ID.
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        public static async Task<List<BilletAssignment>> DBLoadAllByBillet(string personID)
        {
            try
            {
                List<BilletAssignment> result = new List<BilletAssignment>();
                using (MySqlConnection connection = new MySqlConnection(Properties.ConnectionString))
                {
                    await connection.OpenAsync();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = string.Format("SELECT * FROM `{0}` WHERE `BilletID` = @BilletID", _tableName);

                    command.Parameters.AddWithValue("@BilletID", personID);

                    using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            while (await reader.ReadAsync())
                            {
                                result.Add(new BilletAssignment()
                                {
                                    BilletID = reader["BilletID"] as string,
                                    ID = reader["ID"] as string,
                                    PersonID = reader["PersonID"] as string
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
        /// Deletes all billet assignments for a given person's ID.
        /// </summary>
        /// <param name="personID"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public static async Task DBDeleteAllByPersonID(string personID, MySqlTransaction transaction)
        {
            try
            {
                using (MySqlCommand command = new MySqlCommand("", transaction.Connection, transaction))
                {
                    command.CommandText = string.Format("DELETE FROM `{0}` WHERE `PersonID` = @PersonID", _tableName);

                    command.Parameters.AddWithValue("PersonID", personID);

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
}
