using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;

namespace CDBServiceHost
{
    class Program
    {

        static void Main(string[] args)
        {
            try
            {
                System.Collections.IList list;
                NHibernate.ISessionFactory factory =
                    new NHibernate.Cfg.Configuration().Configure().BuildSessionFactory();

                using (NHibernate.ISession session = factory.OpenSession())
                using (var transaction = session.BeginTransaction())
                {

                   
                }

                factory.Close();

                Console.ReadLine();
            }
            catch
            {
                
                throw;
            }
        }

        /*static void Main(string[] args)
        {
            try
            {
                Console.WindowWidth = 200;
                Console.WindowHeight = Console.LargestWindowHeight;
                SetWindowPos(MyConsole, 0, 0, 0, 0, 0, SWP_NOSIZE);

                bool keepLooping = true;

                while (keepLooping)
                {

                    try
                    {
                        Console.Clear();

                        //Display information about the service.  First we're going to build all the information and then display it.
                        bool isInitialized = ServiceManager.Host != null;
                        string isOpen = (!isInitialized) ? "null" : (ServiceManager.Host.State == System.ServiceModel.CommunicationState.Opened).ToString();
                        string activeEndpoints = (isInitialized && ServiceManager.Host.State == System.ServiceModel.CommunicationState.Opened) ? UnifiedServiceFramework.Framework.ServiceManager.EndpointDescriptions.Values.Where(x => x.IsActive).Count().ToString() : "null";
                        string baseAddress = (ServiceManager.Host == null) ? "null" : ServiceManager.Host.BaseAddresses.FirstOrDefault().ToString();
                        string commConnected = (UnifiedServiceFramework.Communicator.IsCommunicatorInitialized) ? "Connected" : "Not Connected";
                        string commFrozen = (!UnifiedServiceFramework.Communicator.IsCommunicatorInitialized) ? "null" : (UnifiedServiceFramework.Communicator.IsFrozen) ? "Frozen" : "Unfrozen";
                        string databaseStatus = "";
                        try
                        {
                            UnifiedServiceFramework.Diagnostics.TestDBConnection(CommandDB_Plugin.Properties.ConnectionString).Wait();
                            databaseStatus = "Connected";
                        }
                        catch (Exception e)
                        {
                            databaseStatus = e.Message;
                        }

                        Console.WriteLine(string.Format("Initialized: {0}", isInitialized.ToString()));
                        Console.WriteLine(string.Format("Opened: {0}", isOpen));
                        Console.WriteLine(string.Format("Base Address: {0}", baseAddress));
                        Console.WriteLine(string.Format("Active Endpoints: {0}", activeEndpoints));
                        Console.WriteLine(string.Format("Communicator Status: {0}", commConnected));
                        Console.WriteLine(string.Format("Communicator States: {0}", commFrozen));
                        Console.WriteLine(string.Format("Database Connection Status: {0}", databaseStatus));
                        Console.WriteLine();

                        //Now that all our data is displayed, show the user the options.
                        Console.WriteLine("1. Start Service");
                        Console.WriteLine("2. Shutdown Service");
                        Console.WriteLine("3. Shutdown Program And Service");
                        Console.WriteLine("4. Unfreeze Communicator");
                        Console.WriteLine("5. Freeze Communicator");
                        Console.WriteLine("6. Clear Window");
                        Console.WriteLine("7. Test Database Connection");
                        Console.WriteLine("8. Execute Non Query");
                        Console.WriteLine("9. Execute Query");
                        Console.WriteLine("10. Edit Permissions");
                        Console.WriteLine("11. Edit Commands");
                        Console.WriteLine("12. Edit Main Data");
                        Console.WriteLine("13. Edit Errors");
                        Console.WriteLine("14. Edit Change Events");
                        Console.WriteLine("15. Edit Lists");
                        Console.WriteLine("16. Define Type");
                        Console.WriteLine("17. Manage Endpoints");
                        Console.WriteLine("18. Manage Cron Operations");

                        int option;

                        if (Int32.TryParse(Console.ReadLine(), out option) && option >= 1 && option <= 18)
                        {
                            switch (option)
                            {
                                case 1: //Start service option
                                    {
                                        Console.Clear();

                                        if (ServiceManager.Host != null)
                                        {
                                            Console.WriteLine("The service is either still active or was not released.  Try to shut it down first.");
                                        }
                                        else
                                        {

                                            bool keepLoopingPortSelection = true;

                                            while (keepLoopingPortSelection)
                                            {
                                                int port;
                                                string portInput;

                                                Console.Clear();
                                                Console.WriteLine("On what port would you like to launch the service?  (Enter a blank line to launch the service on the default port, 1113.)");
                                                portInput = Console.ReadLine();
                                                if (string.IsNullOrWhiteSpace(portInput))
                                                    portInput = "1113";

                                                if (Int32.TryParse(portInput, out port))
                                                {
                                                    if (Utilities.IsPortAvailable(port))
                                                    {
                                                        keepLoopingPortSelection = false;
                                                        ServiceManager.InitializeService(port);
                                                        Console.WriteLine(string.Format("The host is now initialized.  Base address is {0}.", ServiceManager.Host.BaseAddresses.FirstOrDefault()));

                                                        bool keepLoopingServiceStart = true;

                                                        while (keepLoopingServiceStart)
                                                        {
                                                            try
                                                            {
                                                                ServiceManager.StartService();
                                                                Console.WriteLine(string.Format("The host is now active and listening at {0}.", ServiceManager.Host.BaseAddresses.FirstOrDefault()));
                                                                Console.WriteLine("Press any key to continue...");
                                                                Console.ReadKey();

                                                                keepLoopingServiceStart = false;
                                                            }
                                                            catch (Exception e)
                                                            {
                                                                Console.WriteLine(string.Format("An error occurred while trying to start the service:\n\t{0}", e.Message));
                                                                Console.WriteLine("Would you like to try again? (y)");
                                                                if (Console.ReadLine().ToLower() == "y")
                                                                {
                                                                    Console.WriteLine("Reattempting service start...");
                                                                    Console.WriteLine();
                                                                }
                                                                else
                                                                {
                                                                    Console.WriteLine("Service start up cancelled. Releasing service resources...");
                                                                    ServiceManager.ReleaseService();
                                                                    Console.WriteLine("Service resources released.  Press any key to continue...");
                                                                    Console.ReadKey();
                                                                }

                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Console.WriteLine(string.Format("The port, '{0}', is currently reserved.", port));
                                                        Console.WriteLine("Would you like to try again? (y)");
                                                        if (Console.ReadLine().ToLower() != "y")
                                                            keepLoopingPortSelection = false;
                                                    }
                                                }
                                                else
                                                {
                                                    Console.WriteLine("Invalid port.");
                                                    Console.WriteLine("Would you like to try again? (y)");
                                                    if (Console.ReadLine().ToLower() != "y")
                                                        keepLoopingPortSelection = false;
                                                }
                                            }
                                        }

                                        break;
                                    }
                                case 2: //Stop Service option
                                    {
                                        Console.Clear();

                                        if (ServiceManager.Host == null)
                                        {
                                            Console.WriteLine("The service hasn't been started/initialized yet.  Start it first.");
                                        }
                                        else
                                        {
                                            ServiceManager.ReleaseService();
                                            ServiceManager.StopService();

                                            Console.WriteLine("The service is now stopped and has been released.");
                                        }


                                        break;
                                    }
                                case 3: //Stop and shutdown
                                    {
                                        Console.Clear();

                                        if (ServiceManager.Host != null)
                                        {
                                            ServiceManager.ReleaseService();
                                            ServiceManager.StopService();
                                        }

                                        return;
                                    }
                                case 4: // Unfreeze
                                    {
                                        Console.Clear();
                                        UnifiedServiceFramework.Communicator.Unfreeze();
                                        Console.WriteLine("Resuming communications from the service...");
                                        break;
                                    }
                                case 5: //Freeze
                                    {
                                        Console.Clear();
                                        UnifiedServiceFramework.Communicator.Freeze();
                                        Console.WriteLine("Blocking communications from the service...");
                                        break;
                                    }
                                case 6:
                                    {
                                        Console.Clear();
                                        break;
                                    }
                                case 7:
                                    {
                                        Console.Clear();
                                        Console.WriteLine("Testing database connection...");

                                        string result = "";
                                        try
                                        {
                                            UnifiedServiceFramework.Diagnostics.TestDBConnection(CommandDB_Plugin.Properties.ConnectionString).Wait();
                                            result = "Connected";
                                        }
                                        catch (Exception e)
                                        {
                                            result = e.Message;
                                        }

                                        Console.WriteLine(result);
                                        break;
                                    }
                                case 8:
                                    {
                                        Console.Clear();
                                        Console.WriteLine("Enter the query you would like to execute...");
                                        string query = Console.ReadLine();
                                        Console.WriteLine("Executing...");
                                        UnifiedServiceFramework.Administration.Executer.ExecuteNonQuery(query);
                                        Console.WriteLine("Success!");
                                        break;
                                    }
                                case 9:
                                    {
                                        Console.Clear();
                                        Console.WriteLine("Enter the query you would like to execute...");
                                        string query = Console.ReadLine();
                                        Console.WriteLine("Executing...");
                                        string result = UnifiedServiceFramework.Administration.Executer.ExecuteQuery(query);
                                        Console.WriteLine("Success!");
                                        Console.WriteLine("Result:");
                                        Console.WriteLine();
                                        Console.WriteLine(result);
                                        Console.WriteLine();
                                        break;
                                    }
                                case 10:
                                    {
                                        bool keepLoopingPermissionsEditor = true;
                                        while (keepLoopingPermissionsEditor)
                                        {
                                            //update the permissions cache.
                                            List<UnifiedServiceFramework.Authorization.Permissions.PermissionGroup> perms = UnifiedServiceFramework.Authorization.Permissions.DBLoadAll(true).Result;

                                            Console.Clear();

                                            Console.WriteLine("Welcome to the permissions editor!!");
                                            Console.WriteLine(string.Format("There are currently {0} permission groups.  They are...", perms.Count));

                                            Console.WriteLine();
                                            for (int x = 0; x < perms.Count; x++)
                                            {
                                                Console.WriteLine(string.Format("{0}. {1}", x, perms[x].Name));
                                            }

                                            Console.WriteLine();

                                            Console.WriteLine("1. Add New Permission Group");
                                            Console.WriteLine("2. Edit Permission Group");
                                            Console.WriteLine("3. Delete Permission Group");
                                            Console.WriteLine("4. Refresh Permission Groups");
                                            Console.WriteLine("5. Edit User's Permissions");
                                            Console.WriteLine("6. Done");

                                            int permOption = 0;

                                            if (Int32.TryParse(Console.ReadLine(), out permOption))
                                            {

                                                switch (permOption)
                                                {
                                                    case 1:
                                                        {

                                                            #region Add New Permission Group

                                                            Console.Clear();

                                                            string newPermName = "";
                                                            do
                                                            {

                                                                Console.WriteLine("Enter the new permission group's name.  It can't already exist and cannot be white space...");
                                                                newPermName = Console.ReadLine();

                                                            } while (string.IsNullOrWhiteSpace(newPermName) || perms.Exists(x => x.Name.Equals(newPermName, StringComparison.CurrentCultureIgnoreCase)));

                                                            UnifiedServiceFramework.Authorization.Permissions.PermissionGroup newPerm = new UnifiedServiceFramework.Authorization.Permissions.PermissionGroup();
                                                            newPerm.Name = newPermName;
                                                            newPerm.ID = Guid.NewGuid().ToString();


                                                            Interfaces.PermissionGroups.EditPermissionGroup(newPerm);

                                                            break;

                                                            #endregion

                                                        }
                                                    case 2:
                                                        {
                                                            #region Edit Permission Group

                                                            int editPermissionGroupOption = -1;
                                                            do
                                                            {
                                                                Console.Clear();

                                                                Console.WriteLine("Choose a permissions group below to edit...");
                                                                Console.WriteLine();

                                                                for (int x = 0; x < perms.Count; x++)
                                                                {
                                                                    Console.WriteLine(string.Format("{0}. {1}", x, perms[x].Name));
                                                                }
                                                            } while (!Int32.TryParse(Console.ReadLine(), out editPermissionGroupOption) || editPermissionGroupOption < 0 || editPermissionGroupOption > perms.Count - 1);

                                                            Interfaces.PermissionGroups.EditPermissionGroup(perms[editPermissionGroupOption]);

                                                            break;

                                                            #endregion
                                                        }
                                                    case 3:
                                                        {
                                                            #region Delete Permissions Group

                                                            Console.Clear();

                                                            int deletePermissionGroupOption = -1;
                                                            do
                                                            {
                                                                Console.Clear();

                                                                Console.WriteLine("Choose a permissions group below to delete...");
                                                                Console.WriteLine();

                                                                for (int x = 0; x < perms.Count; x++)
                                                                {
                                                                    Console.WriteLine(string.Format("{0}. {1}", x, perms[x].Name));
                                                                }
                                                            } while (!Int32.TryParse(Console.ReadLine(), out deletePermissionGroupOption) || deletePermissionGroupOption < 0 || deletePermissionGroupOption > perms.Count - 1);

                                                            Console.WriteLine(string.Format("Are you sure you want to delete the permissions group '{0}'? (y)", perms[deletePermissionGroupOption].Name));

                                                            if (Console.ReadLine().ToLower() == "y")
                                                            {
                                                                Console.WriteLine("Deleting permission...");
                                                                perms[deletePermissionGroupOption].DBDelete(true).Wait();
                                                                Console.WriteLine("Permission deleted...");
                                                                Console.WriteLine("Scrubbing permission from user profiles...");
                                                                //CommandDB_Plugin.Persons.DBDeletePermissionFromAllUsers(perms[deletePermissionGroupOption]).Wait();
                                                                Console.WriteLine("Permission has been completely deleted!");
                                                                Console.WriteLine("Just kidding... Atwood deleted that code!");
                                                            }

                                                            break;

                                                            #endregion
                                                        }
                                                    case 4:
                                                        {
                                                            #region Refresh Permissions List

                                                            //Funny thing is we don't have to do anything.  The refresh is at the top of the loop so just let the loop turn over again.

                                                            break;

                                                            #endregion
                                                        }
                                                    case 5:
                                                        {

                                                            Interfaces.PermissionGroups.EditUsersPermissions();


                                                            break;
                                                        }
                                                    case 6:
                                                        {

                                                            #region Done

                                                            keepLoopingPermissionsEditor = false;
                                                            break;

                                                            #endregion

                                                        }
                                                    default:
                                                        {
                                                            break;
                                                        }
                                                }

                                            }



                                        }

                                        break;
                                    }
                                case 11: //Edit commands
                                    {

                                        bool keepLoopingCommandsEditor = true;
                                        while (keepLoopingCommandsEditor)
                                        {
                                            //Update the cache.
                                            List<CommandDB_Plugin.Commands.Command> commands = CommandDB_Plugin.Commands.DBLoadAll(true).Result;

                                            Console.Clear();

                                            Console.WriteLine("Welcome to the commands editor!!");
                                            Console.WriteLine(string.Format("There are currently {0} commands.", commands.Count));
                                            Console.WriteLine();
                                            Console.WriteLine("Type the number of the command you would like to edit, type the number followed by a minus sign to delete it, enter a new command name to create a new command, or enter a blank line to cancel.");
                                            Console.WriteLine();

                                            List<string[]> lines = new List<string[]>();
                                            lines.Add(new[] { "#", "Name", "Description", "# of Departments" });
                                            for (int x = 0; x < commands.Count; x++)
                                            {
                                                lines.Add(new[] { x.ToString(), commands[x].Name, commands[x].Description.Truncate(20), commands[x].Departments.Count.ToString() });
                                            }
                                            Console.WriteLine(Interfaces.GenericInterfaces.PadElementsInLines(lines, 3));

                                            string input = Console.ReadLine();
                                            int inputOption = -1;

                                            if (string.IsNullOrWhiteSpace(input))
                                            {
                                                keepLoopingCommandsEditor = false;
                                            }
                                            else
                                                if (input.Last().Equals('-'))
                                                {
                                                    int deleteInput = -1;
                                                    if (Int32.TryParse(input.Substring(0, input.Length - 1), out deleteInput) && deleteInput >= 0 && deleteInput <= commands.Count - 1)
                                                    {
                                                        Console.Clear();

                                                        CommandDB_Plugin.Commands.Command comToDelete = commands[deleteInput];

                                                        int currentInCommand = CommandDB_Plugin.Persons.CountPersonsInCommand(commands[deleteInput].Name).Result;

                                                        if (currentInCommand == 0)
                                                        {
                                                            Console.WriteLine("Are you sure you want to delete the following command?  This is permanent! (y)");
                                                            Console.WriteLine();
                                                            List<string[]> commandEntry = new List<string[]>();
                                                            commandEntry.Add(new[] { "ID", "Name", "Description" });
                                                            commandEntry.Add(new[] { comToDelete.ID, comToDelete.Name, comToDelete.Description.Truncate(20) });
                                                            Console.WriteLine(Interfaces.GenericInterfaces.PadElementsInLines(commandEntry, 3));

                                                            if (Console.ReadLine().ToLower() == "y")
                                                            {
                                                                comToDelete.DBDelete(true).Wait();
                                                            }

                                                        }
                                                        else
                                                        {
                                                            Console.WriteLine(string.Format("{0} user(s) exist in the command named '{1}'!  Cannot delete command until they are assigned to a different command.", currentInCommand, comToDelete.Name));
                                                        }


                                                    }
                                                }
                                                else
                                                    if (Int32.TryParse(input, out inputOption) && inputOption >= 0 && inputOption <= commands.Count - 1)
                                                    {
                                                        Interfaces.CommandsEditor.EditCommand(commands[inputOption]);
                                                    }
                                                    else //User entered a new name for a command.
                                                    {
                                                        Console.Clear();
                                                        Console.WriteLine(string.Format("Do you want to make a new command named '{0}'? (y)", input));

                                                        if (Console.ReadLine().ToLower() == "y")
                                                        {
                                                            CommandDB_Plugin.Commands.Command newCommand = new CommandDB_Plugin.Commands.Command()
                                                            {
                                                                Description = "",
                                                                Departments = new List<CommandDB_Plugin.Commands.Command.Department>(),
                                                                ID = Guid.NewGuid().ToString(),
                                                                Name = input
                                                            };

                                                            commands.Add(newCommand);

                                                            Interfaces.CommandsEditor.EditCommand(newCommand);

                                                        }
                                                    }

                                        }

                                        break;
                                    }
                                case 12: //Main data editor
                                    {
                                        bool keepLoopingMainDataEditor = true;

                                        while (keepLoopingMainDataEditor)
                                        {
                                            List<CommandDB_Plugin.MainData.MainDataItem> mainData = CommandDB_Plugin.MainData.DBLoadAll(true).Result;

                                            Console.Clear();

                                            Console.WriteLine(string.Format("`{0}` main data entries have been made.  The most recent one was made at '{1}'.  Choose an option below...",
                                                mainData.Count, CommandDB_Plugin.MainData.CurrentMainData.Time));
                                            Console.WriteLine();
                                            Console.WriteLine("1. Display all entries");
                                            Console.WriteLine("2. Create new entry");
                                            Console.WriteLine("3. Cancel");

                                            int optionMainData = 0;

                                            if (Int32.TryParse(Console.ReadLine(), out optionMainData) && optionMainData >= 1 && optionMainData <= 3)
                                            {
                                                switch (optionMainData)
                                                {
                                                    case 1:
                                                        {
                                                            Console.Clear();
                                                            Console.WriteLine("All Entries (lol.. I'll implement this later.)");
                                                            break;
                                                        }
                                                    case 2:
                                                        {
                                                            Console.Clear();
                                                            CommandDB_Plugin.MainData.MainDataItem item = new CommandDB_Plugin.MainData.MainDataItem();
                                                            item.ID = Guid.NewGuid().ToString();
                                                            item.Time = DateTime.Now;

                                                            Console.WriteLine(string.Format("The previous version was '{0}'.  What is this one's version?", CommandDB_Plugin.MainData.CurrentMainData.Version));
                                                            item.Version = Console.ReadLine();

                                                            //Copy over old changes.
                                                            item.ChangeLog = CommandDB_Plugin.MainData.CurrentMainData.ChangeLog;
                                                            CommandDB_Plugin.MainData.MainDataItem.ChangeLogItem changeLogItem = new CommandDB_Plugin.MainData.MainDataItem.ChangeLogItem();
                                                            item.ChangeLog.Add(changeLogItem);

                                                            bool keepLoopingChangeLogEditor = true;
                                                            while (keepLoopingChangeLogEditor)
                                                            {
                                                                changeLogItem.ID = Guid.NewGuid().ToString();
                                                                changeLogItem.Time = item.Time;
                                                                changeLogItem.Version = item.Version;


                                                                Console.Clear();
                                                                Console.WriteLine(string.Format("There are '{0}' changes.  Type one's number to edit it, its number plus the minus sign to delete it, type a new change, or type a blank line to cancel.", changeLogItem.Changes.Count));
                                                                Console.WriteLine();

                                                                List<string[]> lines = new List<string[]>();
                                                                for (int x = 0; x < changeLogItem.Changes.Count; x++)
                                                                {
                                                                    lines.Add(new[] { x.ToString(), changeLogItem.Changes[x] });
                                                                }
                                                                Console.WriteLine((lines.Count > 0) ? Interfaces.GenericInterfaces.PadElementsInLines(lines, 3) : "");

                                                                string changeLogEditorOption = Console.ReadLine();
                                                                int changeLogEditorNumOption = -1;

                                                                if (String.IsNullOrWhiteSpace(changeLogEditorOption))
                                                                {
                                                                    keepLoopingChangeLogEditor = false;
                                                                }
                                                                else
                                                                    if (changeLogEditorOption.Last() == '-')
                                                                    {
                                                                        int deleteChangeOption = -1;
                                                                        if (Int32.TryParse(changeLogEditorOption.Substring(0, changeLogEditorOption.Length - 1), out deleteChangeOption) && deleteChangeOption <= changeLogItem.Changes.Count - 1 && changeLogItem.Changes.Count != 0)
                                                                        {
                                                                            changeLogItem.Changes.RemoveAt(deleteChangeOption);
                                                                        }
                                                                    }
                                                                    else
                                                                        if (Int32.TryParse(changeLogEditorOption, out changeLogEditorNumOption))
                                                                        {
                                                                            if (changeLogEditorNumOption >= 0 && changeLogEditorNumOption <= changeLogItem.Changes.Count - 1 && changeLogItem.Changes.Count != 0)
                                                                            {
                                                                                Console.Clear();
                                                                                Console.WriteLine("The change's text has been copied below.  Enter your change and press enter.");
                                                                                Console.WriteLine();
                                                                                System.Windows.Forms.SendKeys.SendWait(changeLogItem.Changes[changeLogEditorNumOption]);
                                                                                changeLogItem.Changes[changeLogEditorNumOption] = Console.ReadLine();
                                                                            }
                                                                        }
                                                                        else //User typed text
                                                                        {
                                                                            changeLogItem.Changes.Add(changeLogEditorOption);
                                                                        }
                                                            }

                                                            item.KnownIssues = CommandDB_Plugin.MainData.CurrentMainData.KnownIssues.Select(x => String.Copy(x)).ToList();

                                                            bool keepLoopingKnownIssuesEditor = true;
                                                            while (keepLoopingKnownIssuesEditor)
                                                            {
                                                                Console.Clear();
                                                                Console.WriteLine("Now we're going to edit the known issues.  They have been copied over from the previous change item.");
                                                                Console.WriteLine("Type the number of one to delete it, the number plus the minus sign to delete it, a new issue to add it, or a blank line to cancel.");

                                                                List<string[]> knownIssuesLines = new List<string[]>();
                                                                for (int x = 0; x < item.KnownIssues.Count; x++)
                                                                {
                                                                    knownIssuesLines.Add(new[] { x.ToString(), item.KnownIssues[x] });
                                                                }
                                                                Console.WriteLine((knownIssuesLines.Count > 0) ? Interfaces.GenericInterfaces.PadElementsInLines(knownIssuesLines, 3) : "");

                                                                string knownIssuesEditorOption = Console.ReadLine();
                                                                int knownIssuesEditorNumOption = -1;

                                                                if (String.IsNullOrWhiteSpace(knownIssuesEditorOption))
                                                                {
                                                                    keepLoopingKnownIssuesEditor = false;
                                                                }
                                                                else
                                                                    if (knownIssuesEditorOption.Last() == '-')
                                                                    {
                                                                        int deleteIssueOption = -1;
                                                                        if (Int32.TryParse(knownIssuesEditorOption.Substring(0, knownIssuesEditorOption.Length - 1), out deleteIssueOption) && deleteIssueOption <= item.KnownIssues.Count - 1 && item.KnownIssues.Count != 0)
                                                                        {
                                                                            item.KnownIssues.RemoveAt(deleteIssueOption);
                                                                        }
                                                                    }
                                                                    else
                                                                        if (Int32.TryParse(knownIssuesEditorOption, out knownIssuesEditorNumOption))
                                                                        {
                                                                            if (knownIssuesEditorNumOption >= 0 && knownIssuesEditorNumOption <= item.KnownIssues.Count - 1 && item.KnownIssues.Count != 0)
                                                                            {
                                                                                Console.Clear();
                                                                                Console.WriteLine("The issues's text has been copied below.  Enter your change and press enter.");
                                                                                Console.WriteLine();
                                                                                System.Windows.Forms.SendKeys.SendWait(item.KnownIssues[knownIssuesEditorNumOption]);
                                                                                item.KnownIssues[knownIssuesEditorNumOption] = Console.ReadLine();
                                                                            }
                                                                        }
                                                                        else //User typed text
                                                                        {
                                                                            item.KnownIssues.Add(knownIssuesEditorOption);
                                                                        }

                                                            }

                                                            Console.WriteLine("Do you want to commit this main data item entry?  If you don't, it will discard the entire item. (y)");

                                                            if (Console.ReadKey().KeyChar.ToString().ToLower() == "y")
                                                                item.DBInsert(true).Wait();


                                                            break;
                                                        }
                                                    case 3:
                                                        {
                                                            keepLoopingMainDataEditor = false;
                                                            break;
                                                        }
                                                    default:
                                                        {
                                                            throw new NotImplementedException();
                                                        }
                                                }

                                            }

                                        }

                                        break;
                                    }
                                case 13: //Errors Editor
                                    {

                                        bool keepLoopingErrorsEditor = true;

                                        while (keepLoopingErrorsEditor)
                                        {
                                            Console.Clear();

                                            List<UnifiedServiceFramework.Framework.Errors.Error> errors = UnifiedServiceFramework.Framework.Errors.DBLoadAll(false).Result;

                                            Console.WriteLine(string.Format("{0} error(s) have occurred in the lifetime of the application.  {1}/{0} have been handled.", errors.Count, errors.Where(x => x.IsHandled).Count()));
                                            Console.WriteLine();

                                            Console.WriteLine("1. View All Errors");
                                            Console.WriteLine("2. View Unhandled Errors");
                                            Console.WriteLine("3. Cancel");

                                            int errorOption = 0;

                                            if (Int32.TryParse(Console.ReadLine(), out errorOption) && errorOption >= 1 && errorOption <= 3)
                                            {
                                                switch (errorOption)
                                                {
                                                    case 1:
                                                        {
                                                            Interfaces.ErrorsEditor.EditErrors(errors.OrderByDescending(x => x.IsHandled).ThenByDescending(x => x.Time).ToList());
                                                            break;
                                                        }
                                                    case 2:
                                                        {
                                                            Interfaces.ErrorsEditor.EditErrors(errors.Where(x => !x.IsHandled).OrderBy(x => x.Time).ToList());
                                                            break;
                                                        }
                                                    case 3:
                                                        {
                                                            keepLoopingErrorsEditor = false;
                                                            break;
                                                        }
                                                    default:
                                                        {
                                                            throw new NotImplementedException("In error Options");
                                                        }
                                                }
                                            }


                                        }


                                        break;
                                    }
                                case 14:
                                    {

                                        Interfaces.ChangeEvents.EditChangeEvents();

                                        break;
                                    }
                                case 15: //Lists options
                                    {

                                        bool keepLoopingListEditor = true;

                                        while (keepLoopingListEditor)
                                        {
                                            Console.Clear();

                                            List<CommandDB_Plugin.CDBLists.CDBList> lists = CommandDB_Plugin.CDBLists.DBLoadAll(true).Result;

                                            Console.WriteLine("Editing lists...");
                                            Console.WriteLine();
                                            Console.WriteLine("Type the number of a list to edit it/read it, the number followed by a minus sign to delete it, a string to add a new list with that name, two minus signs to delete all lists, or a blank line to return.");
                                            Console.WriteLine();

                                            List<string[]> lines = new List<string[]>();
                                            lines.Add(new[] { "#", "Name", "# of Elements" });
                                            for (int x = 0; x < lists.Count; x++)
                                            {
                                                lines.Add(new[] { x.ToString(), lists[x].Name, lists[x].Values.Count.ToString() });
                                            }
                                            Console.WriteLine(Interfaces.GenericInterfaces.PadElementsInLines(lines, 3));

                                            string input = Console.ReadLine();
                                            int numOption;

                                            if (string.IsNullOrWhiteSpace(input)) //user wants out
                                            {
                                                keepLoopingListEditor = false;
                                            }
                                            else
                                                if (input == "--") //user wants to delete all lists
                                                {
                                                    Console.Clear();
                                                    Console.WriteLine("Are you sure you want to delete all lists?  This will affect the database.  This is unrecoverable. (y)");

                                                    if (Console.ReadLine().ToLower() == "y")
                                                    {

                                                        Console.WriteLine("Deleting all lists...");
                                                        lists.ForEach(x =>
                                                        {
                                                            x.DBDelete(true).Wait();
                                                        });
                                                        Console.WriteLine("All lists deleted... hope you meant to do that.");
                                                    }
                                                }
                                                else
                                                    if (input.Contains("-") && Int32.TryParse(input.Replace("-", ""), out numOption) && numOption >= 0 && numOption <= lists.Count - 1) //User wants to delete a specific list
                                                    {
                                                        Console.WriteLine(string.Format("Are you sure you want to delete the list whose name is '{0}'? (y)", lists[numOption].Name));

                                                        if (Console.ReadLine().ToLower() == "y")
                                                        {
                                                            Console.WriteLine("Deleting list...");
                                                            lists[numOption].DBDelete(true).Wait();
                                                            Console.WriteLine("List deleted!");
                                                        }
                                                    }
                                                    else
                                                        if (Int32.TryParse(input, out numOption) && numOption >= 0 && numOption <= lists.Count - 1) //User wants to edit a list
                                                        {
                                                            Interfaces.ListsEditor.EditList(lists[numOption]);
                                                        }
                                                        else //Add a new list
                                                        {
                                                            CommandDB_Plugin.CDBLists.CDBList list = new CommandDB_Plugin.CDBLists.CDBList()
                                                            {
                                                                ID = Guid.NewGuid().ToString(),
                                                                Name = input,
                                                                Values = new List<string>()
                                                            };

                                                            Interfaces.ListsEditor.EditList(list);
                                                        }
                                        }



                                        break;

                                    }
                                case 16: //Defien Type
                                    {
                                        Console.Clear();
                                        Console.WriteLine("What type would you like to define?");
                                        string type = Console.ReadLine();

                                        List<System.Reflection.PropertyInfo> props = null;

                                        switch (type)
                                        {
                                            case "Person":
                                                {
                                                    props = typeof(CommandDB_Plugin.Persons.Person).GetProperties().ToList();
                                                    break;
                                                }
                                            default:
                                                {
                                                    Console.WriteLine("Ask Atwood to write the definition for this.");
                                                    break;
                                                }
                                        }

                                        if (props != null)
                                        {
                                            List<string[]> lines = new List<string[]>();
                                            lines.Add(new[] { "#", "Name", "Type" });
                                            for (int x = 0; x < props.Count; x++)
                                            {
                                                lines.Add(new[] { x.ToString(), props[x].Name, props[x].PropertyType.FullName.Truncate(50) });
                                            }
                                            Console.WriteLine(Interfaces.GenericInterfaces.PadElementsInLines(lines, 3));

                                            Console.WriteLine();
                                            Console.WriteLine("Press any key to continue...");
                                            Console.ReadKey();
                                        }

                                        break;
                                    }
                                case 17: //Endpoint manager
                                    {
                                        bool keepLoopingEndpointManager = true;

                                        while (keepLoopingEndpointManager)
                                        {
                                            Console.Clear();

                                            Console.WriteLine("Type an endpoint's number to view it/edit it or a blank line to return.");
                                            Console.WriteLine();

                                            var endpoints = UnifiedServiceFramework.Framework.ServiceManager.EndpointDescriptions;

                                            List<string[]> lines = new List<string[]>();
                                            lines.Add(new[] { "#", "Name", "Description", "State" });
                                            for (int x = 0; x < endpoints.Count; x++)
                                            {
                                                lines.Add(new[] { x.ToString(), endpoints.ElementAt(x).Key, endpoints.ElementAt(x).Value.Description.Truncate(20), endpoints.ElementAt(x).Value.IsActive ? "Active" : "Inactive" });
                                            }
                                            Console.WriteLine(Interfaces.GenericInterfaces.PadElementsInLines(lines, 3));

                                            string endpointsOption = Console.ReadLine();
                                            int endpointsOptionNum;

                                            if (string.IsNullOrWhiteSpace(endpointsOption))
                                            {
                                                keepLoopingEndpointManager = false;
                                            }
                                            else
                                                if (Int32.TryParse(endpointsOption, out endpointsOptionNum) && endpointsOptionNum >= 0 && endpointsOptionNum <= endpoints.Count - 1)
                                                {
                                                    Interfaces.EndpointsEditor.ManageEndpoint(endpoints.ElementAt(endpointsOptionNum));
                                                }
                                        }


                                        break;
                                    }
                                case 18:
                                    {

                                        bool keepLoopingCronOperations = true;

                                        while (keepLoopingCronOperations)
                                        {
                                            Console.Clear();

                                            Console.WriteLine("Managing Cron Operations...");
                                            Console.WriteLine();
                                            Console.WriteLine(string.Format("There are currently {0} cron operations.", UnifiedServiceFramework.CronOperations.CronOperationActions.Count));
                                            Console.WriteLine();
                                            Console.WriteLine(string.Format("Current cron operations interval is '{0}'.", UnifiedServiceFramework.CronOperations.Interval));
                                            Console.WriteLine();
                                            Console.WriteLine(string.Format("Status: {0}", UnifiedServiceFramework.CronOperations.IsActive ? "Active" : "Inactive"));
                                            Console.WriteLine();
                                            Console.WriteLine(string.Format("1. {0} Cron Operations", UnifiedServiceFramework.CronOperations.IsActive ? "Disable" : "Enable"));
                                            Console.WriteLine("2. Cancel");

                                            int optionCronOperations;

                                            if (Int32.TryParse(Console.ReadLine(), out optionCronOperations) && optionCronOperations >= 1 && optionCronOperations <= 2)
                                            {
                                                switch (optionCronOperations)
                                                {
                                                    case 1:
                                                        {
                                                            if (UnifiedServiceFramework.CronOperations.IsActive)
                                                            {
                                                                UnifiedServiceFramework.CronOperations.Stop();
                                                                Console.WriteLine("Cron operations have been stopped.  Any operations already running will continue to their completion.");
                                                            }
                                                            else
                                                            {
                                                                UnifiedServiceFramework.CronOperations.StartCronOperations();
                                                                Console.WriteLine("Cron operations started.");
                                                            }

                                                            Console.WriteLine("Press any key to continue...");
                                                            Console.ReadKey();

                                                            break;
                                                        }
                                                    case 2:
                                                        {
                                                            keepLoopingCronOperations = false;
                                                            break;
                                                        }
                                                    default:
                                                        {
                                                            throw new NotImplementedException("In manage cron operations switch");
                                                        }
                                                }
                                            }

                                        }

                                        break;
                                    }
                                default:
                                    {
                                        throw new NotImplementedException();
                                    }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(string.Format("An error has occured in the host.  This error may or may not affect the service's execution. Error message:\n\t{0}", e.Message));
                    }

                    
                    
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(string.Format("Somehow, and error broke containment!  PANIC! Error Message:\n\t {0}", e.Message));
            }

            Console.WriteLine("The application has shutdown. Press any key to finish shutdown...");
            Console.ReadKey();
        }*/
    }
}
