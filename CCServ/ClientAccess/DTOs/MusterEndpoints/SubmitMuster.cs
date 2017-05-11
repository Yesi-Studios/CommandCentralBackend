using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using FluentValidation;

namespace CCServ.ClientAccess.DTOs.MusterEndpoints
{
    /// <summary>
    /// The dto used for the related endpoint.
    /// </summary>
    public class SubmitMuster : DTOBase
    {

        /// <summary>
        /// The muster submissions.  This is a dictionary.  The key is the Id of the person you want to muster.  The value is an object in the form {MusterStatusId, Remarks}.
        /// </summary>
        public Dictionary<Guid, MusterSubmission> MusterSubmissions { get; set; }

        /// <summary>
        /// Creates a new DTO.
        /// </summary>
        /// <param name="obj"></param>
        public SubmitMuster(JObject obj) : base(obj)
        {
        }

        /// <summary>
        /// A container to hold the muster submission.
        /// </summary>
        public class MusterSubmission
        {
            /// <summary>
            /// The Id of the muster status we want to muster the person as.
            /// </summary>
            public Guid MusterStatusId { get; set; }

            /// <summary>
            /// Any remarks to insert on this person's muster.
            /// </summary>
            public string Remarks { get; set; }
        }

        class Validator : AbstractValidator<SubmitMuster>
        {
            public Validator()
            {
                RuleForEach(x => x.MusterSubmissions).Must((instance, element) =>
                {
                    if (!String.IsNullOrWhiteSpace(element.Value.Remarks) && element.Value.Remarks.Length > 200)
                        return false;

                    return true;
                })
                .WithMessage("Remarks may only be up to 200 characters.");
            }
        }
    }
}
