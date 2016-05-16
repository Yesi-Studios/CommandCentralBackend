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
                Id(x => x.Id).GeneratedBy.Guid();

                Map(x => x.Version).Not.Nullable().Unique().Length(10);
                Map(x => x.Time).Not.Nullable();
                
            }
        }

        #region Client Access

        /// <summary>
        /// WARNING!  THIS METHOD IS EXPOSED TO THE CLIENT AND IS NOT INTENDED FOR INTERNAL USE.  AUTHENTICATION, AUTHORIZATION AND VALIDATION MUST BE HANDLED PRIOR TO DB INTERACTION.
        /// </summary>
        /// Returns all version information items, sorted by Time - descending.
        /// <param name="token"></param>
        /// <returns></returns>
        [EndpointMethod(EndpointName = "LoadVersions", AllowArgumentLogging = true, AllowResponseLogging = true, RequiresAuthentication = false)]
        private static void EndpointMethod_LoadVersions(MessageToken token)
        {
            //Very easily we're just going to throw back all the versions.  Easy day.  We're going to order the versions by time.
            token.SetResult(token.CommunicationSession.QueryOver<VersionInformation>().List<VersionInformation>().OrderByDescending(x => x.Time).ToList());
        }

        #endregion

    }
}
