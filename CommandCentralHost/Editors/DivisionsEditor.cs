using CommandCentral.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;
using CommandCentral.Entities.ReferenceLists;
using NHibernate;

namespace CommandCentralHost.Editors
{
    internal static class DivisionsEditor
    {

        internal static void EditDivisions(Department department, ISession session)
        {
            bool keepLooping = true;

            while (keepLooping)
            {

                Console.Clear();
                "Welcome to the Divisions editor.".WriteLine();
                "Enter the number of a a division to edit, the number followed by '-' to delete it, a new division name to create a new division, or a blank line to cancel.".WriteLine();
                "".WriteLine();

                //And then print them out.
                List<string[]> lines = new List<string[]> { new[] { "#", "Name", "Description" } };
                for (int x = 0; x < department.Divisions.Count; x++)
                    lines.Add(new[] { x.ToString(), department.Divisions[x].Value, department.Divisions[x].Description });
                DisplayUtilities.PadElementsInLines(lines, 3).WriteLine();

                int option;
                string input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                    keepLooping = false;
                else if (input.Last() == '-' && input.Length > 1 && int.TryParse(input.Substring(0, input.Length - 1), out option) && option >= 0 && option <= department.Divisions.Count - 1 && department.Divisions.Any())
                {
                    session.Delete(department.Divisions[option]);
                }
                else if (int.TryParse(input, out option) && option >= 0 && option <= department.Divisions.Count - 1 && department.Divisions.Any())
                {
                    //Client wants to edit an item.
                    EditDivision(department.Divisions[option], session);
                }
                else
                {
                    department.Divisions.Add(new Division
                    {
                        Value = input
                    });

                    session.SaveOrUpdate(department);
                    session.Flush();

                }



            }
        }

        private static void EditDivision(Division division, ISession session)
        {
            bool keepLooping = true;

            while (keepLooping)
            {

                Console.Clear();

                "Editing division '{0}'...".FormatS(division.Value).WriteLine();
                "".WriteLine();
                "Description:\n\t{0}".FormatS(division.Description).WriteLine();
                "".WriteLine();

                "1. Edit Name".WriteLine();
                "2. Edit Description".WriteLine();
                "3. Return".WriteLine();

                int option;
                if (int.TryParse(Console.ReadLine(), out option) && option >= 1 && option <= 3)
                {
                    switch (option)
                    {
                        case 1:
                            {
                                Console.Clear();

                                "Enter a new department name...".WriteLine();
                                division.Value = Console.ReadLine();
                                break;
                            }
                        case 2:
                            {
                                Console.Clear();

                                "Enter a new description...".WriteLine();
                                division.Description = Console.ReadLine();
                                break;
                            }
                        case 3:
                            {
                                keepLooping = false;
                                break;
                            }
                        default:
                            {
                                throw new NotImplementedException("In the division editor switch.");
                            }

                    }
                }

            }
        }

    }
}
