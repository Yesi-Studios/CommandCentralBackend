using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;
using FluentNHibernate.Mapping;
using FluentValidation;

namespace CCServ.Entities
{
    /// <summary>
    /// Defines a single comment, which can be used by objects to create a comment thread.
    /// </summary>
    public class Comment
    {
        #region Properties

        /// <summary>
        /// The unique Id of this comment.
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The text of this comment.
        /// </summary>
        public virtual string Text { get; set; }

        /// <summary>
        /// The time this comment was made.
        /// </summary>
        public virtual DateTime DateCreated { get; set; }

        /// <summary>
        /// The creator of this comment.
        /// </summary>
        public virtual Person Creator { get; set; }

        #endregion

        /// <summary>
        /// Maps a comment to the database.
        /// </summary>
        public class CommentMapping : ClassMap<Comment>
        {
            /// <summary>
            /// Maps a comment to the database.
            /// </summary>
            public CommentMapping()
            {
                Id(x => x.Id).GeneratedBy.Guid();

                References(x => x.Creator).Not.Nullable();

                Map(x => x.Text).Length(500).Not.Nullable();
                Map(x => x.DateCreated).Not.Nullable();
            }
        }

        /// <summary>
        /// Validates a comment.
        /// </summary>
        public class CommentValidator : AbstractValidator<Comment>
        {
            /// <summary>
            /// Validates a comment.
            /// </summary>
            public CommentValidator()
            {
                RuleFor(x => x.Id).NotEmpty();

                RuleFor(x => x.Text).Length(1, 500).NotEmpty();
                RuleFor(x => x.DateCreated).NotEmpty();
                RuleFor(x => x.Creator).NotEmpty();
            }
        }
    }
}
