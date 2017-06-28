using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;
using FluentValidation;

namespace CommandCentral.Entities
{
    /// <summary>
    /// A single frequently asked question.
    /// </summary>
    public class FAQ
    {
        #region Properties

        /// <summary>
        /// The unique Guid for this FAQ
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The name of this FAQ item.
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// The question's text.
        /// </summary>
        public virtual string Question { get; set; }

        /// <summary>
        /// The answer/text for the FAQ.
        /// </summary>
        public virtual IList<string> Paragraphs { get; set; }

        #endregion

        #region Overrides

        /// <summary>
        /// Returns the question.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Question;
        }

        #endregion

        /// <summary>
        /// Maps an FAQ to the database
        /// </summary>
        public class FAQMapping : ClassMap<FAQ>
        {
            /// <summary>
            /// Maps an FAQ to the database
            /// </summary>
            public FAQMapping()
            {
                Id(x => x.Id).GeneratedBy.Assigned();

                Map(x => x.Name).Not.Nullable().Unique();
                Map(x => x.Question).Not.Nullable().Unique();
                HasMany(x => x.Paragraphs)
                    .Table("faqparagraphs")
                    .KeyColumn("FAQId")
                    .Element("Paragraph", x => x.Length(1000));
            }
        }

        /// <summary>
        /// Validates an FAQ
        /// </summary>
        public class FAQValidator : AbstractValidator<FAQ>
        {
            /// <summary>
            /// Validates an FAQ
            /// </summary>
            public FAQValidator()
            {
                RuleFor(x => x.Name).NotEmpty().Length(1, 50).Must(x =>
                {
                    return x.All(y => char.IsLetterOrDigit(y) || y == '-');
                })
                .WithMessage("An FAQ's name must not contains spaces or any other special character aside from dashes.");
                RuleFor(x => x.Question).Length(10, 255).NotEmpty()
                    .WithMessage("Your question must not be blank, not less than 10 characters, but no more than 255.");
                RuleFor(x => x.Paragraphs)
                    .Must(x => x.Sum(y => y.Length) <= 4096)
                    .WithMessage("The total text in the paragraphs must not exceed 4096 characters.");
            }
        }
    }
}
