using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using AtwoodUtils;

namespace CDBServiceHost.Interfaces
{
    public static class PermissionGroups
    {

        public static void EditUsersPermissions()
        {
            Console.Clear();
            Console.WriteLine("Type the last name of the user whose permissions you would like to edit...");
            string input = Console.ReadLine();

            var results = CommandCentral.Persons.DBSimpleSearch(input, new List<string>()
            {
                "LastName"
            }, new List<string>() 
            {
                "ID", "FirstName", "LastName"
            }, null, null).Result
            .Select(x =>
                {
                    return new
                    {
                        ID = x["ID"] as string,
                        FirstName = x["FirstName"] as string,
                        LastName = x["LastName"] as string
                    };
                }).ToList();

            
            bool keepLooping = true;
            while (keepLooping)
            {

                Console.Clear();


                Console.Clear();
                Console.WriteLine(string.Format("{0} result(s) were found. Type the number of the user you want to edit, type a new last name to search for, or type an empty line to go back.", results.Count));
                Console.WriteLine();
                for (int x = 0; x < results.Count; x++)
                {
                    Console.WriteLine(string.Format("{0}. {1}", x, string.Format("{0}, {1}", results[x].LastName, results[x].FirstName)));
                }

                string option = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(option) || string.IsNullOrEmpty(option)) 
                {
                    return;
                }
                else
                    if (option.All(char.IsLetter)) //user typed a new name
                    {
                        results = CommandCentral.Persons.DBSimpleSearch(input, new List<string>()
                        {
                            "LastName"
                        }, new List<string>() 
                        {
                            "ID", "FirstName", "LastName"
                        }, null, null).Result
                        .Select(x =>
                            {
                                return new
                                {
                                    ID = x["ID"] as string,
                                    FirstName = x["FirstName"] as string,
                                    LastName = x["LastName"] as string
                                };
                            }).ToList();
                    }
                    else
                    {
                        int numOption = -1;

                        if (Int32.TryParse(option, out numOption) && numOption >= 0 && numOption <= results.Count - 1)
                        {

                            bool keepLoopingEditPermissions = true;

                            while (keepLoopingEditPermissions)
                            {
                                Console.Clear();

                                string personID = results[numOption].ID ;
                                string name = string.Format("{0}, {1}", results[numOption].LastName, results[numOption].FirstName);

                                Console.WriteLine(string.Format("Editing permissions for '{0}'...", name));
                                Console.WriteLine();

                                Console.WriteLine("The user has the following permission groups...");
                                Console.WriteLine();

                                List<UnifiedServiceFramework.Authorization.Permissions.PermissionGroup> perms = CommandCentral.CustomAuthorization.CustomPermissions.GetPermissionGroupsForUser(personID).Result;
                                for (int x = 0; x < perms.Count; x++)
                                {
                                    Console.WriteLine(string.Format("{0}. {1}", x, perms[x].Name));
                                }

                                Console.WriteLine();
                                Console.WriteLine("Choose an option...");
                                Console.WriteLine();
                                Console.WriteLine("1. Add/Remove groups");
                                Console.WriteLine("2. Cancel");

                                int editPermissionsOption = 0;

                                if (Int32.TryParse(Console.ReadLine(), out editPermissionsOption) && editPermissionsOption >= 1 && editPermissionsOption <= 2)
                                {
                                    switch (editPermissionsOption)
                                    {
                                        case 1:
                                            {

                                                List<string> activePermissionGroups = perms.Select(x => x.Name).ToList();
                                                List<string> inactivePermissionGroups = UnifiedServiceFramework.Authorization.Permissions.GetAllPermissionGroups().Where(x => !activePermissionGroups.Contains(x.Name)).Select(x => x.Name).ToList();

                                                GenericInterfaces.EditElementsOfLists(activePermissionGroups, inactivePermissionGroups, "Active Permission Groups", "Inactive Permission Groups", "Edit User Permissions");

                                                Console.Clear();
                                                Console.WriteLine("Updating permissions...");

                                                CommandCentral.CustomAuthorization.CustomPermissions.SetUserPermissionGroups(personID, 
                                                    UnifiedServiceFramework.Authorization.Permissions.GetAllPermissionGroups()
                                                        .Where(x => activePermissionGroups.Contains(x.Name)).Select(x => x.ID).ToList()).Wait();

                                                Console.WriteLine("Permissions updated...");
                                                Console.WriteLine("Press any key...");
                                                Console.ReadKey();

                                                break;
                                            }
                                        case 2:
                                            {
                                                keepLoopingEditPermissions = false;
                                                break;
                                            }
                                        default:
                                            {
                                                throw new NotImplementedException("In edit user permissions.");
                                            }
                                    }
                                }

                            }

                            

                        }

                    }

                

                

            }
        }

