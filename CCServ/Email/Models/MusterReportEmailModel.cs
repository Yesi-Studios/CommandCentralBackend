using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;

namespace CCServ.Email.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class MusterReportEmailModel
    {
        /// <summary>
        /// The link to the muster report.
        /// </summary>
        public string ReportLink { get; set; }

        public string CreatorName { get; set; }

        /// <summary>
        /// The day of the year for which this muster report was created.
        /// </summary>
        public int MusterDayOfYear { get; set; }

        /// <summary>
        /// The year for which this muster report was created.
        /// </summary>
        public int MusterYear { get; set; }

        private DateTime MusterDateTime { get; set; }

        /// <summary>
        /// The time that the muster rolls over.
        /// </summary>
        public Time RollOverTime { get; set; }

        public string DisplayDay
        {
            get
            {
                return MusterDateTime.ToString("D",
                  CultureInfo.CreateSpecificCulture("en-US"));
            }
        }

        public List<Entities.Muster.MusterRecord> Records { get; set; }

        private List<MusterGroupContainer> Containers { get; set; }

        public string ReportText
        {
            get
            {
                return String.Join("<br>", Containers);
            }
        }

        public MusterReportEmailModel(IEnumerable<Entities.Muster.MusterRecord> records, Entities.Person creator, DateTime musterDateTime)
        {
            MusterDateTime = musterDateTime;

            if (creator == null)
                CreatorName = "SYSTEM";
            else
                CreatorName = creator.ToString();

            if (records == null || !records.Any())
                throw new ArgumentException("The 'records' parameter must contain records.");

            //First, let's do some validation and ensure that all the records are from the same day and there are no duplicates.
            //To determine the day, we'll just take the first record and assume that is the day we must be on.
            MusterYear = records.First().MusterYear;
            MusterDayOfYear = records.First().MusterDayOfYear;

            var containers = new List<MusterGroupContainer>();

            foreach (var record in records)
            {

                if (record.MusterDayOfYear != MusterDayOfYear || record.MusterYear != MusterYear)
                    throw new ArgumentException("One or more records were from different days or years.  A muster report may only be built from the records from the same time frame.");

                var container = containers.FirstOrDefault(x => x.DutyStatus.SafeEquals(record.DutyStatus));

                if (container == null)
                {
                    containers.Add(new MusterGroupContainer
                    {
                        DutyStatus = record.DutyStatus,
                        Mustered = String.Equals(record.MusterStatus, MusterStatuses.UA.ToString()) ? 0 : 1,
                        Total = 1
                    });
                }
                else
                {
                    container.Total++;
                    if (!String.Equals(record.MusterStatus, MusterStatuses.UA.ToString()))
                        container.Mustered++;
                }
            }

            Containers = containers;
        }

        public class MusterGroupContainer
        {
            public string DutyStatus { get; set; }
            public int Total { get; set; }
            public int Mustered { get; set; }
            public double Percentage
            {
                get
                {
                    return Math.Round(((double)Mustered / (double)Total) * 100, 2);
                }
            }

            public override string ToString()
            {
                return "{0} Mustered / {0} Total : {1}% ({2}/{3})".FormatS(DutyStatus, Percentage, Mustered, Total);
            }
        }

    }
}
