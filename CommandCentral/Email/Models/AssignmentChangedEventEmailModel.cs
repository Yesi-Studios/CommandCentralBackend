using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Email.Models
{
    /// <summary>
    /// The email model used for the assignment changed event.
    /// </summary>
    public class AssignmentChangedEventEmailModel
    {
        /// <summary>
        /// The change event this email references.
        /// </summary>
        public ChangeEventSystem.ChangeEvents.AssignmentChangedEvent ChangeEvent { get; set; }
    }
}
