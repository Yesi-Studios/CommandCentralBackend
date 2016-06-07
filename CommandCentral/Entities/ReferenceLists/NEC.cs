using FluentNHibernate.Mapping;

namespace CommandCentral.Entities.ReferenceLists
{
    /// <summary>
    /// Describes a single NEC.
    /// </summary>
    public class NEC : ReferenceListItemBase
    {
        /// <summary>
        /// Maps an NEC to the database.
        /// </summary>
        public class NECMapping : ClassMap<NEC>
        {
            /// <summary>
            /// Maps an NEC to the database.
            /// </summary>
            public NECMapping()
            {
                Id(x => x.Id).GeneratedBy.Guid();

                Map(x => x.Value).Not.Nullable().Unique().Length(10);
                Map(x => x.Description).Nullable().Length(40);

                Cache.ReadWrite();
            }
        }
    }
}
