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
    /// Describes a single Department and all of its divisions.
    /// </summary>
    public class Department
    {
        #region Properties

        /// <summary>
        /// The ID of this department.
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// The Name of this department.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// A short description of this department.
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// A list of those divisions that belong to this department.
        /// </summary>
        public List<Division> Divisions { get; set; }


        #endregion

        /// <summary>
        /// Maps a department to the database.
        /// </summary>
        public class DepartmentMapping : ClassMap<Department>
        {
            /// <summary>
            /// Maps a department to the database.
            /// </summary>
            public DepartmentMapping()
            {
                Table("departments");

                Id(x => x.ID).GeneratedBy.Guid();

                Map(x => x.Name).Not.Nullable().Unique().Length(20);
                Map(x => x.Description).Nullable().Length(50);

                HasMany(x => x.Divisions);
            }
        }

    }
}
