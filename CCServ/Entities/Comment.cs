using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using FluentValidation;
using NHibernate.Type;

namespace CommandCentral.Entities
{
    /// <summary>
    /// A comment.  It's assigned to an object where users can comment on it.
    /// </summary>
    public class Comment
    {

        #region Properties

        /// <summary>
        /// The unique Id of this comment.
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The person who created this comment.
        /// </summary>
        public virtual Person Creator { get; set; }

        /// <summary>
        /// This is the text of the comment.
        /// </summary>
        public virtual string Text { get; set; }

        /// <summary>
        /// The datetime at which this comment was made.
        /// </summary>
        public virtual DateTime Time { get; set; }

        #endregion

        /// <summary>
        /// Maps this object to the database.
        /// </summary>
        public class CommentMapping : ClassMap<Comment>
        {
            /// <summary>
            /// Maps this object to the database.
            /// </summary>
            public CommentMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                References(x => x.Creator);

                Map(x => x.Text).Length(1000).Not.Nullable();
                Map(x => x.Time).Not.Nullable().CustomType<UtcDateTimeType>();
            }
        }

        /// <summary>
        /// Validates the parent object.
        /// </summary>
        public class CommentValidator : AbstractValidator<Comment>
        {
            /// <summary>
            /// Validates the parent object.
            /// </summary>
            public CommentValidator()
            {
                RuleFor(x => x.Creator).NotEmpty();
                RuleFor(x => x.Text).NotEmpty().Length(1, 1000);
                RuleFor(x => x.Time).NotEmpty();
            }
        }
    }
}
