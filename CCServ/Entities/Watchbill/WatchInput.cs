using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using FluentValidation;
using NHibernate.Type;

namespace CCServ.Entities.Watchbill
{
    /// <summary>
    /// Represents a watch input object.  This is the way that a person indicates they can not stand a watch.
    /// </summary>
    public class WatchInput
    {

        #region Properties

        /// <summary>
        /// The unique Id of this watch input.
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// These are the watch shifts for which this watch input will apply.
        /// </summary>
        public virtual IList<WatchShift> WatchShifts { get; set; }

        /// <summary>
        /// This is the reason for which the person says they can not stand the watch.
        /// </summary>
        public virtual ReferenceLists.Watchbill.WatchInputReason InputReason { get; set; }

        /// <summary>
        /// The comments on this watch input.
        /// </summary>
        public virtual IList<Comment> Comments { get; set; }

        /// <summary>
        /// The person for whom this watch input was made.
        /// </summary>
        public virtual Person Person { get; set; }

        /// <summary>
        /// The person who created this watch input.
        /// </summary>
        public virtual Person SubmittedBy { get; set; }

        /// <summary>
        /// The person who confirmed this input.
        /// </summary>
        public virtual Person ConfirmedBy { get; set; }

        /// <summary>
        /// The datetime at which this watch input was confirmed.
        /// </summary>
        public virtual DateTime? DateConfirmed { get; set; }

        /// <summary>
        /// The datetime at which this watch input was created/submitted.
        /// </summary>
        public virtual DateTime DateSubmitted { get; set; }

        /// <summary>
        /// Indicates if this watch input has been confirmed.
        /// </summary>
        public virtual bool IsConfirmed { get; set; }

        #endregion

        /// <summary>
        /// Maps this object to the database.
        /// </summary>
        public class WatchInputMapping : ClassMap<WatchInput>
        {
            /// <summary>
            /// Maps this object to the database.
            /// </summary>
            public WatchInputMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                References(x => x.InputReason).Not.Nullable();
                References(x => x.Person).Not.Nullable();
                References(x => x.SubmittedBy).Not.Nullable();
                References(x => x.ConfirmedBy);
                
                HasMany(x => x.Comments)
                    .KeyColumn("EntityOwner_id")
                    .ForeignKeyConstraintName("none");

                HasManyToMany(x => x.WatchShifts).Inverse();

                Map(x => x.DateConfirmed).CustomType<UtcDateTimeType>();
                Map(x => x.DateSubmitted).Not.Nullable().CustomType<UtcDateTimeType>();
                Map(x => x.IsConfirmed).Default(false.ToString());
            }
        }

        /// <summary>
        /// Validates the parent object.
        /// </summary>
        public class WatchInputValidator : AbstractValidator<WatchInput>
        {
            /// <summary>
            /// Validates the parent object.
            /// </summary>
            public WatchInputValidator()
            {
                RuleFor(x => x.InputReason).NotEmpty();
                RuleFor(x => x.Person).NotEmpty();
                RuleFor(x => x.SubmittedBy).NotEmpty();
                RuleFor(x => x.DateSubmitted).NotEmpty();

                RuleFor(x => x.WatchShifts).NotEmpty();
                RuleFor(x => x.Comments).SetCollectionValidator(new Comment.CommentValidator());

                When(x => x.ConfirmedBy != null || x.DateConfirmed.HasValue || x.DateConfirmed.Value != default(DateTime) || x.IsConfirmed, () =>
                {
                    RuleFor(x => x.DateConfirmed).NotEmpty();
                    RuleFor(x => x.DateConfirmed).Must(x => x.Value != default(DateTime));
                    RuleFor(x => x.IsConfirmed).Must(x => x == true);
                    RuleFor(x => x.ConfirmedBy).NotEmpty();
                });
            }
        }
    }
}
