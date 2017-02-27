using CCServ.ClientAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Criterion;
using AtwoodUtils;

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

            if (token == null || token.AuthenticationSession == null)
                model.CreatorName = "SYSTEM";
            else
                model.CreatorName = token.AuthenticationSession.Person.ToString();

            var containers = new List<MusterGroupContainer>();
            
            //Now we need to go get the records.
            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    var records = session.QueryOver<Entities.MusterRecord>()
                        .Where(x => x.MusterDate == this.MusterDate)
                        .List();

                    foreach (var record in records)
                    {
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

                    model.ReportText = containers.Select(x => x.ToString()).Aggregate((current, newElement) => current + "<p>" + newElement + "</p>");



                    //Ok, now we need to send the email.
                    Email.EmailInterface.CCEmailMessage
                        .CreateTestingDefault()
                        .To(new System.Net.Mail.MailAddress(
                            ServiceManagement.ServiceManager.CurrentConfigState.DeveloperDistroAddress,
                            ServiceManagement.ServiceManager.CurrentConfigState.DeveloperDistroDisplayName),
                        new System.Net.Mail.MailAddress("usn.gordon.inscom.list.nsag-nioc-ga-muster@mail.mil", "Muster Distro"),
                        new System.Net.Mail.MailAddress("usn.gordon.inscom.list.nsag-nioc-ga-n11@mail.mil", "N11"),
                        new System.Net.Mail.MailAddress("usn.gordon.inscom.list.nsag-nioc-ga-dept-chiefs@mail.mil", "Department Chiefs"),
                        new System.Net.Mail.MailAddress("usn.gordon.inscom.list.nsag-nioc-ga-dept-heads@mail.mil", "Department Heads"),
                        new System.Net.Mail.MailAddress("usn.gordon.inscom.list.nsag-nioc-ga-co@mail.mil", "CO"),
                        new System.Net.Mail.MailAddress("usn.gordon.inscom.list.nsag-nioc-ga-cmc@mail.mil", "CMC"),
                        new System.Net.Mail.MailAddress("usn.gordon.inscom.list.nsag-nioc-ga-xo@mail.mil", "XO"))
                        .BCC(ServiceManagement.ServiceManager.CurrentConfigState.DeveloperPersonalAddresses)
                        .Subject("Muster Report - " + model.DisplayDay)
                        .HTMLAlternateViewUsingTemplateFromEmbedded("CCServ.Email.Templates.MusterReport_HTML.html", model)
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
