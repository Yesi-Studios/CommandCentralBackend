using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;
using CCServ.ClientAccess;
using FluentNHibernate.Mapping;
using FluentValidation;
using FluentValidation.Results;
using Humanizer;
using NHibernate.Criterion;

namespace CCServ.Entities.ReferenceLists.Watchbill
{
    /// <summary>
    /// Defines a watch input reason.  This is used to provide a selection of reasons as to why a person can not stand watch.
    /// </summary>
    public class WatchInputReason : EditableReferenceListItemBase
    {
        /// <summary>
        /// We do not implement a validator
        /// </summary>
        /// <returns></returns>
        public override ValidationResult Validate()
        {
            return new WatchInputReasonValidator().Validate(this);
        }

        /// <summary>
        /// Maps this object to the database.
        /// </summary>
        public class WatchInputReasonMapping : ClassMap<WatchInputReason>
        {
            /// <summary>
            /// Maps this object to the database.
            /// </summary>
            public WatchInputReasonMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Value).Not.Nullable().Unique();
                Map(x => x.Description);

                Cache.ReadWrite();
            }
        }

        /// <summary>
        /// Validates the object.
        /// </summary>
        public class WatchInputReasonValidator : AbstractValidator<WatchInputReason>
        {
            /// <summary>
            /// Validates the object.
            /// </summary>
            public WatchInputReasonValidator()
            {
                RuleFor(x => x.Description).Length(0, 255)
                    .WithMessage("The description of a watch input reason must be no more than 255 characters.");
                RuleFor(x => x.Value).NotEmpty()
                    .WithMessage("The value must not be null.");
            }
        }
    }
}
