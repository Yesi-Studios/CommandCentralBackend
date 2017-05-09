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
    public class CompletePasswordReset : DTOBase
    {

        /// <summary>
        /// The new password the client wants on their profile.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// The id that was sent to the user's .mil email address.  This Id acts as a key allowing the caller to reset the password.
        /// </summary>
        public Guid PasswordResetId { get; set; }

        /// <summary>
        /// Creates a new DTO.
        /// </summary>
        /// <param name="obj"></param>
        public CompletePasswordReset(JObject obj) : base(obj)
        {
        }

        class Validator : AbstractValidator<CompletePasswordReset>
        {
            public Validator()
            {
                //TODO real validation
                RuleFor(x => x.Password).Must(x => true)
                    .WithMessage("dkfjghsdjfg");
            }
        }
    }
}
