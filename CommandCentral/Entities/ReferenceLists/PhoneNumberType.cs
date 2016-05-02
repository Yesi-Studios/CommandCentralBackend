using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;

namespace CommandCentral.Entities.ReferenceLists
{
    /// <summary>
    /// Describes a single phone number type.
    /// </summary>
    public class PhoneNumberType : ReferenceListItemBase
    {

        /// <summary>
        /// Maps a single phone number type to the database.
        /// </summary>
        public class PhoneNumberTypeMapping : ClassMap<PhoneNumberType>
        {
            /// <summary>
            /// Maps a single phone number type to the database.
            /// </summary>
            public PhoneNumberTypeMapping()
            {
                Table("phone_number_types");

                Id(x => x.ID);

                Map(x => x.Value).Length(50).Not.Nullable().Unique();
                Map(x => x.Description).Length(50).Nullable();

                Cache.ReadOnly();
            }
        }


    }
}
