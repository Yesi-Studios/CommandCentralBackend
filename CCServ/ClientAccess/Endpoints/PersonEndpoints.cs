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
        [EndpointMethod(EndpointName = "CreatePerson", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void EndpointMethod_CreatePerson(MessageToken token)
        {
            //Just make sure the client is logged in.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to create a person.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            //You have permission?
            if (!token.AuthenticationSession.Person.PermissionGroups.Any(x => x.AccessibleSubModules.Contains(SubModules.CreatePerson.ToString(), StringComparer.CurrentCultureIgnoreCase)))
            {
                token.AddErrorMessage("You don't have permission to create persons.", ErrorTypes.Authorization, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            //Ok, since the client has permission to create a person, we'll assume they have permission to udpate all of the required fields.
            if (!token.Args.ContainsKey("person"))
            {
                token.AddErrorMessage("You failed to send a 'person' parameter!", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            var personFromClient = token.Args["person"].CastJToken<Person>();

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
                DateOfBirth = personFromClient.DateOfBirth,
                DateOfArrival = personFromClient.DateOfArrival,
                DutyStatus = personFromClient.DutyStatus,
                Id = Guid.NewGuid(),
                IsClaimed = false,
                PRD = personFromClient.PRD
            };
            newPerson.CurrentMusterStatus = MusterRecord.CreateDefaultMusterRecordForPerson(newPerson, token.CallTime);

            //We're also going to add on the default permission groups.
            newPerson.PermissionGroups = Authorization.Groups.PermissionGroup.AllPermissionGroups.Where(x => x.IsDefault).ToList();

            //Now for validation!
            var results = new Person.PersonValidator().Validate(newPerson);

            if (results.Errors.Any())
            {
                token.AddErrorMessages(results.Errors.Select(x => x.ErrorMessage), ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            //Cool, since everything is good to go, let's also add the account history.
            newPerson.AccountHistory = new List<AccountHistoryEvent> { new AccountHistoryEvent
            {
                AccountHistoryEventType = AccountHistoryTypes.Creation,
                EventTime = token.CallTime
            } };

            using (var session = NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    //Let's make sure no one with that SSN exists...
                    var result = session.QueryOver<Person>().Where(x => x.SSN == newPerson.SSN).SingleOrDefault();

                    if (result != null)
                    {
                        token.AddErrorMessage("A person with that SSN already exists.  Please consider using the search function to look for your user.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        return;
                    }

                    //The person is a valid object.  Let's go ahead and insert it.  If insertion fails it's most likely because we violated a Uniqueness rule in the database.
                    session.Save(newPerson);

                    //And now return the person's Id.
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
        [EndpointMethod(EndpointName = "LoadPerson", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void EndpointMethod_LoadPerson(MessageToken token)
        {

            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to load persons.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            //First, let's make sure the args are present.
            if (!token.Args.ContainsKey("personid"))
                token.AddErrorMessage("You didn't send a 'personid' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);

            //If there were any errors from the above checks, then stop now.
            if (token.HasError)
                return;

            Guid personId;
            if (!Guid.TryParse(token.Args["personid"] as string, out personId))
            {
                token.AddErrorMessage("The person Id you sent was not in the right format.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            using (var session = NHibernateHelper.CreateStatefulSession())
            {
                //Now let's load the person and then set any fields the client isn't allowed to see to null.
                var person = session.Get<Person>(personId);

                //If person is null then we need to stop here.
                if (person == null)
                {
                    token.AddErrorMessage("The Id you sent appears to be invalid.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                    return;
                }

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
                            case "currentmusterstatus":
                                {
                                    if (person.CurrentMusterStatus == null)
                                    {
                                        returnData.Add(propertyName, null);
                                    }
                                    else
                                    {
                                        returnData.Add(propertyName, new
                                        {
                                            person.CurrentMusterStatus.Command,
                                            person.CurrentMusterStatus.Department,
                                            person.CurrentMusterStatus.Division,
                                            person.CurrentMusterStatus.DutyStatus,
                                            person.CurrentMusterStatus.HasBeenSubmitted,
                                            person.CurrentMusterStatus.Id,
                                            person.CurrentMusterStatus.MusterDate,
                                            Musteree = person.CurrentMusterStatus.Musteree.ToBasicPerson(),
                                            Musterer = person.CurrentMusterStatus.Musterer == null ? null : person.CurrentMusterStatus.Musterer.ToBasicPerson(),
                                            person.CurrentMusterStatus.MusterStatus,
                                            person.CurrentMusterStatus.Paygrade,
                                            person.CurrentMusterStatus.SubmitTime,
                                            person.CurrentMusterStatus.UIC
                                        });
                                    }

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
        [EndpointMethod(AllowArgumentLogging = true, AllowResponseLogging = true, EndpointName = "LoadAccountHistoryByPerson", RequiresAuthentication = true)]
        private static void EndpointMethod_LoadAccountHistoryByPerson(MessageToken token)
        {
            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to view the news.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            //First, let's make sure the args are present.
            if (!token.Args.ContainsKey("personid"))
            {
                token.AddErrorMessage("You didn't send a 'personid' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            Guid personId;
            if (!Guid.TryParse(token.Args["personid"] as string, out personId))
            {
                token.AddErrorMessage("The person ID you sent was not in the right format.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            //Let's load the person we were given.  We need the object for the permissions check.
            using (var session = NHibernateHelper.CreateStatefulSession())
            {
                Person person = session.Get<Person>(personId);

                if (person == null)
                {
                    token.AddErrorMessage("The person Id you sent did not resolve to an actual person. :(", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                    return;
                }

                //Now let's get permissions and see if the client is allowed to view AccountHistory.
                bool canView = token.AuthenticationSession.Person.PermissionGroups.Resolve(token.AuthenticationSession.Person, person)
                    .ReturnableFields["Main"]["Person"].Contains(PropertySelector.SelectPropertiesFrom<Person>(x => x.AccountHistory).First().Name);

                if (!canView)
                {
                    token.AddErrorMessage("You don't have permission to view the account history for this person's profile.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                    return;
                }

                token.SetResult(session.Get<Person>(personId).AccountHistory);
            }
        }

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Returns a person's chain of command.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(EndpointName = "GetChainOfCommandOfPerson", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void EndpointMethod_GetChainOfCommandOfPerson(MessageToken token)
        {
            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to search.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            if (!token.Args.ContainsKey("personid"))
            {
                token.AddErrorMessage("You failed to send a 'personid' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            Guid personId;
            if (!Guid.TryParse(token.Args["personid"] as string, out personId))
            {
                token.AddErrorMessage("Your person id parameter was not in the correct format.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }
            Person person;
            using (var session = NHibernateHelper.CreateStatefulSession())
            {
                person = session.Get<Person>(personId);
            }

            if (person == null)
            {
                token.AddErrorMessage("Your person id parameter was not valid.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            var result = person.GetChainOfCommand();

            token.SetResult(result);
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
        [EndpointMethod(EndpointName = "SimpleSearchPersons", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void EndpointMethod_SimpleSearchPersons(MessageToken token)
        {
            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to search.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            //If you can search persons then we'll assume you can search/return the required fields.
            if (!token.Args.ContainsKey("searchterm"))
            {
                token.AddErrorMessage("You did not send a 'searchterm' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            string searchTerm = token.Args["searchterm"] as string;

            //Let's require a search term.  That's nice.
            if (String.IsNullOrEmpty(searchTerm))
            {
                token.AddErrorMessage("You must send a search term. A blank term isn't valid. Sorry :(", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            bool showHidden = false;
            if (token.Args.ContainsKey("showhidden"))
            {
                showHidden = (bool)token.Args["showhidden"];
            }

            using (var session = NHibernateHelper.CreateStatefulSession())
            {
                var queryProvider = new Person.PersonQueryProvider();

                //Build the query over simple search for each of the search terms.  It took like a fucking week to learn to write simple search in NHibernate.
                var resultToken = queryProvider.CreateSimpleSearchQuery(searchTerm);

                if (resultToken.HasErrors)
                {
                    token.AddErrorMessages(resultToken.Errors, ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                    return;
                }

                var simpleSearchMembers = queryProvider.GetMembersThatAreUsedIn(QueryTypes.Simple);

                //If we weren't told to show hidden, then hide hidden members.
                if (!showHidden)
                {
                    resultToken.Query = resultToken.Query.Where(x => x.DutyStatus != DutyStatuses.Loss);
                }
                
                
                //And finally, return the results.  We need to project them into only what we want to send to the client so as to remove them from the proxy shit that NHibernate has sullied them with.
                var results = resultToken.Query.GetExecutableQueryOver(session).List<Person>().Select(x =>
                {

                    //Do our permissions check here for each person.
                    var returnableFields = token.AuthenticationSession.Person.PermissionGroups.Resolve(token.AuthenticationSession.Person, x).ReturnableFields["Main"]["Person"];

                    Dictionary<string, string> result = new Dictionary<string, string>();
                    result.Add("Id", x.Id.ToString());
                    foreach (var member in simpleSearchMembers)
                    {
                        if (returnableFields.Contains(member.Name))
                        {
                            var value = (member as PropertyInfo).GetValue(x);

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
                    Fields = simpleSearchMembers.Select(x => x.Name)
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
        [EndpointMethod(EndpointName = "AdvancedSearchPersons", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void EndpointMethod_AdvancedSearchPersons(MessageToken token)
        {
            //Just make sure the client is logged in.  The endpoint's description should've handled this but you never know.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to search.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            //Let's find which fields the client wants to search in.  This should be a dictionary.
            if (!token.Args.ContainsKey("filters"))
            {
                token.AddErrorMessage("You didn't send a 'filters' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }
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
                            //The client wants to limit everything to their Division.  Sweet.  Do they have a division?
                            if (token.AuthenticationSession.Person.Division == null)
                            {
                                token.AddErrorMessage("You can't limit by division if you don't have a division.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                                return;
                            }

                            //First, if the filters have division, delete it.
                            if (filters.ContainsKey("Division"))
                                filters.Remove("Division");

                            //Now ask if the client is allowed to search in all these fields.
                            var failures = filters.Keys.Where(x => !resolvedPermissions.PrivelegedReturnableFields["Main"]["Division"].Concat(resolvedPermissions.ReturnableFields["Main"]["Person"]).Contains(x));

                            if (failures.Any())
                            {
                                token.AddErrorMessage("You were not allowed to search in these fields: {0}".FormatS(String.Join(", ", failures)), ErrorTypes.Authorization, System.Net.HttpStatusCode.Forbidden);
                                return;
                            }

                            break;
                        }
                    case "Department":
                        {
                            //The client wants to limit everything to their Department.  Sweet.
                            if (token.AuthenticationSession.Person.Department == null)
                            {
                                token.AddErrorMessage("You can't limit by department if you don't have a department.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                                return;
                            }

                            //First, if the filters have department, delete it.
                            if (filters.ContainsKey("Department"))
                                filters.Remove("Department");

                            //Now ask if the client is allowed to search in all these fields.
                            var failures = filters.Keys.Where(x => !resolvedPermissions.PrivelegedReturnableFields["Main"]["Department"].Concat(resolvedPermissions.ReturnableFields["Main"]["Person"]).Contains(x));

                            if (failures.Any())
                            {
                                token.AddErrorMessage("You were not allowed to search in these fields: {0}".FormatS(String.Join(", ", failures)), ErrorTypes.Authorization, System.Net.HttpStatusCode.Forbidden);
                                return;
                            }

                            break;
                        }
                    case "Command":
                        {
                            //The client wants to limit everything to their Command.  Sweet.
                            if (token.AuthenticationSession.Person.Command == null)
                            {
                                token.AddErrorMessage("You can't limit by command if you don't have a command.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                                return;
                            }

                            //First, if the filters have command, delete it.
                            if (filters.ContainsKey("Command"))
                                filters.Remove("Command");

                            //Now ask if the client is allowed to search in all these fields.
                            var failures = filters.Keys.Where(x => !resolvedPermissions.PrivelegedReturnableFields["Main"]["Command"].Concat(resolvedPermissions.ReturnableFields["Main"]["Person"]).Contains(x));

                            if (failures.Any())
                            {
                                token.AddErrorMessage("You were not allowed to search in these fields: {0}".FormatS(String.Join(", ", failures)), ErrorTypes.Authorization, System.Net.HttpStatusCode.Forbidden);
                                return;
                            }

                            break;
                        }
                    default:
                        {
                            token.AddErrorMessage("The searchlevel you sent was not in the correct format.  It must only be Command, Department, or Division.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                            return;
                        }
                }
            }
            else
            {
                //We weren't told to limit the search at all, meaning the searchable fields are the client's normal returnable fields.
                //So let's just test the fields against that.
                var failures = filters.Keys.Where(x => !resolvedPermissions.ReturnableFields["Main"]["Person"].Contains(x));

                if (failures.Any())
                {
                    //There were one or more fields you weren't allowed to search in.
                    token.AddErrorMessage("You weren't allowed to search in these fields: {0}".FormatS(String.Join(", ", failures)), ErrorTypes.Authorization, System.Net.HttpStatusCode.Forbidden);
                    return;
                }
            }

            //Now let's determine if we're doing a geo query.
            bool isGeoQuery = false;
            double centerLat = -1, centerLong = -1, radius = -1;

            //If the client sent any one of the three geoquery paramters, then make sure they sent all of them.
            if (token.Args.Keys.Any(x => x.SafeEquals("centerlat") || x.SafeEquals("centerlong") || x.SafeEquals("radius")))
            {
                if (!token.Args.ContainsKeys("centerlat", "centerlong", "radius"))
                {
                    token.AddErrorMessage("If you send any geo query parameter, then you must send all of them.  They are 'centerlat', 'centerlong', 'radius'.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                    return;
                }

                //Ok, we're doing a geo query.
                isGeoQuery = true;

                //Now parse everything.
                centerLat = (double)token.Args["centerlat"];
                centerLong = (double)token.Args["centerlong"];
                radius = (double)token.Args["radius"];
            }

            //And the fields the client wants to return.
            if (!token.Args.ContainsKey("returnfields"))
            {
                token.AddErrorMessage("You didn't send a 'returnfields' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
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
                //Ok the client can search and return everything.  Now we need to build the actual query.
                //To do this we need to determine what type each property is and then add it to the query.
                var queryOver = session.QueryOver<Person>();

                //So you remember the searchlevel from before?  We need to use that here.  If the client gave it to us.
                if (token.Args.ContainsKey("searchlevel"))
                {
                    //Ok there's a search level.  We need to do something different based on division, department or command.
                    switch (token.Args["searchlevel"] as string)
                    {
                        case "Division":
                            {

                                var disjunction = Restrictions.Disjunction();
                                disjunction.Add(Subqueries.WhereProperty<Person>(x => x.Division.Id).In(QueryOver.Of<Division>().Where(x => x.Id == token.AuthenticationSession.Person.Division.Id).Select(x => x.Id)));
                                queryOver = queryOver.Where(disjunction);

                                break;
                            }
                        case "Department":
                            {
                                var disjunction = Restrictions.Disjunction();
                                disjunction.Add(Subqueries.WhereProperty<Person>(x => x.Department.Id).In(QueryOver.Of<Department>().Where(x => x.Id == token.AuthenticationSession.Person.Department.Id).Select(x => x.Id)));
                                queryOver = queryOver.Where(disjunction);

                                break;
                            }
                        case "Command":
                            {
                                var disjunction = Restrictions.Disjunction();
                                disjunction.Add(Subqueries.WhereProperty<Person>(x => x.Command.Id).In(QueryOver.Of<Command>().Where(x => x.Id == token.AuthenticationSession.Person.Command.Id).Select(x => x.Id)));
                                queryOver = queryOver.Where(disjunction);

                                break;
                            }
                        default:
                            {
                                throw new NotImplementedException("Fell to the second searchlevel default in the advanced search endpoint.");
                            }
                    }
                }

                var convertedFilters = filters.ToDictionary(
                    x => (MemberInfo)PropertySelector.SelectPropertyFrom<Person>(x.Key),
                    x => x.Value);

                var resultQueryToken = new Person.PersonQueryProvider().CreateAdvancedQueryFor(convertedFilters, queryOver);

                //The client is telling us to show hidden profiles or not.
                if (!showHidden)
                {
                    resultQueryToken.Query = resultQueryToken.Query.Where(x => x.DutyStatus != DutyStatuses.Loss);
                }

                if (resultQueryToken.HasErrors)
                {
                    token.AddErrorMessages(resultQueryToken.Errors, ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                    return;
                }

                //Here we iterate over every returned person, do an authorization check and cast the results into DTOs.
                //Important note: the client expects every field to be a string.  We don't return object results. :(
                List<Dictionary<string, string>> result = new List<Dictionary<string, string>>();

                var rawResults = resultQueryToken.Query.GetExecutableQueryOver(session)
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
                    foreach (var propertyName in returnableFields)
                    {
                        var propertyInfo = PropertySelector.SelectPropertyFrom<Person>(propertyName);

                        //if the client isn't allowed to return this field, replace its value with "redacted"
                        if (returnableFields.Contains(propertyName))
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
        [EndpointMethod(EndpointName = "UpdatePerson", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void EndpointMethod_UpdatePerson(MessageToken token)
        {

            //First make sure we have a session.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to edit a person.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            //Ok, now we need to find the person the client sent us and try to parse it into a person.
            if (!token.Args.ContainsKey("person"))
            {
                token.AddErrorMessage("In order to update a person, you must send a person... ", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            //Try the parse
            Person personFromClient;
            try
            {
                personFromClient = token.Args["person"].CastJToken<Person>();

                if (personFromClient == null)
                {
                    token.AddErrorMessage("An error occurred while trying to parse the person into its proper form.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                    return;
                }
            }
            catch
            {
                token.AddErrorMessage("An error occurred while trying to parse the person into its proper form.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
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
                                            .SingleOrDefault();


                    //If we got no profile lock, then bail
                    if (profileLock == null)
                    {
                        token.AddErrorMessage("In order to edit this person, you must first take a lock on the person.", ErrorTypes.Authorization, System.Net.HttpStatusCode.Forbidden);
                        return;
                    }

                    //We also require the lock to be valid.
                    if (!profileLock.IsValid())
                    {
                        token.AddErrorMessage("Your lock on this profile is no longer valid.", ErrorTypes.Authorization, System.Net.HttpStatusCode.Forbidden);
                        return;
                    }

                    //Ok, well there is a lock on the person, now let's make sure the client owns that lock.
                    if (profileLock.Owner.Id != token.AuthenticationSession.Person.Id)
                    {
                        token.AddErrorMessage("The lock on this person is owned by '{0}' and will expire in {1} minutes unless the owner closes the profile prior to that.".FormatS(profileLock.Owner.ToString(), profileLock.GetTimeRemaining().TotalMinutes), ErrorTypes.LockOwned, System.Net.HttpStatusCode.Forbidden);
                        return;
                    }

                    //Ok, so it's a valid person and the client owns the lock, now let's load the person by their ID, and see what they look like in the database.
                    Person personFromDB = session.Get<Person>(personFromClient.Id);

                    //De-proxy the object.  I'm pretty sure we don't need due to our no proxy mappings this but I'm unwilling to test without it right now.
                    personFromDB = session.GetSessionImplementation().PersistenceContext.Unproxy(personFromDB) as Person;

                    //Did we get a person?  If not, the person the client gave us is bullshit.
                    if (personFromDB == null)
                    {
                        token.AddErrorMessage("The person you supplied had an Id that belongs to no actual person.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        return;
                    }

                    

                    var resolvedPermissions = token.AuthenticationSession.Person.PermissionGroups.Resolve(token.AuthenticationSession.Person, personFromDB);

                    //Get the editable and returnable fields and also those fields that, even if they are edited, will be ignored.
                    var editableFields = resolvedPermissions.EditableFields["Main"]["Person"];
                    var returnableFields = resolvedPermissions.ReturnableFields["Main"]["Person"];

                    //Go through all returnable fields that don't ignore edits and then move the values into the person from the database.
                    foreach (var field in returnableFields)
                    {
                        var property = typeof(Person).GetProperty(field);

                        property.SetValue(personFromDB, property.GetValue(personFromClient));
                    }

                    //Determine what changed.
                    var variances = session.GetDirtyProperties(personFromDB).ToList();

                    //Ok, let's validate the entire person object.  This will be what it used to look like plus the changes from the client.
                    var results = new Person.PersonValidator().Validate(personFromDB);

                    //If there are any errors with the validation, let's throw those back to the client.
                    if (results.Errors.Any())
                    {
                        token.AddErrorMessages(results.Errors.Select(x => x.ErrorMessage), ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                        return;
                    }

                    //Ok so the client only changed what they are allowed to see.  Now are those edits authorized.
                    var unauthorizedEdits = variances.Where(x => !editableFields.Contains(x.PropertyName));
                    if (unauthorizedEdits.Any())
                    {
                        token.AddErrorMessages(unauthorizedEdits.Select(x => "You lacked permission to edit the field '{0}'.".FormatS(x.PropertyName)), ErrorTypes.Authorization, System.Net.HttpStatusCode.Forbidden);
                        return;
                    }

                    //Here we determine any events we need to raise.  All variances are assumed to be correct.
                    List<Action> changeEvents = new List<Action>();

                    if (variances.Any(x => PropertySelector.SelectPropertiesFrom<Person>(y => y.FirstName, y => y.LastName, y => y.MiddleName)
                        .Select(y => y.Name).Any(y => x.PropertyName.SafeEquals(y))))
                    {
                        changeEvents.Add(() => new ChangeEventSystem.ChangeEvents.NameChangedEvent().RaiseEvent(new Email.Models.NameChangedEventEmailModel
                        {
                            NewName = personFromClient.ToString(),
                            OldName = personFromDB.ToString(),
                            PersonId = personFromDB.Id.ToString()
                        }, personFromDB));
                    }

                    //Ok, so the client is authorized to edit all the fields that changed.  Let's submit the update to the database.
                    session.Merge(personFromDB);

                    //Since everything went off well so far, let's fire off all the change events we found.
                    changeEvents.ForEach(x => x());

                    //And then we're done!
                    token.SetResult("Success");

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

        #region 

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Gets the metadata associated with the person object. Metadata describes the different properties of a person, how to search them, and more.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(EndpointName = "GetPersonMetadata", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void EndpointMethod_GetPersonMetadata(MessageToken token)
        {
            //Just make sure the client is logged in.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to access this endpoint.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

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
