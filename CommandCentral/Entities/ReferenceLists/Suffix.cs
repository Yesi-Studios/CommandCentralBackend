using FluentNHibernate.Mapping;

namespace CommandCentral.Entities.ReferenceLists
{
    /// <summary>
    /// Describes a single suffix
    /// </summary>
    public class Suffix : ReferenceListItemBase
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

                Id(x => x.Id);

                Map(x => x.Value).Not.Nullable().Unique().Length(10);
                Map(x => x.Description).Nullable().Length(40);

                Cache.ReadWrite();
            }
        }
    }
}
