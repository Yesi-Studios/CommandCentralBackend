using CommandCentral.DataAccess;
using CommandCentral.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
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
                "3. Manage User Permissions".WriteLine();
                "4. Return".WriteLine();

                int option;
                if (int.TryParse(Console.ReadLine(), out option) && option >= 1 && option <= 4)
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
                                EditModelPermissions();
                                break;
                            }
                        case 3:
                            {
                                EditUserPermissions();
                                break;
                            }
                        case 4:
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

        private static void EditPermissionGroups()
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
                        "Welcome to the permission groups editor.".WriteLine();
                        "Enter the number of a permission group to edit, the number followed by '-' to delete it, a new permission group name to create a new one, or a blank line to cancel.".WriteLine();
                        "".WriteLine();

                        //Let's go get all the permission groups.
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
                            //Client wants to make a new permission group.
                            var item = new PermissionGroup { Name = input, PermissionTrack = PermissionTracks.None };
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
                "Permission Level:\n\t{0}".FormatS(group.PermissionLevel).WriteLine();
                "".WriteLine();
                "Permission Track:\n\t{0}".FormatS(group.PermissionTrack).WriteLine();
                "".WriteLine();

                "1. Edit Name".WriteLine();
                "2. Edit Description".WriteLine();
                "3. View/Edit Model Permissions".WriteLine();
                "4. View/Edit Special Permissions".WriteLine();
                "5. View/Edit Subordinate Permission Groups".WriteLine();
                "6. View/Edit Permission Level".WriteLine();
                "7. View/Edit Permission Track".WriteLine();
                "8. Return".WriteLine();

                int option;
                if (int.TryParse(Console.ReadLine(), out option) && option >= 1 && option <= 8)
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
                                var allSpecialPermissions = Enum.GetNames(typeof(SpecialPermissions)).ToEnumList().ToList();

                                ListEditor.EditList(group.SpecialPermissions, allSpecialPermissions, "Special Permissions Editor");

                                break;
                            }
                        case 5:
                            {
                                var allPermissionGroups = session.QueryOver<PermissionGroup>().List().ToList();

                                ListEditor.EditList(group.SubordinatePermissionGroups, allPermissionGroups, "Subordinate Permission Groups Editor");

                                break;
                            }
                        case 6:
                            {
                                Console.Clear();
                                "The current permission level is, '{0}'. Choose a new one from below.".FormatS(group.PermissionLevel).WriteLine();
                                "".WriteLine();

                                var names = Enum.GetNames(typeof(PermissionLevels));
                                for (int x = 0; x < names.Length; x++)
                                    "{0}. {1}".FormatS(x, names[x]).WriteLine();

                                int levelOption = -1;
                                if (int.TryParse(Console.ReadLine(), out levelOption) && levelOption >= 0 && levelOption < names.Length)
                                {
                                    group.PermissionLevel = (PermissionLevels)Enum.Parse(typeof(PermissionLevels), names[levelOption]);
                                }

                                break;
                            }
                        case 7:
                            {
                                Console.Clear();
                                "The current permission track is, '{0}'. Choose a new one from below.".FormatS(group.PermissionTrack).WriteLine();
                                "".WriteLine();

                                var names = Enum.GetNames(typeof(PermissionTracks));
                                for (int x = 0; x < names.Length; x++)
                                    "{0}. {1}".FormatS(x, names[x]).WriteLine();

                                int levelOption = -1;
                                if (int.TryParse(Console.ReadLine(), out levelOption) && levelOption >= 0 && levelOption < names.Length)
                                {
                                    group.PermissionTrack = (PermissionTracks)Enum.Parse(typeof(PermissionTracks), names[levelOption]);
                                }

                                break;
                            }
                        case 8:
                            {
                                keepLooping = false;

                                break;
                            }
                        default:
                            {
                                throw new NotImplementedException("In the permission group editor switch.");
                            }

                    }
                }
            }
        }

        private static void EditModelPermissions()
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
                        "Welcome to the model permissions editor.".WriteLine();
                        "Enter the number of a model permission to edit, the number followed by '-' to delete it, a new model permission name to create a new one, or a blank line to cancel.".WriteLine();
                        "".WriteLine();

                        //Now let's get all the model permissions
                        IList<ModelPermission> modelPermissions = session.CreateCriteria<ModelPermission>().List<ModelPermission>();

                        //And then print them out.
                        List<string[]> lines = new List<string[]> { new[] { "#", "Name", "Model Name", "# Return Fields", "# Edit Fields", "# Search Fields" } };
                        for (int x = 0; x < modelPermissions.Count; x++)
                        {
                            //We need to go get the total return fields for this model name.
                            int totalProperties = NHibernateHelper.GetEntityMetadata(modelPermissions[x].ModelName).PropertyNames.Length;

                            lines.Add(new[] { x.ToString(), modelPermissions[x].Name, modelPermissions[x].ModelName,
                                "{0}/{1}".FormatS(modelPermissions[x].ReturnableFields.Count, totalProperties),
                                "{0}/{1}".FormatS(modelPermissions[x].EditableFields.Count, totalProperties),
                                "{0}/{1}".FormatS(modelPermissions[x].SearchableFields.Count, totalProperties)});
                        }
                        DisplayUtilities.PadElementsInLines(lines, 3).WriteLine();

                        int option;
                        string input = Console.ReadLine();

                        if (string.IsNullOrWhiteSpace(input))
                            keepLooping = false;
                        else if (input.Last() == '-' && input.Length > 1 && int.TryParse(input.Substring(0, input.Length - 1), out option) && option >= 0 && option <= modelPermissions.Count - 1 && modelPermissions.Any())
                        {
                            session.Delete(modelPermissions[option]);
                        }
                        else if (int.TryParse(input, out option) && option >= 0 && option <= modelPermissions.Count - 1 && modelPermissions.Any())
                        {
                            //Client wants to edit an item.
                            EditModelPermission(modelPermissions[option]);
                        }
                        else
                        {
                            var item = new ModelPermission { Name = input };

                            "New name will be '{0}'".FormatS(item.Name).WriteLine();
                            "".WriteLine();
                            "Now choose a model from the list below...".WriteLine();
                            "".WriteLine();

                            //Gets a list of key/value pair where the key is the entity name and the value is the IClassMetadata.
                            var allEntitityMetadata = NHibernateHelper.GetAllEntityMetadata().ToList();

                            for (int x = 0; x < allEntitityMetadata.Count; x++)
                                "{0}. {1}".FormatS(x, allEntitityMetadata[x].Key).WriteLine();

                            int modelNameOption;
                            if (int.TryParse(Console.ReadLine(), out modelNameOption) && option >= 0 && option < allEntitityMetadata.Count)
                            {
                                item.ModelName = allEntitityMetadata[modelNameOption].Key;
                            }

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

        private static void EditModelPermission(ModelPermission modelPermission)
        {
            bool keepLooping = true;

            while (keepLooping)
            {

                Console.Clear();

                "Editing model permission '{0}'...".FormatS(modelPermission.Name).WriteLine();
                "".WriteLine();
                "Model Name:\n\t{0}".FormatS(modelPermission.ModelName).WriteLine();
                "".WriteLine();

                "1. Edit Name".WriteLine();
                "2. Edit Model Name".WriteLine();
                "3. View/Edit Return Fields".WriteLine();
                "4. View/Edit Edit Fields".WriteLine();
                "5. View/Edit Search Fields".WriteLine();
                "6. Return".WriteLine();

                int option;
                if (int.TryParse(Console.ReadLine(), out option) && option >= 1 && option <= 6)
                {
                    switch (option)
                    {
                        case 1:
                            {
                                Console.Clear();

                                "Enter a new model permission name...".WriteLine();
                                modelPermission.Name = Console.ReadLine();
                                break;
                            }
                        case 2:
                            {
                                Console.Clear();

                                "Choose a model from the list below...".WriteLine();
                                "".WriteLine();

                                //Gets a list of key/value pair where the key is the entity name and the value is the IClassMetadata.
                                var allEntitityMetadata = NHibernateHelper.GetAllEntityMetadata().ToList();

                                for (int x = 0; x < allEntitityMetadata.Count; x++)
                                    "{0}. {1}".FormatS(x, allEntitityMetadata[x].Key).WriteLine();

                                int modelNameOption;
                                if (int.TryParse(Console.ReadLine(), out modelNameOption) && option >= 0 && option < allEntitityMetadata.Count)
                                {
                                    modelPermission.ModelName = allEntitityMetadata[modelNameOption].Key;

                                    //Because the model name has been changed, we're going to reset the fields.  If the user just set it back to what it was before then fuck them.
                                    modelPermission.ReturnableFields.Clear();
                                    modelPermission.EditableFields.Clear();
                                    modelPermission.SearchableFields.Clear();
                                }

                                break;
                            }
                        case 3:
                            {
                                Console.Clear();
                                if (!string.IsNullOrWhiteSpace(modelPermission.ModelName))
                                {
                                    var allFields = NHibernateHelper.GetEntityMetadata(modelPermission.ModelName).PropertyNames.ToList();

                                    ListEditor.EditList(modelPermission.ReturnableFields, allFields, "Returnable Fields Editor");
                                }
                                else
                                {
                                    "You must first set the model name!".WriteLine();
                                }

                                break;
                            }
                        case 4:
                            {
                                Console.Clear();
                                if (!string.IsNullOrWhiteSpace(modelPermission.ModelName))
                                {
                                    var allFields = NHibernateHelper.GetEntityMetadata(modelPermission.ModelName).PropertyNames.ToList();

                                    ListEditor.EditList(modelPermission.EditableFields, allFields, "Editable Fields Editor");
                                }
                                else
                                {
                                    "You must first set the model name!".WriteLine();
                                }

                                break;
                            }
                        case 5:
                            {
                                Console.Clear();
                                if (!string.IsNullOrWhiteSpace(modelPermission.ModelName))
                                {
                                    var allFields = NHibernateHelper.GetEntityMetadata(modelPermission.ModelName).PropertyNames.ToList();

                                    ListEditor.EditList(modelPermission.SearchableFields, allFields, "Searchable Fields Editor");
                                }
                                else
                                {
                                    "You must first set the model name!".WriteLine();
                                }

                                break;
                            }
                        case 6:
                            {
                                keepLooping = false;

                                break;
                            }
                        default:
                            {
                                throw new NotImplementedException("In the model permission editor switch.");
                            }

                    }
                }
            }
        }

        private static void EditUserPermissions()
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
                        "Welcome to the user permissions editor.".WriteLine();
                        "Enter the last name of the person for whom you want to edit permissions or enter a blank line to return.".WriteLine();
                        "".WriteLine();

                        string input = Console.ReadLine();
                        if (string.IsNullOrWhiteSpace(input))
                            keepLooping = false;
                        {
                            Console.Clear();
                            var persons = session.QueryOver<CommandCentral.Entities.Person>()
                                .WhereRestrictionOn(x => x.LastName)
                                .IsInsensitiveLike(input, NHibernate.Criterion.MatchMode.Anywhere)
                                .List();

                            if (persons.Any())
                            {
                                Console.Clear();
                                "Choose the number of the person whose permissions you want to edit or enter a blank line to cancel.".WriteLine();
                                "".WriteLine();

                                //And then print them out.
                                List<string[]> lines = new List<string[]> { new[] { "#", "Rate", "First Name", "Last Name" } };
                                for (int x = 0; x < persons.Count; x++)
                                {
                                    lines.Add(new[] { x.ToString(), persons[x].Designation.Value, persons[x].FirstName, persons[x].LastName });
                                }
                                DisplayUtilities.PadElementsInLines(lines, 3).WriteLine();

                                int option;
                                string selectInput = Console.ReadLine();
                                if (!string.IsNullOrWhiteSpace(selectInput) && int.TryParse(selectInput, out option) && option >= 0 && option < persons.Count)
                                {
                                    var allPermissionGroups = session.QueryOver<PermissionGroup>().List().ToList();

                                    ListEditor.EditList(persons[option].PermissionGroups, allPermissionGroups, "User Permissions Editor");
                                }
                            }
                            else
                            {
                                "Your search returned no users.".WriteLine();
                                "Press any key to try again...".WriteLine();
                                Console.ReadKey();
                            }
                        }
                    }

                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
    }
}
