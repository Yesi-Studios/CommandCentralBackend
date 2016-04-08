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

namespace UnifiedServiceFramework.Authorization
{
    /// <summary>
    /// Provides methods for interacting with permissions including a permission class, the permissions cache, data access methods, etc.
    /// </summary>
    public static class Permissions
    {
        

        /// <summary>
        /// This local, readonly property is intended to standardize all methods in this class that access the database and allow easy maintenance.
        /// </summary>
        private static readonly string _tableName = "permissiongroups";

        /// <summary>
        /// The cache of in-memory, thread-safe permissions.  Intended to be used by the service during operation.
        /// </summary>
        private static ConcurrentDictionary<string, PermissionGroup> _permissionsCache = new ConcurrentDictionary<string, PermissionGroup>();

        /// <summary>
        /// Gets the Permissions Cache.
        /// </summary>
        public static ConcurrentDictionary<string, PermissionGroup> PermissionsCache
        {
            get
            {
                return _permissionsCache;
            }
        }

        /// <summary>
        /// Contains the list of all models and their fields.  These are intended to inform the permissions providers as to what permissions they should expose.
        /// </summary>
        public static ConcurrentDictionary<string, ConcurrentBag<string>> ModelsAndFields = new ConcurrentDictionary<string, ConcurrentBag<string>>();

        /// <summary>
        /// Describes a single permission group and provides methods for DB interaction.
        /// </summary>
        public class PermissionGroup
        {

            #region Properties

            /// <summary>
            /// The ID of this permission group.  This should not change after original creation.
            /// </summary>
            public string ID { get; set; }
            /// <summary>
            /// The name of this permission group.
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// The list of sub-permissions that describe what rights this permission group grants to what model.
            /// </summary>
            public List<ModelPermission> ModelPermissions { get; set; }
            /// <summary>
            /// Additional list of permissions.  Intended to describe access to other parts of the application as defined by the consumer.
            /// </summary>
            public List<string> CustomPermissions { get; set; }
            /// <summary>
            /// A list of those permissions groups' IDs that are subordiante to this permission group.  This is used to determine which groups can promote people into which groups.
            /// </summary>
            public List<string> SubordinatePermissionGroupIDs { get; set; }

            #endregion

            #region Data Access Methods

