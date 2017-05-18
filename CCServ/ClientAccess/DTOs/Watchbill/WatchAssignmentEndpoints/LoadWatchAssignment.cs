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
    public class LoadWatchAssignment : DTOBase
    {

        /// <summary>
        /// The id of the watch assignment to load.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Creates a new dto.
        /// </summary>
        /// <param name="obj"></param>
        public LoadWatchAssignment(JObject obj) : base(obj)
        {
        }
    }
}
