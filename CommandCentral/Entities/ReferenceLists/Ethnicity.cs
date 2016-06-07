using FluentNHibernate.Mapping;

namespace CommandCentral.Entities.ReferenceLists
{
    /// <summary>
    /// Describes a single ethnicity
    /// </summary>
    public class Ethnicity : ReferenceListItemBase
    {
        /// <summary>
        /// Maps an ethnicity to the database.
        /// </summary>
        public class EthnicityMapping : ClassMap<Ethnicity>
        {
            /// <summary>
            /// Maps an ethnicity to the database.
            /// </summary>
            public EthnicityMapping()
            {
                Id(x => x.Id).GeneratedBy.Guid();

                Map(x => x.Value).Not.Nullable().Unique().Length(15);
                Map(x => x.Description).Nullable().Length(40);

                Cache.ReadWrite();
            }
        }
    }
}
