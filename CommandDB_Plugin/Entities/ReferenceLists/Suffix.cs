using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;


namespace CommandCentral.Entities.ReferenceLists
{
    /// <summary>
    /// Describes a single suffix
    /// </summary>
    public class Suffix : ReferenceListItem<Suffix>
    {
        /// <summary>
        /// Maps a suffix to the database.
        /// </summary>
        public class SufffixMapping : ClassMap<Suffix>
        {
            /// <summary>
            /// Maps a suffix to the database.
            /// </summary>
            public SufffixMapping()
            {
                Table("suffixes");

                Id(x => x.ID);

                Map(x => x.Value).Not.Nullable().Unique().Length(10);
                Map(x => x.Description).Nullable().Length(40);

                Cache.ReadOnly();
            }
        }
    }
}
