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
    public class UpdateNewsItem : DTOBase
    {

        /// <summary>
        /// The id of the news item the client wants to update.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The new title to apply to the given news item.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The new paragraphs to apply to the news item.
        /// </summary>
        public List<string> Paragraphs { get; set; }

        /// <summary>
        /// Creates a new DTO.
        /// </summary>
        /// <param name="obj"></param>
        public UpdateNewsItem(JObject obj) : base(obj)
        {
        }

        class Validator : AbstractValidator<UpdateNewsItem>
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
