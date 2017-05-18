using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using CCServ.Entities.ReferenceLists;
using FluentValidation.Results;

namespace CCServ.ClientAccess.DTOs.ReferenceListEndpoints
{
    /// <summary>
    /// The dto for the related endpoint.
    /// </summary>
    public class UpdateReferenceListItem : DTOBase
    {
        /// <summary>
        /// The item the client wants to update.
        /// </summary>
        public ReferenceListDTO Item { get; set; }

        /// <summary>
        /// Creates a new dto.
        /// </summary>
        /// <param name="obj"></param>
        public UpdateReferenceListItem(JObject obj) : base(obj)
        {
        }

        /// <summary>
        /// Encapsulates the values needed for an update.
        /// </summary>
        public class ReferenceListDTO
        {
            /// <summary>
            /// The Id of the reference list to be updated.
            /// </summary>
            public Guid Id { get; set; }

            /// <summary>
            /// The value to set the reference list to.
            /// </summary>
            public string Value { get; set; }

            /// <summary>
            /// The new description of the reference list.
            /// </summary>
            public string Description { get; set; }
        }
    }
}
