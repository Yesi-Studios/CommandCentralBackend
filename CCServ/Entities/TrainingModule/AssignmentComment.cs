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
    /// A comment on an assignment, part of the comment thread.
    /// </summary>
    public class AssignmentComment : CommentBase
    {
        /// <summary>
        /// The assignment to which this comment applies.
        /// </summary>
        public virtual Assignment Assignment { get; set; }

        /// <summary>
        /// Maps an assignment comment to the database.
        /// </summary>
        public class AssignmentCommentMapping : ClassMap<AssignmentComment>
        {
            /// <summary>
            /// Maps an assignment comment to the database.
            /// </summary>
            public AssignmentCommentMapping()
            {
                Id(x => x.Id).GeneratedBy.Guid();

                References(x => x.Creator).Not.Nullable();

                Map(x => x.Text).Length(500).Not.Nullable();
                Map(x => x.DateCreated).Not.Nullable();
            }
        }

        /// <summary>
        /// Validates an assignment comments.
        /// </summary>
        public class AssignmentCommentValidator : AbstractValidator<AssignmentComment>
        {
            /// <summary>
            /// Validates an assignment comments.
            /// </summary>
            public AssignmentCommentValidator()
            {
                RuleFor(x => x.Id).NotEmpty();

                RuleFor(x => x.Creator).NotEmpty();
                RuleFor(x => x.Text).Length(3, 500);

                RuleFor(x => x.Assignment).NotEmpty();
            }
        }
    }
}
