using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;

namespace CDBServiceHost.Interfaces
{
    public static class DivisionsEditor
    {
        public static void EditDivisions(CommandCentral.Commands.Command command, CommandCentral.Commands.Command.Department department, List<CommandCentral.Commands.Command.Department.Division> divisions)
        {
            bool keepLooping = true;
            while (keepLooping)
            {
                Console.Clear();
                Console.WriteLine(string.Format("There are currently {0} division(s).", divisions.Count));
                Console.WriteLine();
                Console.WriteLine("Type the number of the division you would like to edit, type the number followed by a minus sign to delete that division, enter a new division name to create a new division, or enter a blank line to cancel.");
                Console.WriteLine();

                List<string[]> lines = new List<string[]>();
                lines.Add(new[] { "#", "Name", "Description" });
                for (int x = 0; x < divisions.Count; x++)
                {
                    lines.Add(new[] { x.ToString(), divisions[x].Name, divisions[x].Description.Truncate(20) });
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
                        if (Int32.TryParse(input.Substring(0, input.Length - 1), out deleteInput) && deleteInput >= 0 && deleteInput <= divisions.Count - 1)
                        {
                            Console.Clear();

                            CommandCentral.Commands.Command.Department.Division divToDelete = divisions[deleteInput];

                            int currentInDivision = CommandCentral.Persons.CountPersonsInDivision(command.Name, department.Name, divisions[deleteInput].Name).Result;

                            if (currentInDivision == 0)
                            {
                                Console.WriteLine("Are you sure you want to delete the following division? (y)");
                                Console.WriteLine();
                                List<string[]> divisionEntry = new List<string[]>();
                                divisionEntry.Add(new[] { "ID", "Name", "Description" });
                                divisionEntry.Add(new[] { divToDelete.ID, divToDelete.Name, divToDelete.Description.Truncate(20) });
                                Console.WriteLine(Interfaces.GenericInterfaces.PadElementsInLines(divisionEntry, 3));

                                if (Console.ReadLine().ToLower() == "y")
                                {
                                    divisions.Remove(divToDelete);
                                }

                            }
                            else
                            {
                                Console.WriteLine(string.Format("{0} user(s) exist in the division named '{1}'!  Cannot delete division until they are assigned to a different division.", currentInDivision, divToDelete.Name));
                            }


                        }
                    }
                    else
                        if (Int32.TryParse(input, out inputOption) && inputOption >= 0 && inputOption <= divisions.Count - 1)
                        {
                            EditDivision(divisions[inputOption]);
                        }
                        else //User entered a new name for a department.
                        {
                            Console.Clear();
                            Console.WriteLine(string.Format("Do you want to make a new division named '{0}'? (y)", input));

                            if (Console.ReadLine().ToLower() == "y")
                            {
                                CommandCentral.Commands.Command.Department.Division newDivision = new CommandCentral.Commands.Command.Department.Division()
                                {
                                    Description = "",
                                    ID = Guid.NewGuid().ToString(),
                                    Name = input
                                };

                                divisions.Add(newDivision);

                                EditDivision(newDivision);

                            }
                        }

            }
        }

        public static void EditDivision(CommandCentral.Commands.Command.Department.Division division)
        {
            bool keepLooping = true;
            while (keepLooping)
            {
                Console.Clear();
                Console.WriteLine(string.Format("Editing division '{0}'...", division.Name));
                Console.WriteLine();
                Console.WriteLine("Description:");
                Console.WriteLine(string.Format("\t{0}", division.Description));


                Console.WriteLine();
                Console.WriteLine("1. Edit Name");
                Console.WriteLine("2. Edit Description");
                Console.WriteLine("3. Done");

                int option = 0;
                if (Int32.TryParse(Console.ReadLine(), out option))
                {
                    switch (option)
                    {
                        case 1: //Edit Name
                            {

                                #region Edit Name

                                Console.Clear();
                                Console.WriteLine(string.Format("The current name of the division is '{0}'.", division.Name));
                                Console.WriteLine();
                                Console.WriteLine("Type a new name or enter a blank line to cancel.");

                                string input = Console.ReadLine();

                                if (!string.IsNullOrWhiteSpace(input))
                                    division.Name = input;

                                break;

                                #endregion

                            }
                        case 2:
                            {

                                #region Edit Description

                                Console.Clear();
                                Console.WriteLine(string.Format("The current description of the division is \n\t'{0}'.", division.Description));
                                Console.WriteLine();
                                Console.WriteLine("Type a new description or enter a blank line to cancel.");

                                string input = Console.ReadLine();

                                if (!string.IsNullOrWhiteSpace(input))
                                    division.Description = input;

                                break;

                                #endregion

                            }
                        case 3:
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
