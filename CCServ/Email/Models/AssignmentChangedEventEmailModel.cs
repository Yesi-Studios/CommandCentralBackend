using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.Email.Models
{
    /// <summary>
    /// The email model used for the assignement changed event.
    /// </summary>
    public class AssignmentChangedEventEmailModel
    {
        /// <summary>
        /// The person's old assignment.
        /// </summary>
        public AssignmentContainer OldAssignment { get; set; }

        /// <summary>
        /// The person's new assignment.
        /// </summary>
        public AssignmentContainer NewAssignment { get; set; }

        /// <summary>
        /// The Id of the person whose profile changed.
        /// </summary>
        public Guid PersonId { get; set; }

        /// <summary>
        /// The friendly name of the person whose profile changed.
        /// </summary>
        public string FriendlyName { get; set; }

        /// <summary>
        /// Intended to wrap a single assignemnt which is a division/department/command chain.
        /// </summary>
        public class AssignmentContainer
        {
            /// <summary>
            /// Division
            /// </summary>
            public string Division { get; set; }

            /// <summary>
            /// Department
            /// </summary>
            public string Department { get; set; }

            /// <summary>
            /// Command
            /// </summary>
            public string Command { get; set; }
        }
    }
}
