using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.Entities.Watchbill
{
    /// <summary>
    /// A watch assignment which ties a person to a watch shift and indicates if the assignment has been completed, or what status it is in.
    /// </summary>
    public class WatchAssignment
    {

        #region Properties

        /// <summary>
        /// The unique Id of this watch assignment.
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The watch shift that this assignment assigns the person to.
        /// </summary>
        public virtual WatchShift WatchShift { get; set; }

        /// <summary>
        /// The person that this watch assignment assigns to a watch shift.
        /// </summary>
        public virtual Person PersonAssigned { get; set; }

        /// <summary>
        /// The person who assigned the assigned person to the watch shift.  Basically, the person who created this assignment.
        /// </summary>
        public virtual Person AssignedBy { get; set; }

        /// <summary>
        /// The person who acknowledged this watch assignment.  Either the person assigned or someone who did it on their behalf.
        /// </summary>
        public virtual Person AcknowledgedBy { get; set; }
        
        /// <summary>
        /// The datetime at which this assignment was created.
        /// </summary>
        public virtual DateTime DateAssigned { get; set; }

        /// <summary>
        /// The datetime at which a person acknowledged this watch assignment.
        /// </summary>
        public virtual DateTime DateAcknowledged { get; set; }

        /// <summary>
        /// The current state of this watch assignment.
        /// </summary>
        public virtual ReferenceLists.Watchbill.WatchAssignmentState CurrentState { get; set; }

        /// <summary>
        /// If this watch assignment has been superceded (i.e. by a watch swap), this will contain a reference to the watch assignment that supercedes this one.
        /// </summary>
        public virtual WatchAssignment SupercedingWatchAssignment { get; set; }

        #endregion

    }
}
