using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.DTOs
{
    /// <summary>
    /// A DTO for a basic person
    /// </summary>
    public class BasicPersonDTO
    {
        /// <summary>
        /// The Id of the person.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The person's friendly name.
        /// </summary>
        public string FriendlyName { get; set; }
    }
}
