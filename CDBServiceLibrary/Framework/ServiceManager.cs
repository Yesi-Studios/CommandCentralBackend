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
        public static async Task InitializeService(TextWriter communicationsWriter, IEnumerable<Communicator.MessagePriority> listeningPriorities, 
            List<Action> cronOperations)
        {
            try
            {
                DateTime start = DateTime.Now;
                //First, set up the communicator so we can post back to the client.
                Communicator.InitializeCommunicator(communicationsWriter, listeningPriorities.ToList());
                Communicator.Unfreeze();
                Communicator.PostMessageToHost("Communicator Initialized", Communicator.MessagePriority.Informational);

                //Initialize all the caches.
                await Authentication.APIKeys.DBLoadAll(true);
                await Authentication.Sessions.DBLoadAll(true);
                await Authorization.Permissions.DBLoadAll(true);
                await MessageTokens.DBLoadAll(true, true);

                //Add the different cron operations and then start the cron operations timer.  Shut it down if it's already running.
                if (CronOperations.IsActive)
                    CronOperations.StopAndRelease();
                cronOperations.ForEach(x => CronOperations.RegisterCronOperation(x));
                CronOperations.StartCronOperations();
                Communicator.PostMessageToHost("Cron Operations Registered and Started", Communicator.MessagePriority.Informational);

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
