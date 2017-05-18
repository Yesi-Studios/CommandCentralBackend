using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using AtwoodUtils;
using FluentValidation;

namespace CCServ.ClientAccess.DTOs.Watchbill.WatchbillEndpoints
{
    /// <summary>
    /// The dto for the related endpoint.
    /// </summary>
    public class CreateWatchbill : DTOBase
    {

        /// <summary>
        /// The title the client wants to set on the watchbill.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The id of the eligibility group that will be assigned to the new watchbill.
        /// </summary>
        public Guid EligibilityGroupId { get; set; }

        /// <summary>
        /// The time range which represents the min and mex dates of the watchbill.
        /// </summary>
        public TimeRange Range { get; set; }

        /// <summary>
        /// Creates a new dto.
        /// </summary>
        /// <param name="obj"></param>
        public CreateWatchbill(JObject obj) : base(obj)
        {
        }

        class Validator : AbstractValidator<CreateWatchbill>
        {
            public Validator()
            {
                RuleFor(x => x.EligibilityGroupId).Must(x =>
                {
                    return Entities.ReferenceLists.Watchbill.WatchEligibilityGroups.AllWatchEligibilityGroups.Any(y => y.Id == x);
                })
                .WithMessage("The eligibility group did not exist.");
            }
        }
    }
}
