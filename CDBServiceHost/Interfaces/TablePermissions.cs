using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDBServiceHost.Interfaces
{
    public static class TablePermissions
    {

        public static void EditSearchableAndReturnableFields(UnifiedServiceFramework.Authorization.Permissions.PermissionGroup.ModelPermission modelPermission)
        {

            while (true)
            {
                Console.Clear();

                //Tell the user what model we're going to edit.
                Console.WriteLine(string.Format("Editing model permissions for model '{0}'.", modelPermission.ModelName));
                Console.WriteLine();

                //Options
                Console.WriteLine("1. Edit Searchable Fields");
                Console.WriteLine("2. Edit Returnable Fields");
                Console.WriteLine("3. Edit Editable Fields");
                Console.WriteLine("4. Done");

                //If the option isn't a number just restart the loop.  The user will figure it out. lol.

                int option = 0;
                if (Int32.TryParse(Console.ReadLine(), out option))
                {
                    switch (option)
                    {
                        case 1: //Edit Searchable Fields
                            {

                                #region Edit Searchable Fields

                                List<string> includes = modelPermission.SearchableFields;
                                List<string> excludes = UnifiedServiceFramework.Authorization.Permissions.ModelsAndFields[modelPermission.ModelName].Except(includes).ToList();

                                GenericInterfaces.EditElementsOfLists(includes, excludes, "Include", "Exclude", "Edit Searchable Fields");

                                break;

                                #endregion

                            }
                        case 2:
                            {

                                #region Edit Returnable Fields

                                List<string> includes = modelPermission.ReturnableFields;
                                List<string> excludes = UnifiedServiceFramework.Authorization.Permissions.ModelsAndFields[modelPermission.ModelName].Except(includes).ToList();

                                GenericInterfaces.EditElementsOfLists(includes, excludes, "Include", "Exclude", "Edit Returnable Fields");

                                break;

                                #endregion

                            }
                        case 3:
                            {

                                #region Edit Editable Fields

                                List<string> includes = modelPermission.EditableFields;
                                List<string> excludes = UnifiedServiceFramework.Authorization.Permissions.ModelsAndFields[modelPermission.ModelName].Except(includes).ToList();

                                GenericInterfaces.EditElementsOfLists(includes, excludes, "Include", "Exclude", "Edit Editable Fields");

                                break;

                                #endregion

                            }
                        case 4:
                            {

                                #region Done

                                return;

                                #endregion

                            }
                        default:
                            {
                                throw new NotImplementedException();
                            }
                    }
                }
            }
        }

        

    }
}
