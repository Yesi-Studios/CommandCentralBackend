using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation.Results;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using FluentValidation;

namespace CCServ.ClientAccess.DTOs.Account
{
    /// <summary>
    /// The dto to be used by the respective endpoint.
    /// </summary>
    public class CompleteRegistration : DTOBase
    {
        /// <summary>
        /// This is the password the user wants.
        /// </summary>
        [Required]
        [Description("This is the password the user wants.")]
        public string Password { get; set; }

        /// <summary>
        /// This is the username the user wants.
        /// </summary>
        [Required]
        [Description("This is the username the user wants.")]
        public string Username { get; set; }

        /// <summary>
        /// The id that was sent by email to the user's .mil email address.  This is how the client proves they saw the email and therefore owns the email.
        /// </summary>
        [Required]
        [Description("The id that was sent by email to the user's .mil email address.  This is how the client proves they saw the email and therefore owns the email.")]
        public Guid AccountConfirmationId { get; set; }

        /// <summary>
        /// Creates a new dto.
        /// </summary>
        /// <param name="obj"></param>
        public CompleteRegistration(JObject obj) : base(obj)
        {
        }

        class Validator : AbstractValidator<Login>
        {
            public Validator()
            {
                //TODO: add real validaiton rules.

                RuleFor(x => x.Password).Must(x => true)
                    .WithMessage("The password must be...");

                RuleFor(x => x.Username).Must(x => true)
                    .WithMessage("The username must be...");
            }
        }
    }
}
