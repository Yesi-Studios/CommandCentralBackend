using CommandCentral.ClientAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Criterion;
using AtwoodUtils;
using System.Globalization;

namespace CommandCentral
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
            Email.Models.MusterReportEmailModel model = new Email.Models.MusterReportEmailModel()
            {
                MusterDateTime = this.MusterDate
            };

            if (token == null || token.AuthenticationSession == null)
                model.Creator = null;
            else
                model.Creator = token.AuthenticationSession.Person;

            //Now we need to go get the records.
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    model.Records = session.QueryOver<Entities.Person>()
                        .Where(x => x.DutyStatus.Id != Entities.ReferenceLists.DutyStatuses.Loss.Id)
                        .Select(x => x.CurrentMusterRecord)
                        .List<Entities.MusterRecord>()
                        .ToList();

                    //Ok, now we need to send the email.
                    Email.EmailInterface.CCEmailMessage
                        .CreateDefault()
                        .To(Email.EmailInterface.CCEmailMessage.DeveloperAddress,
                            new System.Net.Mail.MailAddress("usn.gordon.inscom.list.nsag-nioc-ga-muster@mail.mil", "Muster Distro"),
                            new System.Net.Mail.MailAddress("usn.gordon.inscom.list.nsag-nioc-ga-n11@mail.mil", "N11"),
                            new System.Net.Mail.MailAddress("usn.gordon.inscom.list.nsag-nioc-ga-dept-chiefs@mail.mil", "Department Chiefs"),
                            new System.Net.Mail.MailAddress("usn.gordon.inscom.list.nsag-nioc-ga-dept-heads@mail.mil", "Department Heads"),
                            new System.Net.Mail.MailAddress("usn.gordon.inscom.list.nsag-nioc-ga-co@mail.mil", "CO"),
                            new System.Net.Mail.MailAddress("usn.gordon.inscom.list.nsag-nioc-ga-cmc@mail.mil", "CMC"),
                            new System.Net.Mail.MailAddress("usn.gordon.inscom.list.nsag-nioc-ga-xo@mail.mil", "XO"))
                        .BCC(Email.EmailInterface.CCEmailMessage.PersonalDeveloperAddresses)
                        .Subject("Muster Report - " + model.MusterDateTime.ToString("D", CultureInfo.CreateSpecificCulture("en-US")))
                        .HTMLAlternateViewUsingTemplateFromEmbedded("CommandCentral.Email.Templates.MusterReport_HTML.html", model)
                        .SendWithRetryAndFailure(TimeSpan.FromSeconds(1));

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        #endregion
    }
}
