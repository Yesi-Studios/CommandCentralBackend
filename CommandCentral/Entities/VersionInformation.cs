using System;
using System.Collections.Generic;
using CommandCentral.ClientAccess;
using FluentNHibernate.Mapping;
using System.Linq;

namespace CommandCentral.Entities
{
    /// <summary>
    /// Describes the main data of the application, which represents dynamically loaded content.
    /// </summary>
    public class VersionInformation
    {

        #region Properties

        /// <summary>
        /// The Id of this main data object.
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The current version of the application.
        /// </summary>
        public virtual string Version { get; set; }

        /// <summary>
        /// The time this main data was made.  
        /// </summary>
        public virtual DateTime Time { get; set; }

        #endregion

        /// <summary>
        /// Maps a version information to the database.
        /// </summary>
        public class VersionInformationMapping : ClassMap<VersionInformation>
        {
            /// <summary>
            /// Maps a version information to the database.
            /// </summary>
            public VersionInformationMapping()
            {
                Table("version_informations");

                Id(x => x.Id).GeneratedBy.Guid();

                Map(x => x.Version).Not.Nullable().Unique().Length(10);
                Map(x => x.Time).Not.Nullable();
                
            }
        }

        #region Client Access

        /// <summary>
        /// WARNING!  THIS IS A CLIENT METHOD.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// <para />
        /// Returns all version information objects.
        /// <para />
        /// Options: 
        /// <para />
        /// None
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private static MessageToken LoadVersions_Client(MessageToken token)
        {
            //Very easily we're just going to throw back all the versions.  Easy day.  We're going to order the versions by time.
            token.Result = token.CommunicationSession.QueryOver<VersionInformation>().List<VersionInformation>().OrderByDescending(x => x.Time).ToList();

            return token;
        }

        /// <summary>
        /// The exposed endpoints
        /// </summary>
        public static Dictionary<string, EndpointDescription> EndpointDescriptions
        {
            get
            {
                return new Dictionary<string, EndpointDescription>
                {
                    { "LoadVersions", new EndpointDescription
                        {
                            AllowArgumentLogging = true,
                            AllowResponseLogging = true,
                            AuthorizationNote = "None",
                            DataMethod = LoadVersions_Client,
                            Description = "Returns all version information objects.",
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
                    }
                };
            }
        }


        #endregion

    }
}
