using FluentNHibernate.Mapping;

namespace CommandCentral.Entities.ReferenceLists
{
    /// <summary>
    /// Describes a single designation.  This is the job title for civilians, the rate for enlisted and the designator for officers.
    /// </summary>
    public class Designation : ReferenceListItemBase
    {
        /// <summary>
        /// Maps a Designation to the database.
        /// </summary>
        public class DesignationMapping : ClassMap<Designation>
        {
            /// <summary>
            /// Maps a Designation to the database.
            /// </summary>
            public DesignationMapping()
            {
                Id(x => x.Id).GeneratedBy.Guid();

                Map(x => x.Value).Not.Nullable().Unique().Length(10);
                Map(x => x.Description).Nullable().Length(40);

                Cache.ReadWrite();
            }
        }
    }
}
