using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace CCServ.ClientAccess.DTOs.Watchbill.WatchShiftEndpoints
{
    /// <summary>
    /// Dto for endpoint
    /// </summary>
    public class LoadWatchShift : DTOBase
    {
        /// <summary>
        /// The id of the watch shift we want to load.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Creates a new dto.
        /// </summary>
        /// <param name="obj"></param>
        public LoadWatchShift(JObject obj) : base(obj)
        {
        }
    }
}
