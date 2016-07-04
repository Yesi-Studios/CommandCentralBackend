using System;
using System.Collections.Generic;
using FluentNHibernate.Mapping;
using System.Linq;
using CommandCentral.ClientAccess;
using AtwoodUtils;

namespace CommandCentral.Authorization
{
    /// <summary>
    /// Describes a single permission group.
    /// </summary>
    public class PermissionGroup
    {
        #region Properties

        /// <summary>
        /// The Id of this permission group.  This should not change after original creation.
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The name of this permission group.
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// A short description of this permission group.
        /// </summary>
        public virtual string Description { get; set; }

        /// <summary>
        /// The list of sub-permissions that describe what rights this permission group grants to what model.
        /// </summary>
        public virtual IList<ModelPermission> ModelPermissions { get; set; }

        /// <summary>
        /// Additional list of permissions.  Intended to describe access to other parts of the application as defined by the consumer.
        /// </summary>
        public virtual IList<SpecialPermissions> SpecialPermissions { get; set; }

        /// <summary>
        /// The level of this permission.  For example, a department level permission allows the user to exercise the permissions granted by this permission group when dealing with members in his/her same department.
        /// </summary>
        public virtual PermissionLevels PermissionLevel { get; set; }

        /// <summary>
        /// Indicates on which track this permission group lies.
        /// </summary>
        public virtual PermissionTracks PermissionTrack { get; set; }

        /// <summary>
        /// A list of those permissions groups that are subordinate to this permission group.  This is used to determine which groups can promote people into which groups.
        /// </summary>
        public virtual IList<PermissionGroup> SubordinatePermissionGroups { get; set; }

        #endregion

        #region Overrides

        /// <summary>
        /// Returns Name.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name;
        }

        #endregion

        #region ctors

        /// <summary>
        /// Creates a new permission group and sets the lists to empty lists.
        /// </summary>
        public PermissionGroup()
        {
            ModelPermissions = new List<ModelPermission>();
            SpecialPermissions = new List<SpecialPermissions>();
            SubordinatePermissionGroups = new List<PermissionGroup>();
        }

        #endregion

