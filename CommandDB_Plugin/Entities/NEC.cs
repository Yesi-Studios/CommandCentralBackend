using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using CommandCentral.DataAccess;

namespace CommandCentral.Entities
{
    /// <summary>
    /// Describes a single NEC.
    /// </summary>
    public class NEC : CachedModel<NEC>
    {

        #region Properties

        /// <summary>
        /// The NEC's unique ID
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// The name of this NEC
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The description of the NEC
        /// </summary>
        public string Description { get; set; }

        #endregion

        /// <summary>
        /// Maps an NEC to the database.
        /// </summary>
        public class NECMapping : ClassMap<NEC>
        {
            /// <summary>
            /// Maps an NEC to the database.
            /// </summary>
            public NECMapping()
            {
                Table("necs");

                Id(x => x.ID).GeneratedBy.Guid();

                Map(x => x.Name).Not.Nullable().Unique().Length(10);
                Map(x => x.Description).Nullable().Length(40);
            }
        }


    }
}
