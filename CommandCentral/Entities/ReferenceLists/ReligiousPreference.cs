using FluentNHibernate.Mapping;

namespace CommandCentral.Entities.ReferenceLists
{
    /// <summary>
    /// Describes a single Religious Preference
    /// </summary>
    public class ReligiousPreference : ReferenceListItemBase
    {
        /// <summary>
        /// Maps a Religious Preference to the database.
        /// </summary>
        public class ReligiousPreferenceMapping : ClassMap<ReligiousPreference>
        {
            /// <summary>
            /// Maps a Religious Preference to the database.
            /// </summary>
            public ReligiousPreferenceMapping()
            {
                Id(x => x.Id).GeneratedBy.Guid();

                Map(x => x.Value).Not.Nullable().Unique().Length(15);
                Map(x => x.Description).Nullable().Length(40);

                Cache.ReadWrite();
            }
        }
    }
}
