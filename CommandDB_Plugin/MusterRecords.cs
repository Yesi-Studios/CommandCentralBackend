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
    /// Provides members that help interact with muster as well as the muster class itself and a cache.  
    /// <para />
    /// I chose not to implement this class as a model and instead to implement it as a cache-based class like MessageTokens in order (to attempt) to speed up the muster's loading process.
    /// <para />
    /// We'll see how it goes. lol.  Fuck it!  Do it live.
    /// </summary>
    /*public static class MusterRecords
    {

        /// <summary>
        /// This local, readonly property is intended to standardize all methods in this class that access the database and allow easy maintenance.
        /// </summary>
        private static readonly string _tableName = "musterrecords";

        /// <summary>
        /// The cache of in-memory, thread-safe message tokens.  Intended to be used by the service during operation.  The cache should be purged of Inactive or Failed Messages
        /// </summary>
        private static ConcurrentDictionary<string, MusterRecord> _musterRecordsCache = new ConcurrentDictionary<string, MusterRecord>();

        /// <summary>
        /// The hour of the day at which the muster will roll over, regardless of its completion status.
        /// </summary>
        private static readonly int _rollOverHour = 16;

        /// <summary>
        /// The hour of the day at which the muster is expected to be completed.
        /// </summary>
        private static readonly int _expectedCompletionHour = 9;

        /// <summary>
        /// The muster date.  It defaults to "today" which is the date that the service starts.
        /// </summary>
        private static DateTime _musterDay = DateTime.Today;

        /// <summary>
        /// Describes a single muster record
        /// </summary>
        public class MusterRecord
        {

            #region Properties

            /// <summary>
            /// This muster records ID
            /// </summary>
            public string ID { get; set; }

            /// <summary>
            /// The ID of the person being mustered by this record.
            /// </summary>
            public string PersonID { get; set; }

            /// <summary>
            /// The person who is inputting this muster.
            /// </summary>
            public string MustererID { get; set; }

            /// <summary>
            /// The time that this muster was inserted into the database.
            /// </summary>
            public DateTime MusterTime { get; set; }

            /// <summary>
            /// The day to which this muster belongs.  This may differ from the muster time in cases where future musters are "predicted" due to leave or some such nonsense.
            /// </summary>
            public DateTime MusterDay { get; set; }

            /// <summary>
            /// The state to set this user as for this muster. This is stuff like Present, Leave, Deployed, etc.  This is powered from the Lists Provider.  
            /// </summary>
            public string MusterStatus { get; set; }

            /// <summary>
            /// The duty status that the user was mustered under at the time of the muster.  Were they a civilian, active duty, reservist, etc.
            /// </summary>
            public string DutyStatus { get; set; }
            
            /// <summary>
            /// The division to which the user belonged at the time of muster.
            /// </summary>
            public string Division { get; set; }

            /// <summary>
            /// The department to which the user belonged at the time of muster.
            /// </summary>
            public string Department { get; set; }

            /// <summary>
            /// The command to which the user belonged at the time of muster.
            /// </summary>
            public string Command { get; set; }

            /// <summary>
            /// The rank the user had when he/she was mustered.
            /// </summary>
            public string Rank { get; set; }

            #endregion

            #region Data Access Methods

            /// <summary>
            /// Inserts the muster record into the database and optionally updates the cache.
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
                        command.CommandText = string.Format("INSERT INTO `{0}` (`ID`,`PersonID`,`MustererID`,`MusterTime`,`MusterDay`,`MusterStatus`,`DutyStatus`,`Division`,`Department`,`Command`,`Rank`)", _tableName) +
                            " VALUES (@ID, @PersonID, @MustererID, @MusterTime, @MusterDay, @MusterStatus, @DutyStatus, @Division, @Department, @Command, @Rank) ";

                        command.Parameters.AddWithValue("@ID", this.ID);
                        command.Parameters.AddWithValue("@PersonID", this.PersonID);
                        command.Parameters.AddWithValue("@MustererID", this.MustererID);
                        command.Parameters.AddWithValue("@MusterTime", this.MusterTime.ToMySqlDateTimeString());
                        command.Parameters.AddWithValue("@MusterDay", this.MusterDay.ToMySqlDateTimeString());
                        command.Parameters.AddWithValue("@MusterStatus", this.MusterStatus);
                        command.Parameters.AddWithValue("@DutyStatus", this.DutyStatus);
                        command.Parameters.AddWithValue("@Division", this.Division);
                        command.Parameters.AddWithValue("@Department", this.Department);
                        command.Parameters.AddWithValue("@Command", this.Command);
                        command.Parameters.AddWithValue("@Rank", this.Rank);

                        await command.ExecuteNonQueryAsync();

                        if (updateCache)
                        {
                            _musterRecordsCache.AddOrUpdate(this.ID, this, (key, value) =>
                            {
                                throw new Exception("There was an issue adding this muster record to the cache");
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
            /// Updates the current muster record instance by resetting all columns to the current instance and uses the ID to index.  The ID itself cannot be updated.
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
                        command.CommandText = string.Format("UPDATE `{0}` SET `PersonID` = @PersonID, `MustererID` = @MustererID, `MusterTime` = @MusterTime, ", _tableName) + //This thing got stupid long fast :(
                            "`MusterDay` = @MusterDay, `MusterStatus` = @MusterStatus, `DutyStatus` = @DutyStatus, `Division` = @Division, `Department` = @Department, `Command` = @Command, `DutyStatus` = @DutyStatus WHERE `ID` = @ID";

                        command.Parameters.AddWithValue("@PersonID", this.PersonID);
                        command.Parameters.AddWithValue("@MustererID", this.MustererID);
                        command.Parameters.AddWithValue("@MusterTime", this.MusterTime.ToMySqlDateTimeString());
                        command.Parameters.AddWithValue("@MusterDay", this.MusterDay.ToMySqlDateTimeString());
                        command.Parameters.AddWithValue("@MusterStatus", this.MusterStatus);
                        command.Parameters.AddWithValue("@DutyStatus", this.DutyStatus);
                        command.Parameters.AddWithValue("@Division", this.Division);
                        command.Parameters.AddWithValue("@Department", this.Department);
                        command.Parameters.AddWithValue("@Command", this.Command);
                        command.Parameters.AddWithValue("@Rank", this.Rank);

                        command.Parameters.AddWithValue("@ID", this.ID);

                        await command.ExecuteNonQueryAsync();

                        if (updateCache)
                        {
                            if (!_musterRecordsCache.ContainsKey(this.ID))
                                throw new Exception("The cache does not have this muster record and so wasn't able to update it.");

                            _musterRecordsCache[this.ID] = this;
                        }

                    }
                }
                catch
                {
                    throw;
                }
            }

            /// <summary>
            /// Deletes the current muster record instance from the database by using the current ID as the primary key.
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
                            MusterRecord temp;
                            if (!_musterRecordsCache.TryRemove(this.ID, out temp))
                                throw new Exception("The cache does not contain this muster record and so wasn't able to delete it.");
                        }
                    }
                }
                catch
                {
                    throw;
                }
            }

            /// <summary>
            /// Returns a boolean indicating whether or not the current muster record instance exists in the database.
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
                        return _musterRecordsCache.ContainsKey(this.ID);

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

            #region Other Methods

            /// <summary>
            /// Runs validation on the current object and throws service exceptions if a field is invalid.  If the call completes with no exception, then the object is valid.
            /// </summary>
            /// <returns></returns>
            public async Task ValidateAndThrow()
            {
                try
                {
                    string template = "The value '{0}' for the '{1}' field was not valid!  If you think this is in error, please contact the development team.";

                    if (!ValidationMethods.IsValidCommand(this.Command))
                        throw new ServiceException(string.Format(template, this.Command, "Command"));
                    
                    if (!ValidationMethods.IsValidDepartment(this.Department))
                        throw new ServiceException(string.Format(template, this.Department, "Department"));

                    if (!ValidationMethods.IsValidDivision(this.Division))
                        throw new ServiceException(string.Format(template, this.Department, "Department"));

                    if (!ValidationMethods.IsValidDutyStatus(this.DutyStatus))
                        throw new ServiceException(string.Format(template, this.DutyStatus, "Duty Status"));

                    if (!ValidationMethods.IsValidGuid(this.ID))
                        throw new ServiceException(string.Format(template, this.ID, "ID"));

                    if (!ValidationMethods.IsValidDateTime(this.MusterDay))
                        throw new ServiceException(string.Format(template, this.MusterDay, "Muster Day"));

                    if (!ValidationMethods.IsValidGuid(this.MustererID) || !(await Persons.DoesPersonIDExist(this.MustererID)))
                        throw new ServiceException(string.Format("The value '{0}' for the 'Musterer ID' field was not valid because it didn't belong to any actual person.", this.MustererID));

                    if (!ValidationMethods.IsValidMusterState(this.MusterStatus))
                        throw new ServiceException(string.Format(template, this.MusterStatus, "Muster Status"));

                    if (!ValidationMethods.IsValidDateTime(this.MusterTime))
                        throw new ServiceException(string.Format(template, this.MusterTime, "Muster Time"));

                    if (!ValidationMethods.IsValidGuid(this.PersonID) || !(await Persons.DoesPersonIDExist(this.PersonID)))
                        throw new ServiceException(string.Format("The value '{0}' for the 'Person ID' field was not valid because it didn't belong to any actual person.", this.PersonID));

                    if (!ValidationMethods.IsValidRank(this.Rank))
                        throw new ServiceException(string.Format(template, this.Rank, "Rank"));

                }
                catch
                {
                    throw;
                }
            }

            #endregion

        }

        #region Statis Data Access Methods

        /// <summary>
        /// Loads all muster records from the database and optionally loads into the cache those muster records after a certain date.
        /// </summary>
        /// <param name="updateCache"></param>
        /// <param name="afterDate"></param>
        /// <returns></returns>
        public static async Task<List<MusterRecord>> DBLoadAll(bool updateCache, DateTime afterDate)
        {
            try
            {
                List<MusterRecord> result = new List<MusterRecord>();
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
                                result.Add(new MusterRecord()
                                {
                                    Command = reader["Command"] as string,
                                    Department = reader["Department"] as string,
                                    Division = reader["Division"] as string,
                                    DutyStatus = reader["DutyStatus"] as string,
                                    ID = reader["ID"] as string,
                                    MusterDay = DateTime.Parse((reader["MusterDay"] as string)),
                                    MustererID = reader["MustererID"] as string,
                                    MusterStatus = reader["MusterStatus"] as string,
                                    MusterTime = DateTime.Parse(reader["MusterTIme"] as string),
                                    PersonID = reader["PersonID"] as string,
                                    Rank = reader["Rank"] as string
                                });
                            }
                        }
                    }
                }

                if (updateCache)
                {
                    _musterRecordsCache = new ConcurrentDictionary<string, MusterRecord>(result
                        .Where(x => x.MusterDay <= afterDate)
                        .Select(x => new KeyValuePair<string, MusterRecord>(x.ID, x)));
                }

                return result;
            }
            catch
            {
                throw;
            }
        }


        /// <summary>
        /// Releases the muster records cache's memory by clearing the cache.
        /// </summary>
        public static void ReleaseCache()
        {
            try
            {
                _musterRecordsCache.Clear();
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
        /// Loads the muster for an optionally given date and optionally uses the cache.
        /// <para />
        /// Options: 
        /// <para />
        /// date : The date for which to load the muster.  Optional.  Default = today.
        /// acceptcachedresults : A boolean that instructs the service to use the cache or not.  Optional.  Default = true
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<MessageTokens.MessageToken> LoadMusterForClientAsync(MessageTokens.MessageToken token)
        {
            try
            {
                
                //Did the client send us a date?  If so, we're going to load the muster for that date.  If the client didn't send as a date, then we're going to load for today.
                DateTime date = DateTime.Today.Date;
                if (token.Args.ContainsKey("date") && ValidationMethods.IsValidDateTime(token.Args["date"]))
                    date = DateTime.Parse(token.Args["Date"] as string);

                //Get the accept cached results that the user may have sent us.  Default to true.
                bool acceptCachedResults = true;
                if (token.Args.ContainsKey("acceptcachedresults") && ValidationMethods.IsValidBoolean(token.Args["acceptcachedresults"]))
                    acceptCachedResults = Boolean.Parse(token.Args["acceptcachedresults"] as string);

                List<MusterRecord> result = new List<MusterRecord>();

                //If the date is not today, we're just going to ask the database for all records from that date and ignore the cache.
                if (date != DateTime.Today.Date)
                {
                    using (MySqlConnection connection = new MySqlConnection(Properties.ConnectionString))
                    {
                        await connection.OpenAsync();

                        MySqlCommand command = connection.CreateCommand();
                        command.CommandType = CommandType.Text;
                        command.CommandText = string.Format("SELECT * FROM `{0}` WHERE `MusterDay` = @MusterDay", date.Date.ToMySqlDateTimeString());

                        using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                        {
                            if (reader.HasRows)
                            {
                                while (await reader.ReadAsync())
                                {
                                    result.Add(new MusterRecord()
                                    {
                                        Command = reader["Command"] as string,
                                        Department = reader["Department"] as string,
                                        Division = reader["Division"] as string,
                                        DutyStatus = reader["DutyStatus"] as string,
                                        ID = reader["ID"] as string,
                                        MusterDay = DateTime.Parse((reader["MusterDay"] as string)),
                                        MustererID = reader["MustererID"] as string,
                                        MusterStatus = reader["MusterStatus"] as string,
                                        MusterTime = DateTime.Parse(reader["MusterTIme"] as string),
                                        PersonID = reader["PersonID"] as string,
                                        Rank = reader["Rank"] as string
                                    });
                                }
                            }
                        }
                    }
                }
                else //The date for which the client wants to load the muster is today.  This means we need to load from the database OR the cache, depending what the client wants.
                {
                    if (acceptCachedResults) //Just use the cache because the cache is today's muster.
                        result = _musterRecordsCache.Values.ToList();
                    else //We need to go load from the database cause the client is greedy
                    {
                        using (MySqlConnection connection = new MySqlConnection(Properties.ConnectionString))
                        {
                            await connection.OpenAsync();

                            MySqlCommand command = connection.CreateCommand();
                            command.CommandType = CommandType.Text;
                            command.CommandText = string.Format("SELECT * FROM `{0}` WHERE `MusterDay` = @MusterDay", DateTime.Today.Date.ToMySqlDateTimeString());

                            using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                            {
                                if (reader.HasRows)
                                {
                                    while (await reader.ReadAsync())
                                    {
                                        result.Add(new MusterRecord()
                                        {
                                            Command = reader["Command"] as string,
                                            Department = reader["Department"] as string,
                                            Division = reader["Division"] as string,
                                            DutyStatus = reader["DutyStatus"] as string,
                                            ID = reader["ID"] as string,
                                            MusterDay = DateTime.Parse((reader["MusterDay"] as string)),
                                            MustererID = reader["MustererID"] as string,
                                            MusterStatus = reader["MusterStatus"] as string,
                                            MusterTime = DateTime.Parse(reader["MusterTIme"] as string),
                                            PersonID = reader["PersonID"] as string,
                                            Rank = reader["Rank"] as string
                                        });
                                    }
                                }
                            }
                        }
                    }
                }

                //Now we're going to loop through the Persons model's unified properties and the Muster Record's proeprties
                //If any of those properties match fields in these results, then we're going to check to see if the client
                //is allowed to return those fields.
                UnifiedServiceFramework.Authorization.Permissions.PermissionGroup.ModelPermission modelPermissions =
                    UnifiedServiceFramework.Authorization.Permissions.GetModelPermissionsForUser(token, Persons.ModelAttribute.Name);
                foreach (PropertyInfo info in typeof(MusterRecord).GetProperties().ToList())
                {
                    if (Persons.UnifiedProperties.ContainsKey(info.Name))
                    {
                        if (!modelPermissions.ReturnableFields.Contains(info.Name))
                        {
                            //If the user isn't allowed to return this field, then we should set all values in this field to "redacted".
                            result.ForEach(x => info.SetValue(x, "REDACTED"));
                        }
                    }
                }


                token.Result = result;

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
        /// Inserts or updates a muster record. Requires the conduct muster permission.
        /// <para />
        /// Options: 
        /// <para />
        /// musterrecord : The muster record to be updated or inserted.  Instead of breaking all the fields apart, I chose, in this method, to try passing the entire object and then box casting it.  
        /// If it doesn't work, I'll need to reassess how to do this.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<MessageTokens.MessageToken> UpdateOrInsertMusterRecordAsync(MessageTokens.MessageToken token)
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

                //Is the user allowed to do muster things?
                if (!(customPerms.Contains(CustomPermissionTypes.Conduct_Muster)))
                    throw new ServiceException("You don't have permission to conduct muster!");

                //Alright, now we need to build the new muster record object.
                if (!token.Args.ContainsKey("musterrecord"))
                    throw new ServiceException("You must a muster record object.  See the documentation for this.");

                //Cast the muster record from the args into an actual muster record
                MusterRecord record = token.Args["musterrecord"] as MusterRecord;
                if (record == null)
                    throw new ServiceException("There was an issue while casting the muster record into its containing object.");

                //Now we need to validate all the fields.
                await record.ValidateAndThrow();

                //Now that we know all the fields are valid, we can go ahead and either update or insert this record.
                if (await record.DBExists(true))
                    await record.DBUpdate(true);
                else //If the muster record doesn't already exist then we shouldn't trust the client to make the ID for us.. we'll make the ID for the client.
                {
                    record.ID = Guid.NewGuid().ToString();
                    await record.DBInsert(true);
                }

                token.Result = record.ID;

                return token;

            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region Cron Operations

        /// <summary>
        /// This operation should be submitted to the Service Framework's Cron Operations Provider.
        /// <para />
        /// Checks the muster and determines if it's been completed, who we're still missing, sends emails, and more.
        /// </summary>
        public static void CheckMuster()
        {
            try
            {
                //First off, we need to know how much time is left in the current muster.  We need to use the roll over hour to figure this out.
                TimeSpan remainingTime = new TimeSpan(0);

                DateTime musterDay = DateTime.Today.Date; //The muster day.

                if (DateTime.Now.Hour >= _rollOverHour) //We're technically in the next day.  So, let's add the remaining time until midnight and then add on the roll over hour.
                {
                    remainingTime.Add(DateTime.Today.AddDays(1.0) - DateTime.Now); // THis will add the time until midnight
                    remainingTime.Add(TimeSpan.FromHours(_rollOverHour)); //And this will add the time in the next day
                    musterDay.AddDays(1); //Since we're technically in the next day, let's add a day.
                }
                else //This means we're in the proper day and we're before the roll over hour.  This is easier.
                {
                    remainingTime.Add(DateTime.Today.AddHours(_rollOverHour).Subtract(DateTime.Now)); //Take today without the time component, add the roll over hours, and then subtract the current time.
                }

                //Cool, now we know how much time we have left and we know the "muster day".  If the static muster day from this class doesn't match what we now think to be the muster day,
                //Then a new day must have rolled over.  In this case, we need to trigger a muster roll over.
                if (musterDay.Date != _musterDay.Date)
                {
                    RollOverMuster();
                    _musterDay = musterDay.Date;
                }
                else //Since a new day hasn't rolled over, we need to send alert emails to all those who haven't mustered, and their chains of command, if the time is within one hour of the muster deadline.
                {
                    //We need to load all users who are eligible for muster and then find out which ones haven't mustered.  Those who haven't mustered, we'll need their email addresses as well as their chain
                    //of command's email address
                    //TODO

                }

            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region Other Methods

        /// <summary>
        /// Rolls over the muster by purging the cache and sending final report emails.
        /// </summary>
        public static void RollOverMuster()
        {
            try
            {
                //These are all the stats we need to collect by the end of this.
                int total, totalMustered, officers, officersMustered, enlisted, enlistedMustered, don, donMustered, reservists, reservistsMustered, contractors,
                    contractorsMustered, pep, pepMustered, td, present, aa, tad, leave, terminalLeave, deployed, siq, ua, other, unaccounted = 0;

                string unaccountedString = "";

                //We need to load all of our users, and then check them against the muster cache.  And build the stats.
                DataTable rawUsers = UnifiedModelHelper.GetFieldsFromSearch(new List<UnifiedProperty>(), new List<KeyValuePair<UnifiedProperty, object>>(),
                    Persons.UnifiedProperties["PersonID"], Persons.ModelAttribute).Result;

                //Now we need to select out only those records that are eligible for muster.
                List<DataRow> allUsers = rawUsers.AsEnumerable().Where(x => x["DutyStatus"] as string != "LOSS").ToList();
                total = allUsers.Count;
                officers = allUsers.Where(x => x["DutyStatus"] as string == "Active" && ((x["Rank"] as string) ?? "").ToLower().Contains("o")).Count();
                enlisted = allUsers.Where(x => x["DutyStatus"] as string == "Active" && !((x["Rank"] as string) ?? "").ToLower().Contains("o")).Count();
                don = allUsers.Where(x => x["DutyStatus"] as string == "Civlian").Count();
                reservists = allUsers.Where(x => x["DutyStatus"] as string == "Reservist").Count();
                contractors = allUsers.Where(x => x["DutyStatus"] as string == "Contractor").Count();
                pep = allUsers.Where(x => x["DutyStatus"] as string == "PEP").Count();

                totalMustered = _musterRecordsCache.Count;
                officersMustered = _musterRecordsCache.Where(x => x.Value.Rank.ToLower().Contains("o")).Count();
                enlistedMustered = _musterRecordsCache.Where(x => !x.Value.Rank.ToLower().Contains("o")).Count();
                donMustered = _musterRecordsCache.Where(x => x.Value.DutyStatus == "Civilian").Count();
                reservistsMustered = _musterRecordsCache.Where(x => x.Value.DutyStatus == "Reservist").Count();
                contractorsMustered = _musterRecordsCache.Where(x => x.Value.DutyStatus == "Contractor").Count();
                pepMustered = _musterRecordsCache.Where(x => x.Value.DutyStatus == "PEP").Count();
                td = _musterRecordsCache.Where(x => x.Value.MusterStatus == "TD").Count();
                present = _musterRecordsCache.Where(x => x.Value.MusterStatus == "Present").Count();
                aa = _musterRecordsCache.Where(x => x.Value.MusterStatus == "AA").Count();
                tad = _musterRecordsCache.Where(x => x.Value.MusterStatus == "TAD").Count();
                leave = _musterRecordsCache.Where(x => x.Value.MusterStatus == "Regular Leave").Count();
                terminalLeave = _musterRecordsCache.Where(x => x.Value.MusterStatus == "Terminal Leave").Count();
                deployed = _musterRecordsCache.Where(x => x.Value.MusterStatus == "Deployed").Count();
                siq = _musterRecordsCache.Where(x => x.Value.MusterStatus == "SIQ").Count();
                ua = _musterRecordsCache.Where(x => x.Value.MusterStatus == "UA").Count();
                other = _musterRecordsCache.Where(x => x.Value.MusterStatus == "Other").Count();

                //Select all those persons that were not accounted for.
                var unaccountedPersonnel = allUsers.Where(x => allUsers.Select(y => y["PersonID"] as string).Except(_musterRecordsCache.Select(y => y.Value.PersonID)).Contains(x["PersonID"] as string))
                    .Select(x => new { Rate = x["Rate"] as string, LastName = x["LastName"] as string, FirstName = x["FirstName"] as string, Division = x["Division"] as string,
                    Department = x["Department"] as string, Command = x["Command"] as string }).ToList();

                unaccounted = unaccountedPersonnel.Count;

                if (unaccountedPersonnel.Count > 0)
                {
                    unaccountedPersonnel.ForEach(x => unaccountedString += string.Format("{0} {1}, {2} | Command {3} | Department {4} | Division {5}\n", 
                        x.Rate, x.LastName, x.FirstName, x.Command, x.Department, x.Division ));
                }

                //We have all of the required information... now we need to know who we're sending the email to.
                //To do this we're going to look for anyone who subscribed to the Final Muster Report event.  Make sure that event exists first.
                if (ChangeEvents.ChangeEventsCache.Where(x => x.Value.Name.SafeEquals("Final Muster Report")).Count() == 0)
                    throw new Exception("There's no event called Final Muster Report!");

                string eventID = ChangeEvents.ChangeEventsCache.First(x => x.Value.Name.SafeEquals("Final Muster Report")).Value.ID;

                //Get all the users that subscribed to this event.
                List<string> emailAddressesTo = UnifiedModelHelper.SearchInManyAsync(new List<string>() { "Final Muster Report" }, new List<UnifiedProperty>() { Persons.UnifiedProperties["ChangeEventSubscriptionIDs"] },
                    new List<UnifiedProperty>()
                    {
                        Persons.UnifiedProperties["PrimaryEmailAddress"]
                    }, new List<KeyValuePair<UnifiedProperty, object>>(), Persons.ModelAttribute, Persons.UnifiedProperties["PersonID"], null, 999999)
                    .Result.AsEnumerable().Select(x => x["PrimaryEmailAddress"] as string).ToList();

                //Now send the email.  The muster day is going to be today.
                EmailHelper.SendFinalMusterReportEmail(emailAddressesTo, DateTime.Today, total, totalMustered, officers, officersMustered, enlisted,
                    enlistedMustered, don, donMustered, reservists, reservistsMustered, contractors, contractorsMustered, pep, pepMustered, td, present, aa, tad, leave, terminalLeave,
                    deployed, siq, ua, other, unaccounted, unaccountedString).Wait();

                //Now that we've sent our email, we can finally purge the cache, in preparation for a new day.
                _musterRecordsCache.Clear();

            }
            catch
            {
                throw;
            }
        }

        #endregion

    }*/
}
