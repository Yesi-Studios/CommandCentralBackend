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

namespace CommandCentral
{
    /// <summary>
    /// Describes members and such for interacting with a phone number
    /// </summary>
    public static class PhoneNumbers
    {

        /// <summary>
        /// This readonly property is intended to standardize all methods that access the database and allow easy maintenance.
        /// </summary>
        public static string TableName
        {
            get
            {
                return "phonenumbers";
            }
        }

        /// <summary>
        /// Describes a single Phone number along with its data access members and properties
        /// </summary>
        public class PhoneNumber
        {

            #region Properties

            /// <summary>
            /// The unique GUID of this phone number.
            /// </summary>
            public string ID { get; set; }

            /// <summary>
            /// The unique GUID of the person to whom this phone number belongs.
            /// </summary>
            public string OwnerID { get; set; }

            /// <summary>
            /// The actual phone number of this phone number object.
            /// </summary>
            public string Number { get; set; }

            /// <summary>
            /// The carrier of this phone number, eg.  Verizon, etc.
            /// </summary>
            public string Carrier { get; set; }

            /// <summary>
            /// The carrier's SMS email address.
            /// </summary>
            public string CarrierMailAddress
            {
                get
                {
                    if (this.Carrier == null)
                        return null;
                    else
                        return TextMessageHelper.PhoneCarrierMailDomainMappings[this.Carrier];
                }
            }

            /// <summary>
            /// Indicates whether or not the person who owns this phone number wants any contact to occur using it.
            /// </summary>
            public bool IsContactable { get; set; }

            /// <summary>
            /// Indicates whether or not the person who owns this phone number prefers contact to occur on it.
            /// </summary>
            public bool IsPreferred { get; set; }

            /// <summary>
            /// The type of this phone. eg. Cell, Home, Work
            /// </summary>
            public string PhoneType { get; set; }

            #endregion

            #region Data Access Methods