        #region Client Access

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// </summary>
        /// Gets all model permissions for the client and sorts those model permissions as a dictionary grouped by the model name.
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(EndpointName = "GetModelPermissions", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void EndpointMethod_GetModelPermissions(MessageToken token)
        {
            //Make sure we're logged in.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to get your permissions to an object.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            //Now we're going to get all the client's model permissions, remove duplicates and then make it a dictionary where the key is the model name.
            var result = token.AuthenticationSession.Person.PermissionGroups
                            .SelectMany(x => x.ModelPermissions)
                            .GroupBy(x => x.ModelName)
                            .Select(x =>
                            {
                                ModelPermission flattenedModelPermission = new ModelPermission { Name = "Flattened Permissions", ModelName = x.Key, Id = Guid.Empty };
                                flattenedModelPermission.ReturnableFields = x.SelectMany(y => y.ReturnableFields).Distinct().ToList();
                                flattenedModelPermission.EditableFields = x.SelectMany(y => y.EditableFields).Distinct().ToList();
                                flattenedModelPermission.SearchableFields = x.SelectMany(y => y.SearchableFields).Distinct().ToList();

                                //Ok so now we have the flattened model permission.  Let's put it into a keyvalue pair.
                                return new KeyValuePair<string, ModelPermission>(x.Key, flattenedModelPermission);
                            }).ToDictionary(x => x.Key, x => x.Value);

            //And here's the result!
            token.SetResult(result);
        }

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// </summary>
        /// Returns all permissions groups currently available to all users.
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(EndpointName = "LoadPermissionGroups", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = false)]
        private static void EndpointMethod_LoadPermissionGroups(MessageToken token)
        {
            token.SetResult(token.CommunicationSession.QueryOver<PermissionGroup>().List());
        }

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// </summary>
        /// Returns all permission groups, along with the given person's permission groups along with which of the all permission groups are editable.
        /// <para />
        /// Client Parameters: <para />
        ///     personid - The ID of the person to load permission for in relation to the client.
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(EndpointName = "LoadPermissionGroupsByPerson", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void EndpointMethod_LoadPermissionGroupsByPerson(MessageToken token)
        {
            //First make sure we have a session.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to view permissions.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            if (!token.Args.ContainsKey("personid"))
            {
                token.AddErrorMessage("You failed to send a 'personid' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            Guid personId;
            if (!Guid.TryParse(token.Args["personid"] as string, out personId))
            {
                token.AddErrorMessage("Your person Id was in a wrong format.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            //Get the person we were given and check to see if it's null.
            Entities.Person person = token.CommunicationSession.Get<Entities.Person>(personId);

            if (person == null)
            {
                token.AddErrorMessage("The person Id you provided belongs to no person.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadGateway);
                return;
            }

            var personPermissionGroups = person.PermissionGroups;
            var clientPermissionGroups = token.AuthenticationSession.Person.PermissionGroups;
            var allPermissionGroups = token.CommunicationSession.QueryOver<PermissionGroup>().List();

            var editablePermissionGroups = new List<PermissionGroup>();

            foreach (var group in allPermissionGroups)
            {
                if (group.PermissionLevel == PermissionLevels.Command && person.Command.Id == token.AuthenticationSession.Person.Command.Id &&
                    clientPermissionGroups.SelectMany(x => x.SubordinatePermissionGroups).Contains(group))
                    editablePermissionGroups.Add(group);
                else
                    if (group.PermissionLevel == PermissionLevels.Department && person.Command.Id == token.AuthenticationSession.Person.Command.Id &&
                        person.Department.Id == token.AuthenticationSession.Person.Department.Id && clientPermissionGroups.SelectMany(x => x.SubordinatePermissionGroups).Contains(group))
                        editablePermissionGroups.Add(group);
                    else 
                        if (group.PermissionLevel == PermissionLevels.Division && person.Command.Id == token.AuthenticationSession.Person.Command.Id &&
                            person.Department.Id == token.AuthenticationSession.Person.Department.Id && person.Division.Id == token.AuthenticationSession.Person.Division.Id &&
                            clientPermissionGroups.SelectMany(x => x.SubordinatePermissionGroups).Contains(group))
                            editablePermissionGroups.Add(group);
            }

            token.SetResult(new { CurrentPermissionGroups = personPermissionGroups, AllPermissionGroups = allPermissionGroups, EditablePermissionGroups = editablePermissionGroups,
                ForFunsies = false, SuperSerious = true, TrollMode = "Kappa", FriendlyName = person.ToString() });
        }

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// </summary>
        /// Given a person's Id and a list of desired permission groups, attempts to update the permission groups of the person.
        /// <para />
        /// Client Parameters: <para />
        ///     personid - The ID of the person to load permission for in relation to the client.
        ///     permissionslist - the list of permission groups.
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(EndpointName = "UpdatePermissionGroupsByPerson", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = true)]
        private static void EndpointMethod_UpdatePermissionGroupsByPerson(MessageToken token)
        {
            //First make sure we have a session.
            if (token.AuthenticationSession == null)
            {
                token.AddErrorMessage("You must be logged in to edit a person.", ErrorTypes.Authentication, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            //Get the person param
            if (!token.Args.ContainsKey("personid"))
            {
                token.AddErrorMessage("You failed to send a 'personid' parameter.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            //Validate that shit
            Guid personId;
            if (!Guid.TryParse(token.Args["personid"] as string, out personId))
            {
                token.AddErrorMessage("Your person Id was in a wrong format.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            //More validate that shit
            Entities.Person person = token.CommunicationSession.Get<Entities.Person>(personId);

            if (person == null)
            {
                token.AddErrorMessage("That person Id is not a real person's Id.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            //now the permissions list.
            if (!token.Args.ContainsKey("permissionslist"))
            {
                token.AddErrorMessage("You failed to send a ", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                return;
            }

            //Get that shit.
            var permissionsGroupsFromClient = new List<PermissionGroup>();

            //foreach that shit and make sure each one is some real shit and not some fake shit. 
            foreach (var id in token.Args["permissionslist"].CastJToken<List<string>>())
            {
                Guid permId;
                if (!Guid.TryParse(id, out permId))
                {
                    token.AddErrorMessage("One or more Ids were in an invalid format.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                    return;
                }

                var group = token.CommunicationSession.Get<PermissionGroup>(permId);

                if (group == null)
                {
                    token.AddErrorMessage("One or more Ids were not real Ids.", ErrorTypes.Validation, System.Net.HttpStatusCode.BadRequest);
                    return;
                }

                //if we got here, then it's a legit permission group's Id.
                permissionsGroupsFromClient.Add(group);
            }

            //Ok, so we have all legit things, now let's find out what the client added or removed.
            var changedGroups = new List<Guid>();

            var permissionListFromDB = person.PermissionGroups.Select(x => x.Id).ToList();

            foreach (var id in permissionsGroupsFromClient.Select(x => x.Id).ToList())
            {
                if (!permissionListFromDB.Contains(id))
                    changedGroups.Add(id);
            }

            foreach (var id in permissionListFromDB)
            {
                if (!permissionsGroupsFromClient.Exists(x => x.Id == id))
                    changedGroups.Add(id);
            }

            //Ok, now we know what changed, let's find out what the client was allowed to change. 
            //Instead of finding all the editable groups, we could find only those the client edited, but whatever, this is easy.
            var clientPermissionGroups = token.AuthenticationSession.Person.PermissionGroups;
            var allPermissionGroups = token.CommunicationSession.QueryOver<PermissionGroup>().List();

            var editablePermissionGroups = new List<PermissionGroup>();

            foreach (var group in allPermissionGroups)
            {
                if (group.PermissionLevel == PermissionLevels.Command && person.Command.Id == token.AuthenticationSession.Person.Command.Id &&
                    clientPermissionGroups.SelectMany(x => x.SubordinatePermissionGroups).Contains(group))
                    editablePermissionGroups.Add(group);
                else
                    if (group.PermissionLevel == PermissionLevels.Department && person.Command.Id == token.AuthenticationSession.Person.Command.Id &&
                        person.Department.Id == token.AuthenticationSession.Person.Department.Id && clientPermissionGroups.SelectMany(x => x.SubordinatePermissionGroups).Contains(group))
                    editablePermissionGroups.Add(group);
                else
                        if (group.PermissionLevel == PermissionLevels.Division && person.Command.Id == token.AuthenticationSession.Person.Command.Id &&
                            person.Department.Id == token.AuthenticationSession.Person.Department.Id && person.Division.Id == token.AuthenticationSession.Person.Division.Id &&
                            clientPermissionGroups.SelectMany(x => x.SubordinatePermissionGroups).Contains(group))
                    editablePermissionGroups.Add(group);
            }

            var unauthorizedEdits = changedGroups.Where(x => !editablePermissionGroups.Select(y => y.Id).ToList().Contains(x)).ToList();

            if (unauthorizedEdits.Any())
            {
                token.AddErrorMessage("You were not allowed to edit one or more of the permission groups' membership that you tried to edit.", ErrorTypes.Authorization, System.Net.HttpStatusCode.Unauthorized);
                return;
            }

            //ALright this means the client's permission groups list is ok.  Now jsut set it and update it.
            person.PermissionGroups = permissionsGroupsFromClient;

            token.CommunicationSession.Update(person);

            //If we get here, then success.
            token.SetResult(new { WasSelf = person.Id == token.AuthenticationSession.Person.Id });
        }

        #endregion Client Access

        #region Startup Methods

        /// <summary>
        /// Reads all permission groups from the database and posts a message to the host with which permission groups were loaded.
        /// </summary>
        [ServiceManagement.StartMethod(Priority = 6)]
        private static void ReadPermissionGroups()
        {
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                var groups = session.QueryOver<PermissionGroup>().List();

                Communicator.PostMessageToHost("Found {0} permission group(s): {1}".FormatS(groups.Count, String.Join(",", groups.Select(x => x.Name))), Communicator.MessageTypes.Informational);
            }
        }

        #endregion

        /// <summary>
        /// Maps a permission group to the database
        /// </summary>
        public class PermissionGroupMapping : ClassMap<PermissionGroup>
        {
            /// <summary>
            /// Maps a permission group to the database
            /// </summary>
            public PermissionGroupMapping()
            {
                Id(x => x.Id).GeneratedBy.Guid();

                Map(x => x.Name).Not.Nullable().Unique().Length(20);
                Map(x => x.Description).Nullable().Length(50);
                Map(x => x.PermissionLevel).Default("'{0}'".FormatS(PermissionLevels.None.ToString())).Not.Nullable(); //We have to tell it to put '' marks or else the SQL it makes is wrong.  :(
                Map(x => x.PermissionTrack).Default("'{0}'".FormatS(PermissionTracks.None.ToString())).Not.Nullable(); //Same as above

                HasMany(x => x.SpecialPermissions)
                    .KeyColumn("PermissionGroupId")
                    .Element("SpecialPermission");

                HasManyToMany(x => x.ModelPermissions).Fetch.Select();
                HasManyToMany(x => x.SubordinatePermissionGroups)
                    .ParentKeyColumn("PermissionGroupID")
                    .ChildKeyColumn("SubordinatePermissionGroupID");

            }
        }

    }
}