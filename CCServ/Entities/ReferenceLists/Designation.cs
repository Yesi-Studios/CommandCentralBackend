using FluentNHibernate.Mapping;
using FluentValidation;

namespace CCServ.Entities.ReferenceLists
{
    /// <summary>
    /// Describes a single designation.  This is the job title for civilians, the rate for enlisted and the designator for officers.
    /// </summary>
    public class Designation : ReferenceListItemBase
    {
        /// <summary>
        /// Validates this designation.
        /// </summary>
        /// <returns></returns>
        public override FluentValidation.Results.ValidationResult Validate()
        {
            return new DesignationValidator().Validate(this);
        }

        /// <summary>
        /// Maps a Designation to the database.
        /// </summary>
        public class DesignationMapping : ClassMap<Designation>
        {
            /// <summary>
            /// Maps a Designation to the database.
            /// </summary>
            public DesignationMapping()
            {
                Id(x => x.Id).GeneratedBy.Guid();

                Map(x => x.Value).Not.Nullable().Unique().Length(10);
                Map(x => x.Description).Nullable().Length(40);

                Cache.ReadWrite();
            }
        }

        /// <summary>
        /// Validates a designation.
        /// </summary>
        public class DesignationValidator : AbstractValidator<Designation>
        {
            /// <summary>
            /// Validates a designation.
            /// </summary>
            public DesignationValidator()
            {
                RuleFor(x => x.Description).Length(0, 40)
                    .WithMessage("The description of a designation can be no more than 40 characters.");
                RuleFor(x => x.Value).Length(1, 10)
                    .WithMessage("The value of a designation must be between one and ten characters.");
            }
        }
    }
}
