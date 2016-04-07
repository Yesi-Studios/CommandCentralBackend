using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;

namespace CDBServiceHost.Interfaces
{
    public static class DepartmentsEditor
    {
        public static void EditDepartments(CommandDB_Plugin.Commands.Command command, List<CommandDB_Plugin.Commands.Command.Department> departments)
        {

            bool keepLooping = true;
            while (keepLooping)
            {
                Console.Clear();
                Console.WriteLine(string.Format("There are currently {0} department(s).", departments.Count));
                Console.WriteLine();
                Console.WriteLine("Type the number of the department you would like to edit, type the number followed by a minus sign to delete it, enter a new department name to create a new department, or enter a blank line to cancel.");
                Console.WriteLine();

                List<string[]> lines = new List<string[]>();
                lines.Add(new[] { "#", "Name", "Description", "# of Divisions" });
                for (int x = 0; x < departments.Count; x++)
                {
                    lines.Add(new[] { x.ToString(), departments[x].Name, departments[x].Description.Truncate(20), departments[x].Divisions.Count.ToString() });
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
                        if (Int32.TryParse(input.Substring(0, input.Length - 1), out deleteInput) && deleteInput >= 0 && deleteInput <= departments.Count - 1)
                        {
                            Console.Clear();

                            CommandDB_Plugin.Commands.Command.Department depToDelete = departments[deleteInput];

                            int currentInDepartment = CommandDB_Plugin.Persons.CountPersonsInDepartment(command.Name, departments[deleteInput].Name).Result;

                            if (currentInDepartment == 0)
                            {
                                Console.WriteLine("Are you sure you want to delete the following department? (y)");
                                Console.WriteLine();
                                List<string[]> departmentEntry = new List<string[]>();
                                departmentEntry.Add(new[] { "ID", "Name", "Description" });
                                departmentEntry.Add(new[] { depToDelete.ID, depToDelete.Name, depToDelete.Description.Truncate(20) });
                                Console.WriteLine(Interfaces.GenericInterfaces.PadElementsInLines(departmentEntry, 3));

                                if (Console.ReadLine().ToLower() == "y")
                                {
                                    departments.Remove(depToDelete);
                                }

                            }
                            else
                            {
                                Console.WriteLine(string.Format("{0} user(s) exist in the department named '{1}'!  Cannot delete department until they are assigned to a different department.", currentInDepartment, depToDelete.Name));
                            }


                        }
                    }
                    else
                        if (Int32.TryParse(input, out inputOption) && inputOption >= 0 && inputOption <= departments.Count - 1)
                        {
                            EditDepartment(command, departments[inputOption]);
                        }
                        else //User entered a new name for a department.
                        {
                            Console.Clear();
                            Console.WriteLine(string.Format("Do you want to make a new department named '{0}'? (y)", input));

                            if (Console.ReadLine().ToLower() == "y")
                            {
                                CommandDB_Plugin.Commands.Command.Department newDepartment = new CommandDB_Plugin.Commands.Command.Department()
                                {
                                    Description = "",
                                    Divisions = new List<CommandDB_Plugin.Commands.Command.Department.Division>(),
                                    ID = Guid.NewGuid().ToString(),
                                    Name = input
                                };

                                departments.Add(newDepartment);

                                EditDepartment(command, newDepartment);

                            }
                        }
            }

            

        }

        public static void EditDepartment(CommandDB_Plugin.Commands.Command command, CommandDB_Plugin.Commands.Command.Department department)
        {

            bool keepLooping = true;
            while (keepLooping)
            {
                Console.Clear();
                Console.WriteLine(string.Format("Editing department '{0}'...", department.Name));
                Console.WriteLine();
                Console.WriteLine("Description:");
                Console.WriteLine(string.Format("\t{0}", department.Description));
                Console.WriteLine();
                Console.WriteLine("Divisions:");
                Console.WriteLine();

                List<string[]> lines = new List<string[]>();
                lines.Add(new[] { "Name", "Description" });
                for (int x = 0; x < department.Divisions.Count; x++)
                {
                    lines.Add(new[] { department.Divisions[x].Name, department.Divisions[x].Description.Truncate(20) });
                }
                Console.WriteLine(Interfaces.GenericInterfaces.PadElementsInLines(lines, 3));

                Console.WriteLine();
                Console.WriteLine("1. Edit Name");
                Console.WriteLine("2. Edit Description");
                Console.WriteLine("3. Edit Divisions");
                Console.WriteLine("4. Done");

                int option = 0;
                if (Int32.TryParse(Console.ReadLine(), out option))
                {
                    switch (option)
                    {
                        case 1: //Edit Name
                            {

                                #region Edit Name

                                Console.Clear();
                                Console.WriteLine(string.Format("The current name of the department is '{0}'.", department.Name));
                                Console.WriteLine();
                                Console.WriteLine("Type a new name or enter a blank line to cancel.");

                                string input = Console.ReadLine();

                                if (!string.IsNullOrWhiteSpace(input))
                                    department.Name = input;

                                break;

                                #endregion

                            }
                        case 2:
                            {

                                #region Edit Description

                                Console.Clear();
                                Console.WriteLine(string.Format("The current description of the department is \n\t'{0}'.", department.Description));
                                Console.WriteLine();
                                Console.WriteLine("Type a new description or enter a blank line to cancel.");

                                string input = Console.ReadLine();

                                if (!string.IsNullOrWhiteSpace(input))
                                    department.Description = input;

                                break;

                                #endregion

                            }
                        case 3:
                            {

                                #region Edit Divisions


                                DivisionsEditor.EditDivisions(command, department, department.Divisions);

                                break;

                                #endregion

                            }
                        case 4:
                            {

                                #region Done

                                keepLooping = false;
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
