using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;

namespace CommandCentral.Entities.ReferenceLists
{
    /// <summary>
    /// Describes a single Rate.
    /// </summary>
    public class Rate : ReferenceListItemBase
    {
        /// <summary>
        /// Maps a Rate to the database.
        /// </summary>
        public class RateMapping : ClassMap<Rate>
        {
            /// <summary>
            /// Maps a Rate to the database.
            /// </summary>
            public RateMapping()
            {
                Table("rates");

                Id(x => x.ID).GeneratedBy.Guid();

                Map(x => x.Value).Not.Nullable().Unique().Length(10);
                Map(x => x.Description).Nullable().Length(40);

                Cache.ReadOnly();
            }
        }

    }
}
