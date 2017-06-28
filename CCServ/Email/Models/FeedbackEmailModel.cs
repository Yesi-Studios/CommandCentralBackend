using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Email.Models
{
    /// <summary>
    /// The email model for a feedback email.
    /// </summary>
    public class FeedbackEmailModel
    {
        /// <summary>
        /// The title of the feedback message.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The body of the feedback message.
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// The person's name that submitted the feedback.
        /// </summary>
        public string FriendlyName { get; set; }
    }
}
