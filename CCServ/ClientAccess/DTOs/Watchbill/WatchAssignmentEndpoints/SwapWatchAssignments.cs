using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using FluentValidation;

namespace CCServ.ClientAccess.DTOs.Watchbill.WatchAssignmentEndpoints
{
    /// <summary>
    /// The dto for the related endpoint.
    /// </summary>
    public class SwapWatchAssignments : DTOBase
    {
        /// <summary>
        /// The id of the first watch assignment the client wants to swap.
        /// </summary>
        public Guid Id1 { get; set; }

        /// <summary>
        /// The id of the second watch assignment the client wants to swap.
        /// </summary>
        public Guid Id2 { get; set; }

        /// <summary>
        /// Creates a new dto.
        /// </summary>
        /// <param name="obj"></param>
        public SwapWatchAssignments(JObject obj) : base(obj)
        {
        }

        class Validator : AbstractValidator<SwapWatchAssignments>
        {
            public Validator()
            {
                Custom(instance =>
                {
                    if (instance.Id1 == instance.Id2)
                        return new FluentValidation.Results.ValidationFailure(nameof(instance.Id1), "Both Ids may not be equal.");

                    return null;
                });
            }
        }
    }
}
