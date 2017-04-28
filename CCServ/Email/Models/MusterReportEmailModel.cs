using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;
using CCServ.CustomTypes;
using CCServ.Entities;

namespace CCServ.Email.Models
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
        /// The date time of the muster.
        /// </summary>
        public DateTime MusterDateTime { get; set; }

        /// <summary>
        /// The time that the muster rolls over.
        /// </summary>
        public Time RollOverTime
        {
            get
            {
                return MusterRecord.RolloverTime;
            }
        }

        /// <summary>
        /// The list of muster records in this report.
        /// </summary>
        public List<MusterRecord> Records { get; set; }

        /// <summary>
        /// Returns all string muster statuses.
        /// </summary>
        public List<string> MusterStatuses
        {
            get
            {
                return Entities.ReferenceLists.MusterStatuses.AllMusterStatuses.Select(x => x.Value).OrderBy(x => x).ToList();
            }
        }

        /// <summary>
        /// Returns all string duty statuses.
        /// </summary>
        public List<string> DutyStatuses
        {
            get
            {
                return Entities.ReferenceLists.DutyStatuses.AllDutyStatuses.Select(x => x.Value).OrderBy(x => x).ToList();
            }
        }

        /// <summary>
        /// Returns the total records with the given muster status.
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public List<MusterRecord> GetRecordsWithMusterStatus(string status)
        {
            return Records.Where(x => x.MusterStatus.SafeEquals(status)).ToList();
        }

        /// <summary>
        /// Returns the total records with the given duty status.
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public List<MusterRecord> GetRecordsWithDutyStatus(string status)
        {
            return Records.Where(x => x.DutyStatus.SafeEquals(status)).ToList();
        }

    }
}
