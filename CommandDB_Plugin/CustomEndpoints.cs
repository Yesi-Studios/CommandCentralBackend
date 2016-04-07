using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnifiedServiceFramework.Framework;
using AtwoodUtils;

namespace CommandDB_Plugin
{
    /// <summary>
    /// Contains's the plugin's custom endpoint descriptions that are supposed to be passed into the framework.
    /// </summary>
    public static class CustomEndpoints
    {

        /// <summary>
        /// Describes all endpoints and information about them.  Endpoints not declared here can not be invoked through the generic endpoint invokation method and must be separately defined in the interface.
        /// <para />
        /// This dictionary's keys are not case sensitive.
        /// </summary>
        public static ConcurrentDictionary<string,  EndpointDescription> CustomEndpointDescriptions = new ConcurrentDictionary<string, EndpointDescription>(new List<KeyValuePair<string, EndpointDescription>>()
        {
            new KeyValuePair<string, EndpointDescription>("Logout", new EndpointDescription() 
            {
                DataMethodAsync = AccountServices.Logout,
                Description = "Logs out the client by invalidating the session in both the cache and by updating it in the database.",
                RequiresAuthentication = true,
                AllowArgumentLogging = true,
                AllowResponseLogging = true,
                RequiredParameters = new List<string>() 
                { 
                    "apikey - The unique GUID token assigned to your application for metrics purposes.", 
                    "authenticationtoken - The GUID authentication token for the user that was retrieved after successful login." 
                },
                AuthorizationNote = "No authorization is done on this endpoint.",
                ExampleOutput = () => "Success - This return value can be ignored entirely and the string that is returned (“Success”) can be replaced with anything else.  Instead, I recommend checking the return container’s .HasError property.  If error is false, then you can assume the method completed successfully.",
                IsActive = true
            }),
            new KeyValuePair<string, EndpointDescription>("Login", new EndpointDescription() 
            {
                DataMethodAsync = AccountServices.Login,
                Description = "Logs in the user given a proper username/password combination and returns a GUID.  This GUID is the client's authentication token and must be included in all subsequent authentication-required requests.",
                RequiresAuthentication = false,
                AllowArgumentLogging = false,
                AllowResponseLogging = true,
                RequiredParameters = new List<string>() 
                { 
                    "apikey - The unique GUID token assigned to your application for metrics purposes.", 
                    "username - The user's case sensitive username.", 
                    "password - The user's case sensitive password." 
                },
                AuthorizationNote = "No authorization is done on this endpoint.",
                ExampleOutput = () => 
                    {
                        return new { PersonID = Guid.NewGuid().ToString(), AuthenticationToken = Guid.NewGuid().ToString() }.Serialize();
                    },
                IsActive = true
            }),
            new KeyValuePair<string, EndpointDescription>("BeginRegistration", new EndpointDescription() 
            {
                DataMethodAsync = AccountServices.BeginRegistration,
                Description = "Begins the registration process.",
                RequiresAuthentication = false,
                AllowArgumentLogging = false,
                AllowResponseLogging = true,
                RequiredParameters = new List<string>() 
                { 
                    "apikey - The unique GUID token assigned to your application for metrics purposes.", 
                    "ssn - The user's SSN.  SSNs are expected to consist of numbers only.  Non-digit characters will cause an exception." 
                },
                AuthorizationNote = "No authorization is done on this endpoint.",
                ExampleOutput = () => "Success - This return value can be ignored entirely and the string that is returned (“Success”) can be replaced with anything else.  Instead, I recommend checking the return container’s .HasError property.  If error is false, then you can assume the method completed successfully.",
                IsActive = true
            }),
            new KeyValuePair<string, EndpointDescription>("CompleteRegistration", new EndpointDescription() 
            {
                DataMethodAsync = AccountServices.CompleteRegistration_Client,
                Description = "Completes the registration process and assigns the username and password to the desired user account.",
                RequiresAuthentication = false,
                AllowArgumentLogging = false,
                AllowResponseLogging = true,
                RequiredParameters = new List<string>() 
                { 
                    "apikey - The unique GUID token assigned to your application for metrics purposes.", 
                    "username - The username the client wants to be assigned to the account.", 
                    "password - The password the client wants to be assigned to the account.", 
                    "emailconfirmationid - The unique GUID token that was sent to the user through their DOD email." 
                },
                AuthorizationNote = "No authorization is done on this endpoint.",
                ExampleOutput = () => "Success - This return value can be ignored entirely and the string that is returned (“Success”) can be replaced with anything else.  Instead, I recommend checking the return container’s .HasError property.  If error is false, then you can assume the method completed successfully.",
                IsActive = true
            }),
            new KeyValuePair<string, EndpointDescription>("InitiatePasswordReset", new EndpointDescription() 
            {
                DataMethodAsync = AccountServices.InitiatePasswordReset,
                Description = "Starts the password reset process by sending the client an email with a link they can click on to reset their password.",
                RequiresAuthentication = false,
                AllowArgumentLogging = true,
                AllowResponseLogging = true,
                RequiredParameters = new List<string>() 
                { 
                    "apikey - The unique GUID token assigned to your application for metrics purposes.", 
                    "ssn - The user's SSN.  SSNs are expected to consist of numbers only.  Non-digit characters will cause an exception.", 
                    "email - The email address of the account we want to reset.  This must be a DOD email address and be on the same account as the given SSN." 
                },
                ExampleOutput = () => "Success - This return value can be ignored entirely and the string that is returned (“Success”) can be replaced with anything else.  Instead, I recommend checking the return container’s .HasError property.  If error is false, then you can assume the method completed successfully.",
                IsActive = true
            }),
            new KeyValuePair<string, EndpointDescription>("FinishPasswordReset", new EndpointDescription() 
            {
                DataMethodAsync = AccountServices.FinishPasswordReset,
                Description = "Finishes the password reset process by setting the password to the received password for the reset password id.",
                RequiresAuthentication = false,
                AllowArgumentLogging = false,
                AllowResponseLogging = true,
                RequiredParameters = new List<string>() 
                { 
                    "apikey - The unique GUID token assigned to your application for metrics purposes.", 
                    "passwordresetid - The password reset id that was emailed to the client during the start password reset endpoint.", 
                    "password - The password the client wants the account to have." 
                },
                ExampleOutput = () => "Success - This return value can be ignored entirely and the string that is returned (“Success”) can be replaced with anything else.  Instead, I recommend checking the return container’s .HasError property.  If error is false, then you can assume the method completed successfully.",
                IsActive = true
            }),
            new KeyValuePair<string, EndpointDescription>("LoadLists", new EndpointDescription() 
            {
                DataMethodAsync = CDBLists.LoadLists_Client,
                Description = "Loads any or all of the dynamically defined lists from the Lists provider.",
                RequiresAuthentication = false,
                AllowArgumentLogging = true,
                AllowResponseLogging = true,
                RequiredParameters = new List<string>() 
                { 
                    "apikey - The unique GUID token assigned to your application for metrics purposes."
                },
                OptionalParameters = new List<string>()
                {
                    "acceptcachedresults - Instructs the service to returns all lists from either the database or the cache.  Default : true",
                    "name - The name of the list the client wants to load.  If not found or if it is empty, returns all of the lists.  Case sensitive."
                },
                AuthorizationNote = "No authorization is done on this endpoint.",
                ExampleOutput = () =>
                    {
                        return new List<CDBLists.CDBList>()
                        {
                            new CDBLists.CDBList()
                            {
                                ID = Guid.NewGuid().ToString(),
                                Name = "SomeList",
                                Values = new List<string>()
                                {
                                    "Item",
                                    "Another Item",
                                    "And Another"
                                }
                            },
                            new CDBLists.CDBList()
                            {
                                ID = Guid.NewGuid().ToString(),
                                Name = "SomeOtherList",
                                Values = new List<string>()
                                {
                                    "Item",
                                    "Another Item",
                                    "And Another"
                                }
                            }
                        }.Serialize();
                    },
                IsActive = true
            }),
            new KeyValuePair<string, EndpointDescription>("LoadMostRecentMainData", new EndpointDescription() 
            {
                DataMethodAsync = MainData.LoadMostRecentMainData,
                Description = "Loads the most recent main data.",
                RequiresAuthentication = false,
                AllowArgumentLogging = true,
                AllowResponseLogging = true,
                RequiredParameters = new List<string>() 
                { 
                    "apikey - The unique GUID token assigned to your application for metrics purposes."
                },
                OptionalParameters = new List<string>()
                {
                    "acceptcachedresults - Instructs the service to return the most recent main data from either the cache or from the database. Default : true"
                },
                ExampleOutput = () =>
                    {
                        return new MainData.MainDataItem()
                        {
                            ChangeLog = new List<MainData.MainDataItem.ChangeLogItem>()
                            {
                                new MainData.MainDataItem.ChangeLogItem()
                                {
                                    Changes = new List<string>()
                                    {
                                        "This is what what changed"
                                    }, 
                                    ID = Guid.NewGuid().ToString(),
                                    Time = DateTime.Now,
                                    Version = "0.21a"
                                },
                                new MainData.MainDataItem.ChangeLogItem()
                                {
                                    Changes = new List<string>()
                                    {
                                        "That is what what changed"
                                    }, 
                                    ID = Guid.NewGuid().ToString(),
                                    Time = DateTime.Now,
                                    Version = "0.20a"
                                }
                            },
                            ID = Guid.NewGuid().ToString(),
                            KnownIssues = new List<string>()
                            {
                                "These are the known issues."
                            },
                            Time = DateTime.Now,
                            Version = "0.21a"
                        }.Serialize();
                    },
                AuthorizationNote = "No authorization is done on this endpoint.  Anyone is allowed to request the main data.",
                IsActive = true
            }),
            new KeyValuePair<string, EndpointDescription>("CreateNewsItem", new EndpointDescription() 
            {
                DataMethodAsync = NewsItems.CreateNewEntry_Client,
                Description = "Creates a new news item entry in the database, ensuring the client has the right permission. The ID, the CreatorID, and the CreationTime will be set for you.  The GUID that is returned is the ID assigned to the news item as it was inserted.",
                RequiresAuthentication = true,
                AllowArgumentLogging = true,
                AllowResponseLogging = true,
                RequiredParameters = new List<string>() 
                { 
                    "apikey - The unique GUID token assigned to your application for metrics purposes.", 
                    "newsitem - A properly formed news item to insert.", 
                    "authenticationtoken - The GUID authentication token for the user that was retrieved after successful login." 
                },
                RequiredSpecialPermissions = new List<string>()
                {
                    "Manage_News"
                },
                AuthorizationNote = "Aside from Manage_News, no other authorization is done on this endpoint.",
                ExampleOutput = () => Guid.NewGuid().ToString(),
                IsActive = true
            }),
            new KeyValuePair<string, EndpointDescription>("DeleteNewsItem", new EndpointDescription() 
            {
                DataMethodAsync = NewsItems.DeleteEntry_Client,
                Description = "Deletes a given news item assuming the client has permission to manage news items.",
                RequiresAuthentication = true,
                AllowArgumentLogging = true,
                AllowResponseLogging = true,
                RequiredParameters = new List<string>() 
                { 
                    "apikey - The unique GUID token assigned to your application for metrics purposes.", 
                    "newsitemid - The ID of the news item you want to delete.", 
                    "authenticationtoken - The GUID authentication token for the user that was retrieved after successful login." 
                },
                IsActive = true
            }),
            new KeyValuePair<string, EndpointDescription>("LoadNewsItems", new EndpointDescription() 
            {
                DataMethodAsync = NewsItems.LoadEntries_Client,
                Description = "Loads all news entries for a client, so long as that client succeeded authentication.  Results are order desc by CreationTime.",
                RequiresAuthentication = true,
                AllowArgumentLogging = true,
                AllowResponseLogging = true,
                RequiredParameters = new List<string>() 
                { 
                    "apikey - The unique GUID token assigned to your application for metrics purposes.", 
                    "authenticationtoken - The GUID authentication token for the user that was retrieved after successful login." 
                },
                IsActive = true
            }),
            new KeyValuePair<string, EndpointDescription>("UpdateNewsItem", new EndpointDescription() 
            {
                DataMethodAsync = NewsItems.UpdateEntry_Client,
                Description = "Updates a news item after ensuring that it actually exists and the client has permission to manage the news.",
                RequiresAuthentication = true,
                AllowArgumentLogging = true,
                AllowResponseLogging = true,
                RequiredParameters = new List<string>() 
                { 
                    "apikey - The unique GUID token assigned to your application for metrics purposes.", 
                    "newsitem - A properly formed news item to insert.", 
                    "authenticationtoken - The GUID authentication token for the user that was retrieved after successful login." 
                },
                IsActive = true
            }),
            new KeyValuePair<string, EndpointDescription>("LoadCommands", new EndpointDescription() 
            {
                DataMethodAsync = Commands.LoadCommands_Client,
                Description = "Loads commands from the database including their departments and divisions.",
                RequiresAuthentication = false,
                AllowArgumentLogging = true,
                AllowResponseLogging = true,
                RequiredParameters = new List<string>() 
                { 
                    "apikey - The unique GUID token assigned to your application for metrics purposes."
                },
                OptionalParameters = new List<string>()
                {
                    "acceptcachedresults - Instructs to the service to either return the commands from the cache or to load them new.  Default : true",
                    "name - The name of the command the client wants to load.  If not found or if it is empty, returns all of the commands.  Not case sensitive."
                },
                IsActive = true
            }),
            new KeyValuePair<string, EndpointDescription>("GetProfileLock", new EndpointDescription() 
            {
                DataMethodAsync = ProfileLocks.GetProfileLock_Client,
                Description = "Given a person ID, gets the profile lock on the given person's profile.  Returns null if no locks are present.",
                RequiresAuthentication = true,
                AllowArgumentLogging = true,
                AllowResponseLogging = true,
                RequiredParameters = new List<string>() 
                { 
                    "apikey - The unique GUID token assigned to your application for metrics purposes.",
                    "authenticationtoken - The GUID authentication token for the user that was retrieved after successful login.",
                    "personid - the person for whom to check for locks and return the lock owner." 
                },
                IsActive = true
            }),
            new KeyValuePair<string, EndpointDescription>("TakeProfileLock", new EndpointDescription() 
            {
                DataMethodAsync = ProfileLocks.TakeProfileLock_Client,
                Description = "Given a person ID, attempts to take a lock on the profile in question.  This method will force a lock to release that is expired, and release the client's other lock, if any. This method will either return the ID of the person who owns the valid lock in an exception of type 'LockOwned' or will return 'Success' if the client is able to take the lock.",
                RequiresAuthentication = true,
                AllowArgumentLogging = true,
                AllowResponseLogging = true,
                RequiredParameters = new List<string>() 
                { 
                    "apikey - The unique GUID token assigned to your application for metrics purposes.", 
                    "authenticationtoken - The GUID authentication token for the user that was retrieved after successful login.",
                    "personid - the person for whom to check for locks and return the lock owner." 
                },
                IsActive = true
            }),
            new KeyValuePair<string, EndpointDescription>("ReleaseClientOwnedProfileLock", new EndpointDescription() 
            {
                DataMethodAsync = ProfileLocks.ReleaseClientOwnedProfileLock_Client,
                Description = "Releases any lock owned by the client and returns 'Success' whether or not a profile lock is released.",
                RequiresAuthentication = true,
                AllowArgumentLogging = true,
                AllowResponseLogging = true,
                RequiredParameters = new List<string>() 
                { 
                    "apikey - The unique GUID token assigned to your application for metrics purposes.", 
                    "authenticationtoken - The GUID authentication token for the user that was retrieved after successful login." 
                },
                IsActive = true
            }),
            new KeyValuePair<string, EndpointDescription>("ReleaseProfileLockByPerson", new EndpointDescription() 
            {
                DataMethodAsync = ProfileLocks.ReleaseProfileLockByPerson_Client,
                Description = "Releases the lock on a profile if the client is its owner or if the lock has expired.  If neither is true, than an authorization error is thrown.",
                RequiresAuthentication = true,
                AllowArgumentLogging = true,
                AllowResponseLogging = true,
                RequiredParameters = new List<string>() 
                { 
                    "apikey - The unique GUID token assigned to your application for metrics purposes.", 
                    "authenticationtoken - The GUID authentication token for the user that was retrieved after successful login.",
                    "personid - The ID of the person's profile for which to attempt to release a lock."
                },
                AuthorizationNote = "The client will be allowed to release a lock if they own it or if the lock is expired.  If neither of these are true, an authorization error will be thrown.",
                ExampleOutput = () => "Success - This return value can be ignored entirely and the string that is returned (“Success”) can be replaced with anything else.  Instead, I recommend checking the return container’s .HasError property.  If error is false, then you can assume the method completed successfully.",
                IsActive = true
            }),
            new KeyValuePair<string, EndpointDescription>("LoadAccountHistoryByPerson", new EndpointDescription() 
            {
                DataMethodAsync = AccountHistoryEvents.LoadAccountHistoryByPerson_Client,
                Description = "Loads the account history for a given person and limits the results by the given limit.  Results are ordered by EventTime, so the limit will return the most recent 'x' results.",
                RequiresAuthentication = true,
                AllowArgumentLogging = true,
                AllowResponseLogging = true,
                RequiredParameters = new List<string>() 
                { 
                    "apikey - The unique GUID token assigned to your application for metrics purposes.", 
                    "authenticationtoken - The GUID authentication token for the user that was retrieved after successful login.",
                    "personid - The person for whom the client wants to load account history events."
                },
                OptionalParameters = new List<string>()
                {
                    "limit - The first 'x' results the client wants to return.  Optional.  If omitted, all account histories are returned.  If the value is less than or equal to zero, an error is thrown."
                },
                AuthorizationNote = "The client must be allowed to return the field 'AccountHistory' from the 'Person' model.",
                ExampleOutput = () =>
                    {
                        return new List<AccountHistoryEvents.AccountHistoryEvent>()
                        {
                            new AccountHistoryEvents.AccountHistoryEvent()
                            {
                                AccountHistoryEventType = AccountHistoryEventTypes.Login,
                                EventTime = DateTime.Now,
                                ID = Guid.NewGuid().ToString(),
                                PersonID = Guid.NewGuid().ToString()
                            },
                            new AccountHistoryEvents.AccountHistoryEvent()
                            {
                                AccountHistoryEventType = AccountHistoryEventTypes.Login,
                                EventTime = DateTime.Now,
                                ID = Guid.NewGuid().ToString(),
                                PersonID = Guid.NewGuid().ToString()
                            },
                            new AccountHistoryEvents.AccountHistoryEvent()
                            {
                                AccountHistoryEventType = AccountHistoryEventTypes.Registration_Completed,
                                EventTime = DateTime.Now,
                                ID = Guid.NewGuid().ToString(),
                                PersonID = Guid.NewGuid().ToString()
                            }
                        }.Serialize();
                    },
                IsActive = true
            }),
            new KeyValuePair<string, EndpointDescription>("TranslatePersonIDToFriendlyName", new EndpointDescription() 
            {
                DataMethodAsync = Persons.TranslatePersonIDToFriendlyName_Client,
                Description = "Translates a given person ID to a user's friendly name.  This friendly name looks like {Rate} {LastName}, {FirstName} {MiddleName}.  Ex: CTI2 Atwood, Daniel K",
                RequiresAuthentication = true,
                AllowArgumentLogging = true,
                AllowResponseLogging = true,
                RequiredParameters = new List<string>() 
                { 
                    "apikey - The unique GUID token assigned to your application for metrics purposes.", 
                    "authenticationtoken - The GUID authentication token for the user that was retrieved after successful login.",
                    "personid - The ID of the person for whom the client wants a friendly name."
                },
                AuthorizationNote = "The client must be allowed to return the fields 'Rate', 'LastName', 'FirstName', and 'MiddleName' from the 'Person' model.",
                ExampleOutput = () => "CTI2 Atwood, Daniel K",
                IsActive = true
            }),
            new KeyValuePair<string, EndpointDescription>("LoadFullProfile", new EndpointDescription() 
            {
                DataMethodAsync = Persons.LoadFullProfile_Client,
                Description = "Loads all fields a client is able to load for a single user.  Uses an ID and the ID is checked to make sure it looks like a proper GUID.  Fields the client can not return a set to null or default." + 
                "  If the client asks to load a profile whose ID is the logged in user's ID, then the entire profile is returned regardless of returnable field permissions.  Finally, whether or not a 'My Profile' was loaded the fields the " + 
                "client can edit and return are included in the response as well as any errors on this profile - these errors indicate issues in validation.",
                RequiresAuthentication = true,
                AllowArgumentLogging = true,
                AllowResponseLogging = true,
                RequiredParameters = new List<string>() 
                { 
                    "apikey - The unique GUID token assigned to your application for metrics purposes.", 
                    "authenticationtoken - The GUID authentication token for the user that was retrieved after successful login.",
                    "personid - the ID of the person the client wants to load."
                },
                AuthorizationNote = "Any fields a client is not allowed to view are set to null.",
                ExampleOutput = () =>
                    {
                        return new { Person = new Persons.Person(), 
                            IsMyProfile = true, 
                            ReturnableFields = typeof(Persons.Person).GetProperties().Select(x => x.Name).ToList(), 
                            EditableFields = new List<string>(),
                            Errors = new List<string>()}.Serialize();
                    },
                IsActive = true
            }),
            new KeyValuePair<string, EndpointDescription>("SimpleSearchPersons", new EndpointDescription() 
            {
                DataMethodAsync = Persons.SimpleSearchPersons_Client,
                Description = "Searches for people in the persons table.  This uses a simple search algorithm: The search term is received as a string and then broken into terms by splitting the string by any whitespace." +
                    "  Then, the search terms are built into a SQL query such that each term is searched in every single field that the client wants it to search in." +
                    "  As long as each term appears in at least one field, the row is returned.  We also allow the client to indicate which fields should be returned." +
                    "  In order to enable this dynamic behavior, whitelisting is used and every field is checked for both validity, and authority prior to db interaction.",
                RequiresAuthentication = true,
                AllowArgumentLogging = true,
                AllowResponseLogging = true,
                RequiredParameters = new List<string>() 
                { 
                    "apikey - The unique GUID token assigned to your application for metrics purposes.", 
                    "authenticationtoken - The GUID authentication token for the user that was retrieved after successful login.", 
                    "searchterm - The search string we are going to use to search.  This string is split by any white space to create an array of search terms."
                },
                OptionalParameters = new List<string>()
                {
                    "limit - Indicates to the service how many results should be returned.  Must be an integer greater than zero.",
                    "orderby - Indicates by which field the results should be ordered.  Case sensitive.  The field must also be included in the client's 'returnfields' parameter."
                },
                RequiredSpecialPermissions = new List<string>()
                {
                    "Search_Users"
                },
                AuthorizationNote = "There is a predefined list of fields that are used in this endpoint for both search and return.  That list is returned with the search results.  If the client is not able to search or return these fields, an authorization error is thrown.",
                ExampleOutput = () =>
                    {
                        var result = new Dictionary<string, Dictionary<string, object>>();
                        result.Add(Guid.NewGuid().ToString(), new Dictionary<string, object>()
                        {
                            { "LastName", "Atwood" },
                            { "FirstName", "Daniel" },
                            { "Rate", "CTI2" },
                        });
                        result.Add(Guid.NewGuid().ToString(), new Dictionary<string, object>()
                        {
                            { "LastName", "McLean" },
                            { "FirstName", "Angus" },
                            { "Rate", "CTI2" },
                        });

                        var final = new { ResultsCount = result.Count, SearchTime = TimeSpan.FromMilliseconds(40), Results = result, SimpleSearchFields = Persons.SimpleSearchFields };

                        return final.Serialize();
                    },
                IsActive = true
            }),
            new KeyValuePair<string, EndpointDescription>("AdvancedSearchPersons", new EndpointDescription() 
            {
                DataMethodAsync = Persons.AdvancedSearchPersons_Client,
                Description = "Searches for people in the persons table.  This uses a key/value pair search scheme.  Keys must be real fields from the Person model and the values are any value you want to search for." +
                    "  Values are searched for using LIKE and the value is additionally wildcarded.  This allows 'atw' to return 'Atwood'.",
                RequiresAuthentication = true,
                AllowArgumentLogging = true,
                AllowResponseLogging = true,
                RequiredParameters = new List<string>() 
                { 
                    "apikey - The unique GUID token assigned to your application for metrics purposes.", 
                    "authenticationtoken - The GUID authentication token for the user that was retrieved after successful login.", 
                    "filters - A list of key/value pairs.  The key is the field to search in while the value is the value to search for.",
                    "returnfields - A list of fields from the Person model you want returned for your search."
                },
                OptionalParameters = new List<string>()
                {
                    "limit - Indicates to the service how many results should be returned.  Must be an integer greater than zero.",
                    "orderby - Indicates by which field the results should be ordered.  Case sensitive.  The field must also be included in the client's 'returnfields' parameter."
                },
                RequiredSpecialPermissions = new List<string>()
                {
                    "Search_Users"
                },
                AuthorizationNote = "The client must be allowed to return all of the fields they request and be allowed to search all of the fields they request to search in.",
                ExampleOutput = () =>
                    {
                        var result = new Dictionary<string, Dictionary<string, object>>();
                        result.Add(Guid.NewGuid().ToString(), new Dictionary<string, object>()
                        {
                            { "LastName", "Atwood" },
                            { "FirstName", "Daniel" },
                            { "Rate", "CTI2" },
                        });
                        result.Add(Guid.NewGuid().ToString(), new Dictionary<string, object>()
                        {
                            { "LastName", "McLean" },
                            { "FirstName", "Angus" },
                            { "Rate", "CTI2" },
                        });

                        var final = new { ResultsCount = result.Count, SearchTime = TimeSpan.FromMilliseconds(40), Results = result };

                        return final.Serialize();
                    },
                IsActive = true
            }),
            new KeyValuePair<string, EndpointDescription>("UpdatePerson", new EndpointDescription() 
            {
                DataMethodAsync = Persons.UpdatePerson_Client,
                Description = "Updates a person, given a person object.  To do this, the client sends a person and we load that person from the database and then compare the two objects.  " +
                "Looking at the variances between the two objects, we throw out any fields the client isn't allowed to view - changes to these fields are ignored.  Then, we look at those fields, which are now the viewable variances,  " +
                "and we ask if the client is allowed to edit those fields.  If so, we pass only these variances into the database to be updated.  Failing the authorization check to edit any fields  " + 
                "will result in a generic 'you could not update one or more fields' message.  Additionally, the client must own a lock on this profile.  The backend does not enforce when the lock must've been taken,  " +
                "only that the lock must've been taken prior to the call to UpdatePerson.  If no lock is owned, an error is thrown.  If the lock is owned by someone else, their friendly name will be included in the error message.  " + 
                "This endpoint does not release locks after update.  If you want the lock released, follow up a successful call to this endpoint with a call to ReleaseLock.",
                RequiresAuthentication = true,
                AllowArgumentLogging = true,
                AllowResponseLogging = true,
                RequiredParameters = new List<string>()
                {
                    "apikey - The unique GUID token assigned to your application for metrics purposes.", 
                    "authenticationtoken - The GUID authentication token for the user that was retrieved after successful login.", 
                    "person - A properly formed person object to attempt to update."
                },
                AuthorizationNote = "Clients must be able to edit any fields that are changed in the person object as it is sent and must own a lock to the person the client wishes to update.",
                ExampleOutput = () => "Success - This return value can be ignored entirely and the string that is returned (“Success”) can be replaced with anything else.  Instead, I recommend checking the return container’s .HasError property.  If error is false, then you can assume the method completed successfully.",
                RequiredSpecialPermissions = new List<string>()
                {
                    "Edit_Users"
                },
                IsActive = true,
                
            }),
            /*
            new KeyValuePair<string, EndpointDescription>("CreatePerson", new EndpointDescription() 
            {
                DataMethodAsync = Persons.CreateNewPerson_Client,
                Description = "Creates a new person record, leaving all of its fields blank.  Further updates should use the UpdatePerson endpoint.",
                RequiresAuthentication = true,
                AllowArgumentLogging = true,
                AllowResponseLogging = true,
                Parameters = new List<string>() { "apikey - The unique GUID token assigned to your application for metrics purposes.", "authenticationtoken - The GUID authentication token for the user that was retrieved after successful login." },
                IsActive = true
            }),
            
            new KeyValuePair<string, EndpointDescription>("LoadPersonChangeLog", new EndpointDescription() 
            {
                DataMethodAsync = Changes.LoadChangesByPersonAsync,
                Description = "Loads all changes for a given user.",
                RequiresAuthentication = true,
                AllowArgumentLogging = true,
                AllowResponseLogging = true,
                Parameters = new List<string>() { "apikey - The unique GUID token assigned to your application for metrics purposes.", "authenticationtoken - The GUID authentication token for the user that was retrieved after successful login.", "personid" },
                IsActive = true
            }),
            
            new KeyValuePair<string, EndpointDescription>("LoadChangeEvents", new EndpointDescription() 
            {
                DataMethodAsync = ChangeEvents.LoadChangeEventsAsync,
                Description = "Loads all change events from the database or from the cache.",
                RequiresAuthentication = false,
                AllowArgumentLogging = true,
                AllowResponseLogging = true,
                Parameters = new List<string>() { "apikey - The unique GUID token assigned to your application for metrics purposes.", "name", "acceptcachedresults" },
                IsActive = true
            }),
            /*new KeyValuePair<string, EndpointDescription>("LoadMusterRecords", new EndpointDescription() 
            {
                DataMethodAsync = MusterRecords.LoadMusterForClientAsync,
                Description = "Loads the muster.",
                RequiresAuthentication = true,
                AllowArgumentLogging = true,
                AllowResponseLogging = true,
                Parameters = new List<string>() { "apikey - The unique GUID token assigned to your application for metrics purposes.", "date", "acceptcachedresults" },
                IsActive = true
            }),
            new KeyValuePair<string, EndpointDescription>("UpdateOrInsertMusterRecord", new EndpointDescription() 
            {
                DataMethodAsync = MusterRecords.UpdateOrInsertMusterRecordAsync,
                Description = "Updates or inserts a new muster record for the client.",
                RequiresAuthentication = true,
                AllowArgumentLogging = true,
                AllowResponseLogging = true,
                Parameters = new List<string>() { "apikey - The unique GUID token assigned to your application for metrics purposes.", "musterrecord" },
                IsActive = true
            })*/

        }, StringComparer.OrdinalIgnoreCase);

    }
}
