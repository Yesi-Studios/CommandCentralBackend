using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;

namespace CommandCentral.Authorization.ReferenceLists
{
    /// <summary>
    /// Describes a single SpecialPermission
    /// </summary>
    public class SpecialPermission : ReferenceListItemBase<SpecialPermission>
    {
        /// <summary>
        /// Maps a SpecialPermission to the database.
        /// </summary>
        public class SpecialPermissionMapping : ClassMap<SpecialPermission>
        {
            /// <summary>
            /// Maps a SpecialPermission to the database.
            /// </summary>
            public SpecialPermissionMapping()
            {
                Table("special_permissions");

                Id(x => x.ID).GeneratedBy.Guid();

                Map(x => x.Value).Not.Nullable().Unique().Length(15);
                Map(x => x.Description).Nullable().Length(40);

                Cache.ReadOnly();
            }
        }
    }
}
