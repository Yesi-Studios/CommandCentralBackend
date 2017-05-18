using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace CCServ.ClientAccess.DTOs.Watchbill.WatchbillEndpoints
{
    /// <summary>
    /// The dto for the related endpoint.
    /// </summary>
    public class LoadWatchbill : DTOBase
    {

        /// <summary>
        /// The Id of the watchbill the client wants to load.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Instructs the service to load the watchbill then randomly assign watches to the watchbill.  These assigned watches are not comitted to the database.
        /// </summary>
        [Optional(false)]
        public bool DoPopulation { get; set; }

        /// <summary>
        /// Creates a new DTO.
        /// </summary>
        /// <param name="obj"></param>
        public LoadWatchbill(JObject obj) : base(obj)
        {
        }
    }
}
