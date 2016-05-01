using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;

namespace CommandCentral.Entities.ReferenceLists
{
    /// <summary>
    /// Describes a single DutyStatus.
    /// </summary>
    public class DutyStatus : ReferenceListItemBase<DutyStatus>
    {
        /// <summary>
        /// Maps a DutyStatus to the database.
        /// </summary>
        public class DutyStatusMapping : ClassMap<DutyStatus>
        {
            /// <summary>
            /// Maps a DutyStatus to the database.
            /// </summary>
            public DutyStatusMapping()
            {
                Table("duty_statuses");

                Id(x => x.ID).GeneratedBy.Guid();

                Map(x => x.Value).Not.Nullable().Unique().Length(10);
                Map(x => x.Description).Nullable().Length(40);

                Cache.ReadOnly();
            }
        }
    }
}
