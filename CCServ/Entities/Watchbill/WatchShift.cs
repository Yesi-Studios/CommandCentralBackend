using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.Entities.Watchbill
{
    /// <summary>
    /// A watch shift represents a single watch, who is assigned to it, and for what day it is as well as from one time to what time.  And some other things.
    /// </summary>
    public class WatchShift
    {

        #region Properties

        /// <summary>
        /// The unique Id of this watchshift.
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The watch day that owns this watch shift.
        /// </summary>
        public virtual WatchDay WatchDay { get; set; }

        /// <summary>
        /// A free text field allowing for this shift to be given a title.
        /// </summary>
        public virtual string Title { get; set; }

        /// <summary>
        /// The date time at which this shift begins.
        /// </summary>
        public virtual DateTime From { get; set; }

        /// <summary>
        /// The date time at which this shift ends.
        /// </summary>
        public virtual DateTime To { get; set; }

        /// <summary>
        /// The points assigned to this shift.  Completion of this shift will grant the member this many points.
        /// </summary>
        public virtual int Points { get; set; }

        /// <summary>
        /// The watch inputs that have been given for this shift.  This is all the persons that have said they can not stand this shift and their given reasons.
        /// </summary>
        public virtual IList<WatchInput> WatchInputs { get; set; }

        /// <summary>
        /// The list of all the assignments for this shift.  Only one assignment should be considered the current assignment while the rest should be only historical.
        /// <para />
        /// An empty collection here indicates this shift has not yet been assigned.
        /// </summary>
        public virtual IList<WatchAssignment> WatchAssignments { get; set; }

        /// <summary>
        /// Indicates the type of this shift:  Is it JOOD, OOD, etc.
        /// </summary>
        public virtual ReferenceLists.Watchbill.WatchShiftType ShiftType { get; set; }





        #endregion

    }
}
