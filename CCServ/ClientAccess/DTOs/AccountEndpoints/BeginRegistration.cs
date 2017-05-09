using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation.Results;
using Newtonsoft.Json.Linq;
using FluentValidation;
using System.ComponentModel;

namespace CCServ.ClientAccess.DTOs.AccountEndpoints
{
    /// <summary>
    /// The dto to be used by the respective endpoint.
    /// </summary>
    public class BeginRegistration : DTOBase
    {
        /// <summary>
        /// This is the link to put in the email for the user to continue the registration process.
        /// </summary>
        [Required]
        [Description("This is the link to put in the email for the user to continue the registration process.")]
        public string ContinueLink { get; set; }

        /// <summary>
        /// The SSN of the user who wants to begin registration of their account.
        /// </summary>
        [Required]
        [Description("The SSN of the user who wants to begin registration of their account.")]
        public string SSN { get; set; }

        /// <summary>
        /// Creates a new dto.
        /// </summary>
        /// <param name="obj"></param>
        public BeginRegistration(JObject obj) : base(obj)
        {
        }

        class Validator : AbstractValidator<BeginRegistration>
        {
            public Validator()
            {
                RuleFor(x => x.ContinueLink).Must(x => Uri.IsWellFormedUriString(x, UriKind.Absolute))
                    .WithMessage("The continue link you sent was not a valid URI.");

                RuleFor(x => x.SSN).Must(x => System.Text.RegularExpressions.Regex.IsMatch(x, @"^(?!\b(\d)\1+-(\d)\1+-(\d)\1+\b)(?!123-45-6789|219-09-9999|078-05-1120)(?!666|000|9\d{2})\d{3}(?!00)\d{2}(?!0{4})\d{4}$"))
                    .WithMessage("The SSN must be valid and contain only numbers.");
            }
        }
    }
}
