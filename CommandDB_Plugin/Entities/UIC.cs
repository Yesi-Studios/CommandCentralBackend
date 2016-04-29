using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using CommandCentral.DataAccess;
using CommandCentral.ClientAccess;

namespace CommandCentral.Entities
{
    /// <summary>
    /// Describes a single UIC.
    /// </summary>
    public class UIC : CachedModel<UIC>
    {
        #region Properties

        /// <summary>
        /// The uqniey ID of the UIC.
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// The name of the UIC.  Such as 40533
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// A description of this UIC.
        /// </summary>
        public string Description { get; set; }

        #endregion

        public class UICMapping : ClassMap<UIC>
        {
            public UICMapping()
            {
                Table("uics");

                Id(x => x.ID).GeneratedBy.Guid();

                Map(x => x.Name).Not.Nullable().Length(10).Unique();
                Map(x => x.Description).Nullable().Length(40);

            }
        }


    }
}
