using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Email.Models
{
    /// <summary>
    /// The email model that should be sent to the watch assigned email template.
    /// </summary>
    public class UpcomingWatchEmailModel
    {
        /// <summary>
        /// The name of the person to whom this email will be sent.
        /// </summary>
        public string FriendlyName
        {
            get
            {
                return WatchAssignment.PersonAssigned.ToString();
            }
        }

        /// <summary>
        /// The name or title of the watchbill this email relates to.
        /// </summary>
        public string Watchbill
        {
            get
            {
                return WatchAssignment.WatchShift.Watchbill.Title;
            }
        }

        /// <summary>
        /// The watch assignment that is referred to in this email.
        /// </summary>
        public Entities.Watchbill.WatchAssignment WatchAssignment { get; set; }

        /// <summary>
        /// The total number of hours until the watch will occur.
        /// </summary>
        public int Hours
        {
            get
            {
                return (int)Math.Round((WatchAssignment.WatchShift.Range.Start - DateTime.UtcNow).TotalHours);
            }
        }
    }
}
