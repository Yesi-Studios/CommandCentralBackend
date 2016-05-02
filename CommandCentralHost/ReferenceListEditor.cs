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
                try
                {
                    while (keepLooping)
                    {
                        Console.Clear();
                        "Welcome to the reference lists editor.".WL();
                        "Enter the number of a list type to edit it.  Or enter a blank line to return.".WL();
                        "".WL();

                        //Let's go get all the reference lists.

                        //First get all of the reference lists currently in the database.
                        List<CommandCentral.ReferenceListItemBase> referenceLists = session.CreateCriteria<CommandCentral.ReferenceListItemBase>().List<CommandCentral.ReferenceListItemBase>().ToList();

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
                                EditReferenceList(referenceListTypes[option], session);
                            }
                            else
                            {
                                "Your input was invalid.  Press any key to try again...".WL();
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
        public static void EditReferenceList(Type type, NHibernate.ISession session)
        {
            if (!type.IsSubclassOf(typeof(CommandCentral.ReferenceListItemBase)))
                throw new Exception("wtf is this");

            bool keepLooping = true;

            while (keepLooping)
            {
                Console.Clear();

                "Editing reference list of type '{0}'.".F(type.Name).WL();
                "Simply type a new value to add, type a number to edit the corresponding list, the number followed by a '-' to delete the item, or an empty line to return.".WL();
                "".WL();

                //Get the current values for this type.
                var values = session.CreateCriteria(type).List().Cast<CommandCentral.ReferenceListItemBase>().ToList();

                //Build the options list.
                List<string[]> lines = new List<string[]> { new[] { "#", "Value", "Description" } };
                for (int x = 0; x < values.Count; x++)
                    lines.Add(new[] { x.ToString(), values[x].Value, (values[x].Description == null) ? "" : values[x].Description });
                DisplayUtilities.PadElementsInLines(lines, 3).WL();

                int option;
                string input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                    keepLooping = false;
                else if (input.Last() == '-' && input.Length > 1 && Int32.TryParse(input.Substring(0, input.Length - 1), out option) && option >= 0 && option <= values.Count && values.Any())
                {
                    session.Delete(values[option]);
                }
                else if (Int32.TryParse(input, out option) && option >= 0 && option <= values.Count && values.Any())
                {
                    //Client wants to edit an item.
                    EditReferenceItem(values[option], session);
                }
                else
                {
                    var item = Activator.CreateInstance(type) as CommandCentral.ReferenceListItemBase;
                    item.Value = input;
                    session.Save(item);
                }


            }
        }

        /// <summary>
        /// Edits the value/Description of a single reference item.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="session"></param>
        public static void EditReferenceItem(CommandCentral.ReferenceListItemBase item, NHibernate.ISession session)
        {
            bool keepLooping = true;

            while (keepLooping)
            {
                Console.Clear();

                "Editing Reference List '{0}' Item.".F(item.GetType().Name).WL();
                "".WL();
                "Value:\n\t{0}".F(item.Value).WL();
                "Description:\n\t{0}".F(item.Description).WL();
                "".WL();

                "1. Edit Value".WL();
                "2. Edit Description".WL();
                "3. Return".WL();

                int option;
                if (Int32.TryParse(Console.ReadLine(), out option) && option >= 1 && option <= 3)
                {
                    Console.Clear();

                    switch (option)
                    {
                        case 1:
                            {
                                "Enter the new value.".WL();
                                item.Value = Console.ReadLine();
                                break;
                            }
                        case 2:
                            {
                                "Enter the new description.".WL();
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
            return System.Reflection.Assembly.GetAssembly(typeof(CommandCentral.ReferenceListItemBase)).GetTypes()
                    .Where(x => x.IsClass && !x.IsAbstract && x.IsSubclassOf(typeof(CommandCentral.ReferenceListItemBase))).ToList();
        }

    }
}
