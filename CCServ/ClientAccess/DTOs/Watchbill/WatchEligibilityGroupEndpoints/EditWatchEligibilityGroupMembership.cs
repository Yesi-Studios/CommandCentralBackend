using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace CCServ.ClientAccess.DTOs.Watchbill.WatchEligibilityGroupEndpoints
{
    /// <summary>
    /// The dto for the related endpoint.
    /// </summary>
    public class EditWatchEligibilityGroupMembership : DTOBase
    {

        /// <summary>
        /// The id of the eligibility group whose membership we want to modify.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// A list of all those persons' ids who should be in the group when we're done.
        /// </summary>
        public List<Guid> PersonIds { get; set; }

        /// <summary>
        /// Creates a new dto.
        /// </summary>
        /// <param name="obj"></param>
        public EditWatchEligibilityGroupMembership(JObject obj) : base(obj)
        {
        }
    }
}
