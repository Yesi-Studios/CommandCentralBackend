using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.ChangeEventSystem.ChangeEventArgs
{
    /// <summary>
    /// The arguments that are used during a change name event.
    /// </summary>
    public class NameChangedEventArgs : ChangeEventArgsBase
    {
        /// <summary>
        /// The name of the person prior to the event.
        /// </summary>
        public string OldName { get; set; }

        /// <summary>
        /// The name of the person after the event.
        /// </summary>
        public string NewName { get; set; }

        /// <summary>
        /// The id of the person whose name changed.
        /// </summary>
        public string PersonId { get; set; }
    }
}
