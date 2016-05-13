using FluentNHibernate.Mapping;

namespace CommandCentral.Entities.ReferenceLists
{
    /// <summary>
    /// Describes a single rank.
    /// </summary>
    public class Rank : ReferenceListItemBase
    {
        /// <summary>
        /// Maps a rank to the database.
        /// </summary>
        public class RankMapping : ClassMap<Rank>
        {
            /// <summary>
            /// Maps a rank to the database.
            /// </summary>
            public RankMapping()
            {
                Id(x => x.Id).GeneratedBy.Guid();

                Map(x => x.Value).Not.Nullable().Unique().Length(10);
                Map(x => x.Description).Nullable().Length(40);

                Cache.ReadWrite();
            }
        }

    }
}
