using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;

namespace CommandCentral.Entities.ReferenceLists
{
    /// <summary>
    /// Describes a single sex.
    /// </summary>
    public class Sex : ReferenceListItem<Sex>
    {
        /// <summary>
        /// Maps sex to the database.
        /// </summary>
        public class SexMapping : ClassMap<Sex>
        {
            /// <summary>
            /// Maps sex to the database.
            /// </summary>
            public SexMapping()
            {
                Table("sexes");

                Id(x => x.ID).GeneratedBy.Guid();

                Map(x => x.Value).Not.Nullable().Unique().Length(10);
                Map(x => x.Description).Nullable().Length(40);

                Cache.ReadOnly();
            }
        }
    }
}
