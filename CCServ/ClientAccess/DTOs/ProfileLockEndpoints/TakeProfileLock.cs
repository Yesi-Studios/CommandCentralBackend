using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace CCServ.ClientAccess.DTOs.ProfileLockEndpoints
{
    /// <summary>
    /// The dto for the related endpoint.
    /// </summary>
    public class TakeProfileLock : DTOBase
    {

        /// <summary>
        /// The id of the person for whom the client wants to take a lock.
        /// </summary>
        public Guid PersonId { get; set; }
        
        /// <summary>
        /// Creates a new dto.
        /// </summary>
        /// <param name="obj"></param>
        public TakeProfileLock(JObject obj) : base(obj)
        {
        }
    }
}
