using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace CCServ.ClientAccess.DTOs.NewsItemEndpoints
{
    /// <summary>
    /// The dto used in the related endpoint.
    /// </summary>
    public class DeleteNewsItem : DTOBase
    {
        /// <summary>
        /// The id of the news item the client wants to delete.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Creates a new DTO.
        /// </summary>
        /// <param name="obj"></param>
        public DeleteNewsItem(JObject obj) : base(obj)
        {
        }
    }
}
