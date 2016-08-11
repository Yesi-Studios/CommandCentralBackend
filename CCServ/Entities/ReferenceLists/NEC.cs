using FluentNHibernate.Mapping;
using FluentValidation;

namespace CCServ.Entities.ReferenceLists
{
    /// <summary>
    /// Describes a single NEC.
    /// </summary>
    public class NEC : ReferenceListItemBase
    {
        /// <summary>
        /// Validates an NEC.  Weeee.
        /// </summary>
        /// <returns></returns>
        public override FluentValidation.Results.ValidationResult Validate()
        {
            return new NECValidator().Validate(this);
        }


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
                Id(x => x.Id).GeneratedBy.Guid();

                Map(x => x.Value).Not.Nullable().Unique().Length(10);
                Map(x => x.Description).Nullable().Length(40);

                Cache.ReadWrite();
            }
        }

        /// <summary>
        /// Validates an NEC
        /// </summary>
        public class NECValidator : AbstractValidator<NEC>
        {
            /// <summary>
            /// Validates an NEC
            /// </summary>
            public NECValidator()
            {
                RuleFor(x => x.Description).Length(0, 40)
                    .WithMessage("The description of an NEC can be no more than 40 characters.");
                RuleFor(x => x.Value).Length(1, 10)
                    .WithMessage("The value of an NEC must be between one and ten characters.");
            }
        }
    }
}
