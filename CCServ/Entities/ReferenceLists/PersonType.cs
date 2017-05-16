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
    /// Enumerates the different types of a person.
    /// </summary>
    public class PersonType : ReferenceListItemBase
    {
        /// <summary>
        /// Maps this object to the database.
        /// </summary>
        public class PersonTypeMapping : ClassMap<PersonType>
        {
            /// <summary>
            /// Maps this object to the database.
            /// </summary>
            public PersonTypeMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Value).Not.Nullable().Unique();
                Map(x => x.Description);

                Cache.ReadWrite();
            }
        }
    }
}
