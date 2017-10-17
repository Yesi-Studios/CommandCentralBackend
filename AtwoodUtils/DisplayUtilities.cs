using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AtwoodUtils
{
    public static class DisplayUtilities
    {

        public static string PadElementsInLines(List<string[]> lines, int padding = 1)
        {

            var numElements = lines[0].Length;
            var maxValues = new int[numElements];
            for (var x = 0; x < numElements; x++)
            {
                maxValues[x] = lines.Max(y => (y[x] ?? "").Length) + padding;
            }

            var sb = new StringBuilder();

            var isFirst = true;
            foreach (var line in lines)
            {
                if (!isFirst)
                {
                    sb.AppendLine();
                }
                
                for (var x = 0; x < line.Length; x++)
                {
                    var value = line[x];
                    sb.Append((value ?? "").PadRight(maxValues[x]));
                }

                if (isFirst)
                    sb.AppendLine();

                isFirst = false;

            }
            return sb.ToString();


        }

    }
}
