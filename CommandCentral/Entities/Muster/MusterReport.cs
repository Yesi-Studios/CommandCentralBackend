using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandCentral.Entities;

namespace CommandCentral.Entities.Muster
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
        /// The time at which the muster was supposed to roll over.  
        /// <para/>
        /// Note: the muster can be forced to roll over prior to this time.
        /// </summary>
        public AtwoodUtils.Time RolloverTime { get; set; }

        /// <summary>
        /// The person that caused this report to be created.  If null, then the system created the report at muster rollover otherwise, this value shows who forced a finalize event.
        /// </summary>
        public Person ReportGeneratedBy { get; set; }

        /// <summary>
        /// Shows the date/time this report was generated.
        /// </summary>
        public DateTime TimeGenerated { get; set; }

        /// <summary>
        /// don't know
        /// </summary>
        public object Report { get; set; }


        #endregion

        #region Helper Methods

        /// <summary>
        /// Ok, we're going to create the current muster report.  This involves getting every person, counting them up, and putting them in a big ole structure.
        /// </summary>
        /// <param name="personGeneratedBy">The person who initiated this muster report creation.  Null means the system generated it.</param>
        /// <returns></returns>
        public static MusterReport GenerateCurrentMusterReport(Person reportGeneratedBy)
        {

            MusterReport report = new MusterReport { ReportGeneratedBy = reportGeneratedBy };
            report.TimeGenerated = DateTime.Now;
            report.Id = Guid.NewGuid();

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            {
                //Get the musterable persons.  Their muster records can be found on their profiles.
                var persons = MusterRecord.GetMusterablePersons(session);

                //Now we need to find out what command, department, and division we're working with.  And then group by muster statuses and then build the DTO.
                var result = persons.GroupBy(x => x.Command).ToDictionary(x => x.Key, 
                                x => x.ToList().GroupBy(y => y.Department).ToDictionary(y => y.Key, 
                                    y => y.ToList().GroupBy(z => z.Division).ToDictionary(z => z.Key, 
                                        z => z.ToList().GroupBy(a => a.CurrentMusterStatus.MusterStatus).ToDictionary(a => a.Key, 
                                            a => a.ToList()
                                                .Select(b =>
                                                {
                                                    return new
                                                    {
                                                        b.Id,
                                                        b.FirstName,
                                                        b.LastName,
                                                        b.MiddleName,
                                                        FriendlyName = b.ToString(),
                                                        b.Paygrade,
                                                        b.Designation,
                                                        b.CurrentMusterStatus,
                                                        b.UIC,
                                                        b.DutyStatus,
                                                    };
                                                })))));
                    

            }

            return report;
        }


        #endregion

    }
}
