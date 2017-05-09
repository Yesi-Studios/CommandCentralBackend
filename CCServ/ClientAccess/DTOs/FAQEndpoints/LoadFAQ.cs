using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace CCServ.ClientAccess.DTOs.FAQEndpoints
{
    /// <summary>
    /// The dto for the related endpoint.
    /// </summary>
    public class LoadFAQ : DTOBase
    {
        /// <summary>
        /// The id of the faq the client wants to load.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Creates a new DTO.
        /// </summary>
        /// <param name="obj"></param>
        public LoadFAQ(JObject obj) : base(obj)
        {
        }
    }
}
