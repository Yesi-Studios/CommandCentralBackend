using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;
using CCServ.CustomTypes;

namespace CCServ.Email.Models
{
    /// <summary>
    /// The email model sent to the muster email template.
    /// </summary>
    public class MusterReportEmailModel
    {
        /*/// <summary>
        /// The link to the muster report.
        /// </summary>
        public string ReportISODateString
        {
            get
            {
                return MusterDateTime.ToString("s", CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// The link to the report which is built using the date string.
        /// </summary>
        public string ReportLink
        {
            get
            {
                return "https://commandcentral/#/muster/archive/" + ReportISODateString;
            }
        }*/

        /// <summary>
        /// The person who generated this muster report.
        /// </summary>
        public Entities.Person Creator { get; set; }

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
                return ServiceManagement.ServiceManager.CurrentConfigState.MusterRolloverTime;
            }
        }

        /*/// <summary>
        /// The date to display.
        /// </summary>
        public string DisplayDay
        {
            get
            {
                return MusterDateTime.ToString("D",
                  CultureInfo.CreateSpecificCulture("en-US"));
            }
        }*/

        /// <summary>
        /// The list of muster records in this report.
        /// </summary>
        public List<Entities.MusterRecord> Records { get; set; }

    }
}
