using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace CCServ.ClientAccess.DTOs.NewsItemEndpoints
{
    /// <summary>
    /// The DTO for the related endpoint.
    /// </summary>
    public class LoadNewsItem : DTOBase
    {

        /// <summary>
        /// The Id of the news item the client wants to load.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Creates a new DTO.
        /// </summary>
        /// <param name="obj"></param>
        public LoadNewsItem(JObject obj) : base(obj)
        {
        }
    }
}
