using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDBServiceHost.Interfaces
{

    public static class GenericInterfaces
    {

        public static string PadElementsInLines(List<string[]> lines, int padding = 1)
        {

            var numElements = lines[0].Length;
            var maxValues = new int[numElements];
            for (int x = 0; x < numElements; x++)
            {
                maxValues[x] = lines.Max(y => y[x].Length) + padding;
            }

            StringBuilder sb = new StringBuilder();

            bool isFirst = true;
            foreach (var line in lines)
            {
                if (!isFirst)
                {
                    sb.AppendLine();
                }
                isFirst = false;
                for (int x = 0; x < line.Length; x++)
                {
                    var value = line[x];
                    sb.Append(value.PadRight(maxValues[x]));
                }
            }
            return sb.ToString();


        }

        /// <summary>
        /// Provides an interface for editing two lists of strings and moving those strings between either side.
        /// </summary>
        /// <param name="includes"></param>
        /// <param name="excludes"></param>
        public static void EditElementsOfLists(List<string> list1, List<string> list2, string list1Title, string list2Title, string mainTitle)
        {
            while (true)
            {
                Console.Clear();


                Console.WriteLine(mainTitle);
                Console.WriteLine();
                Console.WriteLine("The available values are below.  In order to move an item from one list to the other, simply type its number.  Ether  Enter a blank line (just press enter) when you're done.");
                Console.WriteLine();
                Console.WriteLine(string.Format("{0}:", list1Title));
                for (int x = 0; x < list1.Count; x++)
                {
                    Console.WriteLine(string.Format("\t{0}. {1}", x, list1[x]));
                }
                Console.WriteLine();
                Console.WriteLine(string.Format("{0}:", list2Title));
                for (int x = list1.Count; x < list2.Count + list1.Count; x++)
                {
                    Console.WriteLine(string.Format("\t{0}. {1}", x, list2[x - list1.Count]));
                }

                string optionText = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(optionText))
                    return;
                else
                {
                    int option = -1;
                    if (Int32.TryParse(optionText, out option))
                    {
                        if (option >= 0 && option <= list1.Count - 1 && list1.Count > 0)
                        {
                            string item = list1[option];
                            list2.Add(item);
                            list1.Remove(item);
                        }
                        else
                            if (option <= list2.Count + list1.Count)
                            {
                                string item = list2[option - list1.Count];
                                list2.Remove(item);
                                list1.Add(item);
                            }
                    }
                }

            }
        }
    }
}
