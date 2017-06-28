using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;
using CommandCentral.Logging;
using CommandCentral.ServiceManagement;
using CommandCentral.ServiceManagement.Service;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CommandCentral.ServiceManagement
{
    /// <summary>
    /// Contains the actual web host instance and manages stopping and starting the service.
    /// </summary>
    public static class ServiceManager
    {
        private static WebServiceHost _host = null;

        private static CLI.Options.LaunchOptions _options = null;

        /// <summary>
        /// The list of all endpoints contained throughout the application.
        /// </summary>
        public static ConcurrentDictionary<string, ClientAccess.ServiceEndpoint> EndpointDescriptions { get; set; }

        /// <summary>
        /// The registry used for setting up reoccurring, concurrent jobs.
        /// </summary>
        public static FluentScheduler.Registry FluentSchedulerRegistry { get; set; }

        /// <summary>
        /// Starts the service with the given parameters.  By the end of this method, the application will be listening on the assigned port or it will fail.
        /// </summary>
        /// <param name="launchOptions"></param>
        public static void StartService(CLI.Options.LaunchOptions launchOptions)
        {
            try
            {

                Log.RegisterLoggers();
                Email.EmailInterface.CCEmailMessage.InitializeEmail(launchOptions.SMTPHosts);

                //Do arg validation.
                if ((launchOptions.Rebuild) && 
                    String.Equals(launchOptions.Database, "command_central", StringComparison.CurrentCultureIgnoreCase) &&
                    String.Equals(launchOptions.Server, "147.51.62.50", StringComparison.CurrentCultureIgnoreCase))
                    throw new Exception("Go fuck yourself and try to delete the production database or sully it with your garbage.");

                //Let's determine if our given port is usable.
                if (!Utilities.IsPortAvailable(launchOptions.Port))
                {
                    throw new Exception("It appears the port '{0}' is already in use. We cannot continue from this.");
                }

                _options = launchOptions;

                Log.Info("Starting service startup...");

                FluentScheduler.JobManager.UseUtcTime();
                FluentSchedulerRegistry = new FluentScheduler.Registry();
                FluentScheduler.JobManager.Initialize(FluentSchedulerRegistry);

                //Now we need to run all start up methods.
                //First let's make our connection string.
                var rawConnectionString = "server={0};database={1};user={2};password={3};CertificatePassword={4};SSL Mode={5}"
                    .With(launchOptions.Server, launchOptions.Database, launchOptions.Username, launchOptions.Password,
                    launchOptions.CertificatePassword, launchOptions.SecurityMode == CLI.SecurityModes.Both || launchOptions.SecurityMode == CLI.SecurityModes.DBOnly ? "Required" : "None");

                MySql.Data.MySqlClient.MySqlConnectionStringBuilder connectionString = new MySql.Data.MySqlClient.MySqlConnectionStringBuilder(rawConnectionString);

                if (launchOptions.Rebuild)
                {
                    DataAccess.DataProvider.InitializeAndRebuild(connectionString, launchOptions.Database);
                    Entities.Watchbill.WatchAssignment.WatchAssignmentMapping.UpdateForeignKeyRule();

                    //Set up all the pre def lists.
                    PreDefs.PreDefUtility.PersistPreDef<Entities.ReferenceLists.WatchQualification>();
                    PreDefs.PreDefUtility.PersistPreDef<Entities.ReferenceLists.Sex>();
                    PreDefs.PreDefUtility.PersistPreDef<Entities.ReferenceLists.Watchbill.WatchbillStatus>();
                }
                else
                {
                    DataAccess.DataProvider.Initialize(connectionString);
                }

                ClientAccess.ServiceEndpoint.ScanEndpoints();

                Entities.MusterRecord.SetupMuster();
                Entities.Watchbill.Watchbill.SetupAlerts();


                //All startup methods have run, now we need to launch the service itself.

                //Ok, so now we have a valid port.  Let's set up the service.
                if (launchOptions.SecurityMode == CLI.SecurityModes.HTTPSOnly || launchOptions.SecurityMode == CLI.SecurityModes.Both)
                {
                    _host = new WebServiceHost(typeof(CommandCentralService), new Uri("https://localhost:" + launchOptions.Port));
                    _host.AddServiceEndpoint(typeof(ICommandCentralService), new WebHttpBinding() { Security = new WebHttpSecurity { Mode = WebHttpSecurityMode.Transport }, MaxBufferPoolSize = 2147483647, MaxReceivedMessageSize = 2147483647, MaxBufferSize = 2147483647, TransferMode = TransferMode.Streamed }, "");
                    ServiceDebugBehavior stp = _host.Description.Behaviors.Find<ServiceDebugBehavior>();
                    stp.HttpHelpPageEnabled = false;
                }
                else
                {
                    _host = new WebServiceHost(typeof(CommandCentralService), new Uri("http://localhost:" + launchOptions.Port));
                    _host.AddServiceEndpoint(typeof(ICommandCentralService), new WebHttpBinding() { Security = new WebHttpSecurity { Mode = WebHttpSecurityMode.None }, MaxBufferPoolSize = 2147483647, MaxReceivedMessageSize = 2147483647, MaxBufferSize = 2147483647, TransferMode = TransferMode.Streamed }, "");
                    ServiceDebugBehavior stp = _host.Description.Behaviors.Find<ServiceDebugBehavior>();
                    stp.HttpHelpPageEnabled = false;
                }
                
                _host.Open();

                Log.Info("Service is live and listening on '{0}'.".With(_host.BaseAddresses.First().AbsoluteUri));
            }
            catch (Exception e)
            {
                Log.Exception(e, "An error occurred during service start up");
            }
        }

        /// <summary>
        /// Closes the host.
        /// </summary>
        public static void StopService()
        {
            if (_host != null && _host.State != CommunicationState.Closed)
                _host.Close();
        }
    }
}
