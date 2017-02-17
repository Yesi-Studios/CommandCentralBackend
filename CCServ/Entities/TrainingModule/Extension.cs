using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using FluentValidation;
using AtwoodUtils;

namespace CCServ.Entities.TrainingModule
{
    /// <summary>
    /// Describes an extension.  An extension allows clients to extend the time a person has to complete a certain training.
    /// </summary>
    public class Extension
    {
        #region Properties

        /// <summary>
        /// The unique Id of this extension.
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The person who created this extension.
        /// </summary>
        public virtual Person Creator { get; set; }

        /// <summary>
        /// The comments, which together make a comment thread on which users can discuss this extension.
        /// </summary>
        public virtual IList<ExtensionComment> Comments { get; set; }

        /// <summary>
        /// The assignment to which this extension applies.  This extension will (if approved) extend the time a person has to complete the related requirement.
        /// </summary>
        public virtual Assignment Assignment { get; set; }

        /// <summary>
        /// Indicates that this extension has been approved.
        /// </summary>
        public virtual bool IsApproved { get; set; }

        /// <summary>
        /// The person who approved this extension.  Null if is apporved is false.
        /// </summary>
        public virtual Person Approver { get; set; }

        /// <summary>
        /// The date/time at which this extension was approved.
        /// </summary>
        public virtual DateTime DateApproved { get; set; }

        /// <summary>
        /// The date/time at which this extension was initially created.
        /// </summary>
        public virtual DateTime DateCreated { get; set; }

        /// <summary>
        /// Indicates for how many days this extension will, if approved, extend the related assignment's complete by date.
        /// </summary>
        public virtual int Days { get; set; }

        #endregion

        /// <summary>
        /// Maps an extension to the database.
        /// </summary>
        public class ExtensionMapping : ClassMap<Extension>
        {
            /// <summary>
            /// Maps an extension to the database.
            /// </summary>
            public ExtensionMapping()
            {
                Id(x => x.Id).GeneratedBy.Guid();

                References(x => x.Creator).Not.Nullable();
                References(x => x.Approver).Nullable();
                References(x => x.Assignment).Not.Nullable();

                HasMany(x => x.Comments);

                Map(x => x.IsApproved).Default(false.ToString());
                Map(x => x.DateApproved).Nullable();
                Map(x => x.DateCreated).Not.Nullable();
                Map(x => x.Days).Not.Nullable();
            }
        }

        /// <summary>
        /// Validates an extension.
        /// </summary>
        public class ExtensionValidator : AbstractValidator<Extension>
        {
            /// <summary>
            /// Validates an extension.
            /// </summary>
            public ExtensionValidator()
            {
                RuleFor(x => x.Id).NotEmpty();

                RuleFor(x => x.Creator).NotEmpty();
                RuleFor(x => x.Assignment).NotEmpty();
                RuleFor(x => x.DateCreated).NotEmpty();

                RuleFor(x => x.Comments).SetCollectionValidator(new ExtensionComment.ExtensionCommentValidator());

                When(x => !x.IsApproved, () =>
                {
                    RuleFor(x => x.DateApproved).Empty()
                        .WithMessage("If the extension has not been approved, its date approved must not be set.");
                    RuleFor(x => x.Approver).Empty()
                        .WithMessage("If the extension has not been approved, its approver must be set to null.");
                });

                When(x => x.IsApproved, () =>
                {
                    Custom(extension =>
                    {
                        if (extension.DateApproved.Date < extension.DateCreated.Date)
                            return new FluentValidation.Results.ValidationFailure(PropertySelector.SelectPropertyFrom<Extension>(x => x.DateApproved).Name, "The date an extension was approved may not be before the date it was submitted.");

                        return null;
                    });

                    RuleFor(x => x.Approver).NotEmpty()
                        .WithMessage("If an extension has been approved, its approver must be set.");
                });

                RuleFor(x => x.Days).NotEmpty().GreaterThanOrEqualTo(1)
                    .WithMessage("An extension must be for at least one day.");
            }
        }
           
    }
}