        public static void EditPermissionGroup(UnifiedServiceFramework.Authorization.Permissions.PermissionGroup permGroup)
        {
            bool keepLoopingPermissionGroupEditor = true;
            while (keepLoopingPermissionGroupEditor)
            {

                Console.Clear();

                Console.WriteLine(string.Format("Editing Permission Group: '{0}'", permGroup.Name));
                Console.WriteLine();

                Console.WriteLine("1. Edit Model Permissions");
                Console.WriteLine("2. Edit Special Permissions");
                Console.WriteLine("3. Edit Subordinate Permission Groups");
                Console.WriteLine("4. Done & Save");
                Console.WriteLine("5. Done & Don't Save");

                int newPermEditOption = 0;

                if (Int32.TryParse(Console.ReadLine(), out newPermEditOption))
                {
                    switch (newPermEditOption)
                    {
                        case 1:
                            {

                                #region Edit Model Permissions

                                bool keepLoopingModelPermission = true;
                                while (keepLoopingModelPermission)
                                {
                                    Console.Clear();

                                    Console.WriteLine(string.Format("Editing Permission Group: '{0}'", permGroup.Name));
                                    Console.WriteLine();

                                    Console.WriteLine(string.Format("This group currently has permissions for {0}/{1} models:", permGroup.ModelPermissions.Count, UnifiedServiceFramework.Authorization.Permissions.ModelsAndFields.Count));
                                    Console.WriteLine("Type the number next to a model to edit its permissions.");
                                    Console.WriteLine("Type an empty line when you're done.");
                                    Console.WriteLine();

                                    var allModels = new List<UnifiedServiceFramework.Authorization.Permissions.PermissionGroup.ModelPermission>();
                                    allModels.AddRange(permGroup.ModelPermissions.OrderBy(x => x.ReturnableFields.Count + x.SearchableFields.Count + x.EditableFields.Count).ToList());
                                    allModels.AddRange(UnifiedServiceFramework.Authorization.Permissions.ModelsAndFields
                                        .Where(x => !allModels.Exists(y => y.ModelName == x.Key)).ToList()
                                        .Select(x => new UnifiedServiceFramework.Authorization.Permissions.PermissionGroup.ModelPermission() { ModelName = x.Key })
                                        .OrderBy(x => x.ModelName));
                                    
                                    List<string[]> lines = new List<string[]>();
                                    lines.Add(new[] { "#", "Name", "Search", "Return", "Edit" });
                                    for (int x = 0; x < allModels.Count; x++)
                                    {
                                        lines.Add(new[] { x.ToString() + ".", 
                                                          allModels[x].ModelName, 
                                                          string.Format("{0}/{1}", allModels[x].SearchableFields.Count, UnifiedServiceFramework.Authorization.Permissions.ModelsAndFields[allModels[x].ModelName].Count),
                                                          string.Format("{0}/{1}", allModels[x].ReturnableFields.Count, UnifiedServiceFramework.Authorization.Permissions.ModelsAndFields[allModels[x].ModelName].Count),
                                                          string.Format("{0}/{1}", allModels[x].EditableFields.Count, UnifiedServiceFramework.Authorization.Permissions.ModelsAndFields[allModels[x].ModelName].Count)
                                                        });
                                    }
                                    Console.WriteLine(Interfaces.GenericInterfaces.PadElementsInLines(lines, 3));

                                    string modelPermissionsOptionText = Console.ReadLine();
                                    int modelPermissionsOption = -1;

                                    if (string.IsNullOrWhiteSpace(modelPermissionsOptionText))
                                    {
                                        keepLoopingModelPermission = false;
                                        //Remove those model permissions that have nothing in them, to save a little space in the database.
                                        permGroup.ModelPermissions = allModels.Where(x => x.ReturnableFields.Count > 0 || x.SearchableFields.Count > 0 || x.EditableFields.Count > 0).ToList();
                                    }
                                    else
                                        if (Int32.TryParse(modelPermissionsOptionText, out modelPermissionsOption))
                                        {
                                            Interfaces.TablePermissions.EditSearchableAndReturnableFields(allModels[modelPermissionsOption]);

                                            //If the model permission that was just edited now contains permissions, add it to the new perm.  
                                            //We can also take this opportunity to remove empty ones.  This is a bit brutish.  Whatevs.
                                            permGroup.ModelPermissions = allModels.Where(x => x.ReturnableFields.Count > 0 || x.SearchableFields.Count > 0 || x.EditableFields.Count > 0).ToList();
                                        }
                                }

                                break;

                                #endregion

                            }
                        case 2:
                            {

                                #region Edit Special Permissions

                                Console.Clear();

                                List<string> activePerms = permGroup.CustomPermissions.Select(x => x.ToString()).ToList();
                                List<string> inactivePerms = Enum.GetNames(typeof(CommandCentral.SpecialPermissionTypes)).Except(activePerms).ToList();

                                Interfaces.GenericInterfaces.EditElementsOfLists(activePerms, inactivePerms, "Active", "Inactive", "Edit Custom Permissions...");

                                permGroup.CustomPermissions = activePerms;

                                break;

                                #endregion

                            }
                        case 3:
                            {

                                #region Edit Subordinate Groups

                                Console.Clear();

                                List<string> subGroupNames = UnifiedServiceFramework.Authorization.Permissions.GetAllPermissionGroups()
                                    .Where(x => permGroup.SubordinatePermissionGroupIDs.Contains(x.ID))
                                    .Select(x => x.Name).ToList();
                                List<string> nonSubGroupNames = UnifiedServiceFramework.Authorization.Permissions.GetAllPermissionGroups().Select(x => x.Name).Except(subGroupNames).ToList();

                                Interfaces.GenericInterfaces.EditElementsOfLists(subGroupNames, nonSubGroupNames, "Subordinate", "Inactive", "Edit Subordinate Permission Groups...");

                                //Note that we're doing the edits with the permission group names.  This is ok because the names *should* be unique due to the way they're created.
                                //However, when we go to cast those back into the perm group, we need to turn them into their IDs.
                                permGroup.SubordinatePermissionGroupIDs = subGroupNames.Select(x => UnifiedServiceFramework.Authorization.Permissions.GetAllPermissionGroups().First(y => y.Name == x).ID).ToList();

                                break;

                                #endregion

                            }
                        case 4:
                            {

                                #region Done And Save

                                Console.Clear();

                                Console.WriteLine("Are you sure you want to save changes to this permission? (y)");

                                if (Console.ReadLine().ToLower() == "y")
                                {
                                    if (permGroup.DBExists().Result)
                                    {
                                        permGroup.DBUpdate(true).Wait();
                                    }
                                    else
                                    {
                                        permGroup.DBInsert(true).Wait();
                                    }

                                    keepLoopingPermissionGroupEditor = false;
                                }

                                break;

                                #endregion

                            }
                        case 5:
                            {
                                #region Done And Don't Save

                                Console.Clear();

                                Console.WriteLine("Are you sure you want to discard changes to this permission? (y)");

                                if (Console.ReadLine().ToLower() == "y")
                                {
                                    keepLoopingPermissionGroupEditor = false;
                                }

                                break;

                                #endregion
                            }
                    }
                }
            }
        }

    }
}
