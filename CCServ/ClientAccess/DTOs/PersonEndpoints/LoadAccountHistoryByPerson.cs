using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace CCServ.ClientAccess.DTOs.PersonEndpoints
{
    /// <summary>
    /// The dto used for the related endpoint.
    /// </summary>
    public class LoadAccountHistoryByPerson : DTOBase
    {

        /// <summary>
        /// The id of the person for whom we want to load account history.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Creates a new DTO.
        /// </summary>
        /// <param name="obj"></param>
        public LoadAccountHistoryByPerson(JObject obj) : base(obj)
        {
        }
    }
}
