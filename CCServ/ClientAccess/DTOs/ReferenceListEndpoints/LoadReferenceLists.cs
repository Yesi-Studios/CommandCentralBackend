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
    public class LoadReferenceLists : DTOBase
    {
        
        /// <summary>
        /// The names of the entities for which the client wants to load the reference lists.
        /// </summary>
        public List<string> EntityNames { get; set; }

        /// <summary>
        /// Instructs the service to only return editable reference lists. Optiona.  Default = false.
        /// </summary>
        [Optional(false)]
        public bool Editable { get; set; }

        /// <summary>
        /// Instructs the service to load all entities included in the entity names collection (false) OR to exclude all those included in that collection.  Optional.  Default = false.
        /// </summary>
        [Optional(false)]
        public bool Exclude { get; set; }

        /// <summary>
        /// Creates a new dto.
        /// </summary>
        /// <param name="obj"></param>
        public LoadReferenceLists(JObject obj) : base(obj)
        {
        }
    }
}
