using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace CCServ.ClientAccess.DTOs.PersonEndpoints
{
    /// <summary>
    /// The dto used by the related endpoint.
    /// </summary>
    public class LoadPerson : DTOBase
    {

        /// <summary>
        /// The id of the person the client wants to load.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Creates a new DTO.
        /// </summary>
        /// <param name="obj"></param>
        public LoadPerson(JObject obj) : base(obj)
        {
        }
    }
}
