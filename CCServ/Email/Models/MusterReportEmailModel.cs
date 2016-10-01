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
        public string ReportISODateString
        {
            get
            {
                return MusterDateTime.ToString("s", CultureInfo.InvariantCulture);
            }
        }

        public string ReportLink
        {
            get
            {
                return "https://commandcentral/#/muster/archive/" + ReportISODateString;
            }
        }

        public string CreatorName { get; set; }

        private DateTime MusterDateTime { get; set; }

        /// <summary>
        /// The time that the muster rolls over.
        /// </summary>
        public Time RollOverTime { get; set; }

        /// <summary>
        /// The date to display.
        /// </summary>
        public string DisplayDay
        {
            get
            {
                return MusterDateTime.ToString("D",
                  CultureInfo.CreateSpecificCulture("en-US"));
            }
        }

        public List<Entities.MusterRecord> Records { get; set; }

        private List<MusterGroupContainer> Containers { get; set; }

        public string ReportText
        {
            get
            {
                var strs = Containers.Select(x => x.ToString());

                return strs.Aggregate((current, newElement) => current + "<p>" + newElement + "</p>");
            }
        }

        public MusterReportEmailModel(IEnumerable<Entities.MusterRecord> records, Entities.Person creator, DateTime musterDateTime)
        {
            MusterDateTime = musterDateTime;

            if (creator == null)
                CreatorName = "SYSTEM";
            else
                CreatorName = creator.ToString();

            if (records == null || !records.Any())
                throw new ArgumentException("The 'records' parameter must contain records.");

            var containers = new List<MusterGroupContainer>();

            foreach (var record in records)
            {

                if (record.MusterDate.Date != musterDateTime.Date)
                    throw new ArgumentException("One or more records were from different dates.  A muster report may only be built from the records from the same time frame.");

                var container = containers.FirstOrDefault(x => x.GroupTitle.SafeEquals(record.DutyStatus));

                if (container == null)
                {
                    containers.Add(new MusterGroupContainer
                    {
                        GroupTitle = record.DutyStatus,
                        Mustered = String.Equals(record.MusterStatus, Entities.ReferenceLists.MusterStatuses.UA.ToString()) ? 0 : 1,
                        Total = 1
                    });
                }
                else
                {
                    container.Total++;
                    if (!String.Equals(record.MusterStatus, Entities.ReferenceLists.MusterStatuses.UA.ToString()))
                        container.Mustered++;
                }
            }

            //Now, let's make a "total" container.
            containers.Insert(0, new MusterGroupContainer
            {
                GroupTitle = "Total",
                Mustered = containers.Sum(x => x.Mustered),
                Total = containers.Sum(x => x.Total)
            });

            Containers = containers;
        }

        public class MusterGroupContainer
        {
            public string GroupTitle { get; set; }
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
                return "{0} : {1}% ({2}/{3})".FormatS(GroupTitle, Percentage, Mustered, Total);
            }
        }

    }
}
