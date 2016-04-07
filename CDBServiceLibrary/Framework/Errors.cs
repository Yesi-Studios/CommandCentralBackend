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
using AtwoodUtils;

namespace UnifiedServiceFramework.Framework
{
    /// <summary>
    /// Describes errors and provides methods for interacting with the errors table.  This class does not implement a cache.
    /// </summary>
    public static class Errors
    {

        /// <summary>
        /// This local, readonly property is intended to standardize all methods in this class that access the database and allow easy maintenance.
        /// </summary>
        private static readonly string _tableName = "errors";

        /// <summary>
        /// Descrbies a single error including its properties and moethods for interacting with it.
        /// </summary>
        public class Error
        {
            #region Properties

            /// <summary>
            /// The unique ID assigned to this error
            /// </summary>
            public string ID { get; set; }

            /// <summary>
            /// The message that was raised for this error
            /// </summary>
            public string Message { get; set; }

            /// <summary>
            /// The stack trace that lead to this error
            /// </summary>
            public string StackTrace { get; set; }

            /// <summary>
            /// Any inner exception's message
            /// </summary>
            public string InnerException { get; set; }

            /// <summary>
            /// The ID of the session's user when this error occurred.
            /// </summary>
            public string LoggedInUserID { get; set; }

            /// <summary>
            /// The Date/Time this error occurred.
            /// </summary>
            public DateTime Time { get; set; }

            /// <summary>
            /// Indicates whether or not the development/maintenance team has dealt with what caused this error.
            /// </summary>
            public bool IsHandled { get; set; }

            #endregion

            #region Data Access Methods

            /// <summary>
            /// Very simply, this method inserts a new error.
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
                        command.CommandText = string.Format("INSERT INTO `{0}` (`ID`,`Message`,`StackTrace`,`InnerException`,`LoggedInUserID`,`Time`,`IsHandled`) VALUES (@ID, @Message, @StackTrace, @InnerException, @LoggedInUserID, @Time, @IsHandled)", _tableName);

                        command.Parameters.AddWithValue("@ID", this.ID);
                        command.Parameters.AddWithValue("@Message", this.Message);
                        command.Parameters.AddWithValue("@StackTrace", this.StackTrace);
                        command.Parameters.AddWithValue("@InnerException", this.InnerException);
                        command.Parameters.AddWithValue("@LoggedInUserID", this.LoggedInUserID);
                        command.Parameters.AddWithValue("@Time", this.Time.ToMySqlDateTimeString());
                        command.Parameters.AddWithValue("@IsHandled", this.IsHandled);


                        await command.ExecuteNonQueryAsync();
                    }
                }
                catch
                {
                    throw;
                }
            }

            /// <summary>
            /// Updates this error instance by using the ID.  The ID itself is not updated.
            /// </summary>
            /// <returns></returns>
            public async Task DBUpdate()
            {
                try
                {
                    using (MySqlConnection connection = new MySqlConnection(Settings.ConnectionString))
                    {
                        await connection.OpenAsync();

                        MySqlCommand command = connection.CreateCommand();
                        command.CommandType = CommandType.Text;
                        command.CommandText = string.Format("UPDATE `{0}` SET `Message` = @Message, `StackTrace` = @StackTrace, `InnerException` = @InnerException, `LoggedInUserID` = @LoggedInUserID, `Time` = @Time, `IsHandled` = @IsHandled WHERE `ID` = @ID", _tableName);

                        command.Parameters.AddWithValue("@Message", this.Message);
                        command.Parameters.AddWithValue("@StackTrace", this.StackTrace);
                        command.Parameters.AddWithValue("@InnerException", this.InnerException);
                        command.Parameters.AddWithValue("@LoggedInUserID", this.LoggedInUserID);
                        command.Parameters.AddWithValue("@Time", this.Time.ToMySqlDateTimeString());
                        command.Parameters.AddWithValue("@IsHandled", this.IsHandled);
                        command.Parameters.AddWithValue("@ID", this.ID);

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

        #region Static Data Acces Methods

        /// <summary>
        /// Loads all errors from the database and optionally loads only the unhandled errors.
        /// </summary>
        /// <param name="loadUnhandledOnly"></param>
        /// <returns></returns>
        public static async Task<List<Error>> DBLoadAll(bool loadUnhandledOnly)
        {
            try
            {
                List<Error> result = new List<Error>();
                using (MySqlConnection connection = new MySqlConnection(Settings.ConnectionString))
                {
                    await connection.OpenAsync();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    if (loadUnhandledOnly)
                    {
                        command.CommandText = string.Format("SELECT * FROM `{0}` WHERE `IsHandled` = @IsHandled", _tableName);
                        command.Parameters.AddWithValue("@IsHandled", false);
                    }
                    else
                    {
                        command.CommandText = string.Format("SELECT * FROM `{0}`", _tableName);
                    }

                    using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            while (await reader.ReadAsync())
                            {
                                result.Add(new Error()
                                {
                                    ID = reader["ID"].ToString(),
                                    InnerException = reader["InnerException"].ToString(),
                                    IsHandled = (reader["IsHandled"].ToString() != "0"),
                                    LoggedInUserID = reader["LoggedInUserID"].ToString(),
                                    Message = reader["Message"].ToString(),
                                    StackTrace = reader["StackTrace"].ToString(),
                                    Time = Convert.ToDateTime(reader["Time"].ToString())
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
