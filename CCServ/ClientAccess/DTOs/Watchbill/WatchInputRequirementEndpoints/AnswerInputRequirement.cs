using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace CCServ.ClientAccess.DTOs.Watchbill.WatchInputRequirementEndpoints
{
    /// <summary>
    /// The dto for the endpoint.
    /// </summary>
    public class AnswerInputRequirement : DTOBase
    {

        /// <summary>
        /// The id of the watchbill on which there is an input requirement.
        /// </summary>
        public Guid WatchbillId { get; set; }

        /// <summary>
        /// The id of the person for whom to answer any input requirements.
        /// </summary>
        public Guid PersonId { get; set; }

        /// <summary>
        /// Creates a new dto.
        /// </summary>
        /// <param name="obj"></param>
        public AnswerInputRequirement(JObject obj) : base(obj)
        {
        }
    }
}
