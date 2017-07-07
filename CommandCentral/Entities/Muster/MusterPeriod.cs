using AtwoodUtils;
using CommandCentral.Entities.ReferenceLists;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Entities.Muster
{
    public class MusterPeriod
    {

        public Guid Id { get; set; }

        public TimeRange Range { get; set; }

        public bool IsClosed { get; set; }

        public Command Command { get; set; }

        public IList<MusterRecord> MusterRecords { get; set; }

        public void SendReportEmail(Person client)
        {
            Email.Models.MusterReportEmailModel model = new Email.Models.MusterReportEmailModel
            {
                Creator = client,
                MusterPeriod = this
            };

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
                        .Subject($"Muster Report ({this.Range.Start}-{this.Range.End})")
                        .HTMLAlternateViewUsingTemplateFromEmbedded("CommandCentral.Email.Templates.MusterReport_HTML.html", model)
                        .SendWithRetryAndFailure(TimeSpan.FromSeconds(1));
        }

    }
}
