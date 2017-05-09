using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using FluentValidation;

namespace CCServ.ClientAccess.DTOs.CommentEndpoints
{
    /// <summary>
    /// The dto for the related endpoint
    /// </summary>
    public class CreateComment : DTOBase
    {
        /// <summary>
        /// The text of the comment.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// This is the object to which you want to post this comment.  For example, if this comment is related to a watch assignment, then give the assignment's Id.
        /// </summary>
        public Guid EntityOwnerId { get; set; }

        /// <summary>
        /// Creates a new dto.
        /// </summary>
        /// <param name="obj"></param>
        public CreateComment(JObject obj) : base(obj)
        {
        }

        class Validator : AbstractValidator<CreateComment>
        {
            public Validator()
            {
                RuleFor(x => x.Text).NotEmpty().Length(1, 1000);
            }
        }
    }
}
