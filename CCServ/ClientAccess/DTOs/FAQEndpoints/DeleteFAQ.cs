using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace CCServ.ClientAccess.DTOs.FAQEndpoints
{
    /// <summary>
    /// The DTO used for the related endpoint.
    /// </summary>
    public class DeleteFAQ : DTOBase
    {

        /// <summary>
        /// The Id of the FAQ the client wants to delete.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Creates a new DTO.
        /// </summary>
        /// <param name="obj"></param>
        public DeleteFAQ(JObject obj) : base(obj)
        {
        }
    }
}
