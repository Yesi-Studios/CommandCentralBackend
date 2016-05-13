using FluentNHibernate.Mapping;

namespace CommandCentral.Entities.ReferenceLists
{
    /// <summary>
    /// Describes a single ChangeEventLevel.
    /// </summary>
    public class ChangeEventLevel : ReferenceListItemBase
    {
        /// <summary>
        /// Maps ChangeEventLevel to the database.
        /// </summary>
        public class ChangeEventLevelMapping : ClassMap<ChangeEventLevel>
        {
            /// <summary>
            /// Maps ChangeEventLevel to the database.
            /// </summary>
            public ChangeEventLevelMapping()
            {
                Id(x => x.Id).GeneratedBy.Guid();

                Map(x => x.Value).Not.Nullable().Unique().Length(10);
                Map(x => x.Description).Nullable().Length(40);

                Cache.ReadWrite();
            }
        }
    }
}
