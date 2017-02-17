using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;
using FluentNHibernate.Mapping;
using FluentValidation;

namespace CCServ.Entities.TrainingModule
{
    /// <summary>
    /// Defines a pipeline assignment, which is how an entire pipeline gets assigned to a person.
    /// </summary>
    public class PipelineAssignment : TrainingAssignmentBase
    {
        #region Properties

        /// <summary>
        /// The pipeline that is assigned by this pipeline assignment.
        /// </summary>
        public virtual Pipeline Pipeline { get; set; }

        /// <summary>
        /// The requirement assignments assigned to the user as a part of this pipeline.
        /// </summary>
        public virtual IList<RequirementAssignment> RequirementAssignments { get; set; }

        #endregion

        /// <summary>
        /// Maps a pipeline assignment to the database.
        /// </summary>
        public class PipelineAssignmentMapping : ClassMap<PipelineAssignment>
        {
            /// <summary>
            /// Maps a pipeline assignment to the database.
            /// </summary>
            public PipelineAssignmentMapping()
            {
                Id(x => x.Id).GeneratedBy.Guid();

                References(x => x.AssignedBy).Not.Nullable();
                References(x => x.AssignedTo).Not.Nullable();
                References(x => x.Pipeline).Not.Nullable();

                HasManyToMany(x => x.RequirementAssignments)
                    .Table("requirementassignmenttopipelineassignment_requirementassignments");

                Map(x => x.DateAssigned).Not.Nullable();
                Map(x => x.CompleteByDate).Not.Nullable();

                HasMany(x => x.Comments);
            }
        }

        /// <summary>
        /// Pipeline assignment validator... guess what it does.
        /// </summary>
        public class PipelineAssignmentValidator : AbstractValidator<PipelineAssignment>
        {
            /// <summary>
            /// Pipeline assignment validator... guess what it does.
            /// </summary>
            public PipelineAssignmentValidator()
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

                RuleFor(x => x.Comments).SetCollectionValidator(new Comment.CommentValidator());

                RuleFor(x => x.AssignedBy).NotEmpty();
                RuleFor(x => x.AssignedTo).NotEmpty();

                RuleFor(x => x.Pipeline).NotEmpty();
                Custom(x =>
                {
                    foreach (var reqAssignment in x.RequirementAssignments)
                    {
                        if (reqAssignment.AssignedBy.Id != x.AssignedBy.Id)
                            return new FluentValidation.Results.ValidationFailure(PropertySelector.SelectPropertyFrom<PipelineAssignment>(y => y.AssignedBy).Name, "The person who assigned a pipeline assigned must be the same on all requirement assignments that are spawned by it.");

                        if (reqAssignment.AssignedTo.Id != x.AssignedTo.Id)
                            return new FluentValidation.Results.ValidationFailure(PropertySelector.SelectPropertyFrom<PipelineAssignment>(y => y.AssignedTo).Name, "The person who a pipeline assignment is assigned to must be the same for all child requirement assignments.");

                        if (reqAssignment.CompleteByDate != x.CompleteByDate)
                            return new FluentValidation.Results.ValidationFailure(PropertySelector.SelectPropertyFrom<PipelineAssignment>(y => y.CompleteByDate).Name, "The complete by date for a pipeline assignment must be the same for all requirements.");
                    }

                    return null;
                });
            }
        }

    }
}
