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
    /// Defines a single watch bill status, which is used to indicate at which position in the process a watchbill is at.  
    /// </summary>
    public class WatchbillStatus : ReferenceListItemBase
    {
        /// <summary>
        /// Maps this object to the database.
        /// </summary>
        public class WatchbillStatusMapping : ClassMap<WatchbillStatus>
        {
            /// <summary>
            /// Maps this object to the database.
            /// </summary>
            public WatchbillStatusMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Value).Not.Nullable().Unique();
                Map(x => x.Description);

                Cache.ReadWrite();
            }
        }

    }
}
