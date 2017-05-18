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
    /// Describes a single ethnicity
    /// </summary>
    public class Ethnicity : EditableReferenceListItemBase
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

                Map(x => x.Value).Not.Nullable().Unique();
                Map(x => x.Description);

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
                RuleFor(x => x.Description).Length(0, 255)
                    .WithMessage("The description of an ethnicity may be no more than 255 characters.");
                RuleFor(x => x.Value).NotEmpty()
                    .WithMessage("The value must not be empty.");
            }
        }

        
    }

}
