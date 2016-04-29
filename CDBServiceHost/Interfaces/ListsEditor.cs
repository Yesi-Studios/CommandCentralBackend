using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDBServiceHost.Interfaces
{
    public static class ListsEditor
    {
        public static void EditList(CommandCentral.CDBLists.CDBList list)
        {

            bool keepLooping = true;
            bool isDirty = false;

            while (keepLooping)
            {
                Console.Clear();

                Console.WriteLine(string.Format("Editing list: '{0}'", list.Name));
                Console.WriteLine();
                Console.WriteLine("Type the number of an element to edit it, the number followed by a minus sign to delete it, a new string to add an element, two minus signs to delete all elements, or a blank line to return.");
                Console.WriteLine();

                List<string[]> lines = new List<string[]>();
                lines.Add(new[] { "#", "Value" });
                for (int x = 0; x < list.Values.Count; x++)
                {
                    lines.Add(new[] { x.ToString(), list.Values[x] });
                }
                Console.WriteLine(Interfaces.GenericInterfaces.PadElementsInLines(lines, 3));

                string input = Console.ReadLine();
                int numOption;

                if (string.IsNullOrWhiteSpace(input)) //user wants out
                {
                    if (isDirty)
                    {
                        Console.Clear();
                        Console.WriteLine(string.Format("Do you want to save changes to the list '{0}'? (y)", list.Name));

                        if (Console.ReadLine().ToLower() == "y")
                        {
                            if (list.DBExists(true).Result)
                                list.DBUpdate(true).Wait();
                            else
                                list.DBInsert(true).Wait();
                            Console.WriteLine("List successfully updated!");
                        }
                        else
                        {
                            Console.WriteLine("Discarding changes...");
                        }
                    }
                    else
                    {
                        Console.WriteLine("No changes detected...");
                    }

                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();

                    keepLooping = false;

                }
                else
                    if (input == "--") //user wants to delete all elements
                    {
                        list.Values.Clear();
                        isDirty = true;
                    }
                    else
                        if (input.Contains("-") && Int32.TryParse(input.Replace("-", ""), out numOption) && numOption >= 0 && numOption <= list.Values.Count - 1) //User wants to delete a specific element
                        {
                            list.Values.RemoveAt(numOption);
                            isDirty = true;
                        }
                        else
                            if (Int32.TryParse(input, out numOption) && numOption >= 0 && numOption <= list.Values.Count - 1) //User wants to edit an element
                            {
                                Console.Clear();
                                Console.WriteLine(string.Format("The value of this element is currently: '{0}'", list.Values[numOption]));
                                Console.WriteLine();
                                Console.WriteLine("Type the new value...");
                                string value = Console.ReadLine();
                                list.Values[numOption] = value;
                                isDirty = true;
                            }
                            else //Add a new element
                            {
                                list.Values.Add(input);
                                isDirty = true;
                            }
            }
        }
    }
}
