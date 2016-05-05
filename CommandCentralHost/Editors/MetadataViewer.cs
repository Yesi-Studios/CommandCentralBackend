using System;
using System.Collections.Generic;
using System.Linq;
using AtwoodUtils;

namespace CommandCentralHost.Editors
{
    public static class MetadataViewer
    {
        internal static void ViewAllEntityMetadata()
        {
            bool keepLooping = true;

            while (keepLooping)
            {
                Console.Clear();

                "Welcome to the all entity metadata viewer!".WriteLine();
                "".WriteLine();
                "Choose an entity below or enter an empty line to return.".WriteLine();

                var allEntities = CommandCentral.DataAccess.NHibernateHelper.GetAllEntityMetadata().ToList();

                for (int x = 0; x < allEntities.Count; x++)
                    "{0}. {1}".FormatS(x, allEntities[x].Key).WriteLine();

                int option;
                string input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                    keepLooping = false;
                else
                    if (int.TryParse(input, out option) && option >= 0 && option < allEntities.Count)
                    {
                        ViewEntityMetadata(allEntities[option].Value);
                    }

            }
        }

        private static void ViewEntityMetadata(NHibernate.Metadata.IClassMetadata metadata)
        {
           
            Console.Clear();

            "Welcome to the entity metadata viewer!".WriteLine();
            "".WriteLine();
            "Press any key to leave.".WriteLine();
            "".WriteLine();

            var propertyNames = metadata.PropertyNames.ToList();

            //And then print them out.
            List<string[]> lines = new List<string[]> { new[] { "Property Name", "Type", "Is Nullable" } };
            for (int x = 0; x < propertyNames.Count; x++)
            {
                lines.Add(new[] { propertyNames[x], metadata.PropertyTypes.ElementAt(x).Name.Truncate(50), metadata.PropertyNullability.ElementAt(x).ToString() });
            }
            DisplayUtilities.PadElementsInLines(lines, 3).WriteLine();

            Console.ReadKey();

                

            
        }
    }
}
