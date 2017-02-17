using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using FluentValidation;

namespace CCServ.Entities.TrainingModule
{
    /// <summary>
    /// A comment on an extension.
    /// </summary>
    public class ExtensionComment : CommentBase
    {
        /// <summary>
        /// The extension to which this comment applies.
        /// </summary>
        public Extension Extension { get; set; }

        /// <summary>
        /// Maps an extension comment to the database.
        /// </summary>
        public class ExtensionCommentMapping : ClassMap<ExtensionComment>
        {
            /// <summary>
            /// Maps an extension comment to the database.
            /// </summary>
            public ExtensionCommentMapping()
            {
                Id(x => x.Id).GeneratedBy.Guid();

                References(x => x.Creator).Not.Nullable();
                References(x => x.Extension).Not.Nullable();

                Map(x => x.Text).Length(500).Not.Nullable();
                Map(x => x.DateCreated).Not.Nullable();
            }
        }

        /// <summary>
        /// Validates an extension comments.
        /// </summary>
        public class ExtensionCommentValidator : AbstractValidator<ExtensionComment>
        {
            /// <summary>
            /// Validates an extension comments.
            /// </summary>
            public ExtensionCommentValidator()
            {
                RuleFor(x => x.Id).NotEmpty();

                RuleFor(x => x.Creator).NotEmpty();
                RuleFor(x => x.Text).Length(3, 500);

                RuleFor(x => x.Extension).NotEmpty();
                RuleFor(x => x.DateCreated).NotEmpty();
            }
        }
    }
}
