using CommandCentral.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using AtwoodUtils;
using CommandCentral.Entities.ReferenceLists;
using NHibernate;

namespace CommandCentralHost.Editors
{
    public static class CommandsEditor
    {

        internal static void EditCommands()
        {
            bool keepLooping = true;

            using (var session = NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    while (keepLooping)
                    {

                        Console.Clear();
                        "Welcome to the Commands editor.".WriteLine();
                        "Enter the number of a a command to edit, the number followed by '-' to delete it, a new command name to create a new command, or a blank line to cancel.".WriteLine();
                        "".WriteLine();

                        //Get all the commands
                        IList<Command> commands = session.QueryOver<Command>().List<Command>();

                        if (commands.Any())
                        {
                            //And then print them out.
                            List<string[]> lines = new List<string[]> { new[] { "#", "Name", "Description" } };
                            for (int x = 0; x < commands.Count; x++)
                                lines.Add(new[] { x.ToString(), commands[x].Value, commands[x].Description });
                            DisplayUtilities.PadElementsInLines(lines, 3).WriteLine();
                        }

                        int option;
                        string input = Console.ReadLine();

                        if (string.IsNullOrWhiteSpace(input))
                            keepLooping = false;
                        else if (input.Last() == '-' && input.Length > 1 && int.TryParse(input.Substring(0, input.Length - 1), out option) && option >= 0 && option <= commands.Count - 1 && commands.Any())
                        {
                            session.Delete(commands[option]);
                        }
                        else if (int.TryParse(input, out option) && option >= 0 && option <= commands.Count - 1 && commands.Any())
                        {
                            //Client wants to edit an item.
                            EditCommand(commands[option], session);
                        }
                        else
                        {
                            var item = new Command { Value = input, Departments = new List<Department>() };
                            session.SaveOrUpdate(item);
                            session.Flush();
                        }

                        

                    }
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }

            }
        }

        private static void EditCommand(Command command, ISession session)
        {
            bool keepLooping = true;

            while (keepLooping)
            {

                Console.Clear();

                "Editing command '{0}'...".FormatS(command.Value).WriteLine();
                "".WriteLine();
                "Description:\n\t{0}".FormatS(command.Description).WriteLine();
                "".WriteLine();

                "1. Edit Name".WriteLine();
                "2. Edit Description".WriteLine();
                "3. View/Edit Departmnts".WriteLine();
                "4. Return".WriteLine();

                int option;
                if (int.TryParse(Console.ReadLine(), out option) && option >= 1 && option <= 4)
                {
                    switch (option)
                    {
                        case 1:
                            {
                                Console.Clear();

                                "Enter a new command name...".WriteLine();
                                command.Value = Console.ReadLine();
                                break;
                            }
                        case 2:
                            {
                                Console.Clear();

                                "Enter a new description...".WriteLine();
                                command.Description = Console.ReadLine();
                                break;
                            }
                        case 3:
                            {
                                DepartmentsEditor.EditDepartments(command, session);
                                break;
                            }
                        case 4:
                            {
                                keepLooping = false;
                                break;
                            }
                        default:
                            {
                                throw new NotImplementedException("In the command editor switch.");
                            }

                    }
                }

            }
        }

    }
}
