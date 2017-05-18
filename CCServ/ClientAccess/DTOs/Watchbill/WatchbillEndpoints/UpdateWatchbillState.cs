using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace CCServ.ClientAccess.DTOs.Watchbill.WatchbillEndpoints
{
    /// <summary>
    /// The dto used for the related endpoint.
    /// </summary>
    public class UpdateWatchbillState : DTOBase
    {

        /// <summary>
        /// The Id of the watchbill to update.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The id of the watchbill state to set the watchbill to.
        /// </summary>
        public Guid StateId { get; set; }

        /// <summary>
        /// Creates a new dto.
        /// </summary>
        /// <param name="obj"></param>
        public UpdateWatchbillState(JObject obj) : base(obj)
        {
        }
    }
}
