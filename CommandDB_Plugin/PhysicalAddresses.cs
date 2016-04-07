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
    public static class PhysicalAddresses
    {
        /// <summary>
        /// This readonly property is intended to standardize all methods that access the database and allow easy maintenance.
        /// </summary>
        public static string TableName
        {
            get
            {
                return "physicaladdresses";
            }
        }

        public class PhysicalAddress
        {

            #region Properties

            /// <summary>
            /// The unique GUID of this physical address.
            /// </summary>
            public string ID { get; set; }

            /// <summary>
            /// The unique GUID of the person who owns this physical address.
            /// </summary>
            public string OwnerID { get; set; }

            /// <summary>
            /// The street number.
            /// </summary>
            public string StreetNumber { get; set; }

            /// <summary>
            /// The Route...
            /// </summary>
            public string Route { get; set; }

            /// <summary>
            /// The city.
            /// </summary>
            public string City { get; set; }

            /// <summary>
            /// The state.
            /// </summary>
            public string State { get; set; }

            /// <summary>
            /// The zip code.
            /// </summary>
            public string ZipCode { get; set; }

            /// <summary>
            /// The country.
            /// </summary>
            public string Country { get; set; }

            /// <summary>
            /// Indicates whether or not the person lives at this address
            /// </summary>
            public bool IsHomeAddress { get; set; }

            private float? _latitude = null;
            public float? Latitude
            {
                get
                {
                    return _latitude;
                }
                set
                {
                    _latitude = value;
                }
            }

            private float? _longitude = null;
            public float? Longitude
            {
                get
                {
                    return _longitude;
                }
                set
                {
                    _longitude = value;
                }
            }

            #endregion

            #region Data Access Methods

            /// <summary>
            /// Inserts the physical address into the database.
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
                        command.CommandText = string.Format("INSERT INTO `{0}` (`ID`, `OwnerID`, `StreetNumber`, `Route`,`City`,`State`,`ZipCode`,`Country`,`IsHomeAddress`,`Latitude`,`Longitude`)", TableName) +
                            " VALUES (@ID, @OwnerID, @StreetNumber, @Route, @City, @State, @ZipCode, @Country, @IsHomeAddress, @Latitude, @Longitude)";

                        command.Parameters.AddWithValue("@ID", this.ID);
                        command.Parameters.AddWithValue("@OwnerID", this.OwnerID);
                        command.Parameters.AddWithValue("@StreetNumber", this.StreetNumber);
                        command.Parameters.AddWithValue("@Route", this.Route);
                        command.Parameters.AddWithValue("@City", this.City);
                        command.Parameters.AddWithValue("@State", this.State);
                        command.Parameters.AddWithValue("@ZipCode", this.ZipCode);
                        command.Parameters.AddWithValue("@Country", this.Country);
                        command.Parameters.AddWithValue("@IsHomeAddress", this.IsHomeAddress);
                        command.Parameters.AddWithValue("@Latitude", this.Latitude);
                        command.Parameters.AddWithValue("@Longitude", this.Longitude);

                        await command.ExecuteNonQueryAsync();
                    }
                }
                catch
                {
                    throw;
                }
            }

            /// <summary>
            /// Inserts the physical address into the database.
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
                        command.CommandText = string.Format("INSERT INTO `{0}` (`ID`, `OwnerID`, `StreetNumber`, `Route`,`City`,`State`,`ZipCode`,`Country`,`IsHomeAddress`,`Latitude`,`Longitude`)", TableName) +
                            " VALUES (@ID, @OwnerID, @StreetNumber, @Route, @City, @State, @ZipCode, @Country, @IsHomeAddress, @Latitude, @Longitude)";

                        command.Parameters.AddWithValue("@ID", this.ID);
                        command.Parameters.AddWithValue("@OwnerID", this.OwnerID);
                        command.Parameters.AddWithValue("@StreetNumber", this.StreetNumber);
                        command.Parameters.AddWithValue("@Route", this.Route);
                        command.Parameters.AddWithValue("@City", this.City);
                        command.Parameters.AddWithValue("@State", this.State);
                        command.Parameters.AddWithValue("@ZipCode", this.ZipCode);
                        command.Parameters.AddWithValue("@Country", this.Country);
                        command.Parameters.AddWithValue("@IsHomeAddress", this.IsHomeAddress);
                        command.Parameters.AddWithValue("@Latitude", this.Latitude);
                        command.Parameters.AddWithValue("@Longitude", this.Longitude);

                        await command.ExecuteNonQueryAsync();
                    }
                
                }
                catch
                {
                    throw;
                }
            }

            /// <summary>
            /// Updates the current physical address instance by resetting all columns to the current instance and uses the ID to index.  The ID itself cannot be updated.
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
                        command.CommandText = string.Format("UPDATE `{0}` SET `OwnerID` = @OwnerID, `StreetNumber` = @StreetNumber, `Route` = @Route, `City` = @City, `State` = @State, ", TableName) + //This thing got stupid long fast :(
                            "`ZipCode` = @ZipCode, `Country` = @Country `IsHomeAddress` = @IsHomeAddress, `Latitude` = @Latitude, `Longitude` = @Longitude WHERE `ID` = @ID";

                        command.Parameters.AddWithValue("@OwnerID", this.OwnerID);
                        command.Parameters.AddWithValue("@StreetNumber", this.StreetNumber);
                        command.Parameters.AddWithValue("@Route", this.Route);
                        command.Parameters.AddWithValue("@City", this.City);
                        command.Parameters.AddWithValue("@State", this.State);
                        command.Parameters.AddWithValue("@ZipCode", this.ZipCode);
                        command.Parameters.AddWithValue("@Country", this.Country);
                        command.Parameters.AddWithValue("@IsHomeAddress", this.IsHomeAddress);
                        command.Parameters.AddWithValue("@Latitude", this.Latitude);
                        command.Parameters.AddWithValue("@Longitude", this.Longitude);

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
            /// Updates the current physical address instance by resetting all columns to the current instance and uses the ID to index.  The ID itself cannot be updated.
            /// <para />
            /// Uses the transaction that was sent for DB Interaction.
            /// </summary>
            /// <returns></returns>
            public async Task DBUpdate(MySqlTransaction transaction)
            {
                try
                {

                    using (MySqlCommand command = new MySqlCommand("", transaction.Connection, transaction))
                    {
                        command.CommandText = string.Format("UPDATE `{0}` SET `OwnerID` = @OwnerID, `StreetNumber` = @StreetNumber, `Route` = @Route, `City` = @City, `State` = @State, ", TableName) + //This thing got stupid long fast :(
                            "`ZipCode` = @ZipCode, `Country` = @Country, `IsHomeAddress` = @IsHomeAddress, `Latitude` = @Latitude, `Longitude` = @Longitude WHERE `ID` = @ID";

                        command.Parameters.AddWithValue("@OwnerID", this.OwnerID);
                        command.Parameters.AddWithValue("@StreetNumber", this.StreetNumber);
                        command.Parameters.AddWithValue("@Route", this.Route);
                        command.Parameters.AddWithValue("@City", this.City);
                        command.Parameters.AddWithValue("@State", this.State);
                        command.Parameters.AddWithValue("@ZipCode", this.ZipCode);
                        command.Parameters.AddWithValue("@Country", this.Country);
                        command.Parameters.AddWithValue("@IsHomeAddress", this.IsHomeAddress);
                        command.Parameters.AddWithValue("@Latitude", this.Latitude);
                        command.Parameters.AddWithValue("@Longitude", this.Longitude);

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
            /// Deletes the current physical address instance from the database by using the current ID as the primary key.
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
            /// Deletes the current physical address instance from the database by using the current ID as the primary key.
            /// <para />
            /// This version uses the transation that should have been made perviously by another method.
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
            /// Returns a boolean indicating whether or not the current physical address instance exists in the database.  Uses the ID to do this comparison.
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
                PhysicalAddress other = obj as PhysicalAddress;

                if (other == null)
                    return false;

                if (this.City != other.City ||
                    this.Country != other.Country ||
                    this.ID != other.ID ||
                    this.IsHomeAddress != other.IsHomeAddress ||
                    this.Latitude != other.Latitude ||
                    this.Longitude != other.Longitude ||
                    this.OwnerID != other.OwnerID ||
                    this.Route != other.Route ||
                    this.State != other.State ||
                    this.StreetNumber != other.StreetNumber ||
                    this.ZipCode != other.ZipCode)
                    return false;

                return true;
            }

            /// <summary>
            /// Returns a hashcode built from the hash codes of other properties.
            /// </summary>
            /// <returns></returns>
            public override int GetHashCode()
            {
                return this.City.GetHashCode() ^
                    this.Country.GetHashCode() ^
                    this.ID.GetHashCode() ^
                    this.IsHomeAddress.GetHashCode() ^
                    this.Latitude.GetHashCode() ^
                    this.Longitude.GetHashCode() ^
                    this.OwnerID.GetHashCode() ^
                    this.Route.GetHashCode() ^
                    this.State.GetHashCode() ^
                    this.StreetNumber.GetHashCode() ^
                    this.ZipCode.GetHashCode();
            }

            #endregion

        }

        #region Other Methods

        /// <summary>
        /// Validates a physical address and returns a list of errors that indicates which properties had issues.
        /// <para />
        /// Returns null if no errors were found.
        /// </summary>
        /// <param name="physicalAddress"></param>
        /// <returns></returns>
        public static async Task<List<string>> ValidatePhysicalAddress(PhysicalAddress physicalAddress)
        {
            try
            {
                List<string> errors = new List<string>();
                var props = typeof(PhysicalAddress).GetProperties().ToList();

                foreach (var prop in props)
                {
                    var error = await PhysicalAddresses.ValidateProperty(prop.Name, prop.GetValue(physicalAddress));

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
        /// Validates a given property of a physical address.
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
                                return string.Format("The value, '{0}', was not valid for the ID field of a Physical Address; it must be a GUID.", value);

                            break;
                        }
                    case "ownerid":
                        {
                            if (!(await Persons.DoesPersonIDExist(value as string)))
                                return string.Format("The value, '{0}', was not valid for the Owner ID field of a Physical Address; it must be a GUID and belong to an actual person.", value);

                            break;
                        }
                    case "streetnumber":
                    case "route":
                    case "city":
                    case "state":
                        {
                            break;
                        }
                    case "ishomeaddress":
                        {
                            if (!(value is bool))
                                throw new Exception("During validation of a physical address, the value passed as the is home address was not in the right type.");

                            break;
                        }
                    case "Latitude":
                        {
                            var lat = value as float?;

                            if (lat == null)
                                throw new Exception("During validation of a physical address, the latitude was in the wrong type.");

                            if (lat.HasValue)
                            {
                                if (!ValidationMethods.IsValidLatitude(lat.Value))
                                    return string.Format("The value, '{0}', was not valid for the Latitude field of a Physical Address; it must be between -90 and 90 - inclusive.", value);
                            }

                            break;
                        }
                    case "Longitude":
                        {
                            var lng = value as float?;

                            if (lng == null)
                                throw new Exception("During validation of a physical address, the longitude was in the wrong type.");

                            if (lng.HasValue)
                            {
                                if (!ValidationMethods.IsValidLongtitude(lng.Value))
                                    return string.Format("The value, '{0}', was not valid for the Longitude field of a Physical Address; it must be between -180 and 180 - inclusive.", value);
                            }

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
        /// Loads all physical address from the database.
        /// </summary>
        /// <returns></returns>
        public static async Task<List<PhysicalAddress>> DBLoadAll()
        {
            try
            {
                List<PhysicalAddress> result = new List<PhysicalAddress>();
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
                                result.Add(new PhysicalAddress()
                                {
                                    OwnerID = reader["OwnerID"] as string,
                                    City = reader["City"] as string,
                                    Country = reader["Country"] as string,
                                    ID = reader["ID"] as string,
                                    IsHomeAddress = reader.GetBoolean("IsHomeAddress"),
                                    Latitude = reader.GetFloat("Latitude"),
                                    Longitude = reader.GetFloat("Longitude"),
                                    State = reader["State"] as string,
                                    StreetNumber = reader["StreetNumber"] as string,
                                    Route = reader["Route"] as string,
                                    ZipCode = reader["ZipCode"] as string
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
        /// Loads all physical addresses for a given person.  This uses that person's id.
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        public static async Task<List<PhysicalAddress>> DBLoadAll(Persons.Person person)
        {
            try
            {
                List<PhysicalAddress> result = new List<PhysicalAddress>();
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
                                result.Add(new PhysicalAddress()
                                {
                                    OwnerID = reader["OwnerID"] as string,
                                    City = reader["City"] as string,
                                    Country = reader["Country"] as string,
                                    ID = reader["ID"] as string,
                                    IsHomeAddress = reader.GetBoolean("IsHomeAddress"),
                                    Latitude = reader.GetFloat("Latitude"),
                                    Longitude = reader.GetFloat("Longitude"),
                                    State = reader["State"] as string,
                                    StreetNumber = reader["StreetNumber"] as string,
                                    Route = reader["Route"] as string,
                                    ZipCode = reader["ZipCode"] as string
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
        /// Loads all physical addresses for a given person id.
        /// </summary>
        /// <param name="person"></param>
        /// <returns></returns>
        public static async Task<List<PhysicalAddress>> DBLoadAll(string personID)
        {
            try
            {
                List<PhysicalAddress> result = new List<PhysicalAddress>();
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
                                result.Add(new PhysicalAddress()
                                {
                                    OwnerID = reader["OwnerID"] as string,
                                    City = reader["City"] as string,
                                    Country = reader["Country"] as string,
                                    ID = reader["ID"] as string,
                                    IsHomeAddress = reader.GetBoolean("IsHomeAddress"),
                                    Latitude = reader["Latitude"] as string == null ? null : new Nullable<float>((float)Convert.ToDouble(reader["Latitude"] as string)),
                                    Longitude = reader["Longitude"] as string == null ? null : new Nullable<float>((float)Convert.ToDouble(reader["Longitude"] as string)),
                                    State = reader["State"] as string,
                                    StreetNumber = reader["StreetNumber"] as string,
                                    Route = reader["Route"] as string,
                                    ZipCode = reader["ZipCode"] as string
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
        /// Loads a single physical address record from the database for the given physical address ID.
        /// </summary>
        /// <param name="physicalAddressID"></param>
        /// <returns></returns>
        public static async Task<PhysicalAddress> DBLoadOne(string physicalAddressID)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(Properties.ConnectionString))
                {
                    await connection.OpenAsync();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = string.Format("SELECT * FROM `{0}` WHERE `ID` = @ID", TableName);

                    command.Parameters.AddWithValue("@ID", physicalAddressID);

                    using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            await reader.ReadAsync();

                            return new PhysicalAddress()
                            {
                                OwnerID = reader["OwnerID"] as string,
                                City = reader["City"] as string,
                                Country = reader["Country"] as string,
                                ID = reader["ID"] as string,
                                IsHomeAddress = reader.GetBoolean("IsHomeAddress"),
                                Latitude = reader.GetFloat("Latitude"),
                                Longitude = reader.GetFloat("Longitude"),
                                State = reader["State"] as string,
                                StreetNumber = reader["StreetNumber"] as string,
                                Route = reader["Route"] as string,
                                ZipCode = reader["ZipCode"] as string
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
        /// Given a person ID, loads all physical addresses for that person.  Invalid person IDs will just result in an empty list.
        /// <para />
        /// Options: 
        /// <para />
        /// personid : the person for whom to load the physical addresses.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<MessageTokens.MessageToken> LoadAllPhysicalAddresses_Client(MessageTokens.MessageToken token)
        {
            try
            {
                //First we'll need the client's model permissions
                var modelPermission = UnifiedServiceFramework.Authorization.Permissions.GetModelPermissionsForUser(token, "Person");

                //Make sure we were given a person id
                if (!token.Args.ContainsKey("personid"))
                    throw new ServiceException("You must a person id.", ErrorTypes.Validation);
                string personID = token.Args["personid"] as string;

                //Make sure we're allowed to load physical addresses
                if (!modelPermission.ReturnableFields.Contains("PhysicalAddresses"))
                    throw new ServiceException("You don't have permission to load physical addresses!", ErrorTypes.Authorization);

                //Load the result
                token.Result = await PhysicalAddresses.DBLoadAll(personID);

                return token;
            }
            catch
            {
                throw;
            }
        }

        public static async Task<MessageTokens.MessageToken> UpdateOrInsertPhysicalAddress_Client(MessageTokens.MessageToken token)
        {
            try
            {
                //Get the client's permissions
                var clientPermissions = UnifiedServiceFramework.Authorization.Permissions.TranslatePermissionGroupIDs(token.Session.PermissionIDs);

                //Now get the client's model permissions
                var modelPermissions = UnifiedServiceFramework.Authorization.Permissions.GetModelPermissionsForUser(token, "Person");

                //And finally, the flattened list of custom permissions.
                List<CustomPermissionTypes> customPerms = UnifiedServiceFramework.Authorization.Permissions.GetUniqueCustomPermissions(clientPermissions).Select(x =>
                    {
                        CustomPermissionTypes customPerm;
                        if (!Enum.TryParse(x, out customPerm))
                            throw new Exception(string.Format("An error occurred while trying to parse the custom permission '{0}' into the custom permissions enum.", x));

                        return customPerm;
                    }).ToList();

                //Is the client allowed to update/insert physical addresses (persons)?
                if (!customPerms.Contains(CustomPermissionTypes.Edit_Users))
                    throw new ServiceException("You do not have permission to edit users", ErrorTypes.Authorization);

                //Make sure we got a physical address
                if (!token.Args.ContainsKey("physicaladdress"))
                    throw new ServiceException("In order to update or insert a physical address, you must first send one.", ErrorTypes.Validation);

                //And then cast it
                PhysicalAddress physicalAddress = token.Args["physicaladdress"].CastJObject<PhysicalAddress>();

                //Does it already exist?
                if (await physicalAddress.DBExists())
                {
                    //The physical address already exists, so let's update it.  We need the changes as well.
                    //Here's the old physical address as it currently exists in the database.
                    PhysicalAddress oldPhysialAddress = await PhysicalAddresses.DBLoadOne(physicalAddress.ID);

                    //And then the changes...
                    var changes = physicalAddress.DetermineVariances(oldPhysialAddress).Select(x => new Changes.Change()
                    {
                        EditorID = token.Session.PersonID,
                        ID = Guid.NewGuid().ToString(),
                        ObjectID = physicalAddress.ID,
                        ObjectName = "Physical Address",
                        Remarks = "Physical Address Edited",
                        Time = token.CallTime,
                        Variance = x
                    }).ToList();

                    //And then the update...
                    await physicalAddress.DBUpdate();

                    //And then update the changes
                    Changes.DBInsertAll(changes);

                    //TODO: Get the person's email to whom this physical address belongs and inform them that a physical address belonging to them was edited.

                    //And then set the result
                    token.Result = "Success";
                }
                else //if we got to here then the physical address does not already exist and we need to try to insert it.
                { 
                    //Obviously, we can't trust the client's ID.
                    physicalAddress.ID = Guid.NewGuid().ToString();

                    //Now insert it!
                    await physicalAddress.DBInsert();

                    //And then get the changes against a blank object.
                    var changes = physicalAddress.DetermineVariances(new PhysicalAddress()).Select(x => new Changes.Change()
                    {
                        EditorID = token.Session.PersonID,
                        ID = Guid.NewGuid().ToString(),
                        ObjectID = physicalAddress.ID,
                        ObjectName = "Physical Address",
                        Remarks = "Physical Address Created",
                        Time = token.CallTime,
                        Variance = x
                    }).ToList();

                    //Insert the changes
                    Changes.DBInsertAll(changes);

                    //TODO: Alert the user to whom this physical address belongs that we have made it on their profile

                    //Finally, send back the new physical address's ID.
                    token.Result = physicalAddress.ID;

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
