using CommandCentral.DataAccess;
using CommandCentral.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;
using NHibernate;

namespace CommandCentralHost.Editors
{
    internal static class PermissionsEditor
    {

        internal static void PermissionEditorEntry()
        {
            bool keepLooping = true;

            while (keepLooping)
            {
                Console.Clear();

                "1. Edit Permission Groups".WriteLine();
                "2. Edit Model Permissions".WriteLine();
                "3. Return".WriteLine();

                int option;
                if (int.TryParse(Console.ReadLine(), out option) && option >= 1 && option <= 3)
                {
                    switch (option)
                    {

                        case 1:
                            {
                                EditPermissionGroups();
                                break;
                            }
                        case 2:
                            {
                                //EditModelPermissions();
                                break;
                            }
                        case 3:
                            {
                                keepLooping = false;
                                break;
                            }
                        default:
                            {
                                throw new NotImplementedException("In the permission editor entry switch.");
                            }
                    }
                }
            }
        }

        internal static void EditPermissionGroups()
        {
            bool keepLooping = true;

            using (var session = NHibernateHelper.CreateSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    while (keepLooping)
                    {

                        Console.Clear();
                        "Welcome to the permissions editor.".WriteLine();
                        "Enter the number of a a permission to edit, the number followed by '-' to delete it, a new permission name to create a new one, or a blank line to cancel.".WriteLine();
                        "".WriteLine();

                        //Let's go get all the API Keys.
                        IList<PermissionGroup> permissionGroups = session.CreateCriteria<PermissionGroup>().List<PermissionGroup>();

                        //And then print them out.
                        List<string[]> lines = new List<string[]> { new[] { "#", "Name" } };
                        for (int x = 0; x < permissionGroups.Count; x++)
                            lines.Add(new[] { x.ToString(), permissionGroups[x].Name });
                        DisplayUtilities.PadElementsInLines(lines, 3).WriteLine();

                        int option;
                        string input = Console.ReadLine();

                        if (string.IsNullOrWhiteSpace(input))
                            keepLooping = false;
                        else if (input.Last() == '-' && input.Length > 1 && int.TryParse(input.Substring(0, input.Length - 1), out option) && option >= 0 && option <= permissionGroups.Count - 1 && permissionGroups.Any())
                        {
                            session.Delete(permissionGroups[option]);
                        }
                        else if (int.TryParse(input, out option) && option >= 0 && option <= permissionGroups.Count - 1 && permissionGroups.Any())
                        {
                            //Client wants to edit an item.
                            EditPermissionGroup(permissionGroups[option], session);
                        }
                        else
                        {
                            var item = new PermissionGroup { Name = input };
                            session.Save(item);
                        }

                    }
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }

            }
        }

        private static void EditPermissionGroup(PermissionGroup group, ISession session)
        {
            bool keepLooping = true;

            while (keepLooping)
            {

                Console.Clear();

                "Editing permission group '{0}'...".FormatS(group.Name).WriteLine();
                "".WriteLine();
                "Description:\n\t{0}".FormatS(group.Description).WriteLine();
                "".WriteLine();

                "1. Edit Name".WriteLine();
                "2. Edit Description".WriteLine();
                "3. View/Edit Model Permissions".WriteLine();
                "4. View/Edit Special Permissions".WriteLine();
                "5. View/Edit Subordinate Permission Groups".WriteLine();
                "6. Return".WriteLine();

                int option;
                if (int.TryParse(Console.ReadLine(), out option) && option >= 1 && option <= 6)
                {
                    switch (option)
                    {
                        case 1:
                            {
                                Console.Clear();

                                "Enter a new permission group name...".WriteLine();
                                group.Name = Console.ReadLine();
                                break;
                            }
                        case 2:
                            {
                                Console.Clear();

                                "Enter a new description...".WriteLine();
                                group.Description = Console.ReadLine();
                                break;
                            }
                        case 3:
                            {
                                var allModelPermissions = session.QueryOver<ModelPermission>().List().ToList();

                                ListEditor.EditList(group.ModelPermissions, allModelPermissions, "Model Permissions Editor");

                                break;
                            }
                        case 4:
                            {
                                var allSpecialPermissions = session.QueryOver<CommandCentral.Authorization.ReferenceLists.SpecialPermission>().List().ToList();

                                ListEditor.EditList(group.SpecialPermissions, allSpecialPermissions, "Special Permissions Editor");

                                break;
                            }
                        case 5:
                            {
                                var allPermissionGroups = session.QueryOver<CommandCentral.Authorization.PermissionGroup>().List().ToList();

                                ListEditor.EditList(group.SubordinatePermissionGroups, allPermissionGroups, "Subordinate Permission Groups Editor");

                                break;
                            }
                        case 6:
                            {
                                keepLooping = false;

                                break;
                            }
                        default:
                            {
                                throw new NotImplementedException("In the permission editor switch.");
                            }

                    }
                }
            }
        }



    }
}
