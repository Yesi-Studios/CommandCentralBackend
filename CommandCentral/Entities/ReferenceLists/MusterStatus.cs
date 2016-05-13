using FluentNHibernate.Mapping;

namespace CommandCentral.Entities.ReferenceLists
{
    /// <summary>
    /// Describes a single MusterStatus.
    /// </summary>
    public class MusterStatus : ReferenceListItemBase
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
                Id(x => x.Id).GeneratedBy.Guid();

                Map(x => x.Value).Not.Nullable().Unique().Length(10);
                Map(x => x.Description).Nullable().Length(40);

                Cache.ReadWrite();
            }
        }
    }
}
