using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.Entities.TrainingModule
{
    /// <summary>
    /// Defines an assignment, which is how a requirement gets assigned to a person.
    /// </summary>
    public class Assignment
    {
        /// <summary>
        /// Unique Id of this assignment.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The date/time that this assignment was created.
        /// </summary>
        public DateTime DateAssigned { get; set; }

        /// <summary>
        /// A list of comments.  This is the comment thread for the assignment object.
        /// </summary>
        public IList<AssignmentComment> Comments { get; set; }
        

    }
}
