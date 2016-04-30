﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;

namespace CommandCentral.Entities.ReferenceLists
{
    /// <summary>
    /// Describes a single MusterStatus.
    /// </summary>
    public class MusterStatus : ReferenceListItem<MusterStatus>
    {
        /// <summary>
        /// Maps a MusterStatus to the database.
        /// </summary>
        public class MusterStatusMapping : ClassMap<MusterStatus>
        {
            /// <summary>
            /// Maps a MusterStatus to the database.
            /// </summary>
            public MusterStatusMapping()
            {
                Table("muster_statuses");

                Id(x => x.ID).GeneratedBy.Guid();

                Map(x => x.Value).Not.Nullable().Unique().Length(10);
                Map(x => x.Description).Nullable().Length(40);

                Cache.ReadOnly();
            }
        }
    }
}
