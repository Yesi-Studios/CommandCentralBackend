using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;

namespace CommandCentralHost.Editors
{
    internal static class ListEditor
    {
        internal static void EditList<T>(IList<T> currentItems, IList<T> allPossibleItems, string interfaceName = "List Editor")
        {
            if (allPossibleItems == null || !allPossibleItems.Any())
                throw new ArgumentException("allpossibleItems can not be null nor can its count be zero.");

            if (currentItems == null)
                throw new ArgumentException("Current items may not be null.  Please initialize your list.");

            int cursorIndex = 0;
            string cursor = ">";

            bool keepLooping = true;

            var excludedObjects = allPossibleItems.Except(currentItems).ToList();

            while (keepLooping)
            {
                Console.Clear();

                "Welcome to the {0}. (ESC to return, enter to move, ↑ or ↓ to navigate)".FormatS(interfaceName).WriteLine();
                "".WriteLine();
                
                "In List:".WriteLine();
                for (int x = 0; x < currentItems.Count; x++)
                {
                    string value = ((dynamic)currentItems[x]).ToString();

                    if (cursorIndex == x)
                        "\t{0}{1}".FormatS(cursor, value).WriteLine();
                    else
                        "\t{0}".FormatS(value).WriteLine();
                }
                "".WriteLine();
                "Not In List".WriteLine();
                for (int x = 0; x < excludedObjects.Count; x++)
                {
                    string value = ((dynamic)excludedObjects[x]).ToString();

                    if (cursorIndex == x + currentItems.Count)
                        "\t{0}{1}".FormatS(cursor, value).WriteLine();
                    else
                        "\t{0}".FormatS(value).WriteLine();
                }

                ConsoleKey key = Console.ReadKey().Key;

                if (key == ConsoleKey.Escape)
                    keepLooping = false;
                else
                    if (key == ConsoleKey.UpArrow)
                    {
                        if (cursorIndex - 1 >= 0)
                            cursorIndex--;
                    }
                    else
                        if (key == ConsoleKey.DownArrow)
                        {
                            if (cursorIndex + 1 != allPossibleItems.Count)
                                cursorIndex++;
                        }
                        else
                            if (key == ConsoleKey.Enter)
                            {
                                if (cursorIndex <= currentItems.Count - 1)
                                {
                                    var obj = currentItems[cursorIndex];
                                    currentItems.RemoveAt(cursorIndex);
                                    excludedObjects.Add(obj);
                                }
                                else
                                {
                                    var obj = excludedObjects[cursorIndex - currentItems.Count];
                                    excludedObjects.RemoveAt(cursorIndex - currentItems.Count);
                                    currentItems.Add(obj);
                                }
                            }
            }
        }
    }
}
