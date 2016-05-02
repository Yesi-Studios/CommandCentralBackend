using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                Table("change_event_levels");

                Id(x => x.ID).GeneratedBy.Guid();

                Map(x => x.Value).Not.Nullable().Unique().Length(10);
                Map(x => x.Description).Nullable().Length(40);

                Cache.ReadOnly();
            }
        }
    }
}
