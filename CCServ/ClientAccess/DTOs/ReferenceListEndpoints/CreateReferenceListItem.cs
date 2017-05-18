using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace CCServ.ClientAccess.DTOs.ReferenceListEndpoints
{
    /// <summary>
    /// The dto for the related endpoint.
    /// </summary>
    public class CreateReferenceListItem : DTOBase
    {
        /// <summary>
        /// The desired value for the reference list item.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// The desired description for the reference list item.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The entity name which is the list to add the item to.
        /// </summary>
        public string EntityName { get; set; }

        /// <summary>
        /// Creates a new dto.
        /// </summary>
        /// <param name="obj"></param>
        public CreateReferenceListItem(JObject obj) : base(obj)
        {
        }
    }
}
