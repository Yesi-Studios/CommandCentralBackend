using FluentNHibernate.Mapping;
using FluentValidation;

namespace CCServ.Entities.ReferenceLists
{
    /// <summary>
    /// Describes a single Religious Preference
    /// </summary>
    public class ReligiousPreference : ReferenceListItemBase
    {
        /// <summary>
        /// Validates this religious preference.  
        /// </summary>
        /// <returns></returns>
        public override FluentValidation.Results.ValidationResult Validate()
        {
            return new ReligiousPreferenceValidator().Validate(this);
        }
        /// <summary>
        /// Maps a Religious Preference to the database.
        /// </summary>
        public class ReligiousPreferenceMapping : ClassMap<ReligiousPreference>
        {
            /// <summary>
            /// Maps a Religious Preference to the database.
            /// </summary>
            public ReligiousPreferenceMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Value).Not.Nullable().Unique().Length(45);
                Map(x => x.Description).Nullable().Length(40);

                Cache.ReadWrite();
            }
        }

        /// <summary>
        /// Validates the RP
        /// </summary>
        public class ReligiousPreferenceValidator : AbstractValidator<ReligiousPreference>
        {
            /// <summary>
            /// Validates the RP
            /// </summary>
            public ReligiousPreferenceValidator()
            {
                RuleFor(x => x.Description).Length(0, 40)
                    .WithMessage("A religious preference's decription may be no more than 40 characters.");
                RuleFor(x => x.Value).Length(1, 15)
                    .WithMessage("A religious preference's value must be between 1 and 15 characters.");
            }
        }
    }
}
