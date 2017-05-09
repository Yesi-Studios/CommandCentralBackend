using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using FluentValidation;

namespace CCServ.ClientAccess.DTOs.Account
{
    /// <summary>
    /// DTO for the named endpoint.
    /// </summary>
    public class ForgotUsername : DTOBase
    {
        /// <summary>
        /// The SSN of the client who wants to have their username sent to them.
        /// </summary>
        public string SSN { get; set; }

        /// <summary>
        /// Creates a new dto.
        /// </summary>
        /// <param name="obj"></param>
        public ForgotUsername(JObject obj) : base(obj)
        {
        }

        class Validator : AbstractValidator<ForgotUsername>
        {
            public Validator()
            {
                RuleFor(x => x.SSN).Must(x => System.Text.RegularExpressions.Regex.IsMatch(x, @"^(?!\b(\d)\1+-(\d)\1+-(\d)\1+\b)(?!123-45-6789|219-09-9999|078-05-1120)(?!666|000|9\d{2})\d{3}(?!00)\d{2}(?!0{4})\d{4}$"))
                    .WithMessage("The SSN must be valid and contain only numbers.");
            }
        }
    }
}
