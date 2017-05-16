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
    /// A watch qualification, such as JOOD, OOD, etc.
    /// </summary>
    public class WatchQualification : ReferenceListItemBase
    {
        /// <summary>
        /// Maps this object to the database.
        /// </summary>
        public class WatchQualificationMapping : ClassMap<WatchQualification>
        {
            /// <summary>
            /// Maps this object to the database.
            /// </summary>
            public WatchQualificationMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Value).Not.Nullable().Unique();
                Map(x => x.Description);

                Cache.ReadWrite();
            }
        }
    }
}
