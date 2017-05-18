using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace CCServ.ClientAccess.DTOs.Watchbill.WatchInputEndpoints
{
    /// <summary>
    /// The dto related to the endpoint.
    /// </summary>
    public class DeleteWatchInput : DTOBase
    {

        /// <summary>
        /// The id of the watch input you wish to delete.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Creates a new dto.
        /// </summary>
        /// <param name="obj"></param>
        public DeleteWatchInput(JObject obj) : base(obj)
        {
        }
    }
}
