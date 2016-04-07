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
    /// Contains members for interacting with billets which are used to power admin information for profile pages.
    /// </summary>
    public static class Billets
    {


        /// <summary>
        /// This local, readonly property is intended to standardize all methods in this class that access the database and allow easy maintenance.
        /// </summary>
        private static readonly string _tableName = "billets";

        /// <summary>
        /// The cache of in-memory, thread-safe billets.  Intended to be used by the service during operation.  
        /// </summary>
        private static ConcurrentDictionary<string, Billet> _billetsCache = new ConcurrentDictionary<string, Billet>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Describes a single billet along with its data access methods and other members.
        /// </summary>
        public class Billet
        {

            #region Properties

            /// <summary>
            /// The unique ID assigned to this Billet
            /// </summary>
            public string ID { get; set; }

            /// <summary>
            /// The title of this billet.
            /// </summary>
            public string Title { get; set; }

            /// <summary>
            /// The ID Number of this Billet.  This is also called the Billet ID Number or BIN.
            /// </summary>
            public string IDNumber { get; set; }

            /// <summary>
            /// The suffix code of this Billet.  This is also called the Billet Suffix Code or BSC.
            /// </summary>
            public string SuffixCode { get; set; }

            /// <summary>
            /// A free form text field intended to store notes/remarks about this billet.
            /// </summary>
            public string Remarks { get; set; }

            /// <summary>
            /// The designation assigned to a Billet.  For an enlisted Billet, this is the Rate the Billet is intended for.  For officers, this is their designation.
            /// </summary>
            public string Designation { get; set; }

            /// <summary>
            /// The funding line that pays for this particular billet.
            /// </summary>
            public string Funding { get; set; }

            /// <summary>
            /// The NEC assigned to this billet.
            /// </summary>
            public string NEC { get; set; }

            /// <summary>
            /// The UIC assigned to this billet.
            /// </summary>
            public string UIC { get; set; }
        
            #endregion

            #region Data Access Methods

            /// <summary>
            /// Inserts the billet into the database and optionally updates the cache.
            /// </summary>
            /// <returns></returns>
            public async Task DBInsert(bool updateCache)
            {
                try
                {
                    using (MySqlConnection connection = new MySqlConnection(Properties.ConnectionString))
                    {
                        await connection.OpenAsync();

                        MySqlCommand command = connection.CreateCommand();
                        command.CommandType = CommandType.Text;
                        command.CommandText = string.Format("INSERT INTO `{0}` (`ID`,`Title`,`IDNumber`,`SuffixCode`,`Remarks`,`Designation`,`Funding`,`NEC`,`UIC`)", _tableName) +
                            " VALUES (@ID, @Title, @IDNumber, @SuffixCode, @Remarks, @Designation, @Funding, @NEC, @UIC)";

                        command.Parameters.AddWithValue("@ID", this.ID);
                        command.Parameters.AddWithValue("@Title", this.Title);
                        command.Parameters.AddWithValue("@IDNumber", this.IDNumber);
                        command.Parameters.AddWithValue("@SuffixCode", this.SuffixCode);
                        command.Parameters.AddWithValue("@Remarks", this.Remarks);
                        command.Parameters.AddWithValue("@Designation", this.Designation);
                        command.Parameters.AddWithValue("@Funding", this.Funding);
                        command.Parameters.AddWithValue("@NEC", this.NEC);
                        command.Parameters.AddWithValue("@UIC", this.UIC);

                        await command.ExecuteNonQueryAsync();

                        if (updateCache)
                        {
                            _billetsCache.AddOrUpdate(this.ID, this, (key, value) =>
                            {
                                throw new Exception("There was an issue adding this billet to the cache");
                            });
                        }
                    }
                }
                catch
                {
                    throw;
                }
            }

            /// <summary>
            /// Updates the current billet instance by resetting all columns to the current instance and uses the ID to index.  The ID itself cannot be updated.
            /// </summary>
            /// <returns></returns>
            public async Task DBUpdate(bool updateCache)
            {
                try
                {
                    using (MySqlConnection connection = new MySqlConnection(Properties.ConnectionString))
                    {
                        await connection.OpenAsync();

                        MySqlCommand command = connection.CreateCommand();
                        command.CommandType = CommandType.Text;
                        command.CommandText = string.Format("UPDATE `{0}` SET `Title` = @Title, `IDNumber` = @IDNumber, `SuffixCode` = @SuffixCode, ", _tableName) + //This thing got stupid long fast :(
                            "`Remarks` = @Remarks, `Designation` = @Designation, `Funding` = @Funding, `NEC` = @NEC, `UIC` = @UIC WHERE `ID` = @ID";

                        command.Parameters.AddWithValue("@Title", this.Title);
                        command.Parameters.AddWithValue("@IDNumber", this.IDNumber);
                        command.Parameters.AddWithValue("@SuffixCode", this.SuffixCode);
                        command.Parameters.AddWithValue("@Remarks", this.Remarks);
                        command.Parameters.AddWithValue("@Designation", this.Designation);
                        command.Parameters.AddWithValue("@Funding", this.Funding);
                        command.Parameters.AddWithValue("@NEC", this.NEC);
                        command.Parameters.AddWithValue("@UIC", this.UIC);
                        
                        command.Parameters.AddWithValue("@ID", this.ID);

                        await command.ExecuteNonQueryAsync();

                        if (updateCache)
                        {
                            if (!_billetsCache.ContainsKey(this.ID))
                                throw new Exception("The cache does not have this billet and so wasn't able to update it.");

                            _billetsCache[this.ID] = this;
                        }

                    }
                }
                catch
                {
                    throw;
                }
            }

            /// <summary>
            /// Deletes the current billet instance from the database by using the current ID as the primary key.
            /// </summary>
            /// <returns></returns>
            public async Task DBDelete(bool updateCache)
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

                        if (updateCache)
                        {
                            Billet temp;
                            if (!_billetsCache.TryRemove(this.ID, out temp))
                                throw new Exception("The cache does not contain this billet and so wasn't able to delete it.");
                        }
                    }
                }
                catch
                {
                    throw;
                }
            }

            /// <summary>
            /// Returns a boolean indicating whether or not the current billet instance exists in the database.  Uses the ID to do this comparison.
            /// <para />
            /// Optionally, checks the cache or searches the database.
            /// </summary>
            /// <param name="useCache"></param>
            /// <returns></returns>
            public async Task<bool> DBExists(bool useCache)
            {
                try
                {
                    //If we're supposed to use the cache, then this is easy
                    if (useCache)
                        return _billetsCache.ContainsKey(this.ID);

                    //If we're supposed to use the database, then let's go do a normal DBExists.
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
        /// Validates a billet and returns a list of errors that indicates which properties had issues.
        /// <para />
        /// Returns null if no errors were found.
        /// </summary>
        /// <param name="emailAddress"></param>
        /// <returns></returns>
        public static async Task<List<string>> ValidateBillet(Billet billet)
        {
            try
            {
                List<string> errors = new List<string>();
                var props = typeof(Billet).GetProperties().ToList();

                foreach (var prop in props)
                {
                    var error = await Billets.ValidateProperty(prop.Name, prop.GetValue(billet));

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
        /// Validates a property of a Billet
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
                                return string.Format("The value, '{0}', was not valid for the ID field of a Billet; it must be a GUID.", value);

                            break;
                        }
                    case "title":
                        {
                            if ((value as string).Length > 25 || string.IsNullOrWhiteSpace(value as string))
                                return string.Format("The value, '{0}', was not valid for the Title field of a Billet; it must be no more than 25 characters and not be blank.", value);

                            break;
                        }
                    case "idnumber":
                        {
                            if ((value as string).Length > 10 || string.IsNullOrWhiteSpace(value as string))
                                return string.Format("The value, '{0}', was not valid for the ID Number field of a Billet; it must be no more than 10 characters and not be blank.", value);

                            break;
                        }
                    case "suffixcode":
                        {
                            if ((value as string).Length != 5)
                                return string.Format("The value, '{0}', was not valid for the Suffix Code field of a Billet; it must be exactly 5 characters.", value);

                            break;
                        }
                    case "remarks":
                        {
                            if ((value as string).Length > 200)
                                return "The value was not valid for the Remarks field of a Billet; it must be no more than 200 characters.";

                            break;
                        }
                    case "designation":
                        {
                            if ((value as string).Length > 10 || string.IsNullOrWhiteSpace(value as string))
                                return string.Format("The value, '{0}', was not valid for the Designation field of a Billet; it must be no more than 10 characters and not be blank.", value);

                            break;
                        }
                    case "funding":
                        {
                            if ((value as string).Length > 20 || string.IsNullOrWhiteSpace(value as string))
                                return string.Format("The value, '{0}', was not valid for the Funding field of a Billet; it must be no more than 20 characters and not be blank.", value);

                            break;
                        }
                    case "nec":
                        {
                            if (!ValidationMethods.AreValidNECs((value as string).CreateList()))
                                return string.Format("The value, '{0}', was not valid for the NEC field of a Billet; it must be a real NEC.", value);

                            break;
                        }
                    case "uic":
                        {
                            if (!ValidationMethods.IsValidUIC(value))
                                return string.Format("The value, '{0}', was not valid for the UIC field of a Billet; it must be a real UIC.", value);

                            break;
                        }
                    default:
                        {
                            throw new NotImplementedException(string.Format("While performing validaton on a Billet, no validation rules were found for the property '{0}'!", propertyName));
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
        /// Loads all billets from the database, optionally updating the cache.
        /// </summary>
        /// <param name="updateCache"></param>
        /// <returns></returns>
        public static async Task<List<Billet>> DBLoadAll(bool updateCache)
        {
            try
            {
                List<Billet> result = new List<Billet>();
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
                                result.Add(new Billet()
                                {
                                    Designation = reader["Designation"] as string,
                                    Funding = reader["Funding"] as string,
                                    ID = reader["ID"] as string,
                                    IDNumber = reader["IDNumber"] as string,
                                    NEC = reader["NEC"] as string,
                                    Remarks = reader["Remarks"] as string,
                                    SuffixCode = reader["SuffixCode"] as string,
                                    Title = reader["Title"] as string,
                                    UIC = reader["UIC"] as string
                                });
                            }
                        }
                    }
                }

                if (updateCache)
                {
                    _billetsCache = new ConcurrentDictionary<string, Billet>(result.Select(x => new KeyValuePair<string, Billet>(x.ID, x)));
                }

                return result;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Loads a single billet from either the cache or the database for the given ID.  Returns null if no billet is found for the given ID.
        /// </summary>
        /// <param name="useCache"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<Billet> DBLoadOne(bool useCache, string id)
        {
            try
            {
                if (useCache)
                {
                    Billet billet;
                    _billetsCache.TryGetValue(id, out billet);
                    return billet;
                }
                else
                {
                    using (MySqlConnection connection = new MySqlConnection(Properties.ConnectionString))
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
                                //I know I've done it elsewhere in the project but I don't think we need to check for duplicate results form the same ID.  
                                //In the database, that's a primary key column which is impossible to duplicate.  
                                await reader.ReadAsync();
                                return new Billet()
                                {
                                    Designation = reader["Designation"] as string,
                                    Funding = reader["Funding"] as string,
                                    ID = reader["ID"] as string,
                                    IDNumber = reader["IDNumber"] as string,
                                    NEC = reader["NEC"] as string,
                                    Remarks = reader["Remarks"] as string,
                                    SuffixCode = reader["SuffixCode"] as string,
                                    Title = reader["Title"] as string,
                                    UIC = reader["UIC"] as string
                                };
                            }
                        }
                    }
                    return null;
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Returns a boolean indicating whether or not the given ID belongs to a billet.
        /// </summary>
        /// <param name="billetID"></param>
        /// <param name="useCache"></param>
        /// <returns></returns>
        public static async Task<bool> DoesBilletIDExist(string billetID, bool useCache)
        {
            try
            {
                //Just use the billet class for this and pass the use cache parameter into it.
                return await new Billet() { ID = billetID }.DBExists(useCache);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Loads the billet from a billet assignment or returns null if it doesn't exist.
        /// </summary>
        /// <param name="billetAssignment"></param>
        /// <returns></returns>
        public static async Task<Billet> DBLoadOneByBilletAssignment(BilletAssignments.BilletAssignment billetAssignment)
        {
            try
            {
                if (billetAssignment == null)
                    return null;

                return await Billets.DBLoadOne(true, billetAssignment.BilletID);
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
        /// Loads all of the billets and returns them to the client.  No authorization is done on this endpoint because anyone should be allowed to see the billets.
        /// <para />
        /// Options: 
        /// <para />
        /// acceptcachedresults : Instructs the service to load the billets newly from the database or to use the billets currently contained within the cache.  Default = true
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<MessageTokens.MessageToken> LoadAllBilletsAsync(MessageTokens.MessageToken token)
        {
            try
            {
                //Go get the accept cached results parameter we may, or may not have been sent.
                bool acceptCachedResults = true;
                if (token.Args.ContainsKey("acceptcachedresults"))
                {
                    if (!Boolean.TryParse(token.Args["acceptcachedresults"] as string, out acceptCachedResults))
                        throw new ServiceException(string.Format("There was an error while trying to cast the value '{0}' which you gave as the 'acceptcachedresults' parameter.", token.Args["acceptcachedresults"] as string), ErrorTypes.Validation);
                }

                //If we aren't allowed to use the cache, then reload the cache and then use the cache.  Smoke and mirrors bitches, smoke and mirrors.
                if (!acceptCachedResults)
                    await Billets.DBLoadAll(true);

                token.Result = _billetsCache.Values.ToList();

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
        /// Given a billet, this method will attempt to update that billet if its ID is found, or insert the billet, changing its ID in the process.
        /// <para />
        /// This method will also alert persons who subscribe to the "Billet Changed" or the "Billet Created" events.
        /// <para />
        /// Returns either success or the new billet's ID.
        /// <para />
        /// Options: 
        /// <para />
        /// billet : the billet to either update or insert
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<MessageTokens.MessageToken> UpdateOrInsertBilletAsync(MessageTokens.MessageToken token)
        {
            try
            {
                //First get the client's permissions.
                List<UnifiedServiceFramework.Authorization.Permissions.PermissionGroup> clientPermissions =
                    UnifiedServiceFramework.Authorization.Permissions.TranslatePermissionGroupIDs(token.Session.PermissionIDs);

                //Get the flattened list of all the permissions.
                List<CustomPermissionTypes> customPerms = UnifiedServiceFramework.Authorization.Permissions.GetUniqueCustomPermissions(clientPermissions).Select(x =>
                {
                    CustomPermissionTypes customPerm;
                    if (!Enum.TryParse(x, out customPerm))
                        throw new Exception(string.Format("An error occurred while trying to parse the custom permission '{0}' into the custom permissions enum.", x));

                    return customPerm;
                }).ToList();

                //Is the user allowed to edit billets in the database?
                if (!(customPerms.Contains(CustomPermissionTypes.Manpower_Admin)))
                    throw new ServiceException("You don't have permission to edit manpower things!", ErrorTypes.Authorization);

                //Make sure we got a billet.
                if (!token.Args.ContainsKey("billet"))
                    throw new ServiceException("In order to update a billet, you must send me a billet... that makes sense right?  Why you no send billet?!", ErrorTypes.Validation);

                //Now we're going to cast the object.
                //TODO: it is unknown if JSON.NET will call the class's setters, so we should test this.  If JSON.NET doesn't call the setters, that could be a serious issue.
                Billet billet = token.Args["billet"].CastJObject<Billet>();

                //Now we need to know if our billet already exists or not.
                if (await billet.DBExists(true))
                {
                    //Since it exists, we need to do an update.  Doing an update is interesting because we need to track all of the changes.  
                    //To do this, we need to know what the value of this bilelt used to be.
                    //To know that we need to go get the billet.
                    Billet oldBillet = await Billets.DBLoadOne(true, billet.ID);

                    //Alright, now we have the old billet.  Let's compare the differences.
                    var variances = billet.DetermineVariances(oldBillet);

                    //Now use the variances to make full on change objects.
                    var changes = variances.Select(x => new Changes.Change()
                    {
                        EditorID = token.Session.PersonID,
                        ID = Guid.NewGuid().ToString(),
                        ObjectID = billet.ID,
                        ObjectName = "Billet",
                        Remarks = "Billet Edited",
                        Time = token.CallTime,
                        Variance = x
                    }).ToList();

                    //Now that we have the variances, let's go ahead and call an update.  If the update works, then we'll also insert our variances.
                    await billet.DBUpdate(true);

                    //Now with the update done, let's insert the changes!  This will return immediately and the insert will occur elsewhere.
                    Changes.DBInsertAll(changes);

                    //TODO: Get all persons's emails who have the "Billet Updated" event and send them an email.

                    //Because the billet already existed, we don't need to give back the ID.  In this case, we'll just say success.
                    token.Result = "Success";
                }
                else //If we got here, then that means the billet doesn't already exist and we need to insert it.
                {
                    //The billet didn't exist because the ID wasn't on a billet. This means that we could have malformed input.  We can't trust the billet's ID, and we're going to make our own.
                    billet.ID = Guid.NewGuid().ToString();

                    //Now we have to insert it
                    await billet.DBInsert(true);

                    //Now we need to create the changes.  To do this, we're going to get the variances between a blank billet and this one.
                    var changes = billet.DetermineVariances(new Billet()).Select(x => new Changes.Change()
                    {
                        EditorID = token.Session.PersonID,
                        ID = Guid.NewGuid().ToString(),
                        ObjectID = billet.ID,
                        ObjectName = "Billet",
                        Remarks = "Billet Created",
                        Time = token.CallTime,
                        Variance = x
                    }).ToList();

                    //Now we need to insert the changes.
                    Changes.DBInsertAll(changes);

                    //Now we need to alert anyone who cares about the "Billet Created" event.
                    //TODO do the above comment, lol

                    //Because we created a billet, our result will be the new billet's ID.
                    token.Result = billet.ID;
                }

                return token;
            }
            catch
            {
                throw;
            }
        }

        public static async Task<MessageTokens.MessageToken> DeleteBilletAsync(MessageTokens.MessageToken token)
        {
            try
            {
                //First get the client's permissions.
                List<UnifiedServiceFramework.Authorization.Permissions.PermissionGroup> clientPermissions =
                    UnifiedServiceFramework.Authorization.Permissions.TranslatePermissionGroupIDs(token.Session.PermissionIDs);

                //Get the flattened list of all the permissions.
                List<CustomPermissionTypes> customPerms = UnifiedServiceFramework.Authorization.Permissions.GetUniqueCustomPermissions(clientPermissions).Select(x =>
                {
                    CustomPermissionTypes customPerm;
                    if (!Enum.TryParse(x, out customPerm))
                        throw new Exception(string.Format("An error occurred while trying to parse the custom permission '{0}' into the custom permissions enum.", x));

                    return customPerm;
                }).ToList();

                //Is the user allowed to edit billets in the database?
                if (!(customPerms.Contains(CustomPermissionTypes.Manpower_Admin) || customPerms.Contains(CustomPermissionTypes.Developer)))
                    throw new ServiceException("You don't have permission to edit manpower things!", ErrorTypes.Authorization);

                //Make sure we got a billet.
                if (!token.Args.ContainsKey("billet"))
                    throw new ServiceException("In order to delete a billet, you must send me a billet... that makes sense right?  Why you no send billet?! (The billet you send really just needs an ID field for this endpoint)", ErrorTypes.Validation);

                //Now we're going to cast the object.
                //TODO: it is unknown if JSON.NET will call the class's setters, so we should test this.  If JSON.NET doesn't call the setters, that could be a serious issue.
                Billet billet = token.Args["billet"].CastJObject<Billet>();

                //Ok this next part is going to be interesting.  
                //We need to find all users that have this billet as their billet, delete the billet from them, send those users an alert email,
                //Then delete the billet, and then send the man power admins an email.  Ugh.
                //TODO: Do the above stuff.

                token.Result = "Success";

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
