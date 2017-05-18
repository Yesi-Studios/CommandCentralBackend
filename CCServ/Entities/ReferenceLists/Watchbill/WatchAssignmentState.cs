using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CCServ.ClientAccess;
using FluentNHibernate.Mapping;

namespace CCServ.Entities.ReferenceLists.Watchbill
{
    /// <summary>
    /// Defines a watch assignment state, which indicates if a watch was stood, acknowledged, and other things.
    /// </summary>
    public class WatchAssignmentState : ReferenceListItemBase
    {
        /// <summary>
        /// Maps this object to the database.
        /// </summary>
        public class WatchAssignmentStateMapping : ClassMap<WatchAssignmentState>
        {
            /// <summary>
            /// Maps this object to the database.
            /// </summary>
            public WatchAssignmentStateMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Value).Not.Nullable().Unique();
                Map(x => x.Description);

                Cache.ReadWrite();
            }
        }
    }
}
