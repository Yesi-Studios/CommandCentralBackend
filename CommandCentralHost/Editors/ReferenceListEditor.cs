using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using AtwoodUtils;
using CommandCentral;
using CommandCentral.DataAccess;
using NHibernate;

namespace CommandCentralHost.Editors
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

            using (var session = NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    while (keepLooping)
                    {
                        Console.Clear();
                        "Welcome to the reference lists editor.".WriteLine();
                        "Enter the number of a list type to edit it.  Or enter a blank line to return.".WriteLine();
                        "".WriteLine();

                        //Let's go get all the reference lists.

                        //First get all of the reference lists currently in the database.
                        List<ReferenceListItemBase> referenceLists = session.CreateCriteria<ReferenceListItemBase>().List<ReferenceListItemBase>().ToList();

                        //Here are all of the types.
                        List<Type> referenceListTypes = GetAllReferenceListTypes();

                        List<string[]> lines = new List<string[]> { new[] { "#", "Name", "# of Items" } };
                        for (int x = 0; x < referenceListTypes.Count; x++)
                            lines.Add(new[] { x.ToString(), referenceListTypes[x].Name, referenceLists.Count(y => y.GetType() == referenceListTypes[x]).ToString() });
                        DisplayUtilities.PadElementsInLines(lines, 3).WriteLine();

                        string input = Console.ReadLine();

                        if (string.IsNullOrWhiteSpace(input))
                            keepLooping = false;
                        else
                        {
                            int option;
                            if (int.TryParse(input, out option) && option >= 0 && option <= referenceListTypes.Count - 1)
                            {
                                EditReferenceList(referenceListTypes[option], session);
                            }
                            else
                            {
                                "Your input was invalid.  Press any key to try again...".WriteLine();
                                Console.ReadKey();
                            }
                        }
                    }

                    transaction.Commit();
                }
                catch
                {
                    if (!transaction.WasCommitted)
                        transaction.Rollback();
                    throw;
                }
                
            }
        }

        /// <summary>
        /// Edits a single reference list type.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="session"></param>
        private static void EditReferenceList(Type type, ISession session)
        {
            if (!type.IsSubclassOf(typeof(ReferenceListItemBase)))
                throw new Exception("wtf is this");

            bool keepLooping = true;

            while (keepLooping)
            {
                Console.Clear();

                "Editing reference list of type '{0}'.".FormatS(type.Name).WriteLine();
                "Simply type a new value to add, type a number to edit the corresponding list, the number followed by a '-' to delete the item, or an empty line to return.".WriteLine();
                "".WriteLine();

                //Get the current values for this type.
                var values = session.CreateCriteria(type).List().Cast<ReferenceListItemBase>().ToList();

                //Build the options list.
                List<string[]> lines = new List<string[]> { new[] { "#", "Value", "Description" } };
                for (int x = 0; x < values.Count; x++)
                    lines.Add(new[] { x.ToString(), values[x].Value, values[x].Description ?? "" });
                DisplayUtilities.PadElementsInLines(lines, 3).WriteLine();

                int option;
                string input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                    keepLooping = false;
                else if (input.Last() == '-' && input.Length > 1 && int.TryParse(input.Substring(0, input.Length - 1), out option) && option >= 0 && option <= values.Count - 1 && values.Any())
                {
                    session.Delete(values[option]);
                }
                else if (int.TryParse(input, out option) && option >= 0 && option <= values.Count - 1 && values.Any())
                {
                    //Client wants to edit an item.
                    EditReferenceItem(values[option]);
                }
                else
                {
                    var item = Activator.CreateInstance(type) as ReferenceListItemBase;
                    Debug.Assert(item != null, "item != null");
                    item.Value = input;
                    session.Save(item);
                }


            }
        }

        /// <summary>
        /// Edits the value/Description of a single reference item.
        /// </summary>
        /// <param name="item"></param>
        /// 
        private static void EditReferenceItem(ReferenceListItemBase item)
        {
            bool keepLooping = true;

            while (keepLooping)
            {
                Console.Clear();

                "Editing Reference List '{0}' Item.".FormatS(item.GetType().Name).WriteLine();
                "".WriteLine();
                "Value:\n\t{0}".FormatS(item.Value).WriteLine();
                "Description:\n\t{0}".FormatS(item.Description).WriteLine();
                "".WriteLine();

                "1. Edit Value".WriteLine();
                "2. Edit Description".WriteLine();
                "3. Return".WriteLine();

                int option;
                if (int.TryParse(Console.ReadLine(), out option) && option >= 1 && option <= 3)
                {
                    Console.Clear();

                    switch (option)
                    {
                        case 1:
                            {
                                "Enter the new value.".WriteLine();
                                item.Value = Console.ReadLine();
                                break;
                            }
                        case 2:
                            {
                                "Enter the new description.".WriteLine();
                                item.Description = Console.ReadLine();
                                break;
                            }
                        case 3:
                            {
                                keepLooping = false;
                                break;
                            }
                        default:
                            {
                                throw new NotImplementedException("In the reference list item editor switch.");
                            }
                    }
                }
            }
        }

        /// <summary>
        /// Gets all reference list items.
        /// </summary>
        /// <returns></returns>
        private static List<Type> GetAllReferenceListTypes()
        {
            return Assembly.GetAssembly(typeof(ReferenceListItemBase)).GetTypes()
                    .Where(x => x.IsClass && !x.IsAbstract && x.IsSubclassOf(typeof(ReferenceListItemBase))).ToList();
        }

    }
}
