using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.Entities.TrainingModule
{
    /// <summary>
    /// A comment on an assignment, part of the comment thread.
    /// </summary>
    public class AssignmentComment : CommentBase
    {
        /// <summary>
        /// The assignment to which this comment applies.
        /// </summary>
        public Assignment Assignment { get; set; }
    }
}
