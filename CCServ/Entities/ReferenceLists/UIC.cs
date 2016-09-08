﻿using FluentNHibernate.Mapping;
using FluentValidation;

namespace CCServ.Entities.ReferenceLists
{
    /// <summary>
    /// Describes a single UIC.
    /// </summary>
    public class UIC : EditableReferenceListItemBase
    {

        /// <summary>
        /// Returns a validation result which contains the result of validation. lol.
        /// </summary>
        /// <returns></returns>
        public override FluentValidation.Results.ValidationResult Validate()
        {
            return new UICValidator().Validate(this);
        }

        /// <summary>
        /// Maps a UIC to the database.
        /// </summary>
        public class UICMapping : ClassMap<UIC>
        {
            /// <summary>
            /// Maps a UIC to the database.
            /// </summary>
            public UICMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Value).Not.Nullable().Length(10).Unique();
                Map(x => x.Description).Nullable().Length(40);

                Cache.ReadWrite();
            }
        }

        /// <summary>
        /// Validates the UIC.
        /// </summary>
        public class UICValidator : AbstractValidator<UIC>
        {
            /// <summary>
            /// Validates the UIC.
            /// </summary>
            public UICValidator()
            {
                RuleFor(x => x.Description).Length(0, 40)
                    .WithMessage("The description of a UIC must be no more than 40 characters.");
                RuleFor(x => x.Value).Length(1, 10)
                    .WithMessage("The value of a UIC must be between one and ten characters.");
            }
        }


    }
}
