using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using CommandCentral.DataAccess;
using CommandCentral.ClientAccess;

namespace CommandCentral.Entities.ReferenceLists
{
    /// <summary>
    /// Describes a single UIC.
    /// </summary>
    public class UIC : ReferenceListItemBase
    {

        /// <summary>
        /// Maps a UIC to the database.
        /// </summary>
        public class UICMapping : ClassMap<UIC>
        {
            /// <summary>
            /// Maps a UIC to the database.
            /// </summary>
            public UICMapping()
            {
                Table("uics");

                Id(x => x.ID).GeneratedBy.Guid();

                Map(x => x.Value).Not.Nullable().Length(10).Unique();
                Map(x => x.Description).Nullable().Length(40);

                Cache.ReadWrite();

            }
        }


    }
}
