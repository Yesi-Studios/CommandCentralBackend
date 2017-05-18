using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace CCServ.ClientAccess.DTOs.Watchbill.WatchAssignmentEndpoints
{
    /// <summary>
    /// The dto for the related endpoint.
    /// </summary>
    public class CreateWatchAssignments : DTOBase
    {
        /// <summary>
        /// A collection of watch assignment DTOs representing the watch assignments to create.
        /// </summary>
        public List<WatchAssignmentDTO> WatchAssignments { get; set; }

        /// <summary>
        /// The id of the watchbill to which we want to submit these watch assignments.
        /// </summary>
        public Guid WatchbillId { get; set; }

        /// <summary>
        /// Creates a new dto.
        /// </summary>
        /// <param name="obj"></param>
        public CreateWatchAssignments(JObject obj) : base(obj)
        {
        }

        /// <summary>
        /// A wrapper for the essential elements of information we need for a watch assignment in this endpoint.
        /// </summary>
        public class WatchAssignmentDTO
        {
            /// <summary>
            /// The person the client wants to assign to the given shift.
            /// </summary>
            public Guid PersonAssignedId { get; set; }

            /// <summary>
            /// The id of the watch shift to which to tie this assignment.
            /// </summary>
            public Guid WatchShiftId { get; set; }
        }
    }
}
