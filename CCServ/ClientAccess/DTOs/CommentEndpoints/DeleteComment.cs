using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace CCServ.ClientAccess.DTOs.CommentEndpoints
{
    /// <summary>
    /// The dto for the related endpoint.
    /// </summary>
    public class DeleteComment : DTOBase
    {
        /// <summary>
        /// The id of the comment the client wants to delete.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Creates a new dto.
        /// </summary>
        /// <param name="obj"></param>
        public DeleteComment(JObject obj) : base(obj)
        {
        }
    }
}
