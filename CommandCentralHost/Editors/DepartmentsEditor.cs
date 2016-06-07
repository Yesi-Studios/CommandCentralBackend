using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AtwoodUtils;
using CommandCentral.Entities.ReferenceLists;
using NHibernate;

namespace CommandCentralHost.Editors
{
    internal static class DepartmentsEditor
    {

        internal static void EditDepartments(Command command, ISession session)
        {
            bool keepLooping = true;

            while (keepLooping)
            {

                Console.Clear();
                "Welcome to the Departments editor.".WriteLine();
                "Enter the number of a a department to edit, the number followed by '-' to delete it, a new department name to create a new department, or a blank line to cancel.".WriteLine();
                "".WriteLine();

                //And then print them out.
                List<string[]> lines = new List<string[]> { new[] { "#", "Name", "Description" } };
                for (int x = 0; x < command.Departments.Count; x++)
                    lines.Add(new[] { x.ToString(), command.Departments[x].Value, command.Departments[x].Description });
                DisplayUtilities.PadElementsInLines(lines, 3).WriteLine();

                int option;
                string input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                    keepLooping = false;
                else if (input.Last() == '-' && input.Length > 1 && int.TryParse(input.Substring(0, input.Length - 1), out option) && option >= 0 && option <= command.Departments.Count - 1 && command.Departments.Any())
                {
                    session.Delete(command.Departments[option]);
                }
                else if (int.TryParse(input, out option) && option >= 0 && option <= command.Departments.Count - 1 && command.Departments.Any())
                {
                    //Client wants to edit an item.
                    EditDepartment(command.Departments[option], session);
                }
                else
                {
                    command.Departments.Add(new Department
                    {
                        Value = input,
                        Divisions = new List<Division>()
                    });
                    session.SaveOrUpdate(command);
                    session.Flush();
                }



            }
        }

        private static void EditDepartment(Department department, ISession session)
        {
            bool keepLooping = true;

            while (keepLooping)
            {

                Console.Clear();

                "Editing department '{0}'...".FormatS(department.Value).WriteLine();
                "".WriteLine();
                "Description:\n\t{0}".FormatS(department.Description).WriteLine();
                "".WriteLine();

                "1. Edit Name".WriteLine();
                "2. Edit Description".WriteLine();
                "3. View/Edit Divisions".WriteLine();
                "4. Return".WriteLine();

                int option;
                if (int.TryParse(Console.ReadLine(), out option) && option >= 1 && option <= 4)
                {
                    switch (option)
                    {
                        case 1:
                            {
                                Console.Clear();

                                "Enter a new department name...".WriteLine();
                                department.Value = Console.ReadLine();
                                break;
                            }
                        case 2:
                            {
                                Console.Clear();

                                "Enter a new description...".WriteLine();
                                department.Description = Console.ReadLine();
                                break;
                            }
                        case 3:
                            {
                                DivisionsEditor.EditDivisions(department, session);
                                break;
                            }
                        case 4:
                            {
                                keepLooping = false;
                                break;
                            }
                        default:
                            {
                                throw new NotImplementedException("In the department editor switch.");
                            }

                    }
                }

            }
        }

    }
}
