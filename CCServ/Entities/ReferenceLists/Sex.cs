using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CCServ.ClientAccess;
using FluentNHibernate.Mapping;
using FluentValidation.Results;

namespace CCServ.Entities.ReferenceLists
{
    /// <summary>
    /// Indicates the sex of a given person.
    /// </summary>
    public class Sex : ReferenceListItemBase
    {
        /// <summary>
        /// Maps secks to the database.
        /// </summary>
        public class SexMapping : ClassMap<Sex>
        {
            /// <summary>
            /// Maps secks to the database.
            /// </summary>
            public SexMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Value).Not.Nullable().Unique();
                Map(x => x.Description);

                Cache.ReadWrite();
            }
        }
    }
}
