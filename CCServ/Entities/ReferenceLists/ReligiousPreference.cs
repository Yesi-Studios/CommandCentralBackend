using System;
using System.Collections.Generic;
using CCServ.ClientAccess;
using FluentNHibernate.Mapping;
using FluentValidation;
using AtwoodUtils;
using System.Linq;
using NHibernate.Criterion;
using Humanizer;

namespace CCServ.Entities.ReferenceLists
{
    /// <summary>
    /// Describes a single Religious Preference
    /// </summary>
    public class ReligiousPreference : EditableReferenceListItemBase
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

                Map(x => x.Value).Not.Nullable().Unique();
                Map(x => x.Description);

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
                RuleFor(x => x.Description).Length(0, 255)
                    .WithMessage("A religious preference's decription may be no more than 255 characters.");
                RuleFor(x => x.Value).NotEmpty()
                    .WithMessage("The value must not be empty.");
            }
        }

    }
}
