using FluentNHibernate.Mapping;

namespace CommandCentral.Entities.ReferenceLists
{
    /// <summary>
    /// Describes a single sex.
    /// </summary>
    public class Sex : ReferenceListItemBase
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

                Id(x => x.Id).GeneratedBy.Guid();

                Map(x => x.Value).Not.Nullable().Unique().Length(10);
                Map(x => x.Description).Nullable().Length(40);

                Cache.ReadWrite();
            }
        }
    }
}
