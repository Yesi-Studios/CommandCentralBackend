using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;

namespace CommandCentralHost
{
    /// <summary>
    /// Contains the method that enable us to edit the reference lists.
    /// </summary>
    public static class ReferenceListEditor
    {
        /// <summary>
        /// Begins the edit that will edit all reference lists.
        /// </summary>
        public static void EditAllReferenceLists()
        {
            bool keepLooping = true;

            using (var session = CommandCentral.DataAccess.NHibernateHelper.CreateSession())
            using (var transaction = session.BeginTransaction())
            {
                while (keepLooping)
                {
                    Console.Clear();
                    "Welcome to the reference lists editor.".WL();
                    "Enter the number of a list type to edit it.  Or enter a blank line to return.".WL();
                    "".WL();

                    //Let's go get all the reference lists.

                    //First get all of the reference lists currently in the database.
                    List<CommandCentral.ReferenceListItemBase> referenceLists = session.CreateCriteria<CommandCentral.ReferenceListItemBase>().SetCacheable(true).SetCacheMode(NHibernate.CacheMode.Normal).List<CommandCentral.ReferenceListItemBase>().ToList();

                    //Here are all of the types.
                    List<Type> referenceListTypes = GetAllReferenceListTypes();

                    List<string[]> lines = new List<string[]> { new[] { "#", "Name", "# of Items" } };
                    for (int x = 0; x < referenceListTypes.Count; x++)
                        lines.Add(new[] { x.ToString(), referenceListTypes[x].Name, referenceLists.Where(y => y.GetType() == referenceListTypes[x]).Count().ToString() });
                    DisplayUtilities.PadElementsInLines(lines, 3).WL();

                    int option;
                    string input = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(input))
                        keepLooping = false;
                    else
                    {
                        if (Int32.TryParse(input, out option) && option >= 0 && option <= referenceListTypes.Count - 1)
                        {
                            EditReferenceList(referenceListTypes[option]);
                        }
                        else
                        {
                            "Your input was invalid.  Press any key to try again...".WL();
                            Console.ReadKey();
                        }
                    }
                }
            }
        }

        public static void EditReferenceList(Type type)
        {
            if
        }

        /// <summary>
        /// Gets all reference list items.
        /// </summary>
        /// <returns></returns>
        private static List<Type> GetAllReferenceListTypes()
        {
            return System.Reflection.Assembly.GetAssembly(typeof(CommandCentral.ReferenceListItemBase)).GetTypes()
                    .Where(x => x.IsClass && !x.IsAbstract && x.IsSubclassOf(typeof(CommandCentral.ReferenceListItemBase))).ToList();
        }

    }
}
