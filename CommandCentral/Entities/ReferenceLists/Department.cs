using System;
using System.Collections.Generic;
using FluentNHibernate.Mapping;

namespace CommandCentral.Entities.ReferenceLists
{
    /// <summary>
    /// Describes a single Department and all of its divisions.
    /// </summary>
    public class Department
    {
        #region Properties

        /// <summary>
        /// The Department's unique ID
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The value of this department.  Eg. C40
        /// </summary>
        public virtual string Value { get; set; }

        /// <summary>
        /// A short description of this department.
        /// </summary>
        public virtual string Description { get; set; }

        /// <summary>
        /// A list of those divisions that belong to this department.
        /// </summary>
        public virtual IList<Division> Divisions { get; set; }


        #endregion

        #region Overrides

        /// <summary>
        /// Returns the value (name) of this department.
        /// </summary>
        /// <returns></returns>
        public new virtual string ToString()
        {
            return Value;
        }

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

                Id(x => x.Id).GeneratedBy.Guid();

                Map(x => x.Value).Not.Nullable().Unique().Length(20);
                Map(x => x.Description).Nullable().Length(50);

                HasMany(x => x.Divisions).Cascade.All();

                Cache.ReadWrite();
            }
        }

    }
}
