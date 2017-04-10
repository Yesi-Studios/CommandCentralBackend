using System;
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
        /// <para />
        /// Client Parameters: <para />
        ///     person - a properly formatted, optionally partial, person object containing the necessary information.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void CreatePerson(MessageToken token)
        {
            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
                throw new CommandCentralException("You must be logged in to do that.", HttpStatusCodes.AuthenticationFailed);

            //You have permission?
            if (!token.AuthenticationSession.Person.PermissionGroups.Any(x => x.AccessibleSubModules.Contains(SubModules.CreatePerson.ToString(), StringComparer.CurrentCultureIgnoreCase)))
                throw new CommandCentralException("You don't have permission to do that.", HttpStatusCodes.Unauthorized);

            //Ok, since the client has permission to create a person, we'll assume they have permission to udpate all of the required fields.
            if (!token.Args.ContainsKey("person"))
                throw new CommandCentralException("You failed to send a 'person' parameter.", HttpStatusCodes.BadRequest);

            Person personFromClient;

            try
            {
                personFromClient = token.Args["person"].CastJToken<Person>();
            }
            catch
            {
                throw new CommandCentralException("An error occured while processing your person object.", HttpStatusCodes.BadRequest);
            }


            //The person from the client... let's make sure that it is valid.  If it passes validation then it can be inserted.
            //For security we're going to take only the parameters that we need explicitly from the client.  All others will be thrown out. #whitelisting
            Person newPerson = new Person
            {
                FirstName = personFromClient.FirstName,
                MiddleName = personFromClient.MiddleName,
                LastName = personFromClient.LastName,
                Division = token.AuthenticationSession.Person.Division,
                Department = token.AuthenticationSession.Person.Department,
                Command = token.AuthenticationSession.Person.Command,
                Paygrade = personFromClient.Paygrade,
                UIC = personFromClient.UIC,
                Designation = personFromClient.Designation,
                Sex = personFromClient.Sex,
                SSN = personFromClient.SSN,
                DoDId = personFromClient.DoDId,
                DateOfBirth = personFromClient.DateOfBirth,
                DateOfArrival = personFromClient.DateOfArrival,
                DutyStatus = personFromClient.DutyStatus,
                Id = Guid.NewGuid(),
                IsClaimed = false,
                PRD = personFromClient.PRD
            };
            newPerson.CurrentMusterRecord = MusterRecord.CreateDefaultMusterRecordForPerson(newPerson, token.CallTime);

            //We're also going to add on the default permission groups.
            newPerson.PermissionGroups = Authorization.Groups.PermissionGroup.AllPermissionGroups.Where(x => x.IsDefault).ToList();

            //Now for validation!
            var results = new Person.PersonValidator().Validate(newPerson);

            if (!results.IsValid)
                throw new AggregateException(results.Errors.Select(x => new CommandCentralException(x.ErrorMessage, HttpStatusCodes.BadRequest)));

            //Cool, since everything is good to go, let's also add the account history.
            newPerson.AccountHistory = new List<AccountHistoryEvent> { new AccountHistoryEvent
                {
                    AccountHistoryEventType = AccountHistoryTypes.Creation,
                    EventTime = token.CallTime
                }};

            using (var session = NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    //Let's make sure no one with that SSN exists...
                    if (session.QueryOver<Person>().Where(x => x.SSN == newPerson.SSN).RowCount() != 0)
                    {
                        throw new CommandCentralException("A person with that SSN already exists.  Please consider using the search function to look for your user.", HttpStatusCodes.BadRequest);
                    }

                    //The person is a valid object.  Let's go ahead and insert it.  If insertion fails it's most likely because we violated a Uniqueness rule in the database.
                    session.Save(newPerson);

                    //And now return the person.
                    token.SetResult(newPerson.Id);

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

        #region Get/Load/Select/Search

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Loads a single person from the database and sets those fields to null that the client is not allowed to return.  If the client requests their own profile, all fields are returned.
        /// <para />
        /// Client Parameters: <para />
        ///     personid - The ID of the person to load.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void LoadPerson(MessageToken token)
        {
            token.AssertLoggedIn();
            token.Args.AssertContainsKeys("personid");

            if (!Guid.TryParse(token.Args["personid"] as string, out Guid personId))
                throw new CommandCentralException("The person Id you sent was not in the right format.", HttpStatusCodes.BadRequest);

            using (var session = NHibernateHelper.CreateStatefulSession())
            {
                //Now let's load the person and then set any fields the client isn't allowed to see to null.
                var person = session.Get<Person>(personId) ??
                    throw new CommandCentralException("The Id you sent appears to be invalid.", HttpStatusCodes.BadRequest);

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
        /// <para />
        /// Client Parameters: <para />
        ///     personid - The ID of the person for whom to return the account historiiiiies.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void LoadAccountHistoryByPerson(MessageToken token)
        {
            token.AssertLoggedIn();
            token.Args.AssertContainsKeys("personid");
            
            if (!Guid.TryParse(token.Args["personid"] as string, out Guid personId))
                throw new CommandCentralException("The person Id you sent was not in the right format.", HttpStatusCodes.BadRequest);

            //Let's load the person we were given.  We need the object for the permissions check.
            using (var session = NHibernateHelper.CreateStatefulSession())
            {
                //Now let's load the person and then set any fields the client isn't allowed to see to null.
                var person = session.Get<Person>(personId) ??
                    throw new CommandCentralException("The Id you sent appears to be invalid.", HttpStatusCodes.BadRequest);

                //Now let's get permissions and see if the client is allowed to view AccountHistory.
                bool canView = token.AuthenticationSession.Person.PermissionGroups.Resolve(token.AuthenticationSession.Person, person)
                    .ReturnableFields["Main"]["Person"].Contains(PropertySelector.SelectPropertiesFrom<Person>(x => x.AccountHistory).First().Name);

                if (!canView)
                    throw new CommandCentralException("You are not allowed to view the account history of this person's profile.", HttpStatusCodes.Unauthorized);

                token.SetResult(person.AccountHistory);
            }
        }

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Returns a person's chain of command.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void GetChainOfCommandOfPerson(MessageToken token)
        {
            token.AssertLoggedIn();
            token.Args.AssertContainsKeys("personid");

            if (!Guid.TryParse(token.Args["personid"] as string, out Guid personId))
                throw new CommandCentralException("The person Id you sent was not in the right format.", HttpStatusCodes.BadRequest);

            using (var session = NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    //Now let's load the person and then set any fields the client isn't allowed to see to null.
                    var person = session.Get<Person>(personId) ??
                        throw new CommandCentralException("The Id you sent appears to be invalid.", HttpStatusCodes.BadRequest);

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
        /// <para />
        /// Client Parameters: <para />
        ///     searchterm - A single string in which the search terms are broken up by a space.  Intended to be the exact input as given by the user.  This string will be split into an array of search terms by all whitespace.  The search terms are parameterized and no scrubbing of the user input is necessary.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void SimpleSearchPersons(MessageToken token)
        {
            token.AssertLoggedIn();
            token.Args.AssertContainsKeys("searchterm");

            string searchTerm = token.Args["searchterm"] as string;

            //Let's require a search term.  That's nice.
            if (String.IsNullOrEmpty(searchTerm))
                throw new CommandCentralException("You must send a search term. A blank term isn't valid. Sorry :(", HttpStatusCodes.BadRequest);

            bool showHidden = false;
            if (token.Args.ContainsKey("showhidden"))
            {
                showHidden = Convert.ToBoolean(token.Args["showhidden"]);
            }

            using (var session = NHibernateHelper.CreateStatefulSession())
            {
                var queryProvider = new Person.PersonQueryProvider();

                //Build the query over simple search for each of the search terms.  It took like a fucking week to learn to write simple search in NHibernate.
                var query = queryProvider.CreateQuery(QueryTypes.Simple, searchTerm);

                var simpleSearchMembers = queryProvider.GetMembersThatAreUsedIn(QueryTypes.Simple);

                //If we weren't told to show hidden, then hide hidden members.
                if (!showHidden)
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
        /// <para />
        /// Client Parameters: <para />
        ///     filters - The properties to search and the values to search for.
        ///     returnfields - The fields the client would like returned.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void AdvancedSearchPersons(MessageToken token)
        {
            token.AssertLoggedIn();
            token.Args.AssertContainsKeys("filters", "returnfields");

            Dictionary<string, object> filters = token.Args["filters"].CastJToken<Dictionary<string, object>>();

            //Ok, let's figure out what fields the client is allowed to search.
            //This is determined, in part by the existence of the searchlevel parameter.
            //If we don't find the level limit, then continue as normal.  However, if we do find a level limit, then we need to check the client's permissions.
            //We also need to throw out any property they gave us for the relevant level and insert our own.
            var resolvedPermissions = token.AuthenticationSession.Person.PermissionGroups.Resolve(token.AuthenticationSession.Person, null);
            if (token.Args.ContainsKey("searchlevel"))
            {
                //Ok there's a search level.  We need to do something different based on division, department or command.
                switch (token.Args["searchlevel"] as string)
                {
                    case "Division":
                        {
                            if (token.AuthenticationSession.Person.Division == null)
                                throw new CommandCentralException("You can't limit by division if you don't have a division.", HttpStatusCodes.BadRequest);

                            //First, if the filters have division, delete it.
                            if (filters.ContainsKey("Division"))
                                filters.Remove("Division");

                            //Now ask if the client is allowed to search in all these fields.
                            var failures = filters.Keys.Where(x => !resolvedPermissions.PrivelegedReturnableFields["Main"]["Division"].Concat(resolvedPermissions.ReturnableFields["Main"]["Person"]).Contains(x));

                            if (failures.Any())
                                throw new CommandCentralException("You were not allowed to search in these fields: {0}".FormatS(String.Join(", ", failures)), HttpStatusCodes.Forbidden);

                            break;
                        }
                    case "Department":
                        {
                            if (token.AuthenticationSession.Person.Department == null)
                                throw new CommandCentralException("You can't limit by department if you don't have a department.", HttpStatusCodes.BadRequest);

                            //First, if the filters have department, delete it.
                            if (filters.ContainsKey("Department"))
                                filters.Remove("Department");

                            //Now ask if the client is allowed to search in all these fields.
                            var failures = filters.Keys.Where(x => !resolvedPermissions.PrivelegedReturnableFields["Main"]["Department"].Concat(resolvedPermissions.ReturnableFields["Main"]["Person"]).Contains(x));

                            if (failures.Any())
                                throw new CommandCentralException("You were not allowed to search in these fields: {0}".FormatS(String.Join(", ", failures)), HttpStatusCodes.Forbidden);

                            break;
                            
                        }
                    case "Command":
                        {
                            if (token.AuthenticationSession.Person.Department == null)
                                throw new CommandCentralException("You can't limit by command if you don't have a command.", HttpStatusCodes.BadRequest);

                            //First, if the filters have command, delete it.
                            if (filters.ContainsKey("Command"))
                                filters.Remove("Command");

                            //Now ask if the client is allowed to search in all these fields.
                            var failures = filters.Keys.Where(x => !resolvedPermissions.PrivelegedReturnableFields["Main"]["Command"].Concat(resolvedPermissions.ReturnableFields["Main"]["Person"]).Contains(x));

                            if (failures.Any())
                                throw new CommandCentralException("You were not allowed to search in these fields: {0}".FormatS(String.Join(", ", failures)), HttpStatusCodes.Forbidden);

                            break;
                        }
                    default:
                        {
                            throw new CommandCentralException("The searchlevel you sent was not in the correct format.  " +
                                "It must only be Command, Department, or Division.", HttpStatusCodes.BadRequest);
                        }
                }
            }
            else
            {
                //We weren't told to limit the search at all, meaning the searchable fields are the client's normal returnable fields.
                //So let's just test the fields against that.
                var failures = filters.Keys.Where(x => !resolvedPermissions.ReturnableFields["Main"]["Person"].Contains(x));

                if (failures.Any())
                    throw new CommandCentralException("You were not allowed to search in these fields: {0}".FormatS(String.Join(", ", failures)), HttpStatusCodes.Forbidden);
            }

            //Now let's determine if we're doing a geo query.
            bool isGeoQuery = false;
            double centerLat = -1, centerLong = -1, radius = -1;

            //If the client sent any one of the three geoquery paramters, then make sure they sent all of them.
            if (token.Args.Keys.Any(x => x.SafeEquals("centerlat") || x.SafeEquals("centerlong") || x.SafeEquals("radius")))
            {
                if (!token.Args.ContainsKeys("centerlat", "centerlong", "radius"))
                    throw new CommandCentralException("If you send any geo query parameter, then you must send all of them.  They are 'centerlat', 'centerlong', 'radius'.", HttpStatusCodes.BadRequest);

                //Ok, we're doing a geo query.
                isGeoQuery = true;

                //Now parse everything.
                centerLat = (double)token.Args["centerlat"];
                centerLong = (double)token.Args["centerlong"];
                radius = (double)token.Args["radius"];
            }

            List<string> returnFields = token.Args["returnfields"].CastJToken<List<string>>();

            //Instruct us to show hidden profiles, or hide them.
            bool showHidden = false;
            if (token.Args.ContainsKey("showhidden"))
            {
                showHidden = (bool)token.Args["showhidden"];
            }

            //We're going to need the person object's metadata for the rest of this.
            var personMetadata = NHibernateHelper.GetEntityMetadata("Person");

            using (var session = NHibernateHelper.CreateStatefulSession())
            {
                var convertedFilters = filters.ToDictionary(
                    x => PropertySelector.SelectExpressionFrom<Person>(x.Key),
                    x => x.Value);

                var query = new Person.PersonQueryProvider().CreateQuery(QueryTypes.Advanced, convertedFilters);

                //So you remember the searchlevel from before?  We need to use that here.  If the client gave it to us.
                if (token.Args.ContainsKey("searchlevel"))
                {
                    //Ok there's a search level.  We need to do something different based on division, department or command.
                    switch (token.Args["searchlevel"] as string)
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
                if (!showHidden)
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
                                                        new Utilities.LatitudeAndLongitude { Latitude = centerLat, Longitude = centerLong },
                                                        Utilities.DistanceUnit.Miles);

                            //If they failed the distance check, then skip the person.
                            if (distance > radius)
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
                                                        new Utilities.LatitudeAndLongitude { Latitude = centerLat, Longitude = centerLong },
                                                        Utilities.DistanceUnit.Miles);

                                    //If we find one that passes, set passed to true and break.
                                    if (distance < radius)
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
                    foreach (var propertyName in returnFields)
                    {
                        var propertyInfo = PropertySelector.SelectPropertyFrom<Person>(propertyName) ??
                            throw new CommandCentralException("The field, '{0}', does not exist on the person object; therefore, you can not request that it be returned.".FormatS(propertyName), HttpStatusCodes.BadRequest);

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
        /// <para />
        /// Client Parameters: <para />
        ///     person - a properly formatted JSON person to be updated.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void UpdatePerson(MessageToken token)
        {
            token.AssertLoggedIn();
            token.Args.AssertContainsKeys("person");

            //Try the parse
            Person personFromClient;
            try
            {
                personFromClient = token.Args["person"].CastJToken<Person>() ??
                    throw new CommandCentralException("An error occurred while trying to parse the person into its proper form.", HttpStatusCodes.BadRequest);
            }
            catch
            {
                throw new CommandCentralException("An error occurred while trying to parse the person into its proper form.", HttpStatusCodes.BadRequest);
            }

            //Ok, so since we're ready to do ze WORK we're going to do it on a separate session.
            using (var session = NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    //Ok now we need to see if a lock exists for the person the client wants to edit.  Later we'll see if the client owns that lock.
                    ProfileLock profileLock = session.QueryOver<ProfileLock>()
                                            .Where(x => x.LockedPerson.Id == personFromClient.Id)
                                            .SingleOrDefault() ??
                                            throw new CommandCentralException("In order to edit this person, you must first take a lock on the person.", HttpStatusCodes.Forbidden);

                    //We also require the lock to be valid.
                    if (!profileLock.IsValid())
                        throw new CommandCentralException("Your lock on this profile is no longer valid.", HttpStatusCodes.Forbidden);

                    //Ok, well there is a lock on the person, now let's make sure the client owns that lock.
                    if (profileLock.Owner.Id != token.AuthenticationSession.Person.Id)
                        throw new CommandCentralException("The lock on this person is owned by '{0}' and will expire in {1} minutes unless the owner closes the profile prior to that.".FormatS(profileLock.Owner.ToString(), profileLock.GetTimeRemaining().TotalMinutes), HttpStatusCodes.LockOwned);

                    //Ok, so it's a valid person and the client owns the lock, now let's load the person by their ID, and see what they look like in the database.
                    Person personFromDB = session.Get<Person>(personFromClient.Id) ??
                        throw new CommandCentralException("The person you supplied had an Id that belongs to no actual person.", HttpStatusCodes.BadRequest);

                    var resolvedPermissions = token.AuthenticationSession.Person.PermissionGroups.Resolve(token.AuthenticationSession.Person, personFromDB);

                    //Get the editable and returnable fields and also those fields that, even if they are edited, will be ignored.
                    var editableFields = resolvedPermissions.EditableFields["Main"]["Person"];
                    var returnableFields = resolvedPermissions.ReturnableFields["Main"]["Person"];

                    //Go through all returnable fields that the client is allowed to edit and then move the values into the person from the database.
                    foreach (var field in returnableFields.Intersect(editableFields, StringComparer.CurrentCultureIgnoreCase))
                    {
                        var property = typeof(Person).GetProperty(field);

                        property.SetValue(personFromDB, property.GetValue(personFromClient));
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
                        throw new AggregateException(results.Errors.Select(x => new CommandCentralException(x.ErrorMessage, HttpStatusCodes.BadRequest)));

                    //Ok so the client only changed what they are allowed to see.  Now are those edits authorized.
                    var unauthorizedEdits = changes.Where(x => !editableFields.Contains(x.PropertyName));
                    if (unauthorizedEdits.Any())
                        throw new AggregateException(unauthorizedEdits.Select(x => new CommandCentralException("You lacked permission to edit the field '{0}'.".FormatS(x.PropertyName), HttpStatusCodes.Forbidden)));

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
