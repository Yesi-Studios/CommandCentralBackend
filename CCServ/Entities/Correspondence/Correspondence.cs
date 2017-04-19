using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using FluentValidation;
using CCServ.Entities.ReferenceLists;
using CCServ.ClientAccess;
using System.Globalization;
using AtwoodUtils;
using NHibernate.Type;

namespace CCServ.Entities.Correspondence
{
    /// <summary>
    /// Describes a single correspondence object.
    /// </summary>
    public class Correspondence : ICommentable, IAttachmentParent
    {

        #region Properties

        /// <summary>
        /// The unique Id of this correspondence.
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The person that created this correspondence.
        /// </summary>
        public virtual Person Creator { get; set; }

        /// <summary>
        /// The person for whom this correspondence was created.
        /// </summary>
        public virtual Person InCareOf { get; set; }

        /// <summary>
        /// The date/time at which this correspondence was created.
        /// </summary>
        public virtual DateTime DateCreated { get; set; }

        /// <summary>
        /// The subject like of a correspondence.  For example: BAH Request.
        /// </summary>
        public virtual string Subject { get; set; }

        /// <summary>
        /// The body of the correspondence is a single, large text field the client can use to write whatever they want.
        /// </summary>
        public virtual string Body { get; set; }

        /// <summary>
        /// Indicates if this correspondence has received final approval.  If null, then the correspondence is still in progress.
        /// </summary>
        public virtual bool? IsApproved { get; set; }

        /// <summary>
        /// The date/time this correspondence was denied or approved.
        /// </summary>
        public virtual DateTime? DateOfAction { get; set; }

        /// <summary>
        /// This is the final person who took an action on this correspondence to confirm or deny it.
        /// </summary>
        public virtual Person ActionPerson { get; set; }

        /// <summary>
        /// The "discussion thread" for this correspondence.
        /// </summary>
        public virtual IList<Comment> Comments { get; set; }

        /// <summary>
        /// This integer is the current step number we're on.  Any steps that have this number are considered the current step.
        /// </summary>
        public virtual int CurrentStepOrder { get; set; }

        /// <summary>
        /// The files that have been attached to this correspondence.  Note: This collection only contains references to each attachment.
        /// <para />
        /// Each attachment may be loaded from the /LoadAttachment endpoint.
        /// </summary>
        public virtual IList<FileAttachment> Attachments { get; set; }

        /// <summary>
        /// The type of correspondence.
        /// </summary>
        public virtual ReferenceLists.Correspondence.CorrespondenceType CorrespondenceType { get; set; }

        /// <summary>
        /// The steps in this correspondence.
        /// </summary>
        public virtual IList<CorrespondenceStep> Steps { get; set; }

        #endregion

        /// <summary>
        /// Maps this object to the database.
        /// </summary>
        public class CorrespondenceMapping : ClassMap<Correspondence>
        {
            /// <summary>
            /// Maps this object to the database.
            /// </summary>
            public CorrespondenceMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                References(x => x.Creator).Not.Nullable();
                References(x => x.InCareOf).Not.Nullable();
                References(x => x.CorrespondenceType).Not.Nullable();

                HasMany(x => x.Comments)
                    .Cascade.AllDeleteOrphan()
                    .KeyColumn("EntityOwner_id")
                    .ForeignKeyConstraintName("none");

                HasMany(x => x.Attachments)
                    .Cascade.AllDeleteOrphan()
                    .KeyColumn("EntityOwner_id")
                    .ForeignKeyConstraintName("none");

                HasMany(x => x.Steps).Inverse();

                Map(x => x.Subject).Not.Nullable();
                Map(x => x.Body).Not.Nullable().CustomType<StringClobType>();
                Map(x => x.CurrentStepOrder).Not.Nullable();

                Map(x => x.DateCreated).Not.Nullable().CustomType<UtcDateTimeType>();
            }
        }

        /// <summary>
        /// Validates this object.
        /// </summary>
        public class CorrespondenceValidator : AbstractValidator<Correspondence>
        {
            /// <summary>
            /// Validates this object.
            /// </summary>
            public CorrespondenceValidator()
            {
                RuleFor(x => x.Creator).NotEmpty();
                RuleFor(x => x.InCareOf).NotEmpty();
                RuleFor(x => x.DateCreated).NotEmpty();
                RuleFor(x => x.Subject).NotEmpty().Length(1, 255);
                RuleFor(x => x.Body).NotEmpty().Length(1, 5000);
                RuleFor(x => x.CurrentStepOrder).GreaterThanOrEqualTo(0);

                RuleFor(x => x.CorrespondenceType).NotEmpty();

                RuleFor(x => x.Comments).SetCollectionValidator(new Comment.CommentValidator());

                //TODO: step validator and attachment validator
            }
        }

    }
}
