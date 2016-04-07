using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace UnifiedServiceFramework.Framework
{
    /// <summary>
    /// Provides methods to help the host start the service.
    /// </summary>
    public static class ServiceManager
    {
        /// <summary>
        /// Describes all endpoints and information about them.  Endpoints not declared here can not be invoked through the generic endpoint invokation method and must be separately defined in the interface.
        /// <para />
        /// This dictionary's keys are not case sensitive.
        /// </summary>
        public static ConcurrentDictionary<string, EndpointDescription> EndpointDescriptions = new ConcurrentDictionary<string,EndpointDescription>();

        private static readonly List<KeyValuePair<string, EndpointDescription>> _unifiedEndpointDescriptions = new List<KeyValuePair<string, EndpointDescription>>()
        {
            new KeyValuePair<string, EndpointDescription>("LoadModelPermissions", new EndpointDescription() 
            {
                DataMethodAsync = Authorization.Permissions.LoadModelPermission_Client,
                Description = "Loads all permissions a user has to a given model.  This works by going through all permission groups the user is a part of and then collating all of the permissions together into a single object.",
                RequiresAuthentication = true,
                AllowArgumentLogging = true,
                AllowResponseLogging = true,
                RequiredParameters = new List<string>() 
                { 
                    "apikey - The unique GUID token assigned to your application for metrics purposes.", 
                    "authenticationtoken - The GUID authentication token for the user that was retrieved after successful login.", 
                    "model - Instructs the service which model to return permissions for.  If the model isn't in the parameters an error is thrown.  If the model isn't valid, a blank list is returned.  Not case senstive." 
                },
                IsActive = true
            }),
            new KeyValuePair<string, EndpointDescription>("LoadPermissionGroups", new EndpointDescription() 
            {
                DataMethodAsync = Authorization.Permissions.LoadPermissionGroups_Client,
                Description = "Loads all of the permission groups the client is a part of.  The permissions are loaded from the cache so if the permission groups have somehow been edited without updating the cache, strange things could start to happen.",
                RequiresAuthentication = true,
                AllowArgumentLogging = true,
                AllowResponseLogging = true,
                RequiredParameters = new List<string>() 
                { 
                    "apikey - The unique GUID token assigned to your application for metrics purposes.", 
                    "authenticationtoken - The GUID authentication token for the user that was retrieved after successful login.", 
                },
                IsActive = true
            }),
            new KeyValuePair<string, EndpointDescription>("LoadAllPermissionGroups", new EndpointDescription() 
            {
                DataMethodAsync = Authorization.Permissions.LoadAllPermissionGroups_Client,
                Description = "Loads all permission groups...",
                RequiresAuthentication = false,
                AllowArgumentLogging = true,
                AllowResponseLogging = true,
                RequiredParameters = new List<string>() 
                { 
                    "apikey - The unique GUID token assigned to your application for metrics purposes.", 
                    "authenticationtoken - The GUID authentication token for the user that was retrieved after successful login." 
                },
                IsActive = true
            })
        };



        /// <summary>
        /// Prepares the service to respond to client requests by initializing all the caches and setting up other things.
        /// </summary>
        /// <param name="communicationsWriter"></param>
        /// <param name="listeningPriorities"></param>
        /// <param name="connectionString"></param>
        /// <param name="customEndpointDescriptions"></param>
        /// <param name="cronOperations"></param>
        /// <param name="modelsAndFields"></param>
        /// <returns></returns>
        public static async Task InitializeService(TextWriter communicationsWriter, IEnumerable<Communicator.MessagePriority> listeningPriorities, string connectionString, 
            List<KeyValuePair<string, EndpointDescription>> customEndpointDescriptions, List<Action> cronOperations, Dictionary<string, List<string>> modelsAndFields)
        {
            try
            {
                DateTime start = DateTime.Now;
                //First, set up the communicator so we can post back to the client.
                Communicator.InitializeCommunicator(communicationsWriter, listeningPriorities.ToList());
                Communicator.Unfreeze();
                Communicator.PostMessageToHost("Communicator Initialized", Communicator.MessagePriority.Informational);

                //Now let's set up the framework's database connection
                Framework.Settings.ConnectionString = connectionString;
                await Diagnostics.TestDBConnection(Framework.Settings.ConnectionString);
                Communicator.PostMessageToHost("Database Connection Established", Communicator.MessagePriority.Informational);

                //Set up the permissions system
                foreach (var keyValuePair in modelsAndFields)
                {
                    if (!Authorization.Permissions.ModelsAndFields.TryAdd(keyValuePair.Key, new ConcurrentBag<string>(keyValuePair.Value)))
                        throw new Exception("There was an issue while initializing the models and fields cache.");
                }

                //Initialize all the caches.
                await Authentication.APIKeys.DBLoadAll(true);
                await Authentication.Sessions.DBLoadAll(true);
                await Authorization.Permissions.DBLoadAll(true);
                await MessageTokens.DBLoadAll(true, true);
                await Validation.SchemaValidation.LoadDatabaseSchema(Settings.ConnectionString);
                Communicator.PostMessageToHost("Caches Initialized", Communicator.MessagePriority.Informational);

                //Add the unified endpoint descriptions
                _unifiedEndpointDescriptions.ForEach(x =>
                    {
                        if (!EndpointDescriptions.TryAdd(x.Key, x.Value))
                            throw new Exception(string.Format("There was an issue with adding the unfied endpoint '{0}'!", x.Key));
                    });

                //Add the custom endpoints
                customEndpointDescriptions.ForEach(x =>
                    {
                        if (!EndpointDescriptions.TryAdd(x.Key, x.Value))
                            throw new Exception(string.Format("There was an issue with adding the endpoint '{0}'!", x.Key));
                    });
                Communicator.PostMessageToHost("Endpoints Registered", Communicator.MessagePriority.Informational);

                //Add the different cron operations and then start the cron operations timer.  Shut it down if it's already running.
                if (CronOperations.IsActive)
                    CronOperations.StopAndRelease();
                cronOperations.ForEach(x => CronOperations.RegisterCronOperation(x));
                CronOperations.StartCronOperations();
                Communicator.PostMessageToHost("Cron Operations Registered and Started", Communicator.MessagePriority.Informational);

                //Initialize the Files Provider
                FilesProvider.Initialize();
                Communicator.PostMessageToHost("Files Provider Initialized", Communicator.MessagePriority.Informational);

                Communicator.PostMessageToHost(string.Format("Completed In: {0} seconds\n", DateTime.Now.Subtract(start).TotalSeconds), Communicator.MessagePriority.Informational);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Releases all caches and memory used by the service.
        /// </summary>
        /// <returns></returns>
        public static void ReleaseService()
        {
            try
            {
                //Release all the different things we've been using.
                Framework.Settings.ConnectionString = null;
                Authentication.APIKeys.ReleaseCache();
                Authentication.Sessions.ReleaseCache();
                Authorization.Permissions.ReleaseCache();
                MessageTokens.ReleaseCache();
                EndpointDescriptions.Clear();
                CronOperations.StopAndRelease();
                FilesProvider.Release();

                //Clear the models cache
                Authorization.Permissions.ModelsAndFields.Clear();

                //Finally, inform the host that we've finished and then release the communicator.
                Communicator.PostMessageToHost("All resources used by the service have been released.  The communicator will now be released.", Communicator.MessagePriority.Informational);
                Communicator.ReleaseCommunicator();

            }
            catch
            {
                throw;
            }
        }


    }
}
