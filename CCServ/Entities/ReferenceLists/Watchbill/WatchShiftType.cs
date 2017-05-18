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
    /// Defines a watch shift type, such as super, ood, jood, etc.
    /// </summary>
    public class WatchShiftType : ReferenceListItemBase
    {

        #region Properties

        /// <summary>
        /// The collection of watch qualifications a person must have in order to be assigned a watch with this watch type.
        /// </summary>
        public virtual IList<WatchQualification> RequiredWatchQualifications { get; set; }

        #endregion

        /// <summary>
        /// Maps this object to the database.
        /// </summary>
        public class WatchShiftTypeMapping : ClassMap<WatchShiftType>
        {
            /// <summary>
            /// Maps this object to the database.
            /// </summary>
            public WatchShiftTypeMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Value).Not.Nullable().Unique();
                Map(x => x.Description);

                HasMany(x => x.RequiredWatchQualifications);

                Cache.ReadWrite();
            }
        }

    }
}
