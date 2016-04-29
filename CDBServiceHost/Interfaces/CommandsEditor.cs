using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandCentral;

namespace CDBServiceHost.Interfaces
{
    public static class CommandsEditor
    {
        public static void EditCommand(Commands.Command command)
        {
            bool keepLooping = true;
            while (keepLooping)
            {
                Console.Clear();

                Console.WriteLine(string.Format("Editing Command: '{0}'", command.Name));
                Console.WriteLine();
                Console.WriteLine("Description:");
                Console.WriteLine(string.Format("\t{0}", command.Description));
                Console.WriteLine();
                Console.WriteLine("Departments:");
                Console.WriteLine();

                List<string[]> lines = new List<string[]>();
                lines.Add(new[] { "Name", "# of Divisions" });
                for (int x = 0; x < command.Departments.Count; x++)
                {
                    lines.Add(new[] { command.Departments[x].Name, command.Departments[x].Divisions.Count.ToString() });
                }
                Console.WriteLine(Interfaces.GenericInterfaces.PadElementsInLines(lines, 3));

                Console.WriteLine();
                Console.WriteLine("1. Edit Name");
                Console.WriteLine("2. Edit Description");
                Console.WriteLine("3. Edit Departments");
                Console.WriteLine("4. Done & Save");
                Console.WriteLine("5. Done & Don't Save");

                int option = 0;
                if (Int32.TryParse(Console.ReadLine(), out option))
                {
                    switch (option)
                    {
                        case 1: //Edit Name
                            {

                                #region Edit Name

                                Console.Clear();
                                Console.WriteLine(string.Format("The current name of the command is '{0}'.", command.Name));
                                Console.WriteLine();
                                Console.WriteLine("Type a new name or enter a blank line to cancel.");

                                string input = Console.ReadLine();

                                if (!string.IsNullOrWhiteSpace(input))
                                    command.Name = input;

                                break;

                                #endregion

                            }
                        case 2:
                            {

                                #region Edit Description

                                Console.Clear();
                                Console.WriteLine(string.Format("The current description of the command is \n\t'{0}'.", command.Description));
                                Console.WriteLine();
                                Console.WriteLine("Type a new description or enter a blank line to cancel.");

                                string input = Console.ReadLine();

                                if (!string.IsNullOrWhiteSpace(input))
                                    command.Description = input;

                                break;

                                #endregion

                            }
                        case 3:
                            {

                                #region Edit Departments

                                DepartmentsEditor.EditDepartments(command, command.Departments);

                                break;

                                #endregion

                            }
                        case 4:
                            {

                                #region Done and Save

                                Console.Clear();

                                Console.WriteLine("Are you sure you want to save changes to this command? (y)");

                                if (Console.ReadLine().ToLower() == "y")
                                {
                                    if (command.DBExists().Result)
                                    {
                                        command.DBUpdate(true).Wait();
                                    }
                                    else
                                    {
                                        command.DBInsert(true).Wait();
                                    }

                                    return;
                                }

                                break;

                                #endregion

                            }
                        case 5:
                            {

                                #region Done and Don't Save

                                Console.Clear();

                                Console.WriteLine("Are you sure you want to discard changes to this command? (y)");

                                if (Console.ReadLine().ToLower() == "y")
                                {
                                    keepLooping = false;
                                }

                                break;

                                #endregion

                            }
                        default:
                            {
                                throw new NotImplementedException();
                            }
                    }
                }
            }
        }
    }
}
