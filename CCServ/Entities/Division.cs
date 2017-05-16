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
    /// Describes a single Division.
    /// </summary>
    public class Division
    {

        #region Properties

        /// <summary>
        /// The Id of this division.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The value of this division, such as N42.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// The description of this division.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The department to which this division belongs.
        /// </summary>
        public virtual Department Department { get; set; }

        #endregion

        /// <summary>
        /// Maps a division to the database.
        /// </summary>
        public class DivisionMapping : ClassMap<Division>
        {
            /// <summary>
            /// Maps a division to the database.
            /// </summary>
            public DivisionMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Value).Not.Nullable().Unique();
                Map(x => x.Description);

                References(x => x.Department).LazyLoad(Laziness.False);

                Cache.ReadWrite();
            }
        }

        /// <summary>
        /// Validates le division.
        /// </summary>
        public class DivisionValidator : AbstractValidator<Division>
        {
            /// <summary>
            /// Validates the division.
            /// </summary>
            public DivisionValidator()
            {
                RuleFor(x => x.Description).Length(0, 255)
                    .WithMessage("The description of a Department must be no more than 255 characters.");
                RuleFor(x => x.Value).NotEmpty()
                    .WithMessage("The value must not be empty");
            }
        }
    }
}
