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
    /// Defines a requirement assignment, which is how a requirement gets assigned to a person.
    /// </summary>
    public class RequirementAssignment : TrainingAssignmentBase
    {

        #region Properties

        /// <summary>
        /// The requirement that was assigned by this assignment.
        /// </summary>
        public virtual Requirement Requirement { get; set; }

        /// <summary>
        /// The time at which the client completed this training assignment.
        /// </summary>
        public virtual DateTime CompletedDate { get; set; }

        /// <summary>
        /// Indicates that this training requirement has been completed.
        /// </summary>
        public virtual bool IsCompleted { get; set; }

        #endregion

        /// <summary>
        /// Maps a requirement assignment to the database.
        /// </summary>
        public class RequirementAssignmentMapping : ClassMap<RequirementAssignment>
        {
            /// <summary>
            /// Maps a requirement assignment to the database.
            /// </summary>
            public RequirementAssignmentMapping()
            {
                Id(x => x.Id).GeneratedBy.Guid();

                References(x => x.AssignedBy).Not.Nullable();
                References(x => x.AssignedTo).Not.Nullable();
                References(x => x.Requirement).Not.Nullable();

                Map(x => x.DateAssigned).Not.Nullable();
                Map(x => x.CompleteByDate).Not.Nullable();
                Map(x => x.CompletedDate).Nullable();
                Map(x => x.IsCompleted).Default(false.ToString());

                HasMany(x => x.Comments);
            }
        }

        /// <summary>
        /// Requirement assignment validator... guess what it does.
        /// </summary>
        public class RequirementAssignmentValidator : AbstractValidator<RequirementAssignment>
        {
            /// <summary>
            /// Requirement assignment validator... guess what it does.
            /// </summary>
            public RequirementAssignmentValidator()
            {
                RuleFor(x => x.Id).NotEmpty();
                RuleFor(x => x.DateAssigned).NotEmpty();
                RuleFor(x => x.CompleteByDate).NotEmpty();
                Custom(assignment =>
                {
                    if (assignment.CompleteByDate.Date <= assignment.DateAssigned.Date)
                        return new FluentValidation.Results.ValidationFailure(PropertySelector.SelectPropertyFrom<RequirementAssignment>(x => x.CompleteByDate).Name, "The complete by date may not be before or on the same day as the date an assignment is assigned.");
                    
                    return null;
                });

                When(x => x.IsCompleted, () =>
                {
                    Custom(assignment =>
                    {
                        if (assignment.CompletedDate < assignment.DateAssigned)
                            return new FluentValidation.Results.ValidationFailure(PropertySelector.SelectPropertyFrom<RequirementAssignment>(y => y.CompletedDate).Name, "The date a training was complete can not be before the date it was assigned.");

                        return null;
                    });
                });

                RuleFor(x => x.Comments).SetCollectionValidator(new AssignmentComment.AssignmentCommentValidator());

                RuleFor(x => x.AssignedBy).NotEmpty();
                RuleFor(x => x.AssignedTo).NotEmpty();

                RuleFor(x => x.Requirement).NotEmpty();
            }
        }
    }
}
