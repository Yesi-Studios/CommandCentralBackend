using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using FluentValidation;

namespace CCServ.ClientAccess.DTOs.NewsItemEndpoints
{
    /// <summary>
    /// The dto for the related endpoint.
    /// </summary>
    public class CreateNewsItem : DTOBase
    {
        /// <summary>
        /// The title of this news item.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The list of paragraphs in this news item.
        /// </summary>
        public List<string> Paragraphs { get; set; }

        /// <summary>
        /// Creates a new dto.
        /// </summary>
        /// <param name="obj"></param>
        public CreateNewsItem(JObject obj) : base(obj)
        {
        }

        class Validator : AbstractValidator<CreateNewsItem>
        {
            public Validator()
            {
                RuleFor(x => x.Paragraphs)
                    .Must(x => x.Sum(y => y.Length) <= 4096)
                    .WithMessage("The total text in the paragraphs must not exceed 4096 characters.");
                RuleFor(x => x.Title).NotEmpty().Length(3, 50).WithMessage("The title must not be blank and must be between 3 and 50 characters.");
            }
        }
    }
}
