using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace CCServ.ClientAccess.DTOs.ReferenceListEndpoints
{
    /// <summary>
    /// The DTO for the related endpoint.
    /// </summary>
    public class DeleteReferenceListItem : DTOBase
    {

        /// <summary>
        /// The Id of the reference list item to delete.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Instructs the service to forcefully delete the item and set any relationships to null.
        /// </summary>
        [Optional(false)]
        public bool ForceDelete { get; set; }

        /// <summary>
        /// Creates a new dto.
        /// </summary>
        /// <param name="obj"></param>
        public DeleteReferenceListItem(JObject obj) : base(obj)
        {
        }
    }
}
