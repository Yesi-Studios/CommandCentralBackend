using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentralHost
{
    /// <summary>
    /// A dialogue option.
    /// </summary>
    public class DialogueOption
    {
        /// <summary>
        /// The option text to display to the client.
        /// </summary>
        public string OptionText { get; set; }

        /// <summary>
        /// The method that will be run if this option is selected.
        /// </summary>
        public Action Method { get; set; }

        /// <summary>
        /// The method that will run to determine if this option should be shown to the client.
        /// </summary>
        public Func<bool> DisplayCriteria { get; set; }
    }
}
