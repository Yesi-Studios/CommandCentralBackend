using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.Entities.Watchbill
{
    /// <summary>
    /// Describes a single watchbill, which is a collection of watch days, shifts in those days, and inputs.
    /// </summary>
    public class Watchbill
    {

        #region Properties

        /// <summary>
        /// The unique Id of this watchbill.
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The free text title of this watchbill.
        /// </summary>
        public virtual string Title { get; set; }

        /// <summary>
        /// The person who created this watchbill.  This is expected to often be the command watchbill coordinator.
        /// </summary>
        public virtual Person CreatedBy { get; set; }

        /// <summary>
        /// Represents the current state of the watchbill.  Different states should trigger different actions.
        /// </summary>
        public virtual ReferenceLists.Watchbill.WatchbillStatus CurrentState { get; set; }

        /// <summary>
        /// The collection of all the watch days that make up this watchbill.  Together, they should make en entire watchbill but not necessarily an entire month.
        /// </summary>
        public virtual IList<WatchDay> WatchDays { get; set; }

        /// <summary>
        /// Indicates the type of this watchbill.
        /// </summary>
        public virtual ReferenceLists.Watchbill.WatchbillType Type { get; set; }

        /// <summary>
        /// The collection of requirements.  This is how we know who needs to provide inputs and who is available to be on this watchbill.
        /// </summary>
        public virtual IList<WatchInputRequirement> InputRequirements { get; set; }

        /// <summary>
        /// The collection of all the watch inputs given for shifts within this watchbill.
        /// </summary>
        public virtual IList<WatchInput> WatchInputs { get; set; }

        #endregion

    }
}
