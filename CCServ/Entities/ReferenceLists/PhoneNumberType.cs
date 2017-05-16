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
    /// Represents the different types of a phone number such as cell or home.
    /// </summary>
    public class PhoneNumberType : ReferenceListItemBase
    {
        /// <summary>
        /// Maps this object to the database.
        /// </summary>
        public class PhoneNumberTypeMapping : ClassMap<PhoneNumberType>
        {
            /// <summary>
            /// Maps this object to the database.
            /// </summary>
            public PhoneNumberTypeMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Value).Not.Nullable().Unique();
                Map(x => x.Description);

                Cache.ReadWrite();
            }
        }
    }
}
