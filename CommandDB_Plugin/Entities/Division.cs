using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandCentral.DataAccess;
using CommandCentral.ClientAccess;
using FluentNHibernate.Mapping;

namespace CommandCentral.Entities
{
    /// <summary>
    /// Describes a single Division.
    /// </summary>
    public class Division
    {
        #region Properties

        /// <summary>
        /// The ID of this division.
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// The name of this division.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// A short descripion of this division.
        /// </summary>
        public string Description { get; set; }

        #endregion

        public class DivisionMapping : ClassMap<Division>
        {
            public DivisionMapping()
            {
                Table("divisions");

                Id(x => x.ID).GeneratedBy.Guid();

                Map(x => x.Name).Not.Nullable().Unique().Length(20);
                Map(x => x.Description).Nullable().Length(50);
            }
        }
    }
}
