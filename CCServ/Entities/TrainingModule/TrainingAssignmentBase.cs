using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.Entities.TrainingModule
{
    /// <summary>
    /// The base elements of a training assignment.
    /// </summary>
    public abstract class TrainingAssignmentBase
    {

        #region Properties

        /// <summary>
        /// Unique Id of this assignment.
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The date/time that this assignment was created.
        /// </summary>
        public virtual DateTime DateAssigned { get; set; }

        /// <summary>
        /// A list of comments.  This is the comment thread for the assignment object.
        /// </summary>
        public virtual IList<AssignmentComment> Comments { get; set; }

        /// <summary>
        /// The person who assigned this assignment.
        /// </summary>
        public virtual Person AssignedBy { get; set; }

        /// <summary>
        /// The person who this assignment is assigned to.  This could be considered the owner.
        /// </summary>
        public virtual Person AssignedTo { get; set; }

        /// <summary>
        /// The date by which this assignment should be completed.
        /// </summary>
        public virtual DateTime CompleteByDate { get; set; }

        #endregion

    }
}