            /// <summary>
            /// Inserts the phone number into the database.
            /// </summary>
            /// <returns></returns>
            public async Task DBInsert()
            {
                try
                {

                    using (MySqlConnection connection = new MySqlConnection(Properties.ConnectionString))
                    {
                        await connection.OpenAsync();

                        using (MySqlTransaction transaction = await connection.BeginTransactionAsync())
                        {
                            try
                            {

                                using (MySqlCommand command = new MySqlCommand("", transaction.Connection, transaction))
                                {
                                    command.CommandText = string.Format("INSERT INTO `{0}` (`ID`,`OwnerID`,`Number`,`Carrier`,`IsContactable`,`IsPreferred`,`PhoneType`)", TableName) +
                                        " VALUES (@ID, @OwnerID, @Number, @Carrier, @IsContactable, @IsPreferred, @PhoneType)";

                                    command.Parameters.AddWithValue("@ID", this.ID);
                                    command.Parameters.AddWithValue("@OwnerID", this.OwnerID);
                                    command.Parameters.AddWithValue("@Number", this.Number);
                                    command.Parameters.AddWithValue("@Carrier", this.Carrier);
                                    command.Parameters.AddWithValue("@IsContactable", this.IsContactable);
                                    command.Parameters.AddWithValue("@IsPreferred", this.IsPreferred);
                                    command.Parameters.AddWithValue("@PhoneType", this.PhoneType);

                                    await command.ExecuteNonQueryAsync();
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
            /// Inserts the phone number into the database.
            /// <para />
            /// This version uses the transaction as the DB interaction object.
            /// </summary>
            /// <returns></returns>
            public async Task DBInsert(MySqlTransaction transaction)
            {
                try
                {

                    using (MySqlCommand command = new MySqlCommand("", transaction.Connection, transaction))
                    {
                        command.CommandText = string.Format("INSERT INTO `{0}` (`ID`,`OwnerID`,`Number`,`Carrier`,`IsContactable`,`IsPreferred`,`PhoneType`)", TableName) +
                            " VALUES (@ID, @OwnerID, @Number, @Carrier, @IsContactable, @IsPreferred, @PhoneType)";

                        command.Parameters.AddWithValue("@ID", this.ID);
                        command.Parameters.AddWithValue("@OwnerID", this.OwnerID);
                        command.Parameters.AddWithValue("@Number", this.Number);
                        command.Parameters.AddWithValue("@Carrier", this.Carrier);
                        command.Parameters.AddWithValue("@IsContactable", this.IsContactable);
                        command.Parameters.AddWithValue("@IsPreferred", this.IsPreferred);
                        command.Parameters.AddWithValue("@PhoneType", this.PhoneType);

                        await command.ExecuteNonQueryAsync();
                    }
                }
                catch
                {
                    throw;
                }
            }

            /// <summary>
            /// Updates the current phone number instance by resetting all columns to the current instance and uses the ID to index.  The ID itself cannot be updated.
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
                        command.CommandText = string.Format("UPDATE `{0}` SET `OwnerID` = @OwnerID, `Number` = @Number, `Carrier` = @Carrier, ", TableName) + //This thing got stupid long fast :(
                            "`IsContactable` = @IsContactable, `IsPreferred` = @IsPreferred, `PhoneType` = @PhoneType WHERE `ID` = @ID";

                        command.Parameters.AddWithValue("@OwnerID", this.OwnerID);
                        command.Parameters.AddWithValue("@Number", this.Number);
                        command.Parameters.AddWithValue("@Carrier", this.Carrier);
                        command.Parameters.AddWithValue("@IsContactable", this.IsContactable);
                        command.Parameters.AddWithValue("@IsPreferred", this.IsPreferred);
                        command.Parameters.AddWithValue("@PhoneType", this.PhoneType);

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
            /// Updates the current phone number instance by resetting all columns to the current instance and uses the ID to index.  The ID itself cannot be updated.
            /// <para />
            /// This version uses the transaction to do the DB interaction that is passed.
            /// </summary>
            /// <returns></returns>
            public async Task DBUpdate(MySqlTransaction transaction)
            {
                try
                {

                    using (MySqlCommand command = new MySqlCommand("", transaction.Connection, transaction))
                    {
                        command.CommandText = string.Format("UPDATE `{0}` SET `OwnerID` = @OwnerID, `Number` = @Number, `Carrier` = @Carrier, ", TableName) + //This thing got stupid long fast :(
                            "`IsContactable` = @IsContactable, `IsPreferred` = @IsPreferred, `PhoneType` = @PhoneType WHERE `ID` = @ID";

                        command.Parameters.AddWithValue("@OwnerID", this.OwnerID);
                        command.Parameters.AddWithValue("@Number", this.Number);
                        command.Parameters.AddWithValue("@Carrier", this.Carrier);
                        command.Parameters.AddWithValue("@IsContactable", this.IsContactable);
                        command.Parameters.AddWithValue("@IsPreferred", this.IsPreferred);
                        command.Parameters.AddWithValue("@PhoneType", this.PhoneType);

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
            /// Deletes the current phone number instance from the database by using the current ID as the primary key.
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
            /// Deletes the current phone number instance from the database by using the current ID as the primary key.
            /// <para />
            /// This version uses an already-made transaction to do the db interaction.
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
            /// Conducts a deep comparison on the given object against this object.
            /// </summary>
            /// <param name="obj"></param>
            /// <returns></returns>
            public override bool Equals(object obj)
            {
                PhoneNumber other = obj as PhoneNumber;

                if (other == null)
                    return false;

                if (this.Carrier != other.Carrier
                    || this.CarrierMailAddress != other.CarrierMailAddress
                    || this.ID != other.ID
                    || this.IsContactable != other.IsContactable
                    || this.IsPreferred != other.IsPreferred
                    || this.Number != other.Number
                    || this.OwnerID != other.OwnerID
                    || this.PhoneType != other.PhoneType)
                    return false;

                return true;
            }

            /// <summary>
            /// Returns a hash code built from the hashcodes of all properties on this type.
            /// </summary>
            /// <returns></returns>
            public override int GetHashCode()
            {
                return this.Carrier.GetHashCode() ^
                    this.CarrierMailAddress.GetHashCode() ^
                    this.ID.GetHashCode() ^
                    this.IsContactable.GetHashCode() ^
                    this.IsPreferred.GetHashCode() ^
                    this.Number.GetHashCode() ^
                    this.OwnerID.GetHashCode() ^
                    this.PhoneType.GetHashCode();
            }

            #endregion

        }

        #region Other Methods

        /// <summary>
        /// Validates a phone number and returns a list of errors that indicates which properties had issues.
        /// <para />
        /// Returns null if no errors were found.
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <returns></returns>
        public static async Task<List<string>> ValidatePhoneNumber(PhoneNumber phoneNumber)
        {
            try
            {
                List<string> errors = new List<string>();
                var props = typeof(PhoneNumber).GetProperties().ToList();

                foreach (var prop in props)
                {
                    var error = await PhoneNumbers.ValidateProperty(prop.Name, prop.GetValue(phoneNumber));

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
        /// Validates a given property of a phone number.
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
                                return string.Format("The value, '{0}', was not valid for the ID field of a Phone Number; it must be a GUID.", value);

                            break;
                        }
                    case "ownerid":
                        {
                            if (!(await Persons.DoesPersonIDExist(value as string)))
                                return string.Format("The value, '{0}', was not valid for the Owner ID field of a Phone Number; it must be a GUID and belong to an actual person.", value);

                            break;
                        }
                    case "number":
                        {
                            if (!ValidationMethods.IsValidPhoneNumber(value))
                                return string.Format("The value, '{0}', was not valid for the Number field; if you feel this is in error, please contact the development team.", value);

                            break;
                        }
                    case "carrier":
                        {
                            if (!string.IsNullOrWhiteSpace(value as string) && !ValidationMethods.IsValidPhoneCarrier(value))
                                return string.Format("The value, '{0}', was not valid for the Phone Carrier field of a Phone Number; if you feel this is in error, please contact the development team - we most likely have never seen your phone carrier before.", value);

                            break;
                        }
                    case "carriermailaddress":
                        {
                            //This is a read only, calculated field, so we don't validate it.
                            break;
                        }
                    case "iscontactable":
                        {
                            if (!(value is bool))
                                throw new Exception("During validation of a phone number, the value passed as the is contactable field was not in the right type.");

                            break;
                        }
                    case "ispreferred":
                        {
                            if (!(value is bool))
                                throw new Exception("During validation of a phone number, the value passed as the is preferred field was not in the right type.");

                            break;
                        }
                    case "phonetype":
                        {
                            if (!string.IsNullOrWhiteSpace(value as string) && !ValidationMethods.IsValidPhoneType(value))
                                return string.Format("The value, '{0}', was not valid for the Phone Type field of a Phone Number; if you feel this is in error, please contact the development team.", value);

                            break;
                        }
                    default:
                        {
                            throw new NotImplementedException(string.Format("While validating a phone number, the property '{0}' had no validation rules defined.", propertyName));
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
        /// Loads all phone numbers from the database.
        /// </summary>
        /// <returns></returns>
        public static async Task<List<PhoneNumber>> DBLoadAll()
        {
            try
            {
                List<PhoneNumber> result = new List<PhoneNumber>();
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
                                result.Add(new PhoneNumber()
                                {
                                    Carrier = reader["Carrier"] as string,
                                    ID = reader["ID"] as string,
                                    IsContactable = reader.GetBoolean("IsContactable"),
                                    IsPreferred = reader.GetBoolean("IsPreferred"),
                                    Number = reader["Number"] as string,
                                    OwnerID = reader["OwnerID"] as string,
                                    PhoneType = reader["PhoneType"] as string
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
        /// Loads all phone numbers for a given person.  This uses that person's id.
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        public static async Task<List<PhoneNumber>> DBLoadAll(Persons.Person person)
        {
            try
            {
                List<PhoneNumber> result = new List<PhoneNumber>();
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
                                result.Add(new PhoneNumber()
                                {
                                    Carrier = reader["Carrier"] as string,
                                    ID = reader["ID"] as string,
                                    IsContactable = reader.GetBoolean("IsContactable"),
                                    IsPreferred = reader.GetBoolean("IsPreferred"),
                                    Number = reader["Number"] as string,
                                    OwnerID = reader["OwnerID"] as string,
                                    PhoneType = reader["PhoneType"] as string
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
        /// Loads all phone numbers for a given person id.
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        public static async Task<List<PhoneNumber>> DBLoadAll(string personID)
        {
            try
            {
                List<PhoneNumber> result = new List<PhoneNumber>();
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
                                result.Add(new PhoneNumber()
                                {
                                    Carrier = reader["Carrier"] as string,
                                    ID = reader["ID"] as string,
                                    IsContactable = reader.GetBoolean("IsContactable"),
                                    IsPreferred = reader.GetBoolean("IsPreferred"),
                                    Number = reader["Number"] as string,
                                    OwnerID = reader["OwnerID"] as string,
                                    PhoneType = reader["PhoneType"] as string
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
        /// Loads a single phone number record from the database for the given phone number ID.
        /// </summary>
        /// <param name="phoneNumberID"></param>
        /// <returns></returns>
        public static async Task<PhoneNumber> DBLoadOne(string phoneNumberID)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(Properties.ConnectionString))
                {
                    await connection.OpenAsync();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = string.Format("SELECT * FROM `{0}` WHERE `ID` = @ID", TableName);

                    command.Parameters.AddWithValue("@ID", phoneNumberID);

                    using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            await reader.ReadAsync();

                            return new PhoneNumber()
                            {
                                Carrier = reader["Carrier"] as string,
                                ID = reader["ID"] as string,
                                IsContactable = reader.GetBoolean("IsContactable"),
                                IsPreferred = reader.GetBoolean("IsPreferred"),
                                Number = reader["Number"] as string,
                                OwnerID = reader["OwnerID"] as string,
                                PhoneType = reader["PhoneType"] as string
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

        #endregion

        #region Client Access Methods

        /// <summary>
        /// WARNING!  THIS IS A CLIENT METHOD.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Given a person ID, loads all phone numbers for that person.  Invalid person IDs will just result in an empty list.
        /// <para />
        /// Options: 
        /// <para />
        /// personid : the person for whom to load the phone numbers.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<MessageTokens.MessageToken> LoadAllPhoneNumbers_Client(MessageTokens.MessageToken token)
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

                //Make sure the client has permission to request phone numbers
                if (!modelPermission.ReturnableFields.Contains("PhoneNumbers"))
                    throw new ServiceException("You don't have permission to load phone numbers!", ErrorTypes.Authorization);

                //Finally, return our result.
                token.Result = await PhoneNumbers.DBLoadAll(personID);

                return token;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// WARNING!  THIS IS A CLIENT METHOD.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Given a phone number, this method will attempt to update that phone number if its ID is found, or insert the phone number, changing its ID in the process.
        /// <para />
        /// This method will also alert the person to whom this phone number belongs that the phone number was either updated or inserted.
        /// <para />
        /// Returns either success or the new phone number's ID.
        /// <para />
        /// Options: 
        /// <para />
        /// phonenumber : The phone number object to update or insert.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<MessageTokens.MessageToken> UpdateOrInsertPhoneNumber_Client(MessageTokens.MessageToken token)
        {
            try
            {
                //First get the client's permissions.
                List<UnifiedServiceFramework.Authorization.Permissions.PermissionGroup> clientPermissions =
                    UnifiedServiceFramework.Authorization.Permissions.TranslatePermissionGroupIDs(token.Session.PermissionIDs);

                //Now get the client's model permissions.
                UnifiedServiceFramework.Authorization.Permissions.PermissionGroup.ModelPermission modelPermission =
                    UnifiedServiceFramework.Authorization.Permissions.GetModelPermissionsForUser(token, "Person");

                //Get the flattened list of all the permissions.
                List<SpecialPermissionTypes> customPerms = UnifiedServiceFramework.Authorization.Permissions.GetUniqueCustomPermissions(clientPermissions).Select(x =>
                {
                    SpecialPermissionTypes customPerm;
                    if (!Enum.TryParse(x, out customPerm))
                        throw new Exception(string.Format("An error occurred while trying to parse the custom permission '{0}' into the custom permissions enum.", x));

                    return customPerm;
                }).ToList();

                //Is the client allowed to update/insert phone numbers?
                if (!customPerms.Contains(SpecialPermissionTypes.Edit_Users))
                    throw new ServiceException("You do not have permission to edit users.", ErrorTypes.Authorization);

                //Make sure we got a phone number
                if (!token.Args.ContainsKey("phonenumber"))
                    throw new ServiceException("You must send a phone number in order to update/insert a phone number.  ::he said, sarcastically::", ErrorTypes.Validation);

                //And then cast it.
                PhoneNumber phoneNumber = token.Args["phonenumber"].CastJToken<PhoneNumber>();

                //Alright, now we need to know if this phone number already exists or not.
                if (await phoneNumber.DBExists())
                {
                    //The phone number already exists, so let's update it.  We need the changes as well.
                    //Here's the old phone number as it currently exists in the database.
                    PhoneNumber oldPhoneNumber = await PhoneNumbers.DBLoadOne(phoneNumber.ID);

                    //Now here's the changes.  I'm going to do the variances and the changes in one go.
                    var changes = phoneNumber.DetermineVariances(oldPhoneNumber).Select(x => new Changes.Change()
                    {
                        EditorID = token.Session.PersonID,
                        ID = Guid.NewGuid().ToString(),
                        ObjectID = phoneNumber.ID,
                        ObjectName = "Phone Number",
                        Remarks = "Phone Number Edited",
                        Time = token.CallTime,
                        Variance = x
                    }).ToList();

                    //Now do the update.  If it succeeds, then update the changes.
                    await phoneNumber.DBUpdate();

                    //And then update the changes.
                    Changes.DBInsertAll(changes);

                    //TODO: Get the person's email to whom this phone number belongs and inform them a phone number was edited.

                    //And we don't need anything but success.
                    token.Result = "Success";
                }
                else //If we got here, then the Phone Number does not already exist meaning we need to do an insert.
                {
                    //Can't trust the client's ID.
                    phoneNumber.ID = Guid.NewGuid().ToString();

                    //Insert it
                    await phoneNumber.DBInsert();

                    //Get the changes as though mesaured against a phone number that doesn't already exist.
                    var changes = phoneNumber.DetermineVariances(new PhoneNumber()).Select(x => new Changes.Change()
                    {
                        EditorID = token.Session.PersonID,
                        ID = Guid.NewGuid().ToString(),
                        ObjectID = phoneNumber.ID,
                        ObjectName = "Phone Number",
                        Remarks = "Phone Number Created",
                        Time = token.CallTime,
                        Variance = x
                    }).ToList();

                    //And then update the changes.
                    Changes.DBInsertAll(changes);

                    //TODO: alert the user to whom this phone number belongs that a phone number was created on their profile.

                    //Finally, send back the new phone number's ID.
                    token.Result = phoneNumber.ID;
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
