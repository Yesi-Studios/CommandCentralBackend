using System;
using System.Linq;
using System.Collections.Generic;
using CCServ.ClientAccess;
using FluentNHibernate.Mapping;
using FluentValidation;
using AtwoodUtils;
using CCServ.Authorization;
using NHibernate.Type;

namespace CCServ.Entities
{
    /// <summary>
    /// Describes a single News Item and its members, including its DB access members.
    /// </summary>
    public class NewsItem
    {

        #region Properties

        /// <summary>
        /// The Id of the news item.
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The client that created the news item.
        /// </summary>
        public virtual Person Creator { get; set; }

        /// <summary>
        /// The title of the news item.
        /// </summary>
        public virtual string Title { get; set; }

        /// <summary>
        /// The paragraphs contained in this news item.
        /// </summary>
        public virtual IList<string> Paragraphs { get; set; }

        /// <summary>
        /// The time this news item was created.
        /// </summary>
        public virtual DateTime CreationTime { get; set; }

        #endregion

        

        /// <summary>
        /// Maps a news item to the database.
        /// </summary>
        public class NewsItemMapping : ClassMap<NewsItem>
        {
            /// <summary>
            /// Maps a news item to the database.
            /// </summary>
            public NewsItemMapping()
            {
                Id(x => x.Id).GeneratedBy.Guid();

                References(x => x.Creator).LazyLoad(Laziness.False);

                Map(x => x.Title).Not.Nullable();
                HasMany(x => x.Paragraphs)
                    .Table("newsitemparagraphs")
                    .KeyColumn("NewsItemID")
                    .Element("Paragraph", x => x.Length(10000))
                    .Not.LazyLoad();
                Map(x => x.CreationTime).Not.Nullable().Not.LazyLoad();
            }
        }

        /// <summary>
        /// Validates the properties of a news item.
        /// </summary>
        public class NewsItemValidator : AbstractValidator<NewsItem>
        {
            /// <summary>
            /// Validates the properties of a news item.
            /// </summary>
            public NewsItemValidator()
            {
                RuleFor(x => x.CreationTime).NotEmpty();
                RuleFor(x => x.Creator).NotEmpty();
                RuleFor(x => x.Id).NotEmpty();
                RuleFor(x => x.Paragraphs)
                    .Must(x => x.Sum(y => y.Length) <= 4096)
                    .WithMessage("The total text in the paragraphs must not exceed 4096 characters.");
                RuleFor(x => x.Title).NotEmpty().Length(3, 50).WithMessage("The title must not be blank and must be between 3 and 50 characters.");
            }
        }
    }
}
