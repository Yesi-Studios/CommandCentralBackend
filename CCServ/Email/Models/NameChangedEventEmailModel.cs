using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Email.Models
{
    /// <summary>
    /// The email model to be used for a name changed event.
    /// </summary>
    public class NameChangedEventEmailModel
    {
        /// <summary>
        /// The change event referenced by the email.
        /// </summary>
        public ChangeEventSystem.ChangeEvents.NameChangedEvent ChangeEvent { get; set; }
    }
}
