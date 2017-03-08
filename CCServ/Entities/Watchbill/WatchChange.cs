using FluentNHibernate.Mapping;
using FluentValidation;
using NHibernate.Type;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.Entities.Watchbill
{
    /// <summary>
    /// Represents a request by a person to swap an assigned watch with someone else.
    /// </summary>
    public class WatchChange
    {

        #region Properties

        /// <summary>
        /// The unique Id of this watch swap.
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The watch assignment that this watch swap seeks to change.
        /// </summary>
        public virtual WatchAssignment WatchAssignment { get; set; }

        /// <summary>
        /// The datetime at which this watch swap was created.
        /// </summary>
        public virtual DateTime DateCreated { get; set; }

        /// <summary>
        /// The person who created this watch swap request.
        /// </summary>
        public virtual Person CreatedBy { get; set; }

        /// <summary>
        /// This is the person who will be assigned to the watch in the event that this watch swap is approved.
        /// </summary>
        public virtual Person PersonToAssign { get; set; }

        /// <summary>
        /// The person who approved this watch swap.
        /// </summary>
        public virtual Person ApprovedBy { get; set; }

        /// <summary>
        /// The comments on this watch swap.
        /// </summary>
        public virtual IList<Comment> Comments { get; set; }

        /// <summary>
        /// The datetime at which this watch swap was approved.
        /// </summary>
        public virtual DateTime? DateApproved { get; set; }

        /// <summary>
        /// Indicates if this watch swap has been approved.
        /// </summary>
        public virtual bool IsApproved { get; set; }

        #endregion

        /// <summary>
        /// Maps this object to the database.
        /// </summary>
        public class WatchChangeMapping : ClassMap<WatchChange>
        {
            /// <summary>
            /// Maps this object to the database.
            /// </summary>
            public WatchChangeMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                References(x => x.WatchAssignment).Not.Nullable();
                References(x => x.CreatedBy).Not.Nullable();
                References(x => x.PersonToAssign);
                References(x => x.ApprovedBy);

                HasMany(x => x.Comments)
                    .KeyColumn("EntityOwner_id");

                Map(x => x.IsApproved).Default(false.ToString());
                Map(x => x.DateApproved).CustomType<UtcDateTimeType>();
                Map(x => x.DateCreated).Not.Nullable().CustomType<UtcDateTimeType>();
            }
        }

        /// <summary>
        /// Validates the parent object.
        /// </summary>
        public class WatchChangeValidator : AbstractValidator<WatchChange>
        {
            /// <summary>
            /// Validates the parent object.
            /// </summary>
            public WatchChangeValidator()
            {
                //Basic validations first.
                RuleFor(x => x.WatchAssignment).NotEmpty();
                RuleFor(x => x.DateCreated).NotEmpty();
                RuleFor(x => x.CreatedBy).NotEmpty();

                When(x => x.PersonToAssign != null || x.IsApproved || x.DateApproved.HasValue || x.DateApproved.Value != default(DateTime), () =>
                {
                    RuleFor(x => x.DateApproved).NotEmpty();
                    RuleFor(x => x.DateApproved).Must(x => x.Value != default(DateTime));
                    RuleFor(x => x.IsApproved).Must(x => x == true);
                    RuleFor(x => x.PersonToAssign).NotEmpty().WithMessage("You may not approve a watch change without first assigning a person to it.");
                });

                RuleFor(x => x.Comments).SetCollectionValidator(new Comment.CommentValidator());
            }
        }

    }
}
