﻿using System;
using System.Collections.Generic;
using System.Linq;
using CCServ.Authorization;
using CCServ.ClientAccess;
using CCServ.Entities.ReferenceLists;
using CCServ.DataAccess;
using FluentNHibernate.Mapping;
using FluentValidation;
using NHibernate.Transform;
using NHibernate.Criterion;
using NHibernate.Linq;
using AtwoodUtils;
using CCServ.ServiceManagement;
using CCServ.Logging;
using CCServ.Entities;
using System.Reflection;

namespace CCServ.ClientAccess.Endpoints
{
    /// <summary>
    /// The endpoints that effect the person object.
    /// </summary>
    static class PersonEndpoints
    {
        #region Create

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Creates a new person in the database by taking a person object from the client, picking out only the properties we want, and saving them.  Then it returns the Id we assigned to the person.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void CreatePerson(MessageToken token, DTOs.PersonEndpoints.CreatePerson dto)
        {
            token.AssertLoggedIn();

            //You have permission?
            if (!token.AuthenticationSession.Person.PermissionGroups.Any(x => x.AccessibleSubModules.Contains(SubModules.CreatePerson.ToString(), StringComparer.CurrentCultureIgnoreCase)))
                throw new CommandCentralException("You don't have permission to do that.", ErrorTypes.Authorization);

            var personToInsert = dto.Person.MapTo<Person>();
            personToInsert.Id = Guid.NewGuid();
            personToInsert.IsClaimed = false;

            //Initialize the new user's muster entry.
            personToInsert.CurrentMusterRecord = MusterRecord.CreateDefaultMusterRecordForPerson(personToInsert, token.CallTime);

            //Here we're going to set the new user's permission groups to default.
            personToInsert.PermissionGroups = Authorization.Groups.PermissionGroup.AllPermissionGroups.Where(x => x.IsDefault).ToList();

            //Cool, since everything is good to go, let's also add the account history.
            personToInsert.AccountHistory = new List<AccountHistoryEvent> { new AccountHistoryEvent
                {
                    AccountHistoryEventType = AccountHistoryTypes.Creation,
                    EventTime = token.CallTime
                }};

            //Now for validation!
            var results = new Person.PersonValidator().Validate(personToInsert);

            if (!results.IsValid)
                throw new AggregateException(results.Errors.Select(x => new CommandCentralException(x.ErrorMessage, ErrorTypes.Validation)));

            using (var session = NHibernateHelper.CreateStatefulSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        //Let's make sure no one with that SSN exists...
                        if (session.QueryOver<Person>().Where(x => x.SSN == personToInsert.SSN).RowCount() != 0)
                        {
                            throw new CommandCentralException("A person with that SSN already exists.  Please consider using the search function to look for your user.", ErrorTypes.Validation);
                        }

                        //The person is a valid object.  Let's go ahead and insert it.  If insertion fails it's most likely because we violated a Uniqueness rule in the database.
                        session.Save(personToInsert);

                        //And now return the person.
                        token.SetResult(personToInsert.Id);

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

        #endregion

        #region Get/Load/Select/Search

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Loads a single person from the database and sets those fields to null that the client is not allowed to return.  If the client requests their own profile, all fields are returned.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void LoadPerson(MessageToken token, DTOs.PersonEndpoints.LoadPerson dto)
        {
            token.AssertLoggedIn();

            using (var session = NHibernateHelper.CreateStatefulSession())
            {
                //Now let's load the person and then set any fields the client isn't allowed to see to null.
                var person = session.Get<Person>(dto.Id) ??
                    throw new CommandCentralException("The Id you sent appears to be invalid.", ErrorTypes.Validation);

                //Now that we have the person back, let's resolve the permissions for this person.
                var resolvedPermissions = token.AuthenticationSession.Person.PermissionGroups.Resolve(token.AuthenticationSession.Person, person);

                Dictionary<string, object> returnData = new Dictionary<string, object>();

                List<string> returnableFields = resolvedPermissions.ReturnableFields["Main"]["Person"];

                var personMetadata = NHibernateHelper.GetEntityMetadata("Person");

                //Now just set the fields the client is allowed to see.
                foreach (var propertyName in returnableFields)
                {
                    //There's a stupid thing with NHibernate where it sees Ids as, well... Ids instead of as Properties.  So we do need a special case for it.
                    if (propertyName.ToLower() == "id")
                    {
                        returnData.Add("Id", personMetadata.GetIdentifier(person, NHibernate.EntityMode.Poco));
                    }
                    else
                    {
                        bool wasSet = false;

                        switch (propertyName.ToLower())
                        {
                            case "command":
                            case "department":
                            case "division":
                                {
                                    returnData.Add(propertyName, NHibernateHelper.GetIdentifier(personMetadata.GetPropertyValue(person, propertyName, NHibernate.EntityMode.Poco)));

                                    wasSet = true;
                                    break;
                                }
                            case "accounthistory":
                                {
                                    returnData.Add(propertyName, person.AccountHistory.OrderByDescending(x => x.EventTime).Take(5).ToList());

                                    wasSet = true;
                                    break;
                                }
                        }

                        if (!wasSet)
                        {
                            returnData.Add(propertyName, personMetadata.GetPropertyValue(person, propertyName, NHibernate.EntityMode.Poco));
                        }

                    }
                }

                token.SetResult(new
                {
                    Person = returnData,
                    IsMyProfile = token.AuthenticationSession.Person.Id == person.Id,
                    ResolvedPermissions = resolvedPermissions,
                    FriendlyName = person.ToString()
                });
            }
        }

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Returns all account history for the given person.  These are the login events, etc.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void LoadAccountHistoryByPerson(MessageToken token, DTOs.PersonEndpoints.LoadAccountHistoryByPerson dto)
        {
            token.AssertLoggedIn();

            //Let's load the person we were given.  We need the object for the permissions check.
            using (var session = NHibernateHelper.CreateStatefulSession())
            {
                //Now let's load the person and then set any fields the client isn't allowed to see to null.
                var person = session.Get<Person>(dto.Id) ??
                    throw new CommandCentralException("The Id you sent appears to be invalid.", ErrorTypes.Validation);

                //Now let's get permissions and see if the client is allowed to view AccountHistory.
                bool canView = token.AuthenticationSession.Person.PermissionGroups.Resolve(token.AuthenticationSession.Person, person)
                    .ReturnableFields["Main"]["Person"].Contains(PropertySelector.SelectPropertiesFrom<Person>(x => x.AccountHistory).First().Name);

                if (!canView)
                    throw new CommandCentralException("You are not allowed to view the account history of this person's profile.", ErrorTypes.Authorization);

                token.SetResult(person.AccountHistory);
            }
        }

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Returns a person's chain of command.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void GetChainOfCommandOfPerson(MessageToken token, DTOs.PersonEndpoints.GetChainOfCommandOfPerson dto)
        {
            token.AssertLoggedIn();

            using (var session = NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    //Now let's load the person and then set any fields the client isn't allowed to see to null.
                    var person = session.Get<Person>(dto.Id) ??
                        throw new CommandCentralException("The Id you sent appears to be invalid.", ErrorTypes.Validation);

                    token.SetResult(person.GetChainOfCommand());

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Conducts a simple search.  Simple search uses a list of search terms and returns all those rows in which each term appears at least once in each of the search fields.
        /// <para/>
        /// In this case those fields are FirstName, LastName, MiddleName, UIC, Paygrade, Designation, Command, Department and Division.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void SimpleSearchPersons(MessageToken token, DTOs.PersonEndpoints.SimpleSearchPersons dto)
        {
            token.AssertLoggedIn();

            using (var session = NHibernateHelper.CreateStatefulSession())
            {
                var queryProvider = new Person.PersonQueryProvider();

                //Build the query over simple search for each of the search terms.  It took like a fucking week to learn to write simple search in NHibernate.
                var query = queryProvider.CreateQuery(QueryTypes.Simple, dto.SearchTerm);

                var simpleSearchMembers = queryProvider.GetMembersThatAreUsedIn(QueryTypes.Simple);

                //If we weren't told to show hidden, then hide hidden members.
                if (!dto.ShowHidden)
                {
                    query = query.Where(x => x.DutyStatus != DutyStatuses.Loss);
                }

                //And finally, return the results.  We need to project them into only what we want to send to the client so as to remove them from the proxy shit that NHibernate has sullied them with.
                var results = query.GetExecutableQueryOver(session).List<Person>().Select(person =>
                {

                    //Do our permissions check here for each person.
                    var returnableFields = token.AuthenticationSession.Person.PermissionGroups.Resolve(token.AuthenticationSession.Person, person).ReturnableFields["Main"]["Person"];

                    Dictionary<string, string> result = new Dictionary<string, string>
                    {
                        { "Id", person.Id.ToString() }
                    };
                    foreach (var member in simpleSearchMembers.Select(x => x.GetProperty()))
                    {
                        if (returnableFields.Contains(member.Name))
                        {
                            var value = (member as PropertyInfo).GetValue(person);

                            if (value == null)
                            {
                                result.Add(member.Name, "");
                            }
                            else
                            {
                                result.Add(member.Name, value.ToString());
                            }
                        }
                        else
                        {
                            result.Add(member.Name, "REDACTED");
                        }
                    }

                    return result;
                });

                token.SetResult(new
                {
                    Results = results,
                    Fields = simpleSearchMembers.Select(x => x.GetPropertyName())
                });
            }
        }

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Conducts an advanced search.  Advanced search uses a series of key/value pairs where the key is a property to be searched and the value is a string of text which make up the search terms in
        /// a simple search across the property.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void AdvancedSearchPersons(MessageToken token, DTOs.PersonEndpoints.AdvancedSearchPersons dto)
        {
            token.AssertLoggedIn();

            Dictionary<string, object> filters = token.Args["filters"]
                .CastJToken<Dictionary<string, object>>() ??
                    new Dictionary<string, object>();

            //Ok, let's figure out what fields the client is allowed to search.
            //This is determined, in part by the existence of the searchlevel parameter.
            //If we don't find the level limit, then continue as normal.  However, if we do find a level limit, then we need to check the client's permissions.
            //We also need to throw out any property they gave us for the relevant level and insert our own.
            var resolvedPermissions = token.AuthenticationSession.Person.PermissionGroups.Resolve(token.AuthenticationSession.Person, null);
            if (!String.IsNullOrWhiteSpace(dto.SearchLevel))
            {
                //Ok there's a search level.  We need to do something different based on division, department or command.
                switch (dto.SearchLevel)
                {
                    case "Division":
                        {
                            if (token.AuthenticationSession.Person.Division == null)
                                throw new CommandCentralException("You can't limit by division if you don't have a division.", ErrorTypes.Validation);

                            //First, if the filters have division, delete it.
                            if (dto.Filters.ContainsKey("Division"))
                                dto.Filters.Remove("Division");

                            //Now ask if the client is allowed to search in all these fields.
                            var failures = dto.Filters.Keys.Where(x => !resolvedPermissions.PrivelegedReturnableFields["Main"]["Division"].Concat(resolvedPermissions.ReturnableFields["Main"]["Person"]).Contains(x));

                            if (failures.Any())
                                throw new CommandCentralException("You were not allowed to search in these fields: {0}".FormatS(String.Join(", ", failures)), ErrorTypes.Validation);

                            break;
                        }
                    case "Department":
                        {
                            if (token.AuthenticationSession.Person.Department == null)
                                throw new CommandCentralException("You can't limit by department if you don't have a department.", ErrorTypes.Validation);

                            //First, if the filters have department, delete it.
                            if (dto.Filters.ContainsKey("Department"))
                                dto.Filters.Remove("Department");

                            //Now ask if the client is allowed to search in all these fields.
                            var failures = dto.Filters.Keys.Where(x => !resolvedPermissions.PrivelegedReturnableFields["Main"]["Department"].Concat(resolvedPermissions.ReturnableFields["Main"]["Person"]).Contains(x));

                            if (failures.Any())
                                throw new CommandCentralException("You were not allowed to search in these fields: {0}".FormatS(String.Join(", ", failures)), ErrorTypes.Validation);

                            break;
                            
                        }
                    case "Command":
                        {
                            if (token.AuthenticationSession.Person.Department == null)
                                throw new CommandCentralException("You can't limit by command if you don't have a command.", ErrorTypes.Validation);

                            //First, if the filters have command, delete it.
                            if (dto.Filters.ContainsKey("Command"))
                                dto.Filters.Remove("Command");

                            //Now ask if the client is allowed to search in all these fields.
                            var failures = dto.Filters.Keys.Where(x => !resolvedPermissions.PrivelegedReturnableFields["Main"]["Command"].Concat(resolvedPermissions.ReturnableFields["Main"]["Person"]).Contains(x));

                            if (failures.Any())
                                throw new CommandCentralException("You were not allowed to search in these fields: {0}".FormatS(String.Join(", ", failures)), ErrorTypes.Validation);

                            break;
                        }
                    default:
                        {
                            throw new CommandCentralException("The searchlevel you sent was not in the correct format.  " +
                                "It must only be Command, Department, or Division.", ErrorTypes.Validation);
                        }
                }
            }
            else
            {
                //We weren't told to limit the search at all, meaning the searchable fields are the client's normal returnable fields.
                //So let's just test the fields against that.
                var failures = dto.Filters.Keys.Where(x => !resolvedPermissions.ReturnableFields["Main"]["Person"].Contains(x));

                if (failures.Any())
                    throw new CommandCentralException("You were not allowed to search in these fields: {0}".FormatS(String.Join(", ", failures)), ErrorTypes.Validation);
            }

            //Now let's determine if we're doing a geo query.
            bool isGeoQuery = dto.Radius.HasValue;

            //We're going to need the person object's metadata for the rest of this.
            var personMetadata = NHibernateHelper.GetEntityMetadata("Person");

            using (var session = NHibernateHelper.CreateStatefulSession())
            {
                var convertedFilters = dto.Filters.ToDictionary(
                    x => PropertySelector.SelectExpressionFrom<Person>(x.Key),
                    x => x.Value);

                var query = new Person.PersonQueryProvider().CreateQuery(QueryTypes.Advanced, convertedFilters);

                //So you remember the searchlevel from before?  We need to use that here.  If the client gave it to us.
                if (!String.IsNullOrWhiteSpace(dto.SearchLevel))
                {
                    //Ok there's a search level.  We need to do something different based on division, department or command.
                    switch (dto.SearchLevel)
                    {
                        case "Division":
                            {
                                query.Where(Subqueries.WhereProperty<Person>(x => x.Division.Id).In(QueryOver.Of<Division>().Where(x => x.Id == token.AuthenticationSession.Person.Division.Id).Select(x => x.Id)));

                                break;
                            }
                        case "Department":
                            {
                                query.Where(Subqueries.WhereProperty<Person>(x => x.Department.Id).In(QueryOver.Of<Department>().Where(x => x.Id == token.AuthenticationSession.Person.Department.Id).Select(x => x.Id)));

                                break;
                            }
                        case "Command":
                            {
                                query.Where(Subqueries.WhereProperty<Person>(x => x.Command.Id).In(QueryOver.Of<Command>().Where(x => x.Id == token.AuthenticationSession.Person.Command.Id).Select(x => x.Id)));

                                break;
                            }
                        default:
                            {
                                throw new NotImplementedException("Fell to the second searchlevel default in the advanced search endpoint.");
                            }
                    }
                }

                

                //The client is telling us to show hidden profiles or not.
                if (!dto.ShowHidden)
                {
                    query.Where(x => x.DutyStatus != DutyStatuses.Loss);
                }

                //Here we iterate over every returned person, do an authorization check and cast the results into DTOs.
                //Important note: the client expects every field to be a string.  We don't return object results. :(
                List<Dictionary<string, string>> result = new List<Dictionary<string, string>>();

                var rawResults = query.GetExecutableQueryOver(session)
                    .TransformUsing(Transformers.DistinctRootEntity) //This part here "should" give us distinct elements.
                    .List<Person>();

                foreach (var person in rawResults)
                {

                    //Before we do anything, and if this is a geo query, let's determine if this person passes the geo query.
                    if (isGeoQuery)
                    {
                        //If the person has a home address, then use it for the geo query; however, if the person has no home address,
                        //then just see if the person has at least a physical address that passes the geo query.
                        var homeAddress = person.PhysicalAddresses.FirstOrDefault(x => x.IsHomeAddress);

                        if (homeAddress != null)
                        {
                            //If the home address's lat or long are null, then they fail the check.
                            if (!homeAddress.Latitude.HasValue || !homeAddress.Longitude.HasValue)
                                continue;

                            var distance = Utilities.HaversineDistance(new Utilities.LatitudeAndLongitude { Latitude = homeAddress.Latitude.Value, Longitude = homeAddress.Longitude.Value },
                                                        new Utilities.LatitudeAndLongitude { Latitude = dto.CenterLat.Value, Longitude = dto.CenterLong.Value },
                                                        Utilities.DistanceUnit.Miles);

                            //If they failed the distance check, then skip the person.
                            if (distance > dto.Radius.Value)
                                continue;

                        }
                        else
                        {
                            //So there's no home address, so let's check against any address.
                            bool passed = false;

                            foreach (var address in person.PhysicalAddresses)
                            {
                                if (address.Latitude.HasValue && address.Longitude.HasValue)
                                {
                                    var distance = Utilities.HaversineDistance(new Utilities.LatitudeAndLongitude { Latitude = address.Latitude.Value, Longitude = address.Longitude.Value },
                                                        new Utilities.LatitudeAndLongitude { Latitude = dto.CenterLat.Value, Longitude = dto.CenterLong.Value },
                                                        Utilities.DistanceUnit.Miles);

                                    //If we find one that passes, set passed to true and break.
                                    if (distance < dto.Radius.Value)
                                    {
                                        passed = true;
                                        break;
                                    }
                                }
                            }

                            //If they didn't pass the geo query, skip this iteration.
                            if (!passed)
                                continue;
                        }
                    }

                    //We need to know the fields the client is allowed to return for this client.
                    var returnableFields = token.AuthenticationSession.Person.PermissionGroups.Resolve(token.AuthenticationSession.Person, person).ReturnableFields["Main"]["Person"];

                    var returnData = new Dictionary<string, string>();

                    //Now just set the fields the client is allowed to see.
                    foreach (var propertyName in dto.ReturnFields)
                    {
                        var propertyInfo = PropertySelector.SelectPropertyFrom<Person>(propertyName) ??
                            throw new CommandCentralException("The field, '{0}', does not exist on the person object; therefore, you can not request that it be returned.".FormatS(propertyName), ErrorTypes.Validation);

                        //if the client isn't allowed to return this field, replace its value with "redacted"
                        if (returnableFields.Any(x => String.Equals(propertyName, x, StringComparison.CurrentCultureIgnoreCase)))
                        {
                            var value = propertyInfo.GetValue(person);
                            returnData.Add(propertyInfo.Name, value == null ? "" : value.ToString());
                        }
                        else
                        {
                            returnData.Add(propertyInfo.Name, "REDACTED");
                        }
                    }

                    result.Add(returnData);
                }

                token.SetResult(new
                {
                    Results = result
                });
            }
        }

        #endregion

        #region Update

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Given a person, updates the person assuming the person is allowed to edit the properties that have changed.  Additionally, a lock must be owned on the person by the client.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void UpdatePerson(MessageToken token, DTOs.PersonEndpoints.UpdatePerson dto)
        {
            token.AssertLoggedIn();

            //Ok, so since we're ready to do ze WORK we're going to do it on a separate session.
            using (var session = NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    //Ok now we need to see if a lock exists for the person the client wants to edit.  Later we'll see if the client owns that lock.
                    ProfileLock profileLock = session.QueryOver<ProfileLock>()
                                            .Where(x => x.LockedPerson.Id == dto.Person.Id)
                                            .SingleOrDefault() ??
                                            throw new CommandCentralException("In order to edit this person, you must first take a lock on the person.", ErrorTypes.Validation);

                    //We also require the lock to be valid.
                    if (!profileLock.IsValid())
                        throw new CommandCentralException("Your lock on this profile is no longer valid.", ErrorTypes.Validation);

                    //Ok, well there is a lock on the person, now let's make sure the client owns that lock.
                    if (profileLock.Owner.Id != token.AuthenticationSession.Person.Id)
                        throw new CommandCentralException("The lock on this person is owned by '{0}' and will expire in {1} minutes unless the owner closes the profile prior to that.".FormatS(profileLock.Owner.ToString(), profileLock.GetTimeRemaining().TotalMinutes), ErrorTypes.LockOwned);

                    //Ok, so it's a valid person and the client owns the lock, now let's load the person by their ID, and see what they look like in the database.
                    Person personFromDB = session.Get<Person>(dto.Person.Id) ??
                        throw new CommandCentralException("The person you supplied had an Id that belongs to no actual person.", ErrorTypes.Validation);

                    var resolvedPermissions = token.AuthenticationSession.Person.PermissionGroups.Resolve(token.AuthenticationSession.Person, personFromDB);

                    //Get the editable and returnable fields and also those fields that, even if they are edited, will be ignored.
                    var editableFields = resolvedPermissions.EditableFields["Main"]["Person"];
                    var returnableFields = resolvedPermissions.ReturnableFields["Main"]["Person"];

                    //Go through all returnable fields that the client is allowed to edit and then move the values into the person from the database.
                    foreach (var field in returnableFields.Intersect(editableFields, StringComparer.CurrentCultureIgnoreCase))
                    {
                        var property = typeof(Person).GetProperty(field);

                        property.SetValue(personFromDB, property.GetValue(dto.Person));
                    }

                    //Determine what changed.
                    var changes = session.GetVariantProperties(personFromDB)
                        .Select(x =>
                        {
                            //When the yielded return results come back, we're going to tag them with the session-specific information.
                            x.Editee = personFromDB;
                            x.Editor = token.AuthenticationSession.Person;
                            x.Time = token.CallTime;
                            return x;
                        })
                        .ToList();

                    //Ok, let's validate the entire person object.  This will be what it used to look like plus the changes from the client.
                    var results = new Person.PersonValidator().Validate(personFromDB);

                    if (!results.IsValid)
                        throw new AggregateException(results.Errors.Select(x => new CommandCentralException(x.ErrorMessage, ErrorTypes.Validation)));

                    //Ok so the client only changed what they are allowed to see.  Now are those edits authorized.
                    var unauthorizedEdits = changes.Where(x => !editableFields.Contains(x.PropertyName));
                    if (unauthorizedEdits.Any())
                        throw new AggregateException(unauthorizedEdits.Select(x => new CommandCentralException("You lacked permission to edit the field '{0}'.".FormatS(x.PropertyName), ErrorTypes.Validation)));

                    //Since this was all good, just add the changes to the person's profile.
                    changes.ForEach(x => personFromDB.Changes.Add(x));

                    //Ok, so the client is authorized to edit all the fields that changed.  Let's submit the update to the database.
                    session.Merge(personFromDB);

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        #endregion

        #region Person Metadata

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Gets the metadata associated with the person object. Metadata describes the different properties of a person, how to search them, and more.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void GetPersonMetadata(MessageToken token)
        {
            token.AssertLoggedIn();

            var searchStrategy = new Person.PersonQueryProvider();
            Dictionary<string, object> result = new Dictionary<string, object>();
            foreach (var info in typeof(Person).GetProperties())
            {
                result.Add(info.Name, new
                {
                    SearchDataType = searchStrategy.GetSearchDataTypeForProperty(PropertySelector.SelectPropertyFrom<Person>(info.Name))
                });
            }

            token.SetResult(result);
        }

        #endregion
    }
}
