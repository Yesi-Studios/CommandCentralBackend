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
    /// Defines an assignment, which is how a requirement gets assigned to a person.
    /// </summary>
    public class Assignment
    {

        #region Properties

        /// <summary>
        /// Unique Id of this assignment.
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The date/time that this assignment was created.
        /// </summary>
        public virtual DateTime DateAssigned { get; set; }

        /// <summary>
        /// A list of comments.  This is the comment thread for the assignment object.
        /// </summary>
        public virtual IList<AssignmentComment> Comments { get; set; }

        /// <summary>
        /// The person who assigned this assignment.
        /// </summary>
        public virtual Person AssignedBy { get; set; }

        /// <summary>
        /// The person who this assignment is assigned to.  This could be considered the owner.
        /// </summary>
        public virtual Person AssignedTo { get; set; }

        /// <summary>
        /// The requirement that was assigned by this assignment.
        /// </summary>
        public virtual Requirement Requirement { get; set; }

        /// <summary>
        /// The date by which this assignment should be completed.
        /// </summary>
        public virtual DateTime CompleteByDate { get; set; }

        #endregion

        /// <summary>
        /// Maps an assignment to the database.
        /// </summary>
        public class AssignmentMapping : ClassMap<Assignment>
        {
            /// <summary>
            /// Maps an assignment to the database.
            /// </summary>
            public AssignmentMapping()
            {
                Id(x => x.Id).GeneratedBy.Guid();

                References(x => x.AssignedBy).Not.Nullable();
                References(x => x.AssignedTo).Not.Nullable();
                References(x => x.Requirement).Not.Nullable();

                Map(x => x.DateAssigned).Not.Nullable();
                Map(x => x.CompleteByDate).Not.Nullable();

                HasMany(x => x.Comments);
            }
        }

        /// <summary>
        /// Assignment validator... guess what it does.
        /// </summary>
        public class AssignmentValidator : AbstractValidator<Assignment>
        {
            /// <summary>
            /// Assignment validator... guess what it does.
            /// </summary>
            public AssignmentValidator()
            {
                RuleFor(x => x.Id).NotEmpty();
                RuleFor(x => x.DateAssigned).NotEmpty();
                RuleFor(x => x.CompleteByDate).NotEmpty();
                Custom(assignment =>
                {
                    if (assignment.CompleteByDate.Date <= assignment.DateAssigned.Date)
                        return new FluentValidation.Results.ValidationFailure(PropertySelector.SelectPropertyFrom<Assignment>(x => x.CompleteByDate).Name, "The complete by date may not be before or on the same day as the date an assignment is assigned.");

                    return null;
                });

                RuleFor(x => x.Comments).SetCollectionValidator(new AssignmentComment.AssignmentCommentValidator());

                RuleFor(x => x.AssignedBy).NotEmpty();
                RuleFor(x => x.AssignedTo).NotEmpty();

                RuleFor(x => x.Requirement).NotEmpty();
            }
        }
    }
}
