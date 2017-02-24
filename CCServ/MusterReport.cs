using CCServ.ClientAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ
{
    /// <summary>
    /// Generates a muster report for a given day and sends the muster report by email.
    /// </summary>
    public class MusterReport
    {

        #region Properties

        /// <summary>
        /// The date for which to build this msuter report.
        /// </summary>
        public DateTime MusterDate { get; set; }

        #endregion

        #region ctors

        /// <summary>
        /// Creates a muster report.  In order to actually generate the report and cause the database loads, call SendReport().
        /// </summary>
        public MusterReport(DateTime musterDate)
        {
            this.MusterDate = musterDate;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Generates and sends a muster report.
        /// </summary>
        /// <param name="token">The message token representing the request that caused the report to be generated.  If null, the system generates the report.</param>
        public void SendReport(MessageToken token = null)
        {
            Email.Models.MusterReportEmailModel model = new Email.Models.MusterReportEmailModel();

            model.MusterDateTime = this.MusterDate;
        }

        #endregion
    }
}
