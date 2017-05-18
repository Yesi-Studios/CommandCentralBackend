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
    /// Describes a single NEC.
    /// </summary>
    public class NEC : EditableReferenceListItemBase
    {

        /// <summary>
        /// The type of the NEC.
        /// </summary>
        public virtual PersonType NECType { get; set; }

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
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Value).Not.Nullable().Unique();
                Map(x => x.Description);
                
                References(x => x.NECType).Not.LazyLoad();

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
                RuleFor(x => x.Description).Length(0, 255)
                    .WithMessage("The description of an NEC can be no more than 255 characters.");
                RuleFor(x => x.Value).NotEmpty()
                    .WithMessage("The value must not be empty.");
            }
        }

        
    }
}
