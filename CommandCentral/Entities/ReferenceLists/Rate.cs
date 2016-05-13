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
                Id(x => x.Id).GeneratedBy.Guid();

                Map(x => x.Value).Not.Nullable().Unique().Length(10);
                Map(x => x.Description).Nullable().Length(40);

                Cache.ReadWrite();
            }
        }

    }
}
