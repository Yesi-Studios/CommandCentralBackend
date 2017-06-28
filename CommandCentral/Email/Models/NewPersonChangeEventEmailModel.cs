using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Email.Models
{
    /// <summary>
    /// The email model used for the new person event.
    /// </summary>
    public class NewPersonChangeEventEmailModel
    {
        /// <summary>
        /// The change event this email references.
        /// </summary>
        public ChangeEventSystem.ChangeEvents.NewPersonChangeEvent ChangeEvent { get; set; }
    }
}
