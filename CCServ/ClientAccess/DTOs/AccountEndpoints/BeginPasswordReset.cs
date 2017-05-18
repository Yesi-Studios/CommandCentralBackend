using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using FluentValidation;

namespace CCServ.ClientAccess.DTOs.AccountEndpoints
{
    /// <summary>
    /// The dto used for the respective endpoint.
    /// </summary>
    public class BeginPasswordReset : DTOBase
    {
        /// <summary>
        /// The .mil email address of the client.
        /// </summary>
        [Description("The .mil email address of the client.")]
        public string Email { get; set; }

        /// <summary>
        /// The SSN of the client.  This is expected to not have any characters except numbers.
        /// </summary>
        [Description("The SSN of the client.  This is expected to not have any characters except numbers.")]
        public string SSN { get; set; }

        /// <summary>
        /// The link to put in the email which the client will click on to continue the password reset process.
        /// </summary>
        [Description("The link to put in the email which the client will click on to continue the password reset process.")]
        public string ContinueLink { get; set; }

        /// <summary>
        /// Creates a new dto.
        /// </summary>
        /// <param name="obj"></param>
        public BeginPasswordReset(JObject obj) : base(obj)
        {
            
        }

        class Validator : AbstractValidator<BeginPasswordReset>
        {
            public Validator()
            {
                RuleFor(x => x.Email).Must(x =>
                {
                    try
                    {
                        var email = new System.Net.Mail.MailAddress(x);

                        return email.Host == "mail.mil";
                    }
                    catch
                    {
                        return false;
                    }
                })
                .WithMessage("Your email address must be a valid DoD .mil email address.");

                RuleFor(x => x.SSN).Must(x => System.Text.RegularExpressions.Regex.IsMatch(x, @"^(?!\b(\d)\1+-(\d)\1+-(\d)\1+\b)(?!123-45-6789|219-09-9999|078-05-1120)(?!666|000|9\d{2})\d{3}(?!00)\d{2}(?!0{4})\d{4}$"))
                    .WithMessage("The SSN must be valid and contain only numbers.");

                RuleFor(x => x.ContinueLink).Must(x => Uri.IsWellFormedUriString(x, UriKind.Absolute))
                    .WithMessage("The continue link you sent was not a valid URI.");
            }
        }
    }
}
