using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;
using CommandCentral.CustomTypes;
using CommandCentral.Entities;
using CommandCentral.Entities.ReferenceLists;
using CommandCentral.Entities.Muster;

namespace CommandCentral.Email.Models
{
    /// <summary>
    /// The email model sent to the muster email template.
    /// </summary>
    public class MusterReportEmailModel
    {
        /// <summary>
        /// The person who generated this muster report.
        /// </summary>
        public Person Creator { get; set; }

        /// <summary>
        /// The muster period for which we're sending this email.
        /// </summary>
        public MusterPeriod MusterPeriod { get; set; }

        /// <summary>
        /// Returns all string muster statuses.
        /// </summary>
        public List<string> MusterStatuses
        {
            get
            {
                return ReferenceListHelper<MusterStatus>.All().Select(x => x.Value).OrderBy(x => x).ToList();
            }
        }

        /// <summary>
        /// Returns all string duty statuses.
        /// </summary>
        public List<string> DutyStatuses
        {
            get
            {
                return ReferenceListHelper<DutyStatus>.All().Select(x => x.Value).OrderBy(x => x).ToList();
            }
        }
    }
}
