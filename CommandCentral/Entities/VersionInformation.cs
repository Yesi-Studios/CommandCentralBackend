using System;
using System.Collections.Generic;
using CommandCentral.ClientAccess;
using FluentNHibernate.Mapping;

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

        /// <summary>
        /// Exposed endpoints
        /// </summary>
        public static Dictionary<string, EndpointDescription> EndpointDescriptions
        {
            get { throw new NotImplementedException(); }
        }
    }
}
