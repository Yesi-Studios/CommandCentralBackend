using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace CCServ.ClientAccess.DTOs.Watchbill.WatchInputEndpoints
{
    /// <summary>
    /// The dto for the related endpoint.
    /// </summary>
    public class CreateWatchInputs : DTOBase
    {

        /// <summary>
        /// The watch inputs the client wants to create.  These are a collection of the watch input dto object.
        /// </summary>
        public List<WatchInputDTO> WatchInputs { get; set; }

        /// <summary>
        /// Creates a new dto.
        /// </summary>
        /// <param name="obj"></param>
        public CreateWatchInputs(JObject obj) : base(obj)
        {
        }

        /// <summary>
        /// A wrapper which encapsulates all the properties we need to make a watch input.
        /// </summary>
        public class WatchInputDTO
        {
            /// <summary>
            /// The id of the input reason for why the person can'st stand the given shifts.
            /// </summary>
            public Guid InputReasonId { get; set; }

            /// <summary>
            /// The id of the person for whom inputs are being put.
            /// </summary>
            public Guid PersonId { get; set; }

            /// <summary>
            /// The list of the ids of all the shifts to which this input will apply.
            /// </summary>
            public List<Guid> WatchShiftIds { get; set; }
        }
    }
}
