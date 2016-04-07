using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;

namespace CDBServiceHost.Interfaces
{
    public static class ChangeEvents
    {

        public static void EditChangeEvents()
        {
            bool keepLooping = true;
            while (keepLooping)
            {
                Console.Clear();

                //Load the change events in case they haven't been loaded yet.
                CommandDB_Plugin.ChangeEvents.DBLoadAll(true).Wait();

                Console.WriteLine(string.Format("There are currently {0} change events.", CommandDB_Plugin.ChangeEvents.ChangeEventsCache.Count));
                Console.WriteLine();
                Console.WriteLine("Type the number of the change event you would like to edit, type the number followed by a minus sign to delete it, enter a new change event name to create a new change event, or enter a blank line to cancel.");
                Console.WriteLine();

                List<string[]> lines = new List<string[]>();
                lines.Add(new[] { "#", "Name", "Trigger Fields Count", "Trigger Model", "Event Level", "Req Perms Count" });
                for (int x = 0; x < CommandDB_Plugin.ChangeEvents.ChangeEventsCache.Count; x++)
                {
                    CommandDB_Plugin.ChangeEvents.ChangeEvent changeEvent = CommandDB_Plugin.ChangeEvents.ChangeEventsCache.Values.ElementAt(x);

                    lines.Add(new[] { x.ToString(), changeEvent.Name, changeEvent.TriggerFields.Count.ToString(), changeEvent.TriggerModel, changeEvent.EventLevel.ToString(), changeEvent.RequiredSpecialPermissions.Count.ToString() });
                }
                Console.WriteLine(Interfaces.GenericInterfaces.PadElementsInLines(lines, 3));

                string input = Console.ReadLine();
                int inputOption = -1;

                if (string.IsNullOrWhiteSpace(input))
                {
                    keepLooping = false;
                }
                else
                    if (input.Last().Equals('-'))
                    {
                        int deleteInput = -1;
                        if (Int32.TryParse(input.Substring(0, input.Length - 1), out deleteInput) && deleteInput >= 0 && deleteInput <= CommandDB_Plugin.ChangeEvents.ChangeEventsCache.Count - 1)
                        {
                            Console.Clear();

                            CommandDB_Plugin.ChangeEvents.ChangeEvent changeEvent = CommandDB_Plugin.ChangeEvents.ChangeEventsCache.Values.ElementAt(deleteInput);

                            Console.WriteLine("Are you sure you want to delete the following change event? (y)");
                            Console.WriteLine();
                            List<string[]> entry = new List<string[]>();
                            entry.Add(new[] { "ID", "Name" });
                            entry.Add(new[] { changeEvent.ID, changeEvent.Name });
                            Console.WriteLine(Interfaces.GenericInterfaces.PadElementsInLines(entry, 3));

                            if (Console.ReadLine().ToLower() == "y")
                            {
                                changeEvent.DBDelete(true).Wait();
                            }
                        }
                    }
                    else
                        if (Int32.TryParse(input, out inputOption) && inputOption >= 0 && inputOption <= CommandDB_Plugin.ChangeEvents.ChangeEventsCache.Count - 1)
                        {
                            EditChangeEvent(CommandDB_Plugin.ChangeEvents.ChangeEventsCache.Values.ElementAt(inputOption));
                        }
                        else //User entered a new name for a department.
                        {
                            Console.Clear();
                            Console.WriteLine(string.Format("Do you want to make a new event named '{0}'? (y)", input));

                            if (Console.ReadLine().ToLower() == "y")
                            {
                                CommandDB_Plugin.ChangeEvents.ChangeEvent newChangeEvent = new CommandDB_Plugin.ChangeEvents.ChangeEvent()
                                {
                                    EventLevel = CommandDB_Plugin.ChangeEvents.ChangeEventLevels.DEFAULT,
                                    ID = Guid.NewGuid().ToString(),
                                    Name = input,
                                    RequiredSpecialPermissions = new List<CommandDB_Plugin.CustomPermissionTypes>(),
                                    TriggerFields = new List<string>(),
                                    TriggerModel = ""
                                };

                                EditChangeEvent(newChangeEvent);
                            }
                        }

            }
        }

