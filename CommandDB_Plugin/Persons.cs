using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using MySql.Data.MySqlClient;
using UnifiedServiceFramework.Framework;
using AtwoodUtils;

namespace CommandCentral
{

    #region Static Data Access Methods

    /// <summary>
    /// Creates a new person record in all of the persons tables, leaving all fields blank exceot the div/dep/command tree.  Returns the new person's ID.
    /// </summary>
    /// <returns></returns>
    public static async Task<string> DBCreateNew(string division, string department, string cmd)
        {
            try
            {
                //Create the new person.
                string  newID = Guid.NewGuid().ToString();
                using (MySqlConnection connection = new MySqlConnection(Properties.ConnectionString))
                {
                    await connection.OpenAsync();

                    using (MySqlTransaction transaction = await connection.BeginTransactionAsync())
                    {
                        try
                        {

                            using (MySqlCommand command = new MySqlCommand("", connection, transaction))
                            {
                                command.Parameters.AddWithValue("@ID", newID);

                                foreach (string table in _tableNames)
                                {
                                    command.CommandText = string.Format("INSERT INTO `{0}` (`ID`) VALUES (@ID)", table);
                                    await command.ExecuteNonQueryAsync();
                                }

                                //Now that the person is created in all of these tables, we're going to update the division, department, and command.
                                command.Parameters.Clear();
                                command.CommandText = "UPDATE `persons_main` SET `Division` = @Division, `Department` = @Department, `Command` = @Command WHERE `ID` = @ID";

                                command.Parameters.AddWithValue("@Division", division);
                                command.Parameters.AddWithValue("@Department", department);
                                command.Parameters.AddWithValue("@Command", cmd);

                                command.Parameters.AddWithValue("@ID", newID);

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
                return newID;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Updates the persons tables by using a list of variances to determine the fields to be updated and the new values.  
        /// <para />
        /// This method does not do AVA.
        /// <para />
        /// Returns the number of edited rows.
        /// </summary>
        /// <param name="variances"></param>
        /// <param name="personID"></param>
        /// <returns></returns>
        public static async Task<int> DBUpdate(List<Variance> variances, string personID)
        {
            try
            {
                //Make sure we didn't get any duplicate fields cause that'll fuck things up
                if (variances
                    .GroupBy(x => x.PropertyName) //Group the variances by their property name
                    .Select(x => x.Count()).ToList() //Select a list of integers, which represent the number in each group
                    .Exists(x => x > 1)) //Do any of the groups have more than one member?  If so, we have duplicates
                {
                    throw new Exception("An error occurred during Persons.DBUpdate.  There were at least two variances with the same Property Name.");
                }

                //Let's also do a little validation and make sure we're not updating the ID of the person.  We don't allow that.
                if (variances.Exists(x => x.PropertyName.Equals("ID")))
                    throw new Exception("A request was made to Persons.DBUpdate to change a person's ID.  This is not allowed.");

                //At this point, we need to select out the properties that need to be updated in a seperate table from one of the persons tables.
                //These special properties are defined in a private static method in this class.  We're going to handle these recursive updates later.
                var specialVariances = variances.Where(x => _specialProperties.Contains(x.PropertyName)).ToList();
                variances = variances.Except(specialVariances).ToList();

                //Ok now we know that we have unique property updates. Let's go find out what tables we're going to update
                //Here we need to go through all of the fields that we want to update and find out what table they are in and then group the properties by that.
                var propsByTable = typeof(Person).GetProperties() //Get the person class's properties
                    .Where(x => variances.Exists(y => y.PropertyName.Equals(x.Name)) && !_specialProperties.Contains(x.Name)) //Get only those properties that are also in our variances and those properties that aren't in a relational table.
                    .Select(x => new //Find out what table our property came from.  
                    {
                        Table = UnifiedServiceFramework.Validation.SchemaValidation.DatabaseSchema.First(y => y.Key.Contains("persons") && y.Value.Contains(x.Name)).Key,
                        Field = x.Name
                    })
                    .GroupBy(x => x.Table); //Group all the properties by the table they came from

                //If we don't have as many selected properties as we have variances, then there's an issue
                if (propsByTable.SelectMany(x => x.ToList()).Count() != variances.Count)
                {
                    throw new Exception(string.Format("During an update request in Persons.DBUpdate, {0} variances were expected to be updated; however, only {1} properties matched those variances.", propsByTable.SelectMany(x => x.ToList()).Count(), variances.Count));
                }

                //We're going to use this to track how many rows we update.  The expected number of rows updated will be the number of tables there are.  Because we're updating one row in each.
                int rowsUpdated = 0;

                using (MySqlConnection connection = new MySqlConnection(Properties.ConnectionString))
                {
                    await connection.OpenAsync();

                    using (MySqlTransaction transaction = await connection.BeginTransactionAsync())
                    {
                        try
                        {

                            //Now that we know what tables we are going to update, let's loop through the tables and update the properties!
                            foreach (var group in propsByTable)
                            {
                                using (MySqlCommand command = new MySqlCommand("", connection, transaction) { CommandType = CommandType.Text })
                                {
                                    //Go through all the properties and add all of them along with the values.
                                    string sets = "";


                                    var groupIterator = group.ToList();
                                    for (int x = 0; x < groupIterator.Count; x++)
                                    {
                                        if (x != 0)
                                            sets += ",";

                                        sets += string.Format("`{0}` = @{0} ", groupIterator[x].Field);
                                        if (groupIterator[x].Field.SafeEquals("necs"))
                                        {
                                            //HACK: NECs aren't recursive fields so we store them here.  SHoot me, this should be done better.
                                            command.Parameters.AddWithValue(string.Format("@{0}", groupIterator[x].Field), variances.First(y => y.PropertyName.Equals(groupIterator[x].Field)).NewValue.Serialize());
                                        }
                                        else
                                        {
                                            //THis is the generic handling for a field update.
                                            command.Parameters.AddWithValue(string.Format("@{0}", groupIterator[x].Field), variances.First(y => y.PropertyName.Equals(groupIterator[x].Field)).NewValue);

                                        }
                                    }

                                    //Now build the command string
                                    command.CommandText = string.Format("UPDATE `{0}` SET {1} WHERE `ID` = @ID", group.Key, sets);

                                    //Add the ID parameter
                                    command.Parameters.AddWithValue("@ID", personID);

                                    //And finally, run the command
                                    rowsUpdated += await command.ExecuteNonQueryAsync();
                                }
                            }

                            //Ok, before we commit this change let's make sure we updated as many rows as we expected to.
                            //This number should be the same number as the number of tables, because we're updating one row in each table.
                            //Throwing this exception will send us to the catch block with its rollback call.
                            if (rowsUpdated != propsByTable.Count())
                                throw new Exception(string.Format("During Persons.DBUpdate, we expected to update `{0}` tables; however, we updated {1} instead.", propsByTable.Count(), rowsUpdated));

                            //Finally, we're going to update the special properties.  To do this, we're going to pass those properties's update methods
                            //The current transaction.  If any issue occurs in any of those updates, we'll come back here and rollback the transaction.
                            //During this stuff, we'll need to find each property, and then determine which object changed/is new.
                            if (specialVariances.Any())
                            {
                                foreach (var specialVariance in specialVariances)
                                {
                                    switch (specialVariance.PropertyName)
                                    {
                                        case "EmailAddresses":
                                            {
                                                //We're going to see if any email addresses were added or updated.
                                                foreach (var emailAddress in specialVariance.NewValue as IList<EmailAddresses.EmailAddress>)
                                                {
                                                    //If the old does not have an email address that matches this email address, then we need to do something.
                                                    if (!(specialVariance.OldValue as List<EmailAddresses.EmailAddress>).Exists(x => x.Equals(emailAddress)))
                                                    {
                                                        //Ok, so the old list doens't contain this email address.  Now let's ask if the old list has an email address with this email address's ID.
                                                        //If it does contain this ID, then we just need to update this email address.  If it doesn't exist, then we need to insert it.
                                                        if ((specialVariance.OldValue as List<EmailAddresses.EmailAddress>).Exists(x => x.ID == emailAddress.ID))
                                                            await emailAddress.DBUpdate(transaction);
                                                        else
                                                        {
                                                            emailAddress.ID = Guid.NewGuid().ToString();
                                                            emailAddress.OwnerID = personID;
                                                            await emailAddress.DBInsert(transaction);
                                                        }
                                                    }
                                                }

                                                //Now we need to see if any email addresses were deleted.
                                                foreach (var emailAddress in specialVariance.OldValue as IEnumerable<EmailAddresses.EmailAddress>)
                                                {
                                                    //If the old list has a value that the new list doesn't, then we need to delete that email address
                                                    if (!(specialVariance.NewValue as List<EmailAddresses.EmailAddress>).Exists(x => x.ID == emailAddress.ID))
                                                        await emailAddress.DBDelete(transaction);
                                                }

                                                break;
                                            }
                                        case "PhoneNumbers":
                                            {

                                                foreach (var phoneNumber in specialVariance.NewValue as IEnumerable<PhoneNumbers.PhoneNumber>)
                                                {
                                                    if (!(specialVariance.OldValue as List<PhoneNumbers.PhoneNumber>).Exists(x => x.Equals(phoneNumber)))
                                                    {
                                                        if ((specialVariance.OldValue as List<PhoneNumbers.PhoneNumber>).Exists(x => x.ID == phoneNumber.ID))
                                                            await phoneNumber.DBUpdate(transaction);
                                                        else
                                                        {
                                                            phoneNumber.ID = Guid.NewGuid().ToString();
                                                            phoneNumber.OwnerID = personID;
                                                            await phoneNumber.DBInsert(transaction);
                                                        }
                                                    }
                                                }

                                                foreach (var phoneNumber in specialVariance.OldValue as IEnumerable<PhoneNumbers.PhoneNumber>)
                                                {
                                                    if (!(specialVariance.NewValue as List<PhoneNumbers.PhoneNumber>).Exists(x => x.ID == phoneNumber.ID))
                                                        await phoneNumber.DBDelete(transaction);
                                                }

                                                break;
                                            }
                                        case "PhysicalAddresses":
                                            {
                                                foreach (var physicalAddress in specialVariance.NewValue as IEnumerable<PhysicalAddresses.PhysicalAddress>)
                                                {
                                                    if (!(specialVariance.OldValue as List<PhysicalAddresses.PhysicalAddress>).Exists(x => x.Equals(physicalAddress)))
                                                    {
                                                        if ((specialVariance.OldValue as List<PhysicalAddresses.PhysicalAddress>).Exists(x => x.ID == physicalAddress.ID))
                                                            await physicalAddress.DBUpdate(transaction);
                                                        else
                                                        {
                                                            physicalAddress.ID = Guid.NewGuid().ToString();
                                                            physicalAddress.OwnerID = personID;
                                                            await physicalAddress.DBInsert(transaction);
                                                        }
                                                    }
                                                }

                                                foreach (var physicalAddress in specialVariance.OldValue as IEnumerable<PhysicalAddresses.PhysicalAddress>)
                                                {
                                                    if (!(specialVariance.NewValue as List<PhysicalAddresses.PhysicalAddress>).Exists(x => x.ID == physicalAddress.ID))
                                                        await physicalAddress.DBDelete(transaction);
                                                }

                                                break;
                                            }
                                        case "PermissionGroups":
                                            {
                                                await CustomAuthorization.CustomPermissions.SetUserPermissionGroups(
                                                    personID,
                                                    (specialVariance.NewValue as List<UnifiedServiceFramework.Authorization.Permissions.PermissionGroup>).Select(x => x.ID).ToList(),
                                                    transaction);

                                                break;
                                            }
                                        case "SubscribedChangeEvents":
                                            {
                                                /*foreach (var changeEventSubscription in specialVariance.NewValue as IEnumerable<ChangeEventSubscriptions.ChangeEventSubscription>)
                                                {
                                                    //If the change event subscription is not in the old list, then we need to insert it.
                                                    if (!(specialVariance.OldValue as List<ChangeEventSubscriptions.ChangeEventSubscription>).Exists(x => x.Equals(changeEventSubscription)))
                                                        await changeEventSubscription.DBInsert(transaction);
                                                }

                                                foreach (var changeEventSubscription in specialVariance.OldValue as IEnumerable<ChangeEventSubscriptions.ChangeEventSubscription>)
                                                {
                                                    if (!(specialVariance.NewValue as List<ChangeEventSubscriptions.ChangeEventSubscription>).Exists(x => x.Equals(changeEventSubscription)))
                                                        await changeEventSubscription.DBDelete(transaction);
                                                }*/

                                                break;
                                            }
                                        case "Billet":
                                            {
                                                //First off, we're just going to delete all billet assignments for this person
                                                await BilletAssignments.DBDeleteAllByPersonID(personID, transaction);

                                                //Now we're going to assign the person to the billet.  We're going to just use the ID from this billet.
                                                string billetID = (specialVariance.NewValue as Billets.Billet).ID;

                                                //Now insert the new billet assignment
                                                await new BilletAssignments.BilletAssignment()
                                                {
                                                    BilletID = billetID,
                                                    ID = Guid.NewGuid().ToString(),
                                                    PersonID = personID
                                                }.DBInsert(transaction);

                                                break;
                                            }
                                        case "AccountHistory":
                                            {
                                                throw new ServiceException("Account History may not be explicitlty updated through this endpoint.", ErrorTypes.Authorization);
                                            }
                                        default:
                                            {
                                                throw new NotImplementedException(string.Format("While handling special properties in the Persons.DBUpdate method, no special handling was declared for the property '{0}'!", specialVariance.PropertyName));
                                            }
                                    }
                                }
                            }

                            transaction.Commit();

                            return rowsUpdated;
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
        /// Loads a single Person record from the persons tables for the given ID.  Returns null if no records are found.
        /// </summary>
        /// <param name="personID"></param>
        /// <returns></returns>
        public static async Task<Person> DBLoadOne(string personID)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(Properties.ConnectionString))
                {
                    await connection.OpenAsync();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = string.Format("SELECT * FROM {0} WHERE `ID` = @ID", Utilities.BuildJoinStatement(_tableNames, "ID"));

                    command.Parameters.AddWithValue("@ID", personID);

                    using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            await reader.ReadAsync();

                            Person person = new Person();

                            person.AccountHistory = await AccountHistoryEvents.DBLoadByPerson(personID, 5);
                            person.Billet = await Billets.DBLoadOneByBilletAssignment((await BilletAssignments.DBLoadOneByPerson(personID)));
                            person.SubscribedChangeEvents = await ChangeEvents.DBLoadAllByChangeEventSubscriptions(await ChangeEventSubscriptions.DBLoadAllByPerson(personID), true);
                            person.Command = reader["Command"] as string;
                            person.ContactRemarks = reader["ContactRemarks"] as string;
                            person.DateOfArrival = (reader.IsDBNull(reader.GetOrdinal("DateOfArrival")) || string.IsNullOrWhiteSpace(reader["DateOfArrival"] as string)) ? null : new Nullable<DateTime>(DateTime.Parse(reader["DateOfArrival"] as string));
                            person.DateOfBirth = (reader.IsDBNull(reader.GetOrdinal("DateOfBirth")) || string.IsNullOrWhiteSpace(reader["DateOfBirth"] as string)) ? null : new Nullable<DateTime>(DateTime.Parse(reader["DateOfBirth"] as string));
                            person.DateOfDeparture = (reader.IsDBNull(reader.GetOrdinal("DateOfDeparture")) || string.IsNullOrWhiteSpace(reader["DateOfDeparture"] as string)) ? null : new Nullable<DateTime>(DateTime.Parse(reader["DateOfDeparture"] as string));
                            person.Department = reader["Department"] as string;
                            person.Division = reader["Division"] as string;
                            person.DutyStatus = reader["DutyStatus"] as string;
                            person.EAOS = (reader.IsDBNull(reader.GetOrdinal("EAOS")) || string.IsNullOrWhiteSpace(reader["EAOS"] as string)) ? null : new Nullable<DateTime>(DateTime.Parse(reader["EAOS"] as string));
                            person.EmailAddresses = await EmailAddresses.DBLoadAll(personID);
                            person.EmergencyContactInstructions = reader["EmergencyContactInstructions"] as string;
                            person.Ethnicity = reader["Ethnicity"] as string;
                            person.FirstName = reader["FirstName"] as string;
                            person.Gender = reader["Gender"] as string;
                            person.ID = personID;
                            person.IsClaimed = (reader["IsClaimed"] as string == "0" || reader["IsClaimed"] as string == null) ? false : true;
                            person.JobTitle = reader["JobTitle"] as string;
                            person.LastName = reader["LastName"] as string;
                            person.MiddleName = reader["MiddleName"] as string;
                            person.NECs = (reader["NECs"] as string).DeserializeOrDefault<List<string>>();
                            person.PermissionGroups = await CustomAuthorization.CustomPermissions.GetPermissionGroupsForUser(personID);
                            person.PhoneNumbers = await PhoneNumbers.DBLoadAll(personID);
                            person.PhysicalAddresses = await PhysicalAddresses.DBLoadAll(personID);
                            person.Rank = reader["Rank"] as string;
                            person.Rate = reader["Rate"] as string;
                            person.ReligiousPreference = reader["ReligiousPreference"] as string;
                            person.Remarks = reader["Remarks"] as string;
                            person.Shift = reader["Shift"] as string;
                            person.SSN = reader["SSN"] as string;
                            person.Suffix = reader["Suffix"] as string;
                            person.Supervisor = reader["Supervisor"] as string;
                            person.UIC = reader["UIC"] as string;
                            person.WorkCenter = reader["WorkCenter"] as string;
                            person.WorkRemarks = reader["WorkRemarks"] as string;
                            person.WorkRoom = reader["WorkRoom"] as string;

                            return person;
                        }
                        else
                            return null;
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Given a search term, this method will search across all the given search fields for at least one of the searh terms.  
        /// <para />
        /// Returns an empty collection if there are no results.
        /// </summary>
        /// <param name="searchTerm"></param>
        /// <param name="searchFields"></param>
        /// <param name="returnFields"></param>
        /// <param name="orderByField"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public static async Task<List<Dictionary<string, object>>> DBSimpleSearch(string searchTerm, List<string> searchFields, List<string> returnFields, string orderByField, int? limit)
        {
            try
            {
                var results = new List<Dictionary<string, object>>();

                using (MySqlConnection connection = new MySqlConnection(Properties.ConnectionString))
                {
                    await connection.OpenAsync();

                    using (MySqlCommand command = new MySqlCommand("", connection))
                    {
                        //Put together all the fields we want to return.
                        string returnFieldsString = "";
                        if (returnFields == null || returnFields.Count == 0)
                            returnFieldsString = "*";
                        else
                        {
                            //Whether or not we got asked for the ID in the return fields, we're going to add it anyways.
                            if (!returnFields.Contains("ID"))
                                returnFields.Add("ID");

                            for (int x = 0; x < returnFields.Count; x++)
                            {
                                if (x != 0)
                                    returnFieldsString += ",";

                                returnFieldsString += string.Format("`{0}`", returnFields[x]);
                            }
                        }

                        //Join all the tables together.
                        string fromString = Utilities.BuildJoinStatement(_tableNames, "ID");

                        //Build the where clause from the search terms/search fields.
                        string whereString = "";
                        if (!string.IsNullOrWhiteSpace(searchTerm) && searchFields != null && searchFields.Count > 0)
                        {
                            whereString += "WHERE ";

                            var searchTerms = searchTerm.Split((char[])null);

                            for (int x = 0; x < searchTerms.Count(); x++)
                            {
                                whereString += "(";
                                for (int y = 0; y < searchFields.Count; y++)
                                {
                                    whereString += string.Format("`{0}` LIKE @{0}{1}", searchFields[y], x);
                                    command.Parameters.AddWithValue(string.Format("@{0}{1}", searchFields[y], x), string.Format("%{0}%", searchTerms[x]));

                                    if (y + 1 != searchFields.Count)
                                        whereString += " OR ";
                                    else
                                        whereString += ")";
                                }

                                if (x + 1 != searchTerms.Count())
                                    whereString += " AND ";
                            }
                        }

                        //Build the order by string and make sure that the order by field is in the return fields.
                        string orderbyString = "";
                        if (orderByField != null)
                        {
                            if (!returnFields.Contains(orderByField))
                                throw new ArgumentException(string.Format("In order to order by the field '{0}', you must also request that it be returned.", orderByField));

                            orderbyString = string.Format("order by `{0}` desc", orderByField);
                        }

                        //Build the limit string, taking into account the chance that the value is null
                        string limitString = "";
                        if (limit.HasValue)
                        {
                            if (limit.Value <= 0)
                                throw new ArgumentException(string.Format("The limit '{0}' was an invalid value.", limit.Value));
                            else
                                limitString = string.Format("LIMIT {0}", limit.Value);
                        }

                        //Put all the puzzle pieces together.
                        command.CommandText = string.Format("SELECT {0} FROM {1} {2} {3} {4}", returnFieldsString, fromString, whereString, orderbyString, limitString);


                        //We're funally ready to run the query... it's going to be fun
                        using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                        {
                            if (reader.HasRows)
                            {
                                DataTable table = new DataTable();
                                table.Load(reader);

                                var cols = table.Columns.Cast<DataColumn>().ToList();

                                //Cast the datable into a list of dictionaries that represents each search result.
                                results = table.AsEnumerable().Select(x =>
                                    {
                                        Dictionary<string, object> values = new Dictionary<string, object>();

                                        cols.ForEach(y =>
                                            {
                                                values.Add(y.ColumnName, x[y]);
                                            });

                                        return values;
                                    }).ToList(); 
                            }
                        }
                    }
                }
                return results;

            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Searches in the given fields with a key/value pair search and returns the requested fields.
        /// </summary>
        /// <param name="filters"></param>
        /// <param name="returnFields"></param>
        /// <param name="orderByField"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public static async Task<List<Dictionary<string, object>>> DBAdvancedSearch(Dictionary<string, object> filters, List<string> returnFields, int? limit)
        {
            try
            {
                var results = new List<Dictionary<string, object>>();

                //Validate the limit quick.  That's easy, right?
                if (limit.HasValue && limit.Value <= 0)
                    throw new ServiceException(string.Format("The value, '{0}', was not valid for the limit.  It must be greater than zero, or null.", limit.Value), ErrorTypes.Validation);

                //We need to know which fields to handle specially and which to handle normally.
                //These are the fields the client wants returned.
                var specialReturnFields = returnFields.Where(x => _specialProperties.Contains(x)).ToList();
                var normalReturnFields = returnFields.Except(specialReturnFields).ToList();

                //We want to ensure that the client is returning 'ID' from the inner search of the persons object, so we're going to add that.
                if (!normalReturnFields.Contains("ID"))
                    normalReturnFields.Add("ID");

                //Now we need to know which search fields are special and which search fields we can search in the persons tables.
                var specialFilters = filters.Where(x => _specialProperties.Contains(x.Key)).ToList();
                var normalFilters = filters.Except(specialFilters).ToList();

                //Ok, now we know which fields to search in and which fields to return.
                //Let's go ahead and build our query.
                using (MySqlConnection connection = new MySqlConnection(Properties.ConnectionString))
                {
                    await connection.OpenAsync();

                    using (MySqlCommand command = new MySqlCommand("", connection))
                    {
                        //First, we're going to put together the inner select which will handle our query in the persons model.
                        //We know that the normal return fields has at least 'ID' in it, because we made sure it was in there.
                        string innerReturnClause = "";
                        for (int x = 0; x < normalReturnFields.Count; x++)
                        {
                            if (x != 0)
                                innerReturnClause += ",";

                            innerReturnClause += string.Format("`{0}`", normalReturnFields[x]);
                        }

                        //Now we need the where clause, if any, for the inner model.
                        string whereClause = "";
                        if (normalFilters.Any())
                        {
                            whereClause += "WHERE ";

                            for (int x = 0; x < normalFilters.Count; x++)
                            {
                                if (x != 0)
                                    whereClause += " AND ";

                                whereClause += string.Format("`{0}` LIKE @{0}{1}", normalFilters[x].Key, x);
                                command.Parameters.AddWithValue(string.Format("@{0}{1}", normalFilters[x].Key, x), string.Format("%{0}%",  normalFilters[x].Value));
                            }
                        }

                        //Now, we're going to build the inner clause that will do our search across the person model.
                        string innerClause = string.Format("SELECT {0} FROM {1} {2}", innerReturnClause, Utilities.BuildJoinStatement(_tableNames, "ID"), whereClause);

                        //Let's find out if there are any special return or search fields.  If there's not, we can just stop here and run a normal search.
                        if (!specialFilters.Any() && !specialReturnFields.Any())
                        {
                            //We're going to submit this query.
                            if (limit.HasValue)
                                command.CommandText = string.Format("{0} LIMIT {1}", limit.Value);
                            else
                                command.CommandText = innerClause;
                        }
                        else //There are special fields, so we're oging to build them around this inner clause.
                        {

                            #region Added Tables


                            //Ok, cool, we now have built the inner clause.  Now we need to find out if we have special search parameters, and if we do, build the joins around the inner clause.
                            //This variable is what we're going to call the result set form the inner select
                            string innerResultName = "combined";

                            //First, we're going to build the outer return clause, which is all of the inner return fields plus any of the special fields
                            string outerReturnClause = string.Format("`{0}`.*", innerResultName);
                            //And now add the special fields return, which we don't know if we got any.
                            if (specialReturnFields.Any())
                            {
                                for (int x = 0; x < specialReturnFields.Count; x++)
                                {
                                    outerReturnClause += ",";

                                    switch (specialReturnFields[x])
                                    {
                                        case "EmailAddresses":
                                            {
                                                outerReturnClause += string.Format("`{0}`.`Address` AS `EmailAddresses`", EmailAddresses.TableName);

                                                break;
                                            }
                                        case "PhoneNumbers":
                                            {
                                                outerReturnClause += string.Format("`{0}`.`Number` AS `PhoneNumbers`", PhoneNumbers.TableName);

                                                break;
                                            }
                                        case "PhysicalAddresses":
                                            {
                                                outerReturnClause += string.Format("CONCAT_WS(' ',`{0}`.`StreetNumber`,`{0}`.`Route`,`{0}`.`City`,`{0}`.`State`,`{0}`.`Country`,`{0}`.`ZipCode`) AS `PhysicalAddresses`", PhysicalAddresses.TableName);

                                                break;
                                            }
                                        case "PermissionGroups":
                                            {
                                                throw new NotImplementedException("A search was attempted against the permission groups field in a person search.  This is not yet implemented.");
                                            }
                                        case "SubscribedChangeEvents":
                                            {
                                                throw new NotImplementedException("A search was attempted against the change event subscriptions field in a person search.  This is not yet implemented.");
                                            }
                                        case "Billet":
                                            {
                                                throw new NotImplementedException("A search was attempted against the billet field in a person search.  This is not yet implemented.");
                                            }
                                        case "AccountHistory":
                                            {
                                                throw new NotImplementedException("A search was attempted against the account history field in a person search.  This is not yet implemented.");
                                            }
                                        default:
                                            {
                                                throw new NotImplementedException(string.Format("In the Persons.AdvancedSearch special return fields, outer return fields switch, following has no handling instructions: {0}", specialReturnFields[x]));
                                            }
                                    }
                                }
                            }

                            //Alright, now we're finally done with the fucking outer return clause.  Now we need to add the joins for all of our special fields.
                            //For now, we're going to do this in a switch, even though each one is the same, because some of them aren't implemented yet.  
                            //We need to add any tables from which we want to search or return fields.
                            string additionalTablesClause = "";
                            var specialTables = specialReturnFields.Concat(specialFilters.Select(x => x.Key)).Distinct().ToList();

                            //Now we have the list of fields both search and return.  Now let's add the tables we need.
                            for (int x = 0; x < specialTables.Count; x++)
                            {
                                if (x != 0)
                                    additionalTablesClause += " ";

                                switch (specialTables[x])
                                {
                                    case "EmailAddresses":
                                        {
                                            additionalTablesClause += string.Format("LEFT OUTER JOIN `{0}` ON `{1}`.`ID` = `{0}`.`OwnerID` AND `{0}`.`IsPreferred`", EmailAddresses.TableName, innerResultName);

                                            break;
                                        }
                                    case "PhoneNumbers":
                                        {
                                            additionalTablesClause += string.Format("LEFT OUTER JOIN `{0}` ON `{1}`.`ID` = `{0}`.`OwnerID` AND `{0}`.`IsPreferred`", PhoneNumbers.TableName, innerResultName);

                                            break;
                                        }
                                    case "PhysicalAddresses":
                                        {
                                            additionalTablesClause += string.Format("LEFT OUTER JOIN `{0}` ON `{1}`.`ID` = `{0}`.`OwnerID` AND `{0}`.`IsHomeAddress`", PhysicalAddresses.TableName, innerResultName);

                                            break;
                                        }
                                    case "PermissionGroups":
                                        {
                                            throw new NotImplementedException("A search was attempted against the permission groups field in a person search.  This is not yet implemented.");
                                        }
                                    case "SubscribedChangeEvents":
                                        {
                                            throw new NotImplementedException("A search was attempted against the change event subscriptions field in a person search.  This is not yet implemented.");
                                        }
                                    case "Billet":
                                        {
                                            throw new NotImplementedException("A search was attempted against the billet field in a person search.  This is not yet implemented.");
                                        }
                                    default:
                                        {
                                            throw new NotImplementedException(string.Format("In the inner join for the outer loop for the special tables switch, following has no handling instructions: {0}", specialTables[x]));
                                        }
                                }

                            }

                            //Alright, now we have our additional tables clause.  Now we need to do our additional searches from the special filters.
                            //This is getting rough... like rough sex... just rough. No lube.  :(  Shrek is Love, Shrek is Life.
                            string outerWhereClause = "";
                            if (specialFilters.Any())
                            {
                                outerWhereClause += "WHERE ";

                                for (int x = 0; x < specialFilters.Count; x++)
                                {
                                    if (x != 0)
                                        outerWhereClause += " AND ";

                                    switch (specialFilters[x].Key)
                                    {
                                        case "EmailAddresses":
                                            {
                                                outerWhereClause += string.Format("`{0}`.`Address` LIKE @AddressSpecial", EmailAddresses.TableName);
                                                command.Parameters.AddWithValue("@AddressSpecial", string.Format("%{0}%", specialFilters[x].Value));

                                                break;
                                            }
                                        case "PhoneNumbers":
                                            {
                                                outerWhereClause += string.Format("`{0}`.`Number` LIKE @NumberSpecial", EmailAddresses.TableName);
                                                command.Parameters.AddWithValue("@AddressSpecial", string.Format("%{0}%", specialFilters[x].Value));

                                                break;
                                            }
                                        case "PhysicalAddresses":
                                            {
                                                //For physical addresses, since there is more than one field, we're going to do a simple search across the entire object.
                                                //This is kind of shitty right now, but what do you want?
                                                List<string> physicalAddressFields = new List<string>()
                                            {
                                                "City",
                                                "Country",
                                                "Latitude",
                                                "Longitude",
                                                "StreetNumber",
                                                "State",
                                                "Route",
                                                "ZipCode"
                                            };

                                                List<string> searchTerms = (specialFilters[x].Value as string).Split((char[])null).ToList();

                                                for (int y = 0; y < searchTerms.Count; y++)
                                                {
                                                    if (y == 0)
                                                        outerWhereClause += "(";
                                                    else
                                                        outerWhereClause += ") AND (";

                                                    for (int z = 0; z < physicalAddressFields.Count; z++)
                                                    {
                                                        if (z != 0)
                                                            outerWhereClause += " OR ";

                                                        outerWhereClause += string.Format("`{0}` LIKE @{0}{1}{2}simplephysical", physicalAddressFields[z], z, y);
                                                        command.Parameters.AddWithValue(string.Format("@{0}{1}{2}simplephysical", physicalAddressFields[z], z, y), string.Format("%{0}%", searchTerms[y]));
                                                    }

                                                    if (y + 1 == searchTerms.Count)
                                                        outerWhereClause += ")";
                                                }

                                                break;
                                            }
                                        case "PermissionGroups":
                                            {
                                                throw new NotImplementedException("A search was attempted against the permission groups field in a person search.  This is not yet implemented.");
                                            }
                                        case "SubscribedChangeEvents":
                                            {
                                                throw new NotImplementedException("A search was attempted against the change event subscriptions field in a person search.  This is not yet implemented.");
                                            }
                                        case "Billet":
                                            {
                                                throw new NotImplementedException("A search was attempted against the billet field in a person search.  This is not yet implemented.");
                                            }
                                        default:
                                            {
                                                throw new NotImplementedException(string.Format("In athe outer where clause special filters switch, the following had no handling instructions: {0}", specialFilters[x].Key));
                                            }
                                    }
                                }
                            }

                            #endregion

                            if (limit.HasValue)
                                command.CommandText = string.Format("SELECT {0} FROM ({1}) AS `{2}` {3} {4} LIMIT {5} ", outerReturnClause, innerClause, innerResultName, additionalTablesClause, outerWhereClause, limit.Value);
                            else
                                command.CommandText = string.Format("SELECT {0} FROM ({1}) AS `{2}` {3} {4}", outerReturnClause, innerClause, innerResultName, additionalTablesClause, outerWhereClause);
                        }

                        //Now submit the query?  We'll see how the database likes this bad boy.
                        using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                        {
                            if (reader.HasRows)
                            {
                                DataTable table = new DataTable();
                                table.Load(reader);

                                var cols = table.Columns.Cast<DataColumn>().ToList();

                                //Cast the datable into a list of dictionaries that represents each search result.
                                results = table.AsEnumerable().Select(x =>
                                {
                                    Dictionary<string, object> values = new Dictionary<string, object>();

                                    cols.ForEach(y =>
                                    {
                                        values.Add(y.ColumnName, x[y]);
                                    });

                                    return values;
                                }).ToList(); 
                            }
                        }
                    }
                }

                return results;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Returns an integer indicating how many person are in a given division.
        /// </summary>
        /// <param name="division"></param>
        /// <param name="department"></param>
        /// <param name="com"></param>
        /// <returns></returns>
        public static async Task<int> CountPersonsInDivision(string division, string department, string com)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(Properties.ConnectionString))
                {
                    await connection.OpenAsync();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = string.Format("SELECT COUNT(*) FROM {0} WHERE `Division` = @Division AND `Department` = @Department AND `Command` = @Command", Utilities.BuildJoinStatement(_tableNames, "ID"));

                    command.Parameters.AddWithValue("@Division", division);
                    command.Parameters.AddWithValue("@Department", department);
                    command.Parameters.AddWithValue("@Command", command);

                    return Convert.ToInt32((await command.ExecuteScalarAsync()));
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Returns an integer indicating how many person are in a given department.
        /// </summary>
        /// <param name="department"></param>
        /// <param name="com"></param>
        /// <returns></returns>
        public static async Task<int> CountPersonsInDepartment(string department, string com)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(Properties.ConnectionString))
                {
                    await connection.OpenAsync();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = string.Format("SELECT COUNT(*) FROM {0} WHERE `Department` = @Department AND `Command` = @Command", Utilities.BuildJoinStatement(_tableNames, "ID"));

                    command.Parameters.AddWithValue("@Department", department);
                    command.Parameters.AddWithValue("@Command", command);

                    return Convert.ToInt32((await command.ExecuteScalarAsync()));
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Returns an integer indicating how many person are in a given command.
        /// </summary>
        /// <param name="department"></param>
        /// <param name="com"></param>
        /// <returns></returns>
        public static async Task<int> CountPersonsInCommand(string com)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(Properties.ConnectionString))
                {
                    await connection.OpenAsync();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = string.Format("SELECT COUNT(*) FROM {0} WHERE `Command` = @Command", Utilities.BuildJoinStatement(_tableNames, "ID"));

                    command.Parameters.AddWithValue("@Command", command);

                    return Convert.ToInt32((await command.ExecuteScalarAsync()));
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Deterimes whether or not a profile in the Persons tables has the given username.
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public static async Task<bool> DoesUsernameExist(string username)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(Properties.ConnectionString))
                {
                    await connection.OpenAsync();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = string.Format("SELECT CASE WHEN EXISTS(SELECT * FROM `{0}` WHERE `Username` = @Username) THEN 1 ELSE 0 END", "persons_accounts");

                    command.Parameters.AddWithValue("@Username", username);

                    return Convert.ToBoolean(await command.ExecuteScalarAsync());
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Returns a boolean indicating whether or not the given person id exists in all of the persons tables.
        /// </summary>
        /// <param name="personID"></param>
        /// <returns></returns>
        public static async Task<bool> DoesPersonIDExist(string personID)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(Properties.ConnectionString))
                {
                    await connection.OpenAsync();

                    MySqlCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = string.Format("SELECT CASE WHEN EXISTS(SELECT * FROM {0} WHERE `ID` = @ID) THEN 1 ELSE 0 END", Utilities.BuildJoinStatement(_tableNames, "ID"));

                    command.Parameters.AddWithValue("@ID", personID);

                    return Convert.ToBoolean(await command.ExecuteScalarAsync());
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Returns a friendly name for a given person ID or null if the person ID is not found.  This looks like {Rate} {LastName}, {FirstName} {MiddleName} : CTI2 Atwood, Daniel Kurt Roger.
        /// </summary>
        /// <param name="personID"></param>
        /// <returns></returns>
        public static async Task<string> TranslatePersonIDToFriendlyName(string personID)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(Properties.ConnectionString))
                {
                    await connection.OpenAsync();

                    using (MySqlCommand command = new MySqlCommand("", connection))
                    {
                        command.CommandText = "SELECT `Rate`,`LastName`,`FirstName`,`MiddleName` FROM `persons_main` WHERE `ID` = @ID";

                        command.Parameters.AddWithValue("@ID", personID);

                        using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                        {
                            if (reader.HasRows)
                            {
                                await reader.ReadAsync();

                                return string.Format("{0} {1}, {2} {3}", reader["Rank"] as string, reader["LastName"] as string, reader["FirstName"] as string, reader["MiddleName"] as string);
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



        #endregion

        #region Client Access Methods

        /// <summary>
        /// WARNING!  THIS IS A CLIENT METHOD.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Creates a new person in the database and returns that person.  All fields will be blank save for the client's ID.
        /// <para />
        /// Options: 
        /// <para />
        /// None
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<MessageTokens.MessageToken> CreateNewPerson_Client(MessageTokens.MessageToken token)
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

                //Make sure the client has the add new user permission
                if (!customPerms.Contains(CustomPermissionTypes.Add_New_User))
                    throw new ServiceException("You do not have permission to add new persons!", ErrorTypes.Authorization);

                //Create the new user.  To do this, we need to which division, department and command the client is in.
                string division = null, department = null, cmd = null;
                using (MySqlConnection connection = new MySqlConnection(Properties.ConnectionString))
                {
                    await connection.OpenAsync();

                    using (MySqlCommand command = new MySqlCommand("", connection))
                    {
                        command.CommandText = "SELECT `Division`,`Department`,`Command` FROM `persons_main` WHERE `ID` = @ID";

                        command.Parameters.AddWithValue("@ID", token.Session.PersonID);

                        using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                        {
                            if (reader.HasRows)
                            {
                                await reader.ReadAsync();

                                division = reader["Division"] as string;
                                department = reader["Department"] as string;
                                cmd = reader["Command"] as string;
                            }
                        }
                    }
                }

                //Make sure we got division, department, and command from the person.
                if (string.IsNullOrWhiteSpace(division) || string.IsNullOrWhiteSpace(department) || string.IsNullOrWhiteSpace(cmd))
                    throw new ServiceException("You can not create a person because your division, department, or command is not set.", ErrorTypes.Validation);

                token.Result = await Persons.DBCreateNew(division, department, cmd);

                //Here's the token!
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
        /// Loads all fields a client is able to load for a single user.  Uses an ID and the ID is checked to make sure it looks like a proper GUID.
        /// <para />
        /// Options: 
        /// <para />
        /// personid : the ID of the person the client wants to load.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<MessageTokens.MessageToken> LoadFullProfile_Client(MessageTokens.MessageToken token)
        {
            try
            {
                //Alright, to do this we need an ID from the client, and then we're going to go load all fields the client is allowed to view.
                if (!token.Args.ContainsKey("personid"))
                    throw new ServiceException("In order to load the profile of a user, you must send the user's ID.", ErrorTypes.Validation);
                string personID = token.Args["personid"] as string;

                //Let's make sure this person ID at least looks like an ID.
                if (!ValidationMethods.IsValidGuid(personID))
                    throw new ServiceException(string.Format("The ID you sent ({0}) was not considered to be a valid person ID.", personID), ErrorTypes.Validation);

                //At this point, we're going to ask if the client is the same as the requested profile.  If so, we're going to load the profile differently as clients are 

                Person result = await Persons.DBLoadOne(personID);

                //Did we get a person back?
                if (result == null)
                    throw new ServiceException(string.Format("The Person ID, '{0}', did not correspond to an actual person.", personID), ErrorTypes.Validation);

                //Each case below is going to set teh returnable fields differently.
                List<string> returnableFields;

                //First get the client's permissions.
                List<UnifiedServiceFramework.Authorization.Permissions.PermissionGroup> clientPermissions =
                    UnifiedServiceFramework.Authorization.Permissions.TranslatePermissionGroupIDs(token.Session.PermissionIDs);

                //Now get the client's model permissions.
                var modelPermission = UnifiedServiceFramework.Authorization.Permissions.GetModelPermissionsForUser(token, "Person");

                //Now get the authorized fields.
                List<string> editableFields = Persons.GetAuthorizedEditableFields(modelPermission, clientPermissions, personID == token.Session.PersonID,
                    await CustomAuthorization.CustomPermissions.IsClientInChainOfCommandOfPerson(clientPermissions, token.Session.PersonID, personID));

                //Clients are allowed to view all fields on their own profiles.  In this case, set teh returnable fields and do nothing to the object.
                if (personID == token.Session.PersonID)
                {
                    //The client is allowed to return all fields if the client is the person.
                    returnableFields = typeof(Person).GetProperties().Select(x => x.Name).ToList();
                }
                else
                {
                    //The person we're loading isn't the person who is logged in.  so let's set the returnable fields and then use those to null out fields the client can't see.

                    //Now let's get the fields the client can return for a person.  
                    returnableFields = UnifiedServiceFramework.Authorization.Permissions.GetModelPermissionsForUser(token, "Person").ReturnableFields;

                    //And then remove the fields we don't want the client to see.  We do it this way so that the data stays formed like a person object.
                    typeof(Person).GetProperties().ToList().ForEach(x =>
                    {
                        if (!returnableFields.Contains(x.Name))
                            x.SetValue(result, null);
                    });

                }

                //Ok, now that the person object contains all those fields the client is allowed to see, let's validate it and show the client what fields on this profile are wrong.
                List<string> errors = await Persons.ValidatePerson(result);

                //Now we're going to take teh result and combien it with some otehr fields.
                token.Result = new { Person = result, IsMyProfile = personID == token.Session.PersonID, ReturnableFields = returnableFields, EditableFields = editableFields, Errors = errors };

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
        /// Searches for people in the persons table.  This uses a simple search algorithm: The search term is received as a string and then broken into terms by splitting the string by any whitespace.  
        /// Then, the search terms are built into a SQL query such that each term is searched in every single field that the client wants it to search in.  
        /// As long as each term appears in at least one field, the row is returned.  We also allow the client to indicate which fields should be returned.  
        /// In order to enable this dynamic behavior, whitelisting is used and every field is checked for both validity, and authority prior to db interaction.
        /// <para />
        /// Options: 
        /// <para />
        /// limit : Indicates to the service how many results should be returned.  Must be an integer greater than zero.
        /// orderby : Indicates by which field the results should be ordered.  Case sensitive.  The field must also be included in the client's "returnfields" parameter.
        /// searchterm : The search string we are going to use to search.  This string is split by any white space to create an array of search terms.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<MessageTokens.MessageToken> SimpleSearchPersons_Client(MessageTokens.MessageToken token)
        {
            try
            {
                //First get the client's permissions.
                List<UnifiedServiceFramework.Authorization.Permissions.PermissionGroup> clientPermissions =
                    UnifiedServiceFramework.Authorization.Permissions.TranslatePermissionGroupIDs(token.Session.PermissionIDs);

                //Now get the client's model permissions.
                var modelPermission = UnifiedServiceFramework.Authorization.Permissions.GetModelPermissionsForUser(token, "Person");

                //So now let's get the flattended list of custom permissions and make sure we can search for users.
                List<CustomPermissionTypes> customPerms = UnifiedServiceFramework.Authorization.Permissions.GetUniqueCustomPermissions(clientPermissions).Select(x =>
                {
                    CustomPermissionTypes customPerm;
                    if (!Enum.TryParse(x, out customPerm))
                        throw new Exception(string.Format("An error occurred while trying to parse the custom permission '{0}' into the custom permissions enum.", x));

                    return customPerm;
                }).ToList();

                if (!customPerms.Contains(CustomPermissionTypes.Search_Users))
                    throw new ServiceException("You don't have permission to search for users.", ErrorTypes.Authorization);

                //Make sure the client is allowed to return and search in the fields required of a simple search.
                if (!modelPermission.SearchableFields.ContainsAll(SimpleSearchFields) || !modelPermission.ReturnableFields.ContainsAll(SimpleSearchFields))
                    throw new ServiceException(string.Format("In order to conduct a simple search, you must be allowed to search in, and return, the following fields: {0}", string.Join(",", SimpleSearchFields)), ErrorTypes.Authorization);

                //Alright, now let's go get all of our parameters from the client.
                if (!token.Args.ContainsKey("searchterm"))
                    throw new ServiceException("You must send a search term.", ErrorTypes.Validation);
                string searchTerm = token.Args["searchterm"] as string;

                if (string.IsNullOrWhiteSpace(searchTerm))
                    throw new ServiceException("The search term may not be blank.", ErrorTypes.Validation);

                string orderByField = null;
                if (token.Args.ContainsKey("orderby"))
                    orderByField = token.Args["orderby"] as string;

                int? limit = null;
                if (token.Args.ContainsKey("limit"))
                {
                    int temp;
                    if (!Int32.TryParse(token.Args["limit"] as string, out temp))
                        throw new ServiceException(string.Format("The value, '{0}', was not a valid limit - it must be an integer.", token.Args["limit"] as string), ErrorTypes.Validation);
                    
                    if (temp <= 0)
                        throw new ServiceException(string.Format("You asked that the limit, '{0}', be used in your search; however, that is not a valid limit.  It must be a whole number greater than zero.", temp), ErrorTypes.Validation);

                    //Since we got to here, the limit must be good to go.
                    limit = temp;
                }

                

                //Cool, now we can submit all this shit to the DBSimpleSearch!
                var result = await Persons.DBSimpleSearch(searchTerm, SimpleSearchFields, SimpleSearchFields, orderByField, limit);
                token.Result = new { ResultsCount = result.Count, SearchTime = DateTime.Now.Subtract(token.CallTime), Results = result, Fields = SimpleSearchFields };

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
        /// Searches for people in the persons table.  This uses a key/value pair search scheme.  Keys must be real fields from the Person model and the values are any value you want to search for.  
        /// Values are searched for using LIKE and the value is additionally wildcarded.  This allows 'atw' to return 'Atwood'.
        /// <para />
        /// Options: 
        /// <para />
        /// limit : Indicates to the service how many results should be returned.  Must be an integer greater than zero.
        /// orderby : Indicates by which field the results should be ordered.  Case sensitive.  The field must also be included in the client's "returnfields" parameter.
        /// filters : A list of key/value pairs.  The key is the field to search in while the value is the value to search for.
        /// returnfields : A list of fields from the Person model you want returned for your search.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<MessageTokens.MessageToken> AdvancedSearchPersons_Client(MessageTokens.MessageToken token)
        {
            try
            {
                //First get the client's permissions.
                List<UnifiedServiceFramework.Authorization.Permissions.PermissionGroup> clientPermissions =
                    UnifiedServiceFramework.Authorization.Permissions.TranslatePermissionGroupIDs(token.Session.PermissionIDs);

                //Now get the client's model permissions.
                var modelPermission = UnifiedServiceFramework.Authorization.Permissions.GetModelPermissionsForUser(token, "Person");

                //So now let's get the flattended list of custom permissions and make sure we can search for users.
                List<CustomPermissionTypes> customPerms = UnifiedServiceFramework.Authorization.Permissions.GetUniqueCustomPermissions(clientPermissions).Select(x =>
                {
                    CustomPermissionTypes customPerm;
                    if (!Enum.TryParse(x, out customPerm))
                        throw new Exception(string.Format("An error occurred while trying to parse the custom permission '{0}' into the custom permissions enum.", x));

                    return customPerm;
                }).ToList();

                //Can we search for users?
                if (!customPerms.Contains(CustomPermissionTypes.Search_Users))
                    throw new ServiceException("You don't have permission to search for users.", ErrorTypes.Authorization);

                //Ok cool, since we have the basic permissions down, let's start getting our fields.
                if (!token.Args.ContainsKey("filters"))
                    throw new ServiceException("You must send a 'filters' parameter.", ErrorTypes.Validation);
                Dictionary<string, object> filters = token.Args["filters"].CastJToken<Dictionary<string, object>>();
                
                /*if (!(token.Args["filters"] as string).TryDeserialize<Dictionary<string, object>>(out filters))
                    throw new ServiceException(string.Format("The value in the parameter 'filters' was not valid.  It must be a dictionary."), ErrorTypes.Validation);
                */
                //Now let's make sure the client is allowed to search in all of these fields.
                filters.Select(x => x.Key).ToList().ForEach(x =>
                    {
                        if (!modelPermission.SearchableFields.Contains(x))
                            throw new ServiceException(string.Format("You do not have sufficient permissions to search in the '{0}' field.", x), ErrorTypes.Authorization);
                    });

                //Since the client is allowed to search in the field, let's go get the return fields.
                if (!token.Args.ContainsKey("returnfields"))
                    throw new ServiceException("You must send a 'returnfields' parameter.", ErrorTypes.Validation);
                List<string> returnFields = token.Args["returnfields"].CastJToken<List<string>>();

                //Is the client allowed ot return these fields?
                returnFields.ForEach(x =>
                    {
                        if (!modelPermission.ReturnableFields.Contains(x))
                            throw new ServiceException(string.Format("You do not have sufficient permissions to request the '{0}' field.", x), ErrorTypes.Authorization);
                    });

                //Alright, we have our filters and return fields.  Now we just need to the order by field and the limit field.
                string orderByField = null;
                if (token.Args.ContainsKey("orderby"))
                    orderByField = token.Args["orderby"] as string;
                if (!string.IsNullOrWhiteSpace(orderByField) && !returnFields.Contains(orderByField))
                    throw new ServiceException(string.Format("In order to ask that we order the results by the field '{0}', you must ask that field be returned.", orderByField), ErrorTypes.Validation);

                //And then get the limit
                int? limit = null;
                if (token.Args.ContainsKey("limit"))
                    limit = Convert.ToInt32(token.Args["limit"]);
                if (limit.HasValue && limit.Value <= 0)
                    throw new ServiceException(string.Format("The value, '{0}', was not valid for the limit.  It must be greater than zero.", limit.Value), ErrorTypes.Validation);

                //Alright, now that we have everything, let's submit it all to the backing method.
                var result = await Persons.DBAdvancedSearch(filters, returnFields, limit);

                token.Result = new { ResultsCount = result.Count, SearchTime = DateTime.Now.Subtract(token.CallTime), Results = result };

                //Now let's pass these results to the client!
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
        /// Updates a person, given a person object.  To do this, the client sends a person and we load that person from the database and then compare the two objects.  
        /// Looking at the variances between the two objects, we throw out any fields the client isn't allowed to view - changes to these fields are ignored.  Then, we look at those fields, which are now the viewable variances,  
        /// and we ask if the client is allowed to edit those fields.  If so, we pass only these variances into the database to be updated.  Failing the authorization check to edit any fields  
        /// will result in a generic "you could not update one or more fields" message.  Additionally, the client must own a lock on this profile.  The backend does not enforce when the lock must've been taken,  
        /// only that the lock must've been taken prior to the call to UpdatePerson.  If no lock is owned, an error is thrown.  If the lock is owned by someone else, their friendly name will be included in the error message.  
        /// This endpoint does not release locks after update.  If you want the lock released, follow up a successful call to this endpoint with a call to ReleaseLock.
        /// <para />
        /// Options: 
        /// <para />
        /// person - A properly formed person object to attempt to update.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<MessageTokens.MessageToken> UpdatePerson_Client(MessageTokens.MessageToken token)
        {
            try
            {
                //First get the client's permissions.
                var clientPermissions = UnifiedServiceFramework.Authorization.Permissions.TranslatePermissionGroupIDs(token.Session.PermissionIDs);

                //Now get the client's model permissions.
                var modelPermission = UnifiedServiceFramework.Authorization.Permissions.GetModelPermissionsForUser(token, "Person");

                //So now let's get the flattended list of custom permissions and make sure we can search for users.
                List<CustomPermissionTypes> customPerms = UnifiedServiceFramework.Authorization.Permissions.GetUniqueCustomPermissions(clientPermissions).Select(x =>
                {
                    CustomPermissionTypes customPerm;
                    if (!Enum.TryParse(x, out customPerm))
                        throw new Exception(string.Format("An error occurred while trying to parse the custom permission '{0}' into the custom permissions enum.", x));

                    return customPerm;
                }).ToList();

                if (!customPerms.Contains(CustomPermissionTypes.Edit_Users))
                    throw new ServiceException("You don't have permission to search for users.", ErrorTypes.Authorization);

                //Now let's try to get a person object.
                if (!token.Args.ContainsKey("person"))
                    throw new ServiceException("You must send a person object in order to conduct a person update.", ErrorTypes.Validation);
                Person newPerson = token.Args["person"].CastJToken<Person>();

                //Ok, now let's find out who this person is and make sure they exist.
                Person oldPerson = await Persons.DBLoadOne(newPerson.ID);

                //If oldPerson is null, then that id isn't legit.
                if (oldPerson == null)
                    throw new ServiceException(string.Format("You attempted to update the person with the ID, '{0}'; however, that person does not appear to exist.", newPerson.ID), ErrorTypes.Validation);

                //Now we need to make sure the client owns a lock for this person.
                var profileLock = await ProfileLocks.GetProfileLockByPerson(newPerson.ID, true);

                //If the lock is null, then no one has the lock.  If the lock is not null, then the person must own the lock.  Null locks aren't allowed.
                if (profileLock == null)
                    throw new ServiceException("You do not own a profile lock for this profile; therefore, you can not edit it.", ErrorTypes.LockOwned);

                //We do these checks separately so that we can provide some additional information.  In the error message, we send who owns this lock.
                if (profileLock.OwnerID != token.Session.PersonID)
                    throw new ServiceException(string.Format("You can not edit this profile because the lock is currently owned by '{0}'.", await Persons.TranslatePersonIDToFriendlyName(profileLock.OwnerID)), ErrorTypes.LockOwned);

                //Ok, now that we have the old person, let's figure out how that person and the person the client sent us differs.
                var variances = newPerson.DetermineVariances(oldPerson);

                //Now we need to throw out all those fields the client isn't allowed to view.  This is because, to the client, those fields are null; 
                //however, that's only because we wouldn't have given the client anything.
                var viewableVariances = variances.Where(x => modelPermission.ReturnableFields.Contains(x.PropertyName)).ToList();

                //Now throw out variances that the service updates but the client can't.  These will look like false edits.
                var finalVariances = viewableVariances.Where(x => !new[] { "AccountHistory" }.Contains(x.PropertyName)).ToList();

                //Ok, now we should have those fields the client changed AND was allowed to see.
                //Now let's go through every variance and ask is the new value for that variance valid? and is the client allowed to make that change?
                //At this point, we need to go through these individually.  Each field may have different edit requirements for the permissions and 
                //it also depends on whether or not the current client is the person we're trying to edit.
                //This variable will hold all of our errors coming back from the validation process.
                List<string> errors = new List<string>();

                //Before we start, let's collect some information we're going to need.
                bool isClientInUsersChainOfCommand = await CustomAuthorization.CustomPermissions.IsClientInChainOfCommandOfPerson(clientPermissions, token.Session.PersonID, oldPerson.ID);

                //This variable will tell us if the client is the same user as that person we're trying to update.
                bool isClientUser = oldPerson.ID == token.Session.PersonID;

                //The fields teh client is allowed to edit for this specific person.
                List<string> authorizedEditFields = GetAuthorizedEditableFields(modelPermission, clientPermissions, isClientUser, isClientInUsersChainOfCommand);

                foreach (var variance in finalVariances)
                {
                    //First, validation.
                    List<string> objectErrors = await ValidateProperty(variance.PropertyName, variance.NewValue);
                    if (objectErrors != null)
                        errors.Concat(objectErrors);

                    //Now authorization.  We're going to make sure that all the fields the client is trying to edit the client can actually edit.
                    if (!authorizedEditFields.Contains(variance.PropertyName))
                        errors.Add(string.Format("You are not authorized to edit the field '{0}'.", variance.PropertyName));
                }

                //If we got any errors, return those, else, do the udpate.
                if (errors.Any())
                {
                    token.Result = errors;
                }
                else
                {
                    await Persons.DBUpdate(finalVariances, newPerson.ID);
                    token.Result = "Success";
                }

                //Ok, now that we've completed the update, let's log the changes.
                var changes = finalVariances.Select(x => new Changes.Change()
                {
                    EditorID = token.Session.PersonID,
                    ID = Guid.NewGuid().ToString(),
                    ObjectID = oldPerson.ID,
                    ObjectName = "Person",
                    Remarks = "Person Edited",
                    Time = token.CallTime,
                    Variance = x
                }).ToList();

                //And then start the change insert.
                Changes.DBInsertAll(changes);

                //That change insert will have fired off and however long that takes we don't care.
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
        /// Translates a given person ID to a user's friendly name.  This friendly name looks like {Rate} {LastName}, {FirstName} {MiddleName}.  Ex: CTI2 Atwood, Daniel K
        /// <para />
        /// Options: 
        /// <para />
        /// personid - The ID of the person for whom the client wants a friendly name.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<MessageTokens.MessageToken> TranslatePersonIDToFriendlyName_Client(MessageTokens.MessageToken token)
        {
            try
            {
                //First let's validate the person id.
                if (!token.Args.ContainsKey("personid"))
                    throw new ServiceException("You must send a person id for validation.", ErrorTypes.Validation);
                string personID = token.Args["personid"] as string;

                if (!ValidationMethods.IsValidGuid(personID))
                    throw new ServiceException(string.Format("The value, '{0}', was not valid for a person's ID.", personID), ErrorTypes.Validation);

                //Ok, now we need the client's permissions.
                var modelPermission = UnifiedServiceFramework.Authorization.Permissions.GetModelPermissionsForUser(token, "Person");

                //The translation is going to require the rate, lastname, firstname, and middle name.  So let's make sure the client can view those.
                if (!modelPermission.ReturnableFields.ContainsAll(new[] { "Rate", "LastName", "FirstName", "MiddleName" }))
                    throw new ServiceException("You must be able to view a person's Rate, LastName, FirstName, and MiddleName in order to translate person IDs.", ErrorTypes.Authorization);

                //Now we should be good to go!
                string result = await Persons.TranslatePersonIDToFriendlyName(personID);

                //If the result is null, then that means the personID wasn't valid.
                if (result == null)
                    throw new ServiceException(string.Format("The value, '{0}', was not valid for a person ID - it does not appear to exist.", personID), ErrorTypes.Validation);

                //If we made it here, then we're good to go.
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
