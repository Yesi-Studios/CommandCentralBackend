using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using FluentValidation;

namespace CCServ.ClientAccess.DTOs.FeedbackEndpoints
{
    /// <summary>
    /// The DTO for the related endpoint.
    /// </summary>
    public class SubmitFeedback : DTOBase
    {
        /// <summary>
        /// The title of the feedback.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The actual text of the feedback.  This will be the body of the feedback email.
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Creates a new dto.
        /// </summary>
        /// <param name="obj"></param>
        public SubmitFeedback(JObject obj) : base(obj)
        {
        }

        class Validator : AbstractValidator<SubmitFeedback>
        {
            public Validator()
            {
                RuleFor(x => x.Title).NotEmpty().Length(1, 50);

                RuleFor(x => x.Body).NotEmpty().Length(1, 1000);
            }
        }
    }
}
