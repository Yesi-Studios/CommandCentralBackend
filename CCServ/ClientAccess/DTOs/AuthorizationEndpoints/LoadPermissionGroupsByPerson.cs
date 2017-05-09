using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace CCServ.ClientAccess.DTOs.AuthorizationEndpoints
{
    /// <summary>
    /// The dto for the related endpoint.
    /// </summary>
    public class LoadPermissionGroupsByPerson : DTOBase
    {

        /// <summary>
        /// The id of the person for whom the client wasnts to load permissions.
        /// </summary>
        public Guid PersonId { get; set; }

        /// <summary>
        /// Creates a new dto.
        /// </summary>
        /// <param name="obj"></param>
        public LoadPermissionGroupsByPerson(JObject obj) : base(obj)
        {
        }
    }
}
