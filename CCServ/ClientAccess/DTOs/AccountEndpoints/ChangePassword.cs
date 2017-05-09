using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using FluentValidation;

namespace CCServ.ClientAccess.DTOs.AccountEndpoints
{
    /// <summary>
    /// The dto for the related endpoint.
    /// </summary>
    public class ChangePassword : DTOBase
    {
        /// <summary>
        /// The previous password for the client.
        /// </summary>
        public string OldPassword { get; set; }

        /// <summary>
        /// The new password the client wants.
        /// </summary>
        public string NewPassword { get; set; }

        /// <summary>
        /// Creates a new dto.
        /// </summary>
        /// <param name="obj"></param>
        public ChangePassword(JObject obj) : base(obj)
        {
        }

        class Validator : AbstractValidator<ChangePassword>
        {
            public Validator()
            {
                //TODO
                RuleFor(x => x.OldPassword).Must(x => true)
                    .WithMessage("");

                RuleFor(x => x.NewPassword).Must(x => true)
                    .WithMessage("");
            }
        }
    }
}
