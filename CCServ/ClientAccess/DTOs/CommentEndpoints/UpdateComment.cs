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
    /// The dto for the related endpoint.
    /// </summary>
    public class UpdateComment : DTOBase
    {
        /// <summary>
        /// The id of the comment to update.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// This is the text the client wants to set the comment to.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Creates a new dto.
        /// </summary>
        /// <param name="obj"></param>
        public UpdateComment(JObject obj) : base(obj)
        {
        }

        class Validator : AbstractValidator<UpdateComment>
        {
            public Validator()
            {
                RuleFor(x => x.Text).NotEmpty().Length(1, 1000);
            }
        }
    }
}
