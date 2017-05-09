using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation.Results;
using FluentValidation;
using Newtonsoft.Json.Linq;

namespace CCServ.ClientAccess.DTOs.Account
{
    /// <summary>
    /// The dto to be used by the respective endpoint.
    /// </summary>
    public class Login : DTOBase
    {
        /// <summary>
        /// This is the user's password.
        /// </summary>
        [Required]
        [Description("The user's password.")]
        public string Password { get; set; }

        /// <summary>
        /// The user's username.
        /// </summary>
        [Required]
        [Description("The user's username.")]
        public string Username { get; set; }

        /// <summary>
        /// Creates a new login DTO.
        /// </summary>
        /// <param name="obj"></param>
        public Login(JObject obj) : base(obj)
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
