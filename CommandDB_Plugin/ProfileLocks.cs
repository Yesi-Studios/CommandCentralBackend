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
    /// Provides members and methods for taking, releasing, and managing profile locks.  These are used to determine who can update a profile.
    /// </summary>
    public static class ProfileLocks
    {

        /// <summary>
        /// This local, readonly property is intended to standardize all methods in this class that access the database and allow easy maintenance.
        /// </summary>
        private static readonly string _tableName = "profilelocks";

        /// <summary>
        /// The cache of in-memory, thread-safe change events.  Intended to be used by the service during operation.
        /// <para />
        /// The key is the ID of the Change Event
        /// </summary>
        private static ConcurrentDictionary<string, ProfileLock> _changeEventsCache = new ConcurrentDictionary<string, ProfileLock>();

        /// <summary>
        /// Instructs the profile locks and the rest of the service as to how long a profile may remain locked by one user before another user may override that lock.
        /// </summary>
        private static readonly TimeSpan _maxProfileLockAge = TimeSpan.FromMinutes(60);

        /// <summary>
        /// Describes a single profile lock and its data access methods.
        /// </summary>
        public class ProfileLock
        {

            #region Properties

            /// <summary>
            /// The unique GUID assigned to this Profile Lock
            /// </summary>
            public string ID { get; set; }

            /// <summary>
            /// The ID of the person who owns this lock.
            /// </summary>
            public string OwnerID { get; set; }

            /// <summary>
            /// The ID of the profile of the person to whom this lock applies.
            /// </summary>
            public string ProfileID { get; set; }

            /// <summary>
            /// The time at which this lock was submitted.
            /// </summary>
            public DateTime SubmitTime { get; set; }

            #endregion

            #region Data Access Methods

            /// <summary>
            /// Inserts the Profile Lock into the database and optionally the cache, effectively locking the profile.
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
                        command.CommandText = string.Format("INSERT INTO `{0}` (`ID`,`OwnerID`,`ProfileID`,`SubmitTime`)", _tableName) +
                            " VALUES (@ID, @OwnerID, @ProfileID, @SubmitTime)";

                        command.Parameters.AddWithValue("@ID", this.ID);
                        command.Parameters.AddWithValue("@OwnerID", this.OwnerID);
                        command.Parameters.AddWithValue("@ProfileID", this.ProfileID);
                        command.Parameters.AddWithValue("@SubmitTime", this.SubmitTime.ToMySqlDateTimeString());

                        await command.ExecuteNonQueryAsync();

                        if (updateCache)
                        {
                            _changeEventsCache.AddOrUpdate(this.ID, this, (key, value) =>
                                {
                                    throw new Exception("There was an issue adding this profile lock to the cache!");
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
            /// Inserts the Profile Lock into the database and optionally the cache, effectively locking the profile.
            /// <para />
            /// This verison uses the transaction that was made by someone else.
            /// </summary>
            /// <returns></returns>
            public async Task DBInsert(MySqlTransaction transaction, bool updateCache)
            {
                try
                {
                    using (MySqlCommand command = new MySqlCommand("", transaction.Connection, transaction))
                    {
                        command.CommandText = string.Format("INSERT INTO `{0}` (`ID`,`OwnerID`,`ProfileID`,`SubmitTime`)", _tableName) +
                            " VALUES (@ID, @OwnerID, @ProfileID, @SubmitTime)";

                        command.Parameters.AddWithValue("@ID", this.ID);
                        command.Parameters.AddWithValue("@OwnerID", this.OwnerID);
                        command.Parameters.AddWithValue("@ProfileID", this.ProfileID);
                        command.Parameters.AddWithValue("@SubmitTime", this.SubmitTime.ToMySqlDateTimeString());

                        await command.ExecuteNonQueryAsync();

                        if (updateCache)
                        {
                            _changeEventsCache.AddOrUpdate(this.ID, this, (key, value) =>
                            {
                                throw new Exception("There was an issue adding this profile lock to the cache!");
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
            /// Updates the current profile lock instance by resetting all columns to the current instance and uses the ID to index.  The ID itself cannot be updated.
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
                        command.CommandText = string.Format("UPDATE `{0}` SET `OwnerID` = @OwnerID, `ProfileID` = @ProfileID, `SubmitTime` = @SubmitTime WHERE `ID` = @ID ", _tableName);

                        command.Parameters.AddWithValue("@ID", this.ID);
                        command.Parameters.AddWithValue("@OwnerID", this.OwnerID);
                        command.Parameters.AddWithValue("@ProfileID", this.ProfileID);
                        command.Parameters.AddWithValue("@SubmitTime", this.SubmitTime.ToMySqlDateTimeString());

                        await command.ExecuteNonQueryAsync();

                        if (updateCache)
                        {
                            if (!_changeEventsCache.ContainsKey(this.ID))
                                throw new Exception("The cache does not have this profile lock and so wasn't able to update it.");

                            _changeEventsCache[this.ID] = this;
                        }
                    }
                }
                catch
                {
                    throw;
                }
            }

            /// <summary>
            /// Updates the current profile lock instance by resetting all columns to the current instance and uses the ID to index.  The ID itself cannot be updated.
            /// <para />
            /// This version uses a transaction that has already been created to do the update.
            /// </summary>
            /// <returns></returns>
            public async Task DBUpdate(MySqlTransaction transaction, bool updateCache)
            {
                try
                {
                    using (MySqlCommand command = new MySqlCommand("", transaction.Connection, transaction))
                    {
                        command.CommandText = string.Format("UPDATE `{0}` SET `OwnerID` = @OwnerID, `ProfileID` = @ProfileID, `SubmitTime` = @SubmitTime WHERE `ID` = @ID ", _tableName);

                        command.Parameters.AddWithValue("@ID", this.ID);
                        command.Parameters.AddWithValue("@OwnerID", this.OwnerID);
                        command.Parameters.AddWithValue("@ProfileID", this.ProfileID);
                        command.Parameters.AddWithValue("@SubmitTime", this.SubmitTime.ToMySqlDateTimeString());

                        await command.ExecuteNonQueryAsync();

                        if (updateCache)
                        {
                            if (!_changeEventsCache.ContainsKey(this.ID))
                                throw new Exception("The cache does not have this profile lock and so wasn't able to update it.");

                            _changeEventsCache[this.ID] = this;
                        }
                    }
                }
                catch
                {
                    throw;
                }
            }

            /// <summary>
            /// Deletes the current profile lock instance from the database by using the current ID as the primary key and optionally removes it from the cache.  This effectively releases the lock.
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
                            ProfileLock temp;
                            if (!_changeEventsCache.TryRemove(this.ID, out temp))
                                throw new Exception("The cache does not contain this profile lock and so wasn't able to delete it.");
                        }

                    }
                }
                catch
                {
                    throw;
                }
            }

            /// <summary>
            /// Deletes the current profile lock instance from the database by using the current ID as the primary key and optionally removes it from the cache.  This effectively releases the lock.
            /// <para />
            /// This version uses an already-made transaction.
            /// </summary>
            /// <returns></returns>
            public async Task DBDelete(MySqlTransaction transaction, bool updateCache)
            {
                try
                {
                    using (MySqlCommand command = new MySqlCommand("", transaction.Connection, transaction))
                    {
                        command.CommandText = string.Format("DELETE FROM `{0}` WHERE `ID` = @ID", _tableName);

                        command.Parameters.AddWithValue("@ID", this.ID);

                        await command.ExecuteNonQueryAsync();

                        if (updateCache)
                        {
                            ProfileLock temp;
                            if (!_changeEventsCache.TryRemove(this.ID, out temp))
                                throw new Exception("The cache does not contain this profile lock and so wasn't able to delete it.");
                        }
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
            public async Task<bool> DBExists(bool useCache)
            {
                try
                {
                    //If we're allowed to use the cache.
                    if (useCache)
                        return _changeEventsCache.ContainsKey(this.ID);


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
        /// Returns the profile lock for a person's ID if the lock exists, or returns null if no profile lock exists.
        /// </summary>
        /// <param name="ownerID">The ID of the profile to check for a lock.</param>
        /// <returns></returns>
        public static async Task<ProfileLock> GetProfileLockByPerson(string personID, bool useCache)
        {
            try
            {
                //If we're allowed to use the cache, then use it.
                if (useCache)
                {
                    ProfileLock temp = _changeEventsCache.FirstOrDefault(x => x.Value.ProfileID == personID).Value;

                    if (temp == null)
                        return null;
                    else
                        return temp;
                }

                //We're not allowed to use the cache, so instead go ask the database directly.
                using (MySqlConnection connection = new MySqlConnection(Properties.ConnectionString))
                {
                    await connection.OpenAsync();

                    using (MySqlCommand command = new MySqlCommand("", connection))
                    {
                        command.CommandText = string.Format("SELECT * FROM `{0}` WHERE `ProfileID` = @ProfileID", _tableName);

                        command.Parameters.AddWithValue("@ProfileID", personID);

                        using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                        {
                            if (reader.HasRows)
                            {
                                DataTable table = new DataTable();
                                table.Load(reader);

                                if (table.Rows.Count > 1)
                                    throw new Exception(string.Format("While checking for locks on the profile whose ID is '{0}', more than one lock was found.", personID));

                                return new ProfileLock()
                                {
                                    ID = table.Rows[0]["ID"] as string,
                                    OwnerID = table.Rows[0]["ID"] as string,
                                    ProfileID = table.Rows[0]["ID"] as string,
                                    SubmitTime = DateTime.Parse(table.Rows[0]["ID"] as string)
                                };
                            }
                            else
                            {
                                return null;
                            }
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
        /// Searches for any profile locks owned by a given person.  Returns null if none are found, and throws an error if there is more than one.
        /// </summary>
        /// <param name="personID">The ID of the profile to check for a lock.</param>
        /// <returns></returns>
        public static async Task<ProfileLock> GetProfileLockByOwner(string ownerID, bool useCache)
        {
            try
            {
                //If we're allowed to use the cache, then use it.
                if (useCache)
                {
                    ProfileLock temp = _changeEventsCache.FirstOrDefault(x => x.Value.OwnerID == ownerID).Value;

                    if (temp == null)
                        return null;
                    else
                        return temp;
                }

                //We're not allowed to use the cache, so instead go ask the database directly.
                using (MySqlConnection connection = new MySqlConnection(Properties.ConnectionString))
                {
                    await connection.OpenAsync();

                    using (MySqlCommand command = new MySqlCommand("", connection))
                    {
                        command.CommandText = string.Format("SELECT * FROM `{0}` WHERE `OwnerID` = @OwnerID", _tableName);

                        command.Parameters.AddWithValue("@OwnerID", ownerID);

                        using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                        {
                            if (reader.HasRows)
                            {
                                DataTable table = new DataTable();
                                table.Load(reader);

                                if (table.Rows.Count > 1)
                                    throw new Exception(string.Format("While checking for locks owned by the user whose ID is '{0}', more than one lock was found.", ownerID));

                                return new ProfileLock()
                                {
                                    ID = table.Rows[0]["ID"] as string,
                                    OwnerID = table.Rows[0]["ID"] as string,
                                    ProfileID = table.Rows[0]["ID"] as string,
                                    SubmitTime = DateTime.Parse(table.Rows[0]["ID"] as string)
                                };
                            }
                            else
                            {
                                return null;
                            }
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
        /// Releases the lock on a profile and optionally updates the cache.  
        /// </summary>
        /// <param name="personID"></param>
        /// <param name="updateCache"></param>
        /// <returns></returns>
        public static async Task ReleaseLock(string personID, bool updateCache)
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
                            //If we're allowed to use the cache, then go get the lock from the cache (making sure there's only one) and then delete it.
                            if (updateCache)
                            {
                                var locks = _changeEventsCache.Where(x => x.Value.ProfileID == personID).Select(x => x.Value).ToList();

                                if (locks.Count == 0)
                                    throw new ServiceException(string.Format("Cannot release the lock for the person whose ID is '{0}' because it is not locked!", personID), ErrorTypes.Validation);
                                else
                                    if (locks.Count > 1)
                                        throw new Exception(string.Format("While attempting to release the locks for the profile whose ID is '{0}', more than one lock was found for it in the cache.", personID));

                                await locks.First().DBDelete(transaction, updateCache);
                            }
                            else //Since we're not allowed to use the cache and there's no other method to do this, we're going to do it ourselves.
                            {
                                using (MySqlCommand command = new MySqlCommand("", connection, transaction))
                                {
                                    command.CommandText = string.Format("DELETE FROM `{0}` WHERE `ProfileID` = @ProfileID", _tableName);

                                    command.Parameters.AddWithValue("@ProfileID", personID);

                                    if (await command.ExecuteNonQueryAsync() > 1)
                                        throw new Exception(string.Format("While attempting to release the locks for the profile whose ID is '{0}', more than one lock was found for it.", personID));
                                }
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
        /// Updates a cache's submit time to DateTime.Now for a given person ID and optionally updates the cache.
        /// </summary>
        /// <param name="personID"></param>
        /// <param name="updateCache"></param>
        /// <returns></returns>
        public static async Task RefreshLock(string personID, bool updateCache)
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
                            //If we're allowed to use the cache, then go get the lock from the cache (making sure there's only one) and then update it.
                            if (updateCache)
                            {
                                var locks = _changeEventsCache.Where(x => x.Value.ProfileID == personID).Select(x => x.Value).ToList();

                                if (locks.Count == 0)
                                    throw new ServiceException(string.Format("Cannot refresh the lock for the person whose ID is '{0}' because that profile is not locked.", personID), ErrorTypes.Validation);
                                else
                                    if (locks.Count > 1)
                                        throw new Exception(string.Format("While attempting to refresh the locks for the profile whose ID is '{0}', more than one lock was found for it in the cache.", personID));

                                var profileLock = locks.First();
                                profileLock.SubmitTime = DateTime.Now;
                                await profileLock.DBUpdate(transaction, updateCache);
                            }
                            else //Since we're not allowed to use the cache and there's no other method to do this, we're going to do it ourselves.
                            {
                                using (MySqlCommand command = new MySqlCommand("", connection, transaction))
                                {
                                    command.CommandText = string.Format("UPDATE `{0}` SET `SubmitTime` = @SubmitTime WHERE `ProfileID` = @ProfileID", _tableName);

                                    command.Parameters.AddWithValue("@SubmitTime", DateTime.Now.ToMySqlDateTimeString());
                                    command.Parameters.AddWithValue("@ProfileID", personID);

                                    if (await command.ExecuteNonQueryAsync() != 1)
                                        throw new Exception(string.Format("While attempting to refresh the profile lock for the profile whose ID is '{0}', more than or less than 1 lock was updated.  Changes were rolled back.", personID));
                                }
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

        #endregion

        #region Client Access Methods

        /// <summary>
        /// WARNING!  THIS IS A CLIENT METHOD.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Given a person ID, gets the profile lock on the given person's profile.  Returns null if no locks are present.
        /// <para />
        /// Options: 
        /// <para />
        /// personid : the person for whom to check for locks and return the lock owner.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<MessageTokens.MessageToken> GetProfileLock_Client(MessageTokens.MessageToken token)
        {
            try
            {
                if (token.Session == null)
                    throw new ServiceException("You must be logged in to interact with this endpoint!", ErrorTypes.Authentication);

                if (!token.Args.ContainsKey("personid"))
                    throw new ServiceException("You must send a personid parameter.", ErrorTypes.Validation);
                string personID = token.Args["personid"] as string;

                if (!ValidationMethods.IsValidGuid(personID))
                    throw new ServiceException(string.Format("The value, '{0}', was not in the proper GUID format.", personID), ErrorTypes.Validation);

                token.Result = await ProfileLocks.GetProfileLockByPerson(personID, true);

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
        /// Given a person ID, attempts to take a lock on the profile in question.  This method will force a lock to release that is expired, and release the client's other lock, if any.
        /// <para />
        /// This method will either return the ID of the person who owns the valid lock in an exception of type "LockOwned" or will return "Success" if the client is able to take the lock.
        /// <para />
        /// Options: 
        /// <para />
        /// personid : the person for whom to check for locks and return the lock owner.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<MessageTokens.MessageToken> TakeProfileLock_Client(MessageTokens.MessageToken token)
        {
            try
            {
                //Make sure we're signed in.  The authentication path should've taken care of this but whatever.
                if (token.Session == null)
                    throw new ServiceException("You must be logged in to interact with this endpoint!", ErrorTypes.Authentication);

                //First get the client's permissions.
                var clientPermissions = UnifiedServiceFramework.Authorization.Permissions.TranslatePermissionGroupIDs(token.Session.PermissionIDs);
                
                //Now get the client's model permissions.
                var modelPermission = UnifiedServiceFramework.Authorization.Permissions.GetModelPermissionsForUser(token, "Person");

                //Get the flattened list of all the custom permissions.
                List<SpecialPermissionTypes> customPerms = UnifiedServiceFramework.Authorization.Permissions.GetUniqueCustomPermissions(clientPermissions).Select(x =>
                {
                    SpecialPermissionTypes customPerm;
                    if (!Enum.TryParse(x, out customPerm))
                        throw new Exception(string.Format("An error occurred while trying to parse the custom permission '{0}' into the custom permissions enum.", x));

                    return customPerm;
                }).ToList();

                //Is the client allowed to update users?  If not, they don't need a lock.
                if (!customPerms.Contains(SpecialPermissionTypes.Edit_Users))
                    throw new ServiceException("You can not take a lock on this user because you can not edit users.", ErrorTypes.LockImpossible);

                //Is the client allowed to update at least one field on a profile?  If not, they don't need a lock.
                if (!modelPermission.EditableFields.Any())
                    throw new ServiceException("You can not take a lock on this user because you can not edit any fields on a profile.", ErrorTypes.LockImpossible);

                //Now we need the ID of the person the client wants to take a lock on
                if (!token.Args.ContainsKey("personid"))
                    throw new ServiceException("You must send a personid to tell the service on who the client wants to take a lock.", ErrorTypes.Validation);
                string personID = token.Args["personid"] as string;

                //Let's see if this looks like a person ID before asks the database if it exists.
                if (!ValidationMethods.IsValidGuid(personID))
                    throw new ServiceException(string.Format("The value, '{0}', was not in a valid GUID format!", personID), ErrorTypes.Validation);

                //Let's see if this person actually exists.
                if (!await Persons.DoesPersonIDExist(personID))
                    throw new ServiceException(string.Format("The person ID '{0}' was invalid - it doesn't exist.", personID), ErrorTypes.Validation);

                //Is the client in the chain of command of the person?  If not, the client doesn't need a lock cause you can't edit someone outside your chain.
                if (token.Session.PersonID != personID && !await CustomAuthorization.CustomPermissions.IsClientInChainOfCommandOfPerson(clientPermissions, token.Session.PersonID, personID))
                    throw new ServiceException("You can not take a lock on this user because you are not in his/her chain of command.", ErrorTypes.LockImpossible);

                //Ok now that we know that this person really does exist and all the permissions shit is good, let's see if the person is already locked by another client.
                var personLock = await ProfileLocks.GetProfileLockByPerson(personID, true);

                //If someone has a lock then check to see if it's expired, if it's not, then throw a LockOwned error with the ID of the owner.
                //Also, make sure the owner isn't the current user.  If it is, it's going to get cleaned up later on.
                if (personLock != null && personLock.OwnerID != token.Session.PersonID)
                {
                    if (DateTime.Now.Subtract(personLock.SubmitTime) > _maxProfileLockAge)
                        await personLock.DBDelete(true);
                    else
                        throw new ServiceException(string.Format("You can not take a lock on this profile, because a lock is already owned by '{0}'.", await Persons.TranslatePersonIDToFriendlyName(personLock.OwnerID)), ErrorTypes.LockOwned);
                }

                //Ok, now let's see if the client has a lock he/she has already taken out on another profile.
                var clientLock = await ProfileLocks.GetProfileLockByOwner(token.Session.PersonID, true);

                //If there is a client lock - meaning the client has a lock on another profile - then we're going to delete that lock cause we're about to give the
                //client a lock on this profile.  That other profile may be this profile.
                if (clientLock != null)
                    await clientLock.DBDelete(true);

                //Ok now we know that neither the client nor the user has a lock/is locked.
                //Now we're going to lock the user's profile.
                await new ProfileLocks.ProfileLock()
                {
                    ID = Guid.NewGuid().ToString(),
                    OwnerID = token.Session.PersonID,
                    ProfileID = personID,
                    SubmitTime = token.CallTime
                }.DBInsert(true);

                //Now that the profile is locked, just return success!
                token.Result = "Success";

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
        /// Releases any lock owned by the client.
        /// <para />
        /// Returns "Success" whether or not a lock gets released.
        /// <para />
        /// Options: 
        /// <para />
        /// None
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<MessageTokens.MessageToken> ReleaseClientOwnedProfileLock_Client(MessageTokens.MessageToken token)
        {
            try
            {
                //Releasing profile locks by owner is really easy - only the owner can release their own locks.  Therefore, we don't even need any 
                //parameters from the client.  Just release any lock owned by the client.
                var profileLock = await ProfileLocks.GetProfileLockByOwner(token.Session.PersonID, true);

                if (profileLock != null)
                    await profileLock.DBDelete(true);

                token.Result = "Success";

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
        /// Releases the lock on a profile if the client is its owner or if the lock has expired.  If neither is true, than an authorization error is thrown.
        /// <para />
        /// Returns "Success" if the lock on the given profile is released.
        /// <para />
        /// Options: 
        /// <para />
        /// personid : the ID of the person for whom to attempt to release a lock.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<MessageTokens.MessageToken> ReleaseProfileLockByPerson_Client(MessageTokens.MessageToken token)
        {
            try
            {
                //We're going to ask the client what profile they want to release the profile lock for, make sure a lock exists and that the client owns it, and then we're going to release it.
                if (!token.Args.ContainsKey("personid"))
                    throw new ServiceException("You must send the personid of the person you want to release the lock for.", ErrorTypes.Validation);
                string personID = token.Args["personid"] as string;

                //Make sure it could be a person id.
                if (!ValidationMethods.IsValidGuid(personID))
                    throw new ServiceException(string.Format("The value, '{0}', was not a valid GUID.", personID), ErrorTypes.Validation);

                //Go get the lock on this profile.
                var profileLock = await ProfileLocks.GetProfileLockByPerson(personID, true);

                //Make sure there actually is a profile lock.
                if (profileLock == null)
                    throw new ServiceException(string.Format("No profile lock exists for the profile with the ID, '{0}'.", personID), ErrorTypes.Validation);

                //Now that we know we have a profile lock, make sure the client is actually allowed to release it.
                //The client can only release a lock if they own the lock or the lock has expired.
                if (profileLock.OwnerID == token.Session.PersonID)
                    await profileLock.DBDelete(true);
                else
                    if (DateTime.Now.Subtract(profileLock.SubmitTime) > _maxProfileLockAge)
                        await profileLock.DBDelete(true);
                    else
                        throw new ServiceException(string.Format("You are not allowed to release the lock for the profile with the ID '{0}'.  It is neither expired nor are you its owner.", personID), ErrorTypes.Authorization);

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
