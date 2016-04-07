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
    /// Describes members and such for interacting with an email address, including the email address class.
    /// </summary>
    public static class EmailAddresses
    {

        /// <summary>
        /// This readonly property is intended to standardize all methods that access the database and allow easy maintenance.
        /// </summary>
        public static string TableName
        {
            get
            {
                return "emailaddresses";
            }
        }

        /// <summary>
        /// Describes a single email address along with its data access methods
        /// </summary>
        public class EmailAddress
        {

            #region Properties

            /// <summary>
            /// The unique GUID of this Email Address
            /// </summary>
            public string ID { get; set; }

            /// <summary>
            /// The unique GUID of the person who owns this email address
            /// </summary>
            public string OwnerID { get; set; }

            /// <summary>
            /// The actual email address of this object.
            /// </summary>
            public string Address { get; set; }

            /// <summary>
            /// Indicates whether or not a person wants to be contacted using this email address.
            /// </summary>
            public bool IsContactable { get; set; }

            /// <summary>
            /// Indicates whether or not the client prefers to be contacted using this email address.
            /// </summary>
            public bool IsPreferred { get; set; }

            /// <summary>
            /// Indicates whether or not this email address is a mail.mil email address.  This is a calulated field, built using the Address field.
            /// </summary>
            public bool IsDODEmailAddress
            {
                get
                {
                    var elements = this.Address.Split(new[] { "@" }, StringSplitOptions.RemoveEmptyEntries);
                    if (!elements.Any())
                        return false;

                    return elements.Last().SafeEquals(EmailHelper.RequiredDODEmailHost);
                }
            }

            #endregion

            #region Data Access Methods

            /// <summary>
            /// Inserts the Email Address into the database
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
                        command.CommandText = string.Format("INSERT INTO `{0}` (`ID`,`OwnerID`,`Address`,`IsContactable`,`IsPreferred`)", TableName) +
                            " VALUES (@ID, @OwnerID, @Address, @IsContactable, @IsPreferred)";

                        command.Parameters.AddWithValue("@ID", this.ID);
                        command.Parameters.AddWithValue("@OwnerID", this.OwnerID);
                        command.Parameters.AddWithValue("@Address", this.Address);
                        command.Parameters.AddWithValue("@IsContactable", this.IsContactable);
                        command.Parameters.AddWithValue("@IsPreferred", this.IsPreferred);

                        await command.ExecuteNonQueryAsync();
                    }
                }
                catch
                {
                    throw;
                }
            }

            /// <summary>
            /// Inserts the Email Address into the database
            /// <para />
            /// This verison uses the transaction that was made by someone else.
            /// </summary>
            /// <returns></returns>
            public async Task DBInsert(MySqlTransaction transaction)
            {
                try
                {
                    using (MySqlCommand command = new MySqlCommand("", transaction.Connection, transaction))
                    {
                        command.CommandText = string.Format("INSERT INTO `{0}` (`ID`,`OwnerID`,`Address`,`IsContactable`,`IsPreferred`)", TableName) +
                            " VALUES (@ID, @OwnerID, @Address, @IsContactable, @IsPreferred)";

                        command.Parameters.AddWithValue("@ID", this.ID);
                        command.Parameters.AddWithValue("@OwnerID", this.OwnerID);
                        command.Parameters.AddWithValue("@Address", this.Address);
                        command.Parameters.AddWithValue("@IsContactable", this.IsContactable);
                        command.Parameters.AddWithValue("@IsPreferred", this.IsPreferred);

                        await command.ExecuteNonQueryAsync();
                    }
                }
                catch
                {
                    throw;
                }
            }

            /// <summary>
            /// Updates the current email address instance by resetting all columns to the current instance and uses the ID to index.  The ID itself cannot be updated.
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
                        command.CommandText = string.Format("UPDATE `{0}` SET `OwnerID` = @OwnerID, `Address` = @Address, `IsContactable` = @IsContactable, ", TableName) + //This thing got stupid long fast :(
                            "`IsPreferred` = @IsPreferred WHERE `ID` = @ID";

                        command.Parameters.AddWithValue("@OwnerID", this.OwnerID);
                        command.Parameters.AddWithValue("@Address", this.Address);
                        command.Parameters.AddWithValue("@IsContactable", this.IsContactable);
                        command.Parameters.AddWithValue("@IsPreferred", this.IsPreferred);

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
            /// Updates the current email address instance by resetting all columns to the current instance and uses the ID to index.  The ID itself cannot be updated.
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
                        command.CommandText = string.Format("UPDATE `{0}` SET `OwnerID` = @OwnerID, `Address` = @Address, `IsContactable` = @IsContactable, ", TableName) + //This thing got stupid long fast :(
                            "`IsPreferred` = @IsPreferred WHERE `ID` = @ID";

                        command.Parameters.AddWithValue("@OwnerID", this.OwnerID);
                        command.Parameters.AddWithValue("@Address", this.Address);
                        command.Parameters.AddWithValue("@IsContactable", this.IsContactable);
                        command.Parameters.AddWithValue("@IsPreferred", this.IsPreferred);

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
            /// Deletes the current email address instance from the database by using the current ID as the primary key.
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
                        command.CommandText = string.Format("DELETE FROM `{0}` WHERE `ID` = @ID", TableName);

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
            /// Deletes the current email address instance from the database by using the current ID as the primary key.
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
                        command.CommandText = string.Format("DELETE FROM `{0}` WHERE `ID` = @ID", TableName);

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
            /// Returns a boolean indicating whether or not the current phone number exists in the database.  Uses the ID to do this comparison.
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
                        command.CommandText = string.Format("SELECT CASE WHEN EXISTS(SELECT * FROM `{0}` WHERE `ID` = @ID) THEN 1 ELSE 0 END", TableName);

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

            #region Overrides

            /// <summary>
            /// Conducts a deep comparison against another object.
            /// </summary>
            /// <param name="obj"></param>
            /// <returns></returns>
            public override bool Equals(object obj)
            {
                EmailAddress other = obj as EmailAddress;

                if (other == null)
                    return false;

                if (this.Address != other.Address ||
                    this.ID != other.ID ||
                    this.IsContactable != other.IsContactable ||
                    this.IsDODEmailAddress != other.IsDODEmailAddress ||
                    this.IsPreferred != other.IsPreferred ||
                    this.OwnerID != other.OwnerID)
                    return false;

                return true;
            }

            /// <summary>
            /// Returns a hashcode built from the hashcodes from all the other properties.
            /// </summary>
            /// <returns></returns>
            public override int GetHashCode()
            {
                return this.Address.GetHashCode() ^
                    this.ID.GetHashCode() ^
                    this.IsContactable.GetHashCode() ^
                    this.IsDODEmailAddress.GetHashCode() ^
                    this.IsPreferred.GetHashCode() ^
                    this.OwnerID.GetHashCode();
            }

            #endregion

        }

        #region Other Methods

        /// <summary>
        /// Validates an email address and returns a list of errors that indicates which properties had issues.
        /// <para />
        /// Returns null if no errors were found.
        /// </summary>
        /// <param name="emailAddress"></param>
        /// <returns></returns>
        public static async Task<List<string>> ValidateEmailAddress(EmailAddress emailAddress)
        {
            try
            {
                List<string> errors = new List<string>();
                var props = typeof(EmailAddress).GetProperties().ToList();

                foreach (var prop in props)
                {
                    var error = await EmailAddresses.ValidateProperty(prop.Name, prop.GetValue(emailAddress));

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
        /// Validates a property of an Email Address
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
                                return string.Format("The value, '{0}', was not valid for the ID field of an Email Address; it must be a GUID.", value);

                            break;
                        }
                    case "ownerid":
                        {
                            if (!(await Persons.DoesPersonIDExist(value as string)))
                                return string.Format("The value, '{0}', was not valid for the Owner ID field of an Email Address; it must be a GUID and belong to an actual person.", value);

                            break;
                        }
                    case "address":
                        {
                            if (!ValidationMethods.IsValidEmailAddress(value))
                                throw new Exception("During validation of an Email Address, the value passed as the Email Address field was not in the right type.");

                            break;
                        }
                    case "iscontactable":
                        {
                            if (!(value is bool))
                                throw new Exception("During validation of an Email Address, the value passed as the is contactable field was not in the right type.");

                            break;
                        }
                    case "ispreferred":
                        {
                            if (!(value is bool))
                                throw new Exception("During validation of an Email Address, the value passed as the is preferred field was not in the right type.");

                            break;
                        }
                    case "isdodemailaddress":
                        {
                            //No validation is done on this property because it is read only.
                            break;
                        }
                    default:
                        {
                            throw new NotImplementedException(string.Format("While performing validaton on an Email Address, no validation rules were found for the property '{0}'!", propertyName));
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
        /// Loads all email addresses from the database.
        /// </summary>
        /// <returns></returns>
        public static async Task<List<EmailAddress>> DBLoadAll()
        {
            try
            {
                List<EmailAddress> result = new List<EmailAddress>();
                using (MySqlConnection connection = new MySqlConnection(Properties.ConnectionString))
                {
                    await connection.OpenAsync();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = string.Format("SELECT * FROM `{0}`", TableName);

                    using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            while (await reader.ReadAsync())
                            {
                                result.Add(new EmailAddress()
                                {
                                    Address = reader["Address"] as string,
                                    ID = reader["ID"] as string,
                                    IsContactable = Boolean.Parse(reader["IsContactable"] as string),
                                    IsPreferred = Boolean.Parse(reader["IsPreferred"] as string),
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
        /// Loads all email addresses for a given person.  This uses that person's id.
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        public static async Task<List<EmailAddress>> DBLoadAll(Persons.Person person)
        {
            try
            {
                List<EmailAddress> result = new List<EmailAddress>();
                using (MySqlConnection connection = new MySqlConnection(Properties.ConnectionString))
                {
                    await connection.OpenAsync();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = string.Format("SELECT * FROM `{0}` WHERE `OwnerID` = @OwnerID", TableName);

                    command.Parameters.AddWithValue("@OwnerID", person.ID);

                    using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            while (await reader.ReadAsync())
                            {
                                result.Add(new EmailAddress()
                                {
                                    Address = reader["Address"] as string,
                                    ID = reader["ID"] as string,
                                    IsContactable = Boolean.Parse(reader["IsContactable"] as string),
                                    IsPreferred = Boolean.Parse(reader["IsPreferred"] as string),
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
        /// Loads all email addresses for a given person id.
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        public static async Task<List<EmailAddress>> DBLoadAll(string personID)
        {
            try
            {
                List<EmailAddress> result = new List<EmailAddress>();
                using (MySqlConnection connection = new MySqlConnection(Properties.ConnectionString))
                {
                    await connection.OpenAsync();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = string.Format("SELECT * FROM `{0}` WHERE `OwnerID` = @OwnerID", TableName);

                    command.Parameters.AddWithValue("@OwnerID", personID);

                    using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            while (await reader.ReadAsync())
                            {
                                result.Add(new EmailAddress()
                                {
                                    Address = reader["Address"] as string,
                                    ID = reader["ID"] as string,
                                    IsContactable = reader.GetBoolean("IsContactable"),
                                    IsPreferred = reader.GetBoolean("IsPreferred"),
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

        #region Client Access Methods

        /// <summary>
        /// WARNING!  THIS IS A CLIENT METHOD.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Given a person ID, loads all email addresses for that person, redacting information the client isn't allowed to see.  Invalid person IDs will just result in an empty list.
        /// <para />
        /// Options: 
        /// <para />
        /// personid : the person for whom to load the email addresses.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<MessageTokens.MessageToken> LoadAllEmailAddressesAsync(MessageTokens.MessageToken token)
        {
            try
            {

                //Now get the client's model permissions.
                UnifiedServiceFramework.Authorization.Permissions.PermissionGroup.ModelPermission modelPermission =
                    UnifiedServiceFramework.Authorization.Permissions.GetModelPermissionsForUser(token, "Person");

                //Make sure we have a person id
                if (!token.Args.ContainsKey("personid"))
                    throw new ServiceException("You must send a person id.", ErrorTypes.Validation);
                string personID = token.Args["personid"] as string;

                //Check Permissions
                if (!modelPermission.ReturnableFields.Contains("EmailAddresses"))
                    throw new ServiceException("You don't have permission to load Email Addresses", ErrorTypes.Authorization);

                //Load the email addresses for the given person id.
                var result = await EmailAddresses.DBLoadAll(personID);

                //Finally, return our result.
                token.Result = result;

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
