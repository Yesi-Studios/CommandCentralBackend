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
    /// Contains methods and members for dealing with news items, which are the blog like posts on the front page.
    /// </summary>
    public static class NewsItems
    {

        /// <summary>
        /// This local, readonly property is intended to standardize all methods in this class that access the database and allow easy maintenance.
        /// </summary>
        private static readonly string _tableName = "newsitems";

        /// <summary>
        /// Describes a single News Item and its members, including its DB access members.
        /// </summary>
        public class NewsItem
        {

            #region Properties

            /// <summary>
            /// The ID of the news item.
            /// </summary>
            public string ID { get; set; }

            /// <summary>
            /// The ID of the client that created the news item.
            /// </summary>
            public string CreatorID { get; set; }

            /// <summary>
            /// The title of the news item.
            /// </summary>
            public string Title { get; set; }

            /// <summary>
            /// The paragraphs contained in this news item.
            /// </summary>
            public List<string> Paragraphs { get; set; }

            /// <summary>
            /// The time this news item was created.
            /// </summary>
            public DateTime CreationTime { get; set; }

            #endregion

            #region Data Access Methods

            /// <summary>
            /// Inserts the news item into the database.
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
                        command.CommandText = string.Format("INSERT INTO `{0}` (`ID`, `CreatorID`, `Title`, `Paragraphs`, `CreationTime`)", _tableName) +
                            " VALUES (@ID, @CreatorID, @Title, @Paragraphs, @CreationTime)";

                        command.Parameters.AddWithValue("@ID", this.ID);
                        command.Parameters.AddWithValue("@CreatorID", this.CreatorID);
                        command.Parameters.AddWithValue("@Title", this.Title);
                        command.Parameters.AddWithValue("@Paragraphs", this.Paragraphs.Serialize());
                        command.Parameters.AddWithValue("@CreationTime", this.CreationTime.ToMySqlDateTimeString());

                        await command.ExecuteNonQueryAsync();
                    }
                }
                catch
                {
                    throw;
                }
            }

            /// <summary>
            /// Inserts the news item into the database.
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
                        command.CommandText = string.Format("INSERT INTO `{0}` (`ID`, `CreatorID`, `Title`, `Paragraphs`, `CreationTime`)", _tableName) +
                            " VALUES (@ID, @CreatorID, @Title, @Paragraphs, @CreationTime)";

                        command.Parameters.AddWithValue("@ID", this.ID);
                        command.Parameters.AddWithValue("@CreatorID", this.CreatorID);
                        command.Parameters.AddWithValue("@Title", this.Title);
                        command.Parameters.AddWithValue("@Paragraphs", this.Paragraphs.Serialize());
                        command.Parameters.AddWithValue("@CreationTime", this.CreationTime.ToMySqlDateTimeString());

                        await command.ExecuteNonQueryAsync();
                    }

                }
                catch
                {
                    throw;
                }
            }

            /// <summary>
            /// Updates the news item instance by resetting all columns to the current instance and uses the ID to index.  The ID itself cannot be updated.
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
                        command.CommandText = string.Format("UPDATE `{0}` SET `CreatorID` = @CreatorID, `Title` = @Title, `Paragraphs` = @Paragraphs, `CreationTime` = @CreationTime WHERE `ID` = @ID", _tableName);

                        command.Parameters.AddWithValue("@CreatorID", this.CreatorID);
                        command.Parameters.AddWithValue("@Title", this.Title);
                        command.Parameters.AddWithValue("@Paragraphs", this.Paragraphs.Serialize());
                        command.Parameters.AddWithValue("@CreationTime", this.CreationTime.ToMySqlDateTimeString());

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
            /// Updates the news item instance by resetting all columns to the current instance and uses the ID to index.  The ID itself cannot be updated.
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
                        command.CommandText = string.Format("UPDATE `{0}` SET `CreatorID` = @CreatorID, `Title` = @Title, `Paragraphs` = @Paragraphs, `CreationTime` = @CreationTime WHERE `ID` = @ID", _tableName);

                        command.Parameters.AddWithValue("@CreatorID", this.CreatorID);
                        command.Parameters.AddWithValue("@Title", this.Title);
                        command.Parameters.AddWithValue("@Paragraphs", this.Paragraphs.Serialize());
                        command.Parameters.AddWithValue("@CreationTime", this.CreationTime.ToMySqlDateTimeString());

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
            /// Deletes the current news item instance from the database by using the current ID as the primary key.
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
            /// Deletes the current news item instance from the database by using the current ID as the primary key.
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
            /// Returns a boolean indicating whether or not the current news item instance exists in the database.  Uses the ID to do this comparison.
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
        /// Validates a news item and returns a list of errors that indicates which properties had issues.
        /// <para />
        /// Returns null if no errors were found.
        /// </summary>
        /// <param name="emailAddress"></param>
        /// <returns></returns>
        public static async Task<List<string>> ValidateNewsItem(NewsItem item)
        {
            try
            {
                List<string> errors = new List<string>();
                var props = typeof(NewsItem).GetProperties().ToList();

                foreach (var prop in props)
                {
                    var error = await NewsItems.ValidateProperty(prop.Name, prop.GetValue(item));

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
        /// Validates a property of a News Item
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
                                return string.Format("The value, '{0}', was not valid for the ID field of a News Item; it must be a GUID.", value);

                            break;
                        }
                    case "creatorid":
                        {
                            if (!ValidationMethods.IsValidGuid(value) || ! await Persons.DoesPersonIDExist(value as string))
                                return string.Format("The value, '{0}', was not valid for the Creator ID field of a News Item; it must be a GUID and belong to a person.", value);

                            break;
                        }
                    case "title":
                        {
                            if ((value as string).Length > 100 || string.IsNullOrWhiteSpace(value as string))
                                return string.Format("The value, '{0}', was not valid for the Title field of a News Item; it must be no more than 100 characters and not be blank.", value);

                            break;
                        }
                    case "paragraphs":
                        {
                            var list = value as List<string>;

                            if (list == null)
                                throw new Exception("While validating a News Item, the Paragraphs property was in the wrong type or was null.");

                            if (list.Sum(x => x.Count()) > 4096)
                                return "The total number of characters in your News Item may not be greater than 4096 characters.";

                            break;
                        }
                    case "creationtime":
                        {
                            if (!(value is DateTime))
                                throw new Exception("While validating a News Item, the Creation Time property was in the wrong type.");

                            break;
                        }
                    default:
                        {
                            throw new NotImplementedException(string.Format("While performing validaton on a News Item, no validation rules were found for the property '{0}'!", propertyName));
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
        /// Loads all News Items from the database and orders them by the creation date, descending
        /// </summary>
        /// <returns></returns>
        public static async Task<List<NewsItem>> DBLoadAll()
        {
            try
            {
                List<NewsItem> result = new List<NewsItem>();
                using (MySqlConnection connection = new MySqlConnection(Properties.ConnectionString))
                {
                    await connection.OpenAsync();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = string.Format("SELECT * FROM `{0}`", _tableName);

                    using (MySqlDataReader reader = (MySqlDataReader) await command.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            while (await reader.ReadAsync())
                            {
                                result.Add(new NewsItem()
                                {
                                    CreationTime = reader.GetDateTime("CreationTime"),
                                    CreatorID = reader["CreatorID"] as string,
                                    ID = reader["ID"] as string,
                                    Paragraphs = (reader["Paragraphs"] as string).Deserialize<List<string>>(),
                                    Title = reader["Title"] as string
                                });
                            }
                        }
                    }
                }
                return result.OrderByDescending(x => x.CreationTime).ToList();
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Loads a single news item for the given ID and returns null if none is returned.
        /// </summary>
        /// <param name="newsItemID">Guess wtf that is. - McLean, 2016</param>
        /// <returns></returns>
        public static async Task<NewsItem> DBLoadOne(string newsItemID)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(Properties.ConnectionString))
                {
                    await connection.OpenAsync();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = string.Format("SELECT * FROM `{0}` WHERE `ID` = @ID", _tableName);

                    command.Parameters.AddWithValue("@ID", newsItemID);

                    using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            await reader.ReadAsync();

                            return new NewsItem()
                            {
                                CreationTime = reader.GetDateTime("CreationTime"),
                                CreatorID = reader["CreatorID"] as string,
                                ID = reader["ID"] as string,
                                Paragraphs = (reader["Paragraphs"] as string).Deserialize<List<string>>(),
                                Title = reader["Title"] as string
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
        /// Creates a new news item entry in the database, ensuring the client has the right permission.
        /// <para />
        /// The ID, the CreatorID, and the CreationTime will be set for you.
        /// <para />
        /// Options: 
        /// <para />
        /// newsitem : a properly formed news item to insert.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<MessageTokens.MessageToken> CreateNewEntry_Client(MessageTokens.MessageToken token)
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

                //Make sure the client has the required permission
                if (!customPerms.Contains(CustomPermissionTypes.Manage_News))
                    throw new ServiceException("You do not have permission to edit the news!", ErrorTypes.Authorization);

                //Now that the client has permission to manage the news, let's see if they sent us a news object
                //And create it if they did.
                if (!token.Args.ContainsKey("newsitem"))
                    throw new ServiceException("You must send a news item!", ErrorTypes.Validation);
                NewsItem item = token.Args["newsitem"].CastJToken<NewsItem>();

                //Since the client is handing us information to insert, we're going to reset some things.\
                item.ID = Guid.NewGuid().ToString();
                item.CreatorID = token.Session.PersonID;
                item.CreationTime = token.CallTime;

                //Now just insert it!
                await item.DBInsert();

                //Insert all changes made to this item.  Since it hasn't been created, we use a blank news item to compare against.
                Changes.DBInsertAll(item.DetermineVariances(new NewsItem()).Select(x =>
                    new Changes.Change()
                    {
                        EditorID = token.Session.PersonID,
                        ID = Guid.NewGuid().ToString(),
                        ObjectID = item.ID,
                        ObjectName = "NewsItem",
                        Remarks = "News Item Created",
                        Time = token.CallTime,
                        Variance = x
                    }).ToList());

                token.Result = item.ID;

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
        /// Updates a news item after ensuring that it actually exists and the client has permission to manage the news.s
        /// <para />
        /// Options: 
        /// <para />
        /// newsitem : a properly formed news item to update which already exists.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<MessageTokens.MessageToken> UpdateEntry_Client(MessageTokens.MessageToken token)
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

                //Make sure the client has the required permission
                if (!customPerms.Contains(CustomPermissionTypes.Manage_News))
                    throw new ServiceException("You do not have permission to edit the news!", ErrorTypes.Authorization);

                //Now that the client has permission to manage the news, let's see if they sent us a news object
                //And create it if they did.
                if (!token.Args.ContainsKey("newsitem"))
                    throw new ServiceException("You must send a news item!", ErrorTypes.Validation);
                NewsItem item = token.Args["newsitem"].CastJToken<NewsItem>();

                //Now let's make sure that the news item actually exists.
                //We're going to do this by loading the old item as it exists in the database and then we can use the loaded news item for comparison.
                NewsItem oldItem = await NewsItems.DBLoadOne(item.ID);

                //If old item is null, then it doesn't exist.
                if (oldItem == null)
                    throw new ServiceException("The news item does not appear to exist yet!", ErrorTypes.Validation);

                //Now let's build the list of changes.
                var changes = item.DetermineVariances(oldItem).Select(x => new Changes.Change()
                {
                    EditorID = token.Session.PersonID,
                    ID = Guid.NewGuid().ToString(),
                    ObjectID = item.ID,
                    ObjectName = "News Item",
                    Remarks = "News Item Updated",
                    Time = token.CallTime,
                    Variance = x
                }).ToList();

                //Make sure the client isn't trying to update certain fields which are not updateable.
                if (changes.Select(x => x.Variance.PropertyName).ToList().ContainsAny(new[] { "CreationTime", "CreatorID", "ID" }))
                    throw new ServiceException("You attempted to update a field that is not updateable!", ErrorTypes.Validation);

                //Do the update
                await item.DBUpdate();
                
                //And then insert the changes
                Changes.DBInsertAll(changes);

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
        /// Loads all news entries for a client, so long as that client succeeded authentication.  Results are order desc by CreationTime
        /// <para />
        /// Options: 
        /// <para />
        /// none
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<MessageTokens.MessageToken> LoadEntries_Client(MessageTokens.MessageToken token)
        {
            try
            {
                //The authorization for this endpoint is a little different. Here, there are no permissions that are required... we just need to make sure that the session isn't null.
                //Basically, as long as you logged in, you can see the news entries.
                if (token.Session == null)
                    throw new ServiceException("You must be logged in to view the news.", ErrorTypes.Authentication);

                token.Result = await NewsItems.DBLoadAll();

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
        /// Loads a single news entry for the given news item ID and returns null if non exists.
        /// <para />
        /// Options: 
        /// <para />
        /// newsitemid - the ID of the news item to load.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<MessageTokens.MessageToken> LoadEntry_Client(MessageTokens.MessageToken token)
        {
            try
            {
                //The authorization for this endpoint is a little different. Here, there are no permissions that are required... we just need to make sure that the session isn't null.
                //Basically, as long as you logged in, you can see the news entries.
                if (token.Session == null)
                    throw new ServiceException("You must be logged in to view the news.", ErrorTypes.Authentication);

                //Now we're going to get the ID of the entry the client wants to load.
                if (!token.Args.ContainsKey("newsitemid"))
                    throw new ServiceException("You must send a 'newsitemid' parameter.", ErrorTypes.Validation);
                string newsItemID = token.Args["newsitemid"] as string;

                token.Result = await NewsItems.DBLoadOne(newsItemID);

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
        /// Deletes a given news item assuming the client has permission to manage news items.
        /// <para />
        /// Options: 
        /// <para />
        /// newsitemid : The ID of the item the client wants to delete.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<MessageTokens.MessageToken> DeleteEntry_Client(MessageTokens.MessageToken token)
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

                //Make sure the client has the required permission
                if (!customPerms.Contains(CustomPermissionTypes.Manage_News))
                    throw new ServiceException("You do not have permission to edit the news!", ErrorTypes.Authorization);

                //Now that the client has permission to manage the news, let's see if they sent us a news object
                //And create it if they did.
                if (!token.Args.ContainsKey("newsitemid"))
                    throw new ServiceException("You must send a news item's id!", ErrorTypes.Validation);
                NewsItem item = await NewsItems.DBLoadOne(token.Args["newsitemid"] as string);

                //Now let's make sure that the news item actually exists.
                if (item == null)
                    throw new ServiceException("The news item does not appear to exist yet!", ErrorTypes.Validation);

                //Now let's build the list of changes.  Since we're deleting the news item, we're going to compare a blank item (new) to the current item (old)
                var changes = (new NewsItem()).DetermineVariances(item).Select(x => new Changes.Change()
                {
                    EditorID = token.Session.PersonID,
                    ID = Guid.NewGuid().ToString(),
                    ObjectID = item.ID,
                    ObjectName = "News Item",
                    Remarks = "News Item Deleted",
                    Time = token.CallTime,
                    Variance = x
                }).ToList();

                //Now do the delete
                await item.DBDelete();

                //Insert all the changes.
                Changes.DBInsertAll(changes);

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
