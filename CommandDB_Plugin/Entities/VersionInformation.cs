﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Collections.Concurrent;
using System.Reflection;
using MySql.Data.MySqlClient;
using MySql.Data.Common;
using AtwoodUtils;
using FluentNHibernate.Mapping;
using CommandCentral.DataAccess;
using CommandCentral.ClientAccess;

namespace CommandCentral.Entities
{
    /// <summary>
    /// Describes the main data of the application, which reprents dynamically loaded content.
    /// </summary>
    public class VersionInformation : CachedModel<VersionInformation>, IExposable
    {

        #region Properties

        /// <summary>
        /// The ID of this main data object.
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// The current version of the application.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// The time this main data was made.  
        /// </summary>
        public DateTime Time { get; set; }

        #endregion

        /// <summary>
        /// Maps a version information to the database.
        /// </summary>
        public class VersionInformationMapping : ClassMap<VersionInformation>
        {
            /// <summary>
            /// Maps a version infroamtion to the database.
            /// </summary>
            public VersionInformationMapping()
            {
                Table("versioninformations");

                Id(x => x.ID).GeneratedBy.Guid();

                Map(x => x.Version).Not.Nullable().Unique().Length(10);
                Map(x => x.Time).Not.Nullable();
                
            }
        }

        /// <summary>
        /// Exposed endpoints
        /// </summary>
        Dictionary<string, EndpointDescription> IExposable.EndpointDescriptions
        {
            get { throw new NotImplementedException(); }
        }
    }
}
