using System;
using System.Collections.Generic;
using System.Linq;
using AtwoodUtils;
using CCServ.Authorization;
using CCServ.ClientAccess;
using FluentNHibernate.Mapping;
using FluentValidation;
using NHibernate.Criterion;

namespace CCServ.Entities.ReferenceLists
{
    /// <summary>
    /// Describes a single Department and all of its divisions.
    /// </summary>
    public class Department
    {
        #region Properties

        /// <summary>
        /// The unique Id of this Department.
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// THe value of this department.
        /// </summary>
        public virtual string Value { get; set; }

        /// <summary>
        /// The department's description.
        /// </summary>
        public virtual string Description { get; set; }
        
        /// The command to which this department belongs.
        /// </summary>
        public virtual Command Command { get; set; }

        /// <summary>
        /// A list of those divisions that belong to this department.
        /// </summary>
        public virtual IList<Division> Divisions { get; set; }

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
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Value).Not.Nullable().Unique();
                Map(x => x.Description);

                HasMany(x => x.Divisions).Not.LazyLoad().Cascade.DeleteOrphan();

                References(x => x.Command).LazyLoad(Laziness.False);

                Cache.ReadWrite();
            }
        }

        /// <summary>
        /// Validates the Department.
        /// </summary>
        public class DepartmentValidator : AbstractValidator<Department>
        {
            /// <summary>
            /// Validates the Department.
            /// </summary>
            public DepartmentValidator()
            {
                RuleFor(x => x.Description).Length(0, 255)
                    .WithMessage("The description of a department must be no more than 255 characters.");
                RuleFor(x => x.Value).NotEmpty()
                    .WithMessage("The value must not be empty.");
            }
        }
    }
}