            /// <summary>
            /// Inserts the current permission instance into the database.
            /// </summary>
            /// <returns></returns>
            public async Task DBInsert(bool updateCache)
            {
                try
                {
                    using (MySqlConnection connection = new MySqlConnection(Settings.ConnectionString))
                    {
                        await connection.OpenAsync();

                        MySqlCommand command = connection.CreateCommand();
                        command.CommandType = CommandType.Text;
                        command.CommandText = string.Format("INSERT INTO `{0}` (`ID`,`Name`,`ModelPermissions`,`CustomPermissions`,`SubordinatePermissionGroupIDs`) VALUES (@ID, @Name, @ModelPermissions, @CustomPermissions, @SubordinatePermissionGroupIDs)", _tableName);

                        command.Parameters.AddWithValue("@ID", this.ID);
                        command.Parameters.AddWithValue("@Name", this.Name);
                        command.Parameters.AddWithValue("@ModelPermissions", this.ModelPermissions.Serialize());
                        command.Parameters.AddWithValue("@CustomPermissions", this.CustomPermissions.Serialize());
                        command.Parameters.AddWithValue("@SubordinatePermissionGroupIDs", this.SubordinatePermissionGroupIDs.Serialize());


                        await command.ExecuteNonQueryAsync();

                        if (updateCache)
                        {
                            _permissionsCache.AddOrUpdate(this.ID, this, (key, value) =>
                            {
                                throw new Exception("There was an issue adding this permission to the cache");
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
            /// Updates the current permission instance by resetting all columns to the current instance and uses the ID to index.  The ID itself cannot be updated.
            /// </summary>
            /// <returns></returns>
            public async Task DBUpdate(bool updateCache)
            {
                try
                {
                    using (MySqlConnection connection = new MySqlConnection(Settings.ConnectionString))
                    {
                        await connection.OpenAsync();

                        MySqlCommand command = connection.CreateCommand();
                        command.CommandType = CommandType.Text;
                        command.CommandText = string.Format("UPDATE `{0}` SET `Name` = @Name, `ModelPermissions` = @ModelPermissions, `CustomPermissions` = @CustomPermissions, `SubordinatePermissionGroupIDs` = @SubordinatePermissionGroupIDs WHERE `ID` = @ID", _tableName); ;

                        command.Parameters.AddWithValue("@Name", this.Name);
                        command.Parameters.AddWithValue("@ModelPermissions", this.ModelPermissions.Serialize());
                        command.Parameters.AddWithValue("@CustomPermissions", this.CustomPermissions.Serialize());
                        command.Parameters.AddWithValue("@SubordinatePermissionGroupIDs", this.SubordinatePermissionGroupIDs.Serialize());
                        command.Parameters.AddWithValue("@ID", this.ID);

                        await command.ExecuteNonQueryAsync();

                        if (updateCache)
                        {
                            if (!_permissionsCache.ContainsKey(this.ID))
                                throw new Exception("The cache does not have this permission and so wasn't able to update it.");

                            _permissionsCache[this.ID] = this;
                        }

                    }
                }
                catch
                {
                    throw;
                }
            }

            /// <summary>
            /// Deletes the current permission instance from the database by using the current ID as the primary key.
            /// </summary>
            /// <returns></returns>
            public async Task DBDelete(bool updateCache)
            {
                try
                {
                    using (MySqlConnection connection = new MySqlConnection(Settings.ConnectionString))
                    {
                        await connection.OpenAsync();

                        MySqlCommand command = connection.CreateCommand();
                        command.CommandType = CommandType.Text;
                        command.CommandText = string.Format("DELETE FROM `{0}` WHERE `ID` = @ID", _tableName);

                        command.Parameters.AddWithValue("@ID", this.ID);

                        await command.ExecuteNonQueryAsync();

                        if (updateCache)
                        {
                            PermissionGroup temp;
                            if (!_permissionsCache.TryRemove(this.ID, out temp))
                                throw new Exception("The cache does not contain this permission and so wasn't able to delete it.");
                        }
                    }
                }
                catch
                {
                    throw;
                }
            }

            /// <summary>
            /// Returns a boolean indicating if the current instance exists in the database.  This is done by searching for the ID.
            /// </summary>
            /// <returns></returns>
            public async Task<bool> DBExists()
            {
                try
                {
                    using (MySqlConnection connection = new MySqlConnection(Settings.ConnectionString))
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

            #region Overrides

            /// <summary>
            /// Returns the name of the current permission.
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return Name;
            }

            /// <summary>
            /// Performs a deep equals against another object
            /// </summary>
            /// <param name="obj"></param>
            /// <returns></returns>
            public override bool Equals(object obj)
            {
                PermissionGroup other = obj as PermissionGroup;

                if (other == null)
                    return false;

                if (!this.CustomPermissions.ContainsOnlyAll(other.CustomPermissions) ||
                    this.ID != other.ID ||
                    !this.ModelPermissions.ContainsOnlyAll(other.ModelPermissions) ||
                    this.Name != other.Name ||
                    !this.SubordinatePermissionGroupIDs.ContainsOnlyAll(other.SubordinatePermissionGroupIDs))
                    return false;

                return true;
            }

            /// <summary>
            /// Gets a hashcode built from the hashcodes of all the other properties.
            /// </summary>
            /// <returns></returns>
            public override int GetHashCode()
            {
                return this.CustomPermissions.GetHashCode() ^
                    this.ID.GetHashCode() ^
                    this.ModelPermissions.GetHashCode() ^
                    this.Name.GetHashCode() ^
                    this.SubordinatePermissionGroupIDs.GetHashCode();
            }

            #endregion

            #region ctors

            /// <summary>
            /// Instantiates a new permission group and sets all lists to an empty list of that type.
            /// </summary>
            public PermissionGroup()
            {
                ModelPermissions = new List<ModelPermission>();
                CustomPermissions = new List<string>();
                SubordinatePermissionGroupIDs = new List<string>();
            }

            #endregion

            /// <summary>
            /// Describes permissions to a given model.
            /// </summary>
            public class ModelPermission
            {

                #region Properties

                /// <summary>
                /// The name of the model.
                /// </summary>
                public string ModelName { get; set; }
                /// <summary>
                /// The fields the user can search in in the model.
                /// </summary>
                public List<string> SearchableFields { get; set; }
                /// <summary>
                /// The fields a user is allowed to see from the model.
                /// </summary>
                public List<string> ReturnableFields { get; set; }
                /// <summary>
                /// The fields a user is allowed to edit in a model.
                /// </summary>
                public List<string> EditableFields { get; set; }

                #endregion

                #region ctors

                /// <summary>
                /// Creates a new instance of the model permission and sets all the internal lists to empty.
                /// </summary>
                public ModelPermission()
                {
                    SearchableFields = new List<string>();
                    ReturnableFields = new List<string>();
                    EditableFields = new List<string>();
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
                    ModelPermission other = obj as ModelPermission;

                    if (other == null)
                        return false;

                    if (!this.EditableFields.ContainsOnlyAll(other.EditableFields) ||
                        this.ModelName != other.ModelName ||
                        !this.ReturnableFields.ContainsOnlyAll(other.ReturnableFields) ||
                        !this.SearchableFields.ContainsOnlyAll(other.SearchableFields))
                        return false;
                    
                    return true;
                }

                /// <summary>
                /// Gets the hashcode for this object, built from all the other properties.
                /// </summary>
                /// <returns></returns>
                public override int GetHashCode()
                {
                    return this.EditableFields.GetHashCode() ^
                        this.ModelName.GetHashCode() ^
                        this.ReturnableFields.GetHashCode() ^
                        this.SearchableFields.GetHashCode();
                }

                #endregion


            }

        }


        #region Static Data Access Methods

        /// <summary>
        /// Loads all permissions from the database and optionally reset the cache with those results.
        /// </summary>
        /// <returns></returns>
        public static async Task<List<PermissionGroup>> DBLoadAll(bool updateCache)
        {
            try
            {
                List<PermissionGroup> result = new List<PermissionGroup>();
                using (MySqlConnection connection = new MySqlConnection(Settings.ConnectionString))
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
                                result.Add(new PermissionGroup()
                                {
                                    ID = reader["ID"].ToString(),
                                    Name = reader["Name"].ToString(),
                                    CustomPermissions = reader["CustomPermissions"].ToString().Deserialize<List<string>>(),
                                    ModelPermissions = reader["ModelPermissions"].ToString().Deserialize<List<PermissionGroup.ModelPermission>>(),
                                    SubordinatePermissionGroupIDs = reader["SubordinatePermissionGroupIDs"].ToString().Deserialize<List<string>>()
                                });
                            }
                        }
                    }
                }

                if (updateCache)
                {
                    _permissionsCache = new ConcurrentDictionary<string, PermissionGroup>(result.Select(x => new KeyValuePair<string, PermissionGroup>(x.ID, x)));
                }

                return result;
            }
            catch
            {
                throw;
            }
        }

        

        /// <summary>
        /// Converts permission group IDs into permission groups using the cache.
        /// </summary>
        /// <param name="permissionGroupIDs"></param>
        /// <returns></returns>
        public static List<PermissionGroup> TranslatePermissionGroupIDs(List<string> permissionGroupIDs)
        {
            try
            {
                if (permissionGroupIDs == null)
                    return new List<PermissionGroup>();

                return permissionGroupIDs.Select(x =>
                    {
                        return _permissionsCache.FirstOrDefault(y => y.Key.SafeEquals(x)).Value;
                    }).ToList();
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Returns a model permission object containing the sum total of the user's permissions to a model, which are the model permissions from all 
        /// the permission groups the user is a part of flattened into one model permission.
        /// <para />
        /// If the model name doesn't match a model, then we return null.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="modelName"></param>
        /// <returns></returns>
        public static PermissionGroup.ModelPermission GetModelPermissionsForUser(MessageTokens.MessageToken token, string modelName)
        {
            try
            {
                /*
                 *
                 * Here, I loop through all of the permissions and pick out anywhere this model is mentioned.
                 * I do this because it's possible for more than one permission that the user has will have something
                 * to say about this model.  And we want to return the TOTAL amount of fields the user is allowed to 
                 * use from their permissions.
                 * 
                 * During this entire process, if nothing allows the user to search or return or whatever, we should just return an empty list.
                 *
                 */
                List<PermissionGroup.ModelPermission> modelPermissions = new List<PermissionGroup.ModelPermission>();
                (Permissions.TranslatePermissionGroupIDs(token.Session.PermissionIDs)).ForEach(x => 
                    {
                        x.ModelPermissions.ForEach(y =>
                            {
                                if (y.ModelName.SafeEquals(modelName))
                                {
                                    modelPermissions.Add(y);
                                }
                            });
                    });

                //If there are no model permissions after that thing above, then return null cause the model name didn't exist.
                if (!modelPermissions.Any())
                    return null;

                PermissionGroup.ModelPermission modelPermission = new PermissionGroup.ModelPermission();

                //Go through all of the model permissions and add the searchable/returnable/Editable fields, checking along the way that we don't add a duplicate.
                modelPermissions.ForEach(x =>
                    {

                        x.SearchableFields.ForEach(y =>
                            {
                                if (!modelPermission.SearchableFields.Contains(y))
                                    modelPermission.SearchableFields.Add(y);
                            });

                        x.ReturnableFields.ForEach(y =>
                            {
                                if (!modelPermission.ReturnableFields.Contains(y))
                                    modelPermission.ReturnableFields.Add(y);
                            });

                        x.EditableFields.ForEach(y =>
                            {
                                if (!modelPermission.EditableFields.Contains(y))
                                    modelPermission.EditableFields.Add(y);
                            });

                    });

                //When we return the model permission, let's go ahead and refer to the model the way the caller did, meaning that the case may be incorrect.
                return modelPermission;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Returns a boolean indicating whether or not the client of the session in the token can grant a given permission group.  
        /// <para />
        /// This is powered by the sub permission group IDs list.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="permissionGroupID"></param>
        /// <returns></returns>
        public static bool CanClientGrantPermissionGroup(MessageTokens.MessageToken token, string permissionGroupID)
        {
            try
            {
                List<Permissions.PermissionGroup> clientPermGroups = TranslatePermissionGroupIDs(token.Session.PermissionIDs);

                //Select all of the sub permission groups and then ask if they contain the perm group in question.
                if (clientPermGroups.Select(x => x.SubordinatePermissionGroupIDs).SelectMany(x => x).Contains(permissionGroupID))
                    return true;

                return false;
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
        /// No Authorization is required for this method.  If the user is authentic, then the user is allowed to ask what permissions he/she has for a given model. Returns the value that GetModelPermissionForUser returns.
        /// <para />
        /// Options: 
        /// <para />
        /// model : Instructs the service which model to return permissions for.  If the model isn't in the parameters an error is thrown.  If the model isn't valid, a blank list is returned.  Not case senstive.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<MessageTokens.MessageToken> LoadModelPermission_Client(MessageTokens.MessageToken token)
        {
            try
            {

                if (!token.Args.ContainsKey("model"))
                    throw new ServiceException("You must send a model parameter to instruct the service which model permissions you want.", ErrorTypes.Validation);

                token.Result = GetModelPermissionsForUser(token, token.Args["model"].ToString());

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
        /// No Authorization is required for this method.  If the user is authentic, then the user is allowed to ask what permissions he/she has.
        /// <para />
        /// Options: 
        /// <para />
        /// None.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<MessageTokens.MessageToken> LoadPermissionGroups_Client(MessageTokens.MessageToken token)
        {
            try
            {
                token.Result = TranslatePermissionGroupIDs(token.Session.PermissionIDs);

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
        /// No Authorization is required for this method.  Returns all permission groups.
        /// <para />
        /// Options: 
        /// <para />
        /// None.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<MessageTokens.MessageToken> LoadAllPermissionGroups_Client(MessageTokens.MessageToken token)
        {
            try
            {
                bool acceptCachedResults = true;
                if (token.Args.ContainsKey("acceptcachedresults"))
                    acceptCachedResults = Convert.ToBoolean(token.Args["acceptcachedresults"] as string);

                if (acceptCachedResults)
                    token.Result = _permissionsCache.Values.ToList();
                else
                {
                    token.Result = await Permissions.DBLoadAll(false);
                }

                return token;
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region Other Methods

        /// <summary>
        /// Returns a list containing all permission groups from the cache.  This is basically just an accessor.
        /// </summary>
        /// <returns></returns>
        public static List<PermissionGroup> GetAllPermissionGroups()
        {
            try
            {
                return _permissionsCache.Values.ToList();
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Returns a list containing all unique custom permissions in a list of permission groups.
        /// </summary>
        /// <param name="perms"></param>
        /// <returns></returns>
        public static List<string> GetUniqueCustomPermissions(List<PermissionGroup> perms)
        {
            try
            {
                return perms.SelectMany(x => x.CustomPermissions).Distinct().ToList();
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Releases the permissions cache's memory by clearing the cache.
        /// </summary>
        public static void ReleaseCache()
        {
            try
            {
                _permissionsCache.Clear();
            }
            catch
            {
                throw;
            }
        }

        #endregion

    }
}
