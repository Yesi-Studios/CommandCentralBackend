using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CCServ.Entities;

namespace CCServ.Entities.Muster
{
    /// <summary>
    /// Defines a single muster report which is the report that goes our when muster has been finalized for a given day.
    /// </summary>
    public class MusterReport
    {

        #region Properties

        /// <summary>
        /// The unique Id for this Muster Report
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The day of the year for which this muster report was created.
        /// </summary>
        public int MusterDayOfYear { get; set; }

        /// <summary>
        /// The year for which this muster report was created.
        /// </summary>
        public int MusterYear { get; set; }

        /// <summary>
        /// The time that the muster rolls over.
        /// </summary>
        public AtwoodUtils.Time RollOverTime { get; set; }

        /// <summary>
        /// The person that caused this report to be created.  If null, then the system created the report at muster rollover otherwise, this value shows who forced a finalize event.
        /// </summary>
        public Person ReportGeneratedBy { get; set; }

        /// <summary>
        /// Shows the date/time this report was generated.
        /// </summary>
        public DateTime TimeGenerated { get; set; }

        /// <summary>
        /// The list of the muster records contained in this report.
        /// </summary>
        public IList<MusterRecord> Records { get; set; }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Generates a muster report by validating that their are no duplicate records for a musteree and by ensuring that all records are from the same day.
        /// <para />
        /// Then this information is placed into a muster record and returned.
        /// </summary>
        /// <param name="records"></param>
        /// <param name="person"></param>
        /// <returns></returns>
        public static MusterReport BuildReport(IList<MusterRecord> records, Person person)
        {
            if (records == null || !records.Any())
                throw new ArgumentException("The 'records' parameter must contain records.");

            //First, let's do some validation and ensure that all the records are from the same day and there are no duplicates.
            //To determine the day, we'll just take the first record and assume that is the day we must be on.
            int year = records.First().MusterYear;
            int day = records.First().MusterDayOfYear;

            if (records.Any(x => x.MusterDayOfYear != day || x.MusterYear != year))
                throw new ArgumentException("One or more records were from different days or years.  A muster report may only be built from the records from the same time frame.");

            //Now let's also make sure that we don't have records for one person twice - duplicates.
            if (records.GroupBy(x => x.Musteree.Id).Any(x => x.Count() != 1))
                throw new ArgumentException("The 'records' parameter may not contain records in which a person is mustered twice.");

            var report = new MusterReport
            {
                Id = Guid.NewGuid(),
                MusterDayOfYear = day,
                Records = records,
                MusterYear = year,
                ReportGeneratedBy = person,
                TimeGenerated = DateTime.Now
            };

            return report;
        }


        #endregion

    }
}
