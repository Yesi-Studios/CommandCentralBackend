using FluentNHibernate.Mapping;
using FluentValidation;

namespace CCServ.Entities.ReferenceLists
{
    /// <summary>
    /// Describes a single ethnicity
    /// </summary>
    public class Ethnicity : ReferenceListItemBase
    {
        /// <summary>
        /// Validates this ethnicity.
        /// </summary>
        /// <returns></returns>
        public override FluentValidation.Results.ValidationResult Validate()
        {
            return new EthnicityValidator().Validate(this);
        }

        /// <summary>
        /// Maps an ethnicity to the database.
        /// </summary>
        public class EthnicityMapping : ClassMap<Ethnicity>
        {
            /// <summary>
            /// Maps an ethnicity to the database.
            /// </summary>
            public EthnicityMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Value).Not.Nullable().Unique().Length(45);
                Map(x => x.Description).Nullable().Length(40);

                Cache.ReadWrite();
            }
        }

        /// <summary>
        /// Validates an ethnicity.
        /// </summary>
        public class EthnicityValidator : AbstractValidator<Ethnicity>
        {
            /// <summary>
            /// Validates an ethnicity.
            /// </summary>
            public EthnicityValidator()
            {
                RuleFor(x => x.Description).Length(0, 40)
                    .WithMessage("The description of an ethnicity may be no more than 40 characters.");
                RuleFor(x => x.Value).Length(1, 15)
                    .WithMessage("The value of an ethnicity must be between 1 and 15 characters.");
            }
        }
    }

}
