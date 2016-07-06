using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Mapping;

namespace CommandCentral.Entities
{
    /// <summary>
    /// Defines a single muster report which is the report that goes our when muster has been finalized for a given day.
    /// <para/>
    /// We could build this information every time someone asked for it by just querying back over the database but I figure storage is cheaper than CPU time so I may as well store the reports after we make them.
    /// </summary>
    public class MusterReport
    {

        #region Properties

        /// <summary>
        /// The unique Id for this Muster Report
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The day of the year for which this muster report was created.
        /// </summary>
        public virtual int MusterDayOfYear { get; set; }

        /// <summary>
        /// The year for which this muster report was created.
        /// </summary>
        public virtual int MusterYear { get; set; }

        /// <summary>
        /// The time at which the muster was supposed to roll over.  
        /// <para/>
        /// Note: the muster can be forced to roll over prior to this time.
        /// </summary>
        public virtual AtwoodUtils.Time RolloverTime { get; set; }

        /// <summary>
        /// The person that caused this report to be created.  If null, then the system created the report at muster rollover otherwise, this value shows who forced a finalize event.
        /// </summary>
        public virtual Person ReportGeneratedBy { get; set; }

        /// <summary>
        /// Shows the date/time this report was generated.
        /// </summary>
        public virtual DateTime TimeGenerated { get; set; }

        #endregion

        #region Helper Methods

        public static MusterReport GenerateCurrentMusterReport()
        {

        }


        #endregion

        /// <summary>
        /// Maps a muster report to the database.
        /// </summary>
        public class MusterReportMapping : ClassMap<MusterReport>
        {
            /// <summary>
            /// Maps a muster report to the database.
            /// </summary>
            public MusterReportMapping()
            {
                Id(x => x.Id).GeneratedBy.Guid();

                Map(x => x.MusterDayOfYear);
                Map(x => x.MusterYear);
            }
        }


    }
}
