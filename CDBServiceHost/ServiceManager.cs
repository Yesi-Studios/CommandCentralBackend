using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.ServiceModel.Description;
using System.Threading;
using System.ServiceModel.Activation;
using System.Threading.Tasks;

namespace CDBServiceHost
{
    /// <summary>
    /// Exposes members that assist the hosting of the service.  Namely, the actual WebServiceHost object is stored here along with methods to start and stop it.
    /// </summary>
    public static class ServiceManager
    {
        /// <summary>
        /// This is the WebServiceHost.  It is through this host that the service is exposed.
        /// </summary>
        private static WebServiceHost _host;

        /// <summary>
        /// An accessor that enforces readonly access to the host from outside of this service.
        /// </summary>
        public static WebServiceHost Host
        {
            get
            {
                return _host;
            }
        }

        /// <summary>
        /// Initializes the service host and any information the service needs to function.  This also initializes the caches.
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public static void InitializeService(int port)
        {
            try
            {
                _host = new WebServiceHost(typeof(UnifiedServiceFramework.UnifiedService), new Uri("http://localhost:" + port));
                ServiceEndpoint ep = _host.AddServiceEndpoint(typeof(UnifiedServiceFramework.IUnifiedService), new WebHttpBinding(), "");
                ServiceDebugBehavior stp = _host.Description.Behaviors.Find<ServiceDebugBehavior>();
                stp.HttpHelpPageEnabled = false;

                //Initialize the command db plugin's custom lists and caches
                CommandCentral.CDBLists.DBLoadAll(true).Wait();
                CommandCentral.Commands.DBLoadAll(true).Wait();
                CommandCentral.MainData.DBLoadAll(true).Wait();
                CommandCentral.ChangeEvents.DBLoadAll(true).Wait();

                Console.WriteLine("Command DB Plugin caches loaded.");
                Console.WriteLine();

                //Call the framework's initialization method, but first we need to set up the models and fields to send to it.
                var modelsAndFields = new Dictionary<string, List<string>>();

                //Add the Person model
                modelsAndFields.Add("Person", typeof(CommandCentral.Persons.Person).GetProperties().Select(x => x.Name).ToList());

                //Now call the method
                UnifiedServiceFramework.Framework.ServiceManager.InitializeService(Console.Out, new List<UnifiedServiceFramework.Communicator.MessagePriority>()
                {
                    UnifiedServiceFramework.Communicator.MessagePriority.Critical,
                    UnifiedServiceFramework.Communicator.MessagePriority.Important,
                    UnifiedServiceFramework.Communicator.MessagePriority.Informational,
                    UnifiedServiceFramework.Communicator.MessagePriority.Warning
                }, CommandCentral.Properties.ConnectionString, CommandCentral.CustomEndpoints.CustomEndpointDescriptions.ToList(), new List<Action>()
                {
                    () => UnifiedServiceFramework.Authentication.Sessions.ScrubSessions(),
                    () => UnifiedServiceFramework.Framework.MessageTokens.ScrubMessages()
                }, modelsAndFields).Wait();
                
                

                Console.WriteLine("Service initialized and bound...");
                Console.WriteLine();
                
                Console.WriteLine("Checking external internet connection via google.com...");
                try
                {
                    System.Net.NetworkInformation.Ping myPing = new System.Net.NetworkInformation.Ping();
                    String host = "google.com";
                    byte[] buffer = new byte[32];
                    int timeout = 1000;
                    System.Net.NetworkInformation.PingOptions pingOptions = new System.Net.NetworkInformation.PingOptions();
                    System.Net.NetworkInformation.PingReply reply = myPing.Send(host, timeout, buffer, pingOptions);
                    if ((reply.Status == System.Net.NetworkInformation.IPStatus.Success))
                    {
                        Console.WriteLine("External internet connection established via google.com...");
                        Console.WriteLine(string.Format("External internet connection speed to google.com is {0}ms...", reply.RoundtripTime));
                    }
                    else
                    {
                        Console.WriteLine("Failed to establish external internet connection via google.com...");
                    }
                }
                catch
                {
                    Console.WriteLine("Failed to establish external internet connection via google.com...");
                }

                Console.WriteLine();

                Console.WriteLine(string.Format("Checking Unified Service smtp server ({0}) connection...", UnifiedServiceFramework.UnifiedEmailHelper.SmtpHost));
                try
                {
                    System.Net.NetworkInformation.Ping myPing = new System.Net.NetworkInformation.Ping();
                    String host = UnifiedServiceFramework.UnifiedEmailHelper.SmtpHost;
                    byte[] buffer = new byte[32];
                    int timeout = 1000;
                    System.Net.NetworkInformation.PingOptions pingOptions = new System.Net.NetworkInformation.PingOptions();
                    System.Net.NetworkInformation.PingReply reply = myPing.Send(host, timeout, buffer, pingOptions);
                    if ((reply.Status == System.Net.NetworkInformation.IPStatus.Success))
                    {
                        Console.WriteLine(string.Format("Unfied Service smtp server ({0}) connection established", UnifiedServiceFramework.UnifiedEmailHelper.SmtpHost));
                        Console.WriteLine(string.Format("SMTP server connection speed is {0}ms...", reply.RoundtripTime));
                    }
                    else
                    {
                        Console.WriteLine(string.Format("Unfied Service smtp server ({0}) connection failed", UnifiedServiceFramework.UnifiedEmailHelper.SmtpHost));
                    }
                }
                catch
                {
                    Console.WriteLine(string.Format("Unified Service smtp server ({0}) connection failed", UnifiedServiceFramework.UnifiedEmailHelper.SmtpHost));
                }

                Console.WriteLine();

                Console.WriteLine(string.Format("Checking Command DB Plugin smtp server ({0}) connection...", UnifiedServiceFramework.UnifiedEmailHelper.SmtpHost));
                try
                {
                    System.Net.NetworkInformation.Ping myPing = new System.Net.NetworkInformation.Ping();
                    String host = CommandCentral.EmailHelper.SmtpHost;
                    byte[] buffer = new byte[32];
                    int timeout = 1000;
                    System.Net.NetworkInformation.PingOptions pingOptions = new System.Net.NetworkInformation.PingOptions();
                    System.Net.NetworkInformation.PingReply reply = myPing.Send(host, timeout, buffer, pingOptions);
                    if ((reply.Status == System.Net.NetworkInformation.IPStatus.Success))
                    {
                        Console.WriteLine(string.Format("Command DB Plugin smtp server ({0}) connection established", CommandCentral.EmailHelper.SmtpHost));
                        Console.WriteLine(string.Format("SMTP server connection speed is {0}ms...", reply.RoundtripTime));
                    }
                    else
                    {
                        Console.WriteLine(string.Format("Command DB Plugin smtp server ({0}) connection failed", CommandCentral.EmailHelper.SmtpHost));
                    }
                }
                catch
                {
                    Console.WriteLine(string.Format("Command DB Plugin smtp server ({0}) connection failed", CommandCentral.EmailHelper.SmtpHost));
                }

                //Give the user the option of cross checking the permissions.
                CrossCheckPermissionsAgainstSchema();

            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Released all resource used by the service by first releasing the plugin's resources and then releasing the Framework's
        /// </summary>
        public static void ReleaseService()
        {
            try
            {
                //Release the plugin's resources
                CommandCentral.CDBLists.ReleaseCache();
                CommandCentral.ChangeEvents.ReleaseCache();
                CommandCentral.Commands.ReleaseCache();
                CommandCentral.MainData.ReleaseCache();

                //Now release the framework's resources
                UnifiedServiceFramework.Framework.ServiceManager.ReleaseService();
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Opens the service.
        /// </summary>
        public static void StartService()
        {
            try
            {
                _host.Open();
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Stops the service and releases the host's memory.
        /// </summary>
        public static void StopService()
        {
            try
            {
                _host.Close();
                _host = null;
            }
            catch
            {

                throw;
            }
        }

        /// <summary>
        /// This method goes through all of the permission groups and determines if there is a table permission that references a column in a table that no longer exists.
        /// <para />
        /// If we find such a column, we give the user an option to ignore the issue or delete the column name.
        /// </summary>
        private static void CrossCheckPermissionsAgainstSchema()
        {
            Console.WriteLine("Do you want to cross check the permission groups against the database schematic?  (y)");

            if (Console.ReadKey().KeyChar.ToString().ToLower() == "y")
            {

                bool restart = true;

                while (restart)
                {
                    restart = false;

                    Console.WriteLine("Cross checking permission groups against db schema for discrepancies...");

                    for (int x = 0; x < UnifiedServiceFramework.Authorization.Permissions.GetAllPermissionGroups().Count; x++)
                    {
                        for (int y = 0; y < UnifiedServiceFramework.Authorization.Permissions.GetAllPermissionGroups()[x].ModelPermissions.Count; y++)
                        {

                            List<string> fields = UnifiedServiceFramework.Authorization.Permissions.ModelsAndFields.First(z => z.Key == 
                                UnifiedServiceFramework.Authorization.Permissions.GetAllPermissionGroups()[x].ModelPermissions[y].ModelName)
                                .Value.ToList();

                            for (int z = 0; z < UnifiedServiceFramework.Authorization.Permissions.GetAllPermissionGroups()[x].ModelPermissions[y].EditableFields.Count; z++)
                            {
                                if (!fields.Contains(UnifiedServiceFramework.Authorization.Permissions.GetAllPermissionGroups()[x].ModelPermissions[y].EditableFields[z]))
                                {
                                    bool keepLooping = true;

                                    while (keepLooping)
                                    {
                                        Console.Clear();
                                        //We have found a bad column name in a permission group.  Alert the user and give them an option.
                                        Console.WriteLine("A discrepancy was found between the permissions and the database schematic! Details:");
                                        Console.WriteLine();

                                        List<string[]> lines = new List<string[]>();
                                        lines.Add(new[] { "Table Name", "Permission Group Name", "Permission Part", "Non-Existant Column Name" });
                                        lines.Add(new[] { UnifiedServiceFramework.Authorization.Permissions.GetAllPermissionGroups()[x].ModelPermissions[y].ModelName, 
                                            UnifiedServiceFramework.Authorization.Permissions.GetAllPermissionGroups()[x].Name, 
                                            "EditableFields",
                                            UnifiedServiceFramework.Authorization.Permissions.GetAllPermissionGroups()[x].ModelPermissions[y].EditableFields[z] });

                                        Console.WriteLine(Interfaces.GenericInterfaces.PadElementsInLines(lines, 3));
                                        Console.WriteLine();

                                        Console.WriteLine("Choose an option...");
                                        Console.WriteLine("\t1. Remove column name from permission group and restart cross check");
                                        Console.WriteLine("\t2. Ignore and continue (not recommended)");

                                        int option = 0;

                                        if (Int32.TryParse(Console.ReadLine(), out option) && option >= 1 && option <= 3)
                                        {
                                            keepLooping = false;

                                            switch (option)
                                            {
                                                case 1:
                                                    {
                                                        restart = true;

                                                        Console.WriteLine("Removing field and updating database...");

                                                        UnifiedServiceFramework.Authorization.Permissions.GetAllPermissionGroups()[x].ModelPermissions[y].EditableFields
                                                            .Remove(UnifiedServiceFramework.Authorization.Permissions.GetAllPermissionGroups()[x].ModelPermissions[y].EditableFields[z]);

                                                        UnifiedServiceFramework.Authorization.Permissions.GetAllPermissionGroups()[x].DBUpdate(true).Wait();

                                                        Console.WriteLine("Field removed from permission!  Press any key to continue and restart...");
                                                        Console.ReadKey();
                                                        Console.Clear();

                                                        break;
                                                    }
                                                case 2:
                                                    {
                                                        break;
                                                    }
                                            }
                                        }

                                    }
                                }
                                else
                                {
                                    Console.WriteLine("It's baby time!");
                                }

                                if (restart)
                                    break;

                            }

                            for (int z = 0; z < UnifiedServiceFramework.Authorization.Permissions.GetAllPermissionGroups()[x].ModelPermissions[y].ReturnableFields.Count; z++)
                            {
                                if (!fields.Contains(UnifiedServiceFramework.Authorization.Permissions.GetAllPermissionGroups()[x].ModelPermissions[y].ReturnableFields[z]))
                                {
                                    bool keepLooping = true;

                                    while (keepLooping)
                                    {
                                        Console.Clear();
                                        //We have found a bad column name in a permission group.  Alert the user and give them an option.
                                        Console.WriteLine("A discrepancy was found between the permissions and the database schematic! Details:");
                                        Console.WriteLine();

                                        List<string[]> lines = new List<string[]>();
                                        lines.Add(new[] { "Table Name", "Permission Group Name", "Permission Part", "Non-Existant Column Name" });
                                        lines.Add(new[] { UnifiedServiceFramework.Authorization.Permissions.GetAllPermissionGroups()[x].ModelPermissions[y].ModelName, 
                                            UnifiedServiceFramework.Authorization.Permissions.GetAllPermissionGroups()[x].Name, 
                                            "ReturnableFields",
                                            UnifiedServiceFramework.Authorization.Permissions.GetAllPermissionGroups()[x].ModelPermissions[y].ReturnableFields[z] });

                                        Console.WriteLine(Interfaces.GenericInterfaces.PadElementsInLines(lines, 3));
                                        Console.WriteLine();

                                        Console.WriteLine("Choose an option...");
                                        Console.WriteLine("\t1. Remove column name from permission group and restart cross check");
                                        Console.WriteLine("\t2. Ignore and continue (not recommended)");

                                        int option = 0;

                                        if (Int32.TryParse(Console.ReadLine(), out option) && option >= 1 && option <= 3)
                                        {
                                            keepLooping = false;

                                            switch (option)
                                            {
                                                case 1:
                                                    {
                                                        restart = true;

                                                        Console.WriteLine("Removing field and updating database...");

                                                        UnifiedServiceFramework.Authorization.Permissions.GetAllPermissionGroups()[x].ModelPermissions[y].ReturnableFields
                                                            .Remove(UnifiedServiceFramework.Authorization.Permissions.GetAllPermissionGroups()[x].ModelPermissions[y].ReturnableFields[z]);

                                                        UnifiedServiceFramework.Authorization.Permissions.GetAllPermissionGroups()[x].DBUpdate(true).Wait();

                                                        Console.WriteLine("Field removed from permission!  Press any key to continue and restart...");
                                                        Console.ReadKey();
                                                        Console.Clear();

                                                        break;
                                                    }
                                                case 2:
                                                    {
                                                        break;
                                                    }
                                            }
                                        }

                                    }
                                }
                                else
                                {
                                    Console.WriteLine("It's baby time!");
                                }

                                if (restart)
                                    break;

                            }

                            for (int z = 0; z < UnifiedServiceFramework.Authorization.Permissions.GetAllPermissionGroups()[x].ModelPermissions[y].SearchableFields.Count; z++)
                            {
                                if (!fields.Contains(UnifiedServiceFramework.Authorization.Permissions.GetAllPermissionGroups()[x].ModelPermissions[y].SearchableFields[z]))
                                {
                                    bool keepLooping = true;

                                    while (keepLooping)
                                    {
                                        Console.Clear();
                                        //We have found a bad column name in a permission group.  Alert the user and give them an option.
                                        Console.WriteLine("A discrepancy was found between the permissions and the database schematic! Details:");
                                        Console.WriteLine();

                                        List<string[]> lines = new List<string[]>();
                                        lines.Add(new[] { "Table Name", "Permission Group Name", "Permission Part", "Non-Existant Column Name" });
                                        lines.Add(new[] { UnifiedServiceFramework.Authorization.Permissions.GetAllPermissionGroups()[x].ModelPermissions[y].ModelName, 
                                            UnifiedServiceFramework.Authorization.Permissions.GetAllPermissionGroups()[x].Name, 
                                            "SearchableFields",
                                            UnifiedServiceFramework.Authorization.Permissions.GetAllPermissionGroups()[x].ModelPermissions[y].SearchableFields[z] });

                                        Console.WriteLine(Interfaces.GenericInterfaces.PadElementsInLines(lines, 3));
                                        Console.WriteLine();

                                        Console.WriteLine("Choose an option...");
                                        Console.WriteLine("\t1. Remove column name from permission group and restart cross check");
                                        Console.WriteLine("\t2. Ignore and continue (not recommended)");

                                        int option = 0;

                                        if (Int32.TryParse(Console.ReadLine(), out option) && option >= 1 && option <= 3)
                                        {
                                            keepLooping = false;

                                            switch (option)
                                            {
                                                case 1:
                                                    {
                                                        restart = true;

                                                        Console.WriteLine("Removing field and updating database...");

                                                        UnifiedServiceFramework.Authorization.Permissions.GetAllPermissionGroups()[x].ModelPermissions[y].SearchableFields
                                                            .Remove(UnifiedServiceFramework.Authorization.Permissions.GetAllPermissionGroups()[x].ModelPermissions[y].SearchableFields[z]);

                                                        UnifiedServiceFramework.Authorization.Permissions.GetAllPermissionGroups()[x].DBUpdate(true).Wait();

                                                        Console.WriteLine("Field removed from permission!  Press any key to continue and restart...");
                                                        Console.ReadKey();
                                                        Console.Clear();

                                                        break;
                                                    }
                                                case 2:
                                                    {
                                                        break;
                                                    }
                                            }
                                        }

                                    }
                                }
                                else
                                {
                                    Console.WriteLine("It's baby time!");
                                }

                                if (restart)
                                    break;

                            }

                            if (restart)
                                break;
                        }

                        if (restart)
                            break;
                    }


                }

                
            }
            else
            {
                Console.WriteLine("Skipping cross check...");
            }

        }

    }
}
