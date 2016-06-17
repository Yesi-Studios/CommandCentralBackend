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
        /// The permission track in which this permission resides.
        /// </summary>
        public virtual PermissionTrack PermissionTrack { get; set; }

        /// <summary>
        /// The list of sub-permissions that describe what rights this permission group grants to what model.
        /// </summary>
        public virtual IList<ModelPermission> ModelPermissions { get; set; }

        /// <summary>
        /// Additional list of permissions.  Intended to describe access to other parts of the application as defined by the consumer.
        /// </summary>
        public virtual IList<SpecialPermissions> SpecialPermissions { get; set; }

        /// <summary>
        /// The level of this permission.  Though any integer is acceptable, let's try to keep it between 10 and 1.  Permission groups that share the same permission level
        /// </summary>
        public virtual int PermissionLevel { get; set; }

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

        #endregion Client Access

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
                Map(x => x.PermissionLevel).Not.Nullable();

                References(x => x.PermissionTrack);

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
