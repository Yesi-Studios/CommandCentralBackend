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
    public class LoadWatchInputs : DTOBase
    {

        /// <summary>
        /// The id of the watchbill for which we want to load all inputs.
        /// </summary>
        public Guid WatchbillId { get; set; }

        /// <summary>
        /// Creates a new dto.
        /// </summary>
        /// <param name="obj"></param>
        public LoadWatchInputs(JObject obj) : base(obj)
        {
        }
    }
}
