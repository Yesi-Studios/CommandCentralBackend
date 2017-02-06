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

        /// <summary>
        /// The link to the report which is built using the date string.
        /// </summary>
        public string ReportLink
        {
            get
            {
                return "https://commandcentral/#/muster/archive/" + ReportISODateString;
            }
        }

        /// <summary>
        /// The name of the person who generated this email.  Suggest using person.ToString() on the authentication token owner.
        /// </summary>
        public string CreatorName { get; set; }

        /// <summary>
        /// The date time of the muster.
        /// </summary>
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

        /// <summary>
        /// The list of all records involved in this report email.
        /// </summary>
        public List<Entities.MusterRecord> Records { get; set; }

        /// <summary>
        /// The list of all containers in this email.  (These are the "totals" on the email).
        /// </summary>
        private List<MusterGroupContainer> Containers { get; set; }

        /// <summary>
        /// The total text, which is the containers turned into HTML paragraphs.
        /// </summary>
        public string ReportText
        {
            get
            {
                var strs = Containers.Select(x => x.ToString());

                return strs.Aggregate((current, newElement) => current + "<p>" + newElement + "</p>");
            }
        }

        /// <summary>
        /// Takes a number of muster stasuses and builds a muster report email from them.
        /// </summary>
        /// <param name="records"></param>
        /// <param name="creator"></param>
        /// <param name="musterDateTime"></param>
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

            //Now, before we move on to the next part, let's sort the muster containers so that they always have a uniform sorting in the email.
            containers = containers.OrderBy(x => x.GroupTitle).ToList();

            //Let's save the totals so that we're not recalculating them.
            int total = containers.Sum(x => x.Total);
            int totalMustered = containers.Sum(x => x.Mustered);

            //Now, let's make a "total" container.
            containers.Insert(0, new MusterGroupContainer
            {
                GroupTitle = "Total",
                Mustered = totalMustered,
                Total = total
            });

            //We're also going to add an unaccounted for section At the end.
            containers.Add(new MusterGroupContainer
            {
                GroupTitle = "Unaccounted For (UA)",
                Mustered = total - totalMustered,
                Total = total
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
