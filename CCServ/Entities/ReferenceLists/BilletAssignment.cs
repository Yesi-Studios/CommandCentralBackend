using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CCServ.ClientAccess;
using FluentNHibernate.Mapping;

namespace CCServ.Entities.ReferenceLists
{
    /// <summary>
    /// Describes a billet assignment: P2 or P3 which is how the Navy knows who undersigns a person's billet payment.
    /// </summary>
    public class BilletAssignment : ReferenceListItemBase
    {
        /// <summary>
        /// Maps this object to the database.
        /// </summary>
        public class BilletAssignmentMapping : ClassMap<BilletAssignment>
        {
            /// <summary>
            /// Maps this object to the database.
            /// </summary>
            public BilletAssignmentMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Value).Not.Nullable().Unique();
                Map(x => x.Description);

                Cache.ReadWrite();
            }
        }
    }
}
