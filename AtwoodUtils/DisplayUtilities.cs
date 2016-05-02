using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtwoodUtils
{
    public static class DisplayUtilities
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

    }
}
