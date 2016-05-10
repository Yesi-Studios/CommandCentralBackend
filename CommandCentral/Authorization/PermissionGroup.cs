using System;
using System.Collections.Generic;
using CommandCentral.Authorization.ReferenceLists;
using FluentNHibernate.Mapping;
using System.Linq;
using CommandCentral.ClientAccess;

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
        public virtual IList<SpecialPermission> SpecialPermissions { get; set; }

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
        public new virtual string ToString()
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
            SpecialPermissions = new List<SpecialPermission>();
            SubordinatePermissionGroups = new List<PermissionGroup>();
        }

        #endregion

        #region Client Access

        /// <summary>
        /// The endpoints
        /// </summary>
        public static List<EndpointDescription> EndpointDescriptions
        {
            get
            {
                return new List<EndpointDescription>
                {
                    new EndpointDescription
                    {
                        Name = "GetModelPermissions",
                        AllowArgumentLogging = true,
                        AllowResponseLogging = true,
                        AuthorizationNote = "None",
                        DataMethod = GetModelPermissions_Client,
                        Description = "Convenience method that returns the client's permissions to all models in a dictionary where the key is the model's name and the value is a flattened model permission object representing all permissions to the model.",
                        ExampleOutput = () => "TODO",
                        IsActive = true,
                        OptionalParameters = null,
                        RequiredParameters = new List<string>
                        {
                            "apikey - The unique GUID token assigned to your application for metrics purposes.",
                            "authenticationtoken - The GUID authentication token for the user that was retrieved after successful login."
                        },
                        RequiredSpecialPermissions = null,
                        RequiresAuthentication = true
                    },
                    new EndpointDescription
                    {
                        Name = "LoadPermissionGroups",
                        AllowArgumentLogging = true,
                        AllowResponseLogging = true,
                        AuthorizationNote = "None",
                        DataMethod = LoadPermissionGroups_Client,
                        Description = "No Authorization is required for this method.  Returns all permission groups.",
                        ExampleOutput = () => "TODO",
                        IsActive = true,
                        OptionalParameters = null,
                        RequiredParameters = new List<string>
                        {
                            "apikey - The unique GUID token assigned to your application for metrics purposes."
                        },
                        RequiredSpecialPermissions = null,
                        RequiresAuthentication = false
                    }
                };
            }
        }

        /// <summary>
        /// WARNING!  THIS IS A CLIENT METHOD.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Returns the client's permissions to all models in a dictionary where the key is the model's name and the value is a flattened model permission object representing all permissions to the model.
        /// <para />
        /// Options: 
        /// <para />
        /// None
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private static void GetModelPermissions_Client(MessageToken token)
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
        /// WARNING!  THIS IS A CLIENT METHOD.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// No Authorization is required for this method.  Returns all permission groups.
        /// <para />
        /// Options: 
        /// <para />
        /// None.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private static void LoadPermissionGroups_Client(MessageToken token)
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
                Table("permission_groups");

                Id(x => x.Id).GeneratedBy.Guid();

                Map(x => x.Name).Not.Nullable().Unique().Length(20);
                Map(x => x.Description).Nullable().Length(50);

                HasManyToMany(x => x.ModelPermissions);
                HasManyToMany(x => x.SpecialPermissions);
                HasManyToMany(x => x.SubordinatePermissionGroups)
                    .ParentKeyColumn("PermissionGroupID")
                    .ChildKeyColumn("SubordinatePermissionGroupID");

            }
        }

    }
}
