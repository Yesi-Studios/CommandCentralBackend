using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace CCServ.ClientAccess.DTOs.CommentEndpoints
{
    /// <summary>
    /// DTO for the named endpoint
    /// </summary>
    public class LoadComment : DTOBase
    {
        /// <summary>
        /// The id of the comment you want to load.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Creates a new dto.
        /// </summary>
        /// <param name="obj"></param>
        public LoadComment(JObject obj) : base(obj)
        {
        }
    }
}
