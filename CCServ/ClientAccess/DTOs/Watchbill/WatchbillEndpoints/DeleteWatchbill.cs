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
    public class DeleteWatchbill : DTOBase
    {
        /// <summary>
        /// The id of the watchbill we want to delete.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Creates a new dto.
        /// </summary>
        /// <param name="obj"></param>
        public DeleteWatchbill(JObject obj) : base(obj)
        {
        }
    }
}