        public static void EditChangeEvent(CommandDB_Plugin.ChangeEvents.ChangeEvent changeEvent)
        {
            bool keepLooping = true;

            while (keepLooping)
            {
                Console.Clear();
                Console.WriteLine(string.Format("Editing change event '{0}'...", changeEvent.Name));
                Console.WriteLine();
                Console.WriteLine(string.Format("Event Level: {0}", changeEvent.EventLevel.ToString()));
                Console.WriteLine(string.Format("Trigger Fields: {0}", string.Join(",", changeEvent.TriggerFields)));
                Console.WriteLine(string.Format("Trigger Model: {0}", changeEvent.TriggerModel));
                Console.WriteLine(string.Format("Required Permissions: {0}", string.Join(",", changeEvent.RequiredSpecialPermissions)));
                
                Console.WriteLine();
                Console.WriteLine("1. Edit Name");
                Console.WriteLine("2. Edit Event Level");
                Console.WriteLine("3. Edit Trigger Fields");
                Console.WriteLine("4. Edit Trigger Model");
                Console.WriteLine("5. Edit Required Permissions");
                Console.WriteLine("6. Save and Return");
                Console.WriteLine("7. Don't Save and Return");


                int option = 0;
                if (Int32.TryParse(Console.ReadLine(), out option) && option >= 0 && option <= 7)
                {
                    switch (option)
                    {
                        case 1: //Edit Name
                            {
                                Console.Clear();

                                Console.WriteLine(string.Format("The current name of this change event is '{0}'.", changeEvent.Name));
                                Console.WriteLine();
                                Console.WriteLine("Type a new name or enter a blank line to cancel.");
                                
                                string input = Console.ReadLine();
                                if (!string.IsNullOrWhiteSpace(input))
                                    changeEvent.Name = input;

                                break;
                            }
                        case 2: //Edit Event Level
                            {
                                bool keepLoopingEventLevel = true;

                                while (keepLoopingEventLevel)
                                {
                                    Console.Clear();

                                    Console.WriteLine(string.Format("The current event level of this change event is '{0}'.", changeEvent.EventLevel));
                                    Console.WriteLine();
                                    Console.WriteLine("Choose a new event level from the available levels below:");
                                    List<string> changeEventLevelNames = Enum.GetNames(typeof(CommandDB_Plugin.ChangeEvents.ChangeEventLevels)).ToList();
                                    for (int x = 0; x < changeEventLevelNames.Count; x++)
                                    {
                                        Console.WriteLine(string.Format("{0}. {1}", x.ToString(), changeEventLevelNames[x]));
                                    }
                                    string input = Console.ReadLine();

                                    CommandDB_Plugin.ChangeEvents.ChangeEventLevels changeEventLevel;
                                    if (Enum.TryParse<CommandDB_Plugin.ChangeEvents.ChangeEventLevels>(input, out changeEventLevel))
                                    {
                                        changeEvent.EventLevel = changeEventLevel;
                                        keepLoopingEventLevel = false;
                                    }
                                }

                                break;
                            }
                        case 3: //Edit Trigger Fields
                            {

                                Console.Clear();

                                //To know the possible fields, we must know the model
                                if (!UnifiedServiceFramework.Authorization.Permissions.ModelsAndFields.ContainsKey(changeEvent.TriggerModel))
                                {
                                    Console.WriteLine("We can't edit the trigger fields because the trigger model isn't valid.  Edit that first!");
                                    Console.WriteLine("Press any key to continue...");
                                    Console.ReadKey();
                                }
                                else
                                {

                                    List<string> availableFields = UnifiedServiceFramework.Authorization.Permissions.ModelsAndFields[changeEvent.TriggerModel].ToList();
                                    List<string> activeFields = changeEvent.TriggerFields;
                                    List<string> inactiveFields = availableFields.Except(activeFields).ToList();

                                    GenericInterfaces.EditElementsOfLists(activeFields, inactiveFields, "Active Fields", "Inactive Fields", "Edit Trigger Fields");

                                    changeEvent.TriggerFields = activeFields;
                                }

                                break;
                            }
                        case 4: //Edit Trigger Model
                            {
                                bool keepLoopingTriggerModel = true;

                                while (keepLoopingTriggerModel)
                                {
                                    Console.Clear();

                                    var models = UnifiedServiceFramework.Authorization.Permissions.ModelsAndFields.Keys.ToList();

                                    Console.WriteLine(string.Format("The current trigger model is '{0}'.", changeEvent.TriggerModel));
                                    Console.WriteLine();
                                    Console.WriteLine("Choose a new trigger model from the available models below:");
                                    for (int x = 0; x < models.Count; x++)
                                    {
                                        Console.WriteLine(string.Format("{0}. {1}", x.ToString(), models[x]));
                                    }

                                    string input = Console.ReadLine();
                                    int inputOption = -1;
                                    if (Int32.TryParse(input, out inputOption) && inputOption >= 0 && inputOption <= models.Count - 1)
                                    {
                                        changeEvent.TriggerModel = models[inputOption];
                                        keepLoopingTriggerModel = false;
                                    }

                                }

                                break;
                            }
                        case 5: //Edit requierd permissions
                            {

                                List<string> availablePermissions = Enum.GetNames(typeof(CommandDB_Plugin.CustomPermissionTypes)).ToList();
                                List<string> activePermissions = changeEvent.RequiredSpecialPermissions.Select(x => x.ToString()).ToList();
                                List<string> inactivePermissions = availablePermissions.Except(activePermissions).ToList();

                                GenericInterfaces.EditElementsOfLists(activePermissions, inactivePermissions, "Active Permissions", "Inactive Permissions", "Edit Change Event Permissions");

                                changeEvent.RequiredSpecialPermissions = activePermissions.Select(x => (CommandDB_Plugin.CustomPermissionTypes)Enum.Parse(typeof(CommandDB_Plugin.CustomPermissionTypes), x)).ToList();

                                break;
                            }
                        case 6: //Save and Return
                            {

                                Console.Clear();

                                Console.WriteLine("Are you sure you want to save this change event? (y)");

                                if (Console.ReadLine().ToLower() == "y")
                                {
                                    if (changeEvent.DBExists().Result)
                                    {
                                        Console.WriteLine("Updating change event...");
                                        changeEvent.DBUpdate(true).Wait();
                                    }
                                    else
                                    {
                                        Console.WriteLine("Creating change event...");
                                        changeEvent.DBInsert(true).Wait();
                                    }

                                    Console.WriteLine("Press any key to continue...");
                                    Console.ReadKey();

                                    keepLooping = false;
                                }
                                
                                break;
                            }
                        case 7: //Don't save and return
                            {
                                Console.Clear();

                                Console.WriteLine("Are you sure you want to discard all changed to this change event? (y)");

                                if (Console.ReadLine().ToLower() == "y")
                                {
                                    keepLooping = false;
                                }

                                break;
                            }
                        default:
                            {
                                throw new NotImplementedException("In Edit Change Event Switch");
                            }
                    }
                }
            }

        }


    }
}
