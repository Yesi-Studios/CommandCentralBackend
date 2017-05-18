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
    public class ConfirmWatchInput : DTOBase
    {
        /// <summary>
        /// The id of the watch input the client wishes to confirm.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Creates a new dto.
        /// </summary>
        /// <param name="obj"></param>
        public ConfirmWatchInput(JObject obj) : base(obj)
        {
        }
    }
}
