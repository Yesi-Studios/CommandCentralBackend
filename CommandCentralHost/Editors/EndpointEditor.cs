using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;

namespace CommandCentralHost.Editors
{
    public static class EndpointEditor
    {

        public static void EditEndpoints()
        {
            bool keepLooping = true;

            while (keepLooping)
            {
                var endpoints = CommandCentral.ClientAccess.Service.CommandCentralService.EndpointDescriptions;

                Console.Clear();
                "Welcome to the endpoints editor.".WriteLine();
                "Enter the number of an endpoint to enable/disable it or a blank line to cancel.".WriteLine();
                "".WriteLine();

                //And then print them out.
                List<string[]> lines = new List<string[]> { new[] { "#", "Name", "Is Active", "Description" } };
                for (int x = 0; x < endpoints.Count; x++)
                    lines.Add(new[] { x.ToString(), endpoints.ElementAt(x).Key, endpoints.ElementAt(x).Value.IsActive.ToString(), endpoints.ElementAt(x).Value.Description.Truncate(150) });
                DisplayUtilities.PadElementsInLines(lines, 3).WriteLine();

                int option;
                string input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input))
                    keepLooping = false;
                else 
                    if (int.TryParse(input, out option) && option >= 0 && option <= endpoints.Count - 1)
                        endpoints.ElementAt(option).Value.IsActive = !endpoints.ElementAt(option).Value.IsActive;
            }
        }

    }
}
