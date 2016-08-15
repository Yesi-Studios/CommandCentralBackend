using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;

namespace CCServ.Email
{
    public abstract class CCEmailMessage : MailMessage
    {
        public abstract string Template { get; }

        public static string SMTPHost { get; set; }

        public CCEmailMessage()
        {
            IsBodyHtml = true;
            From = new MailAddress("usn.gordon.inscom.list.nsag-nioc-ga-webmaster@mail.mil", "Command Central Communications");
            Sender = new MailAddress("usn.gordon.inscom.list.nsag-nioc-ga-webmaster@mail.mil", "Command Central Communications");
            ReplyToList.Add(new MailAddress("usn.gordon.inscom.list.nsag-nioc-ga-webmaster@mail.mil", "Command Central Communications"));
            Priority = MailPriority.High;
            CC.Add(new MailAddress("usn.gordon.inscom.list.nsag-nioc-ga-webmaster@mail.mil", "Command Central Communications"));
        }


        public void Send()
        {
            new SmtpClient(SMTPHost).Send(this);
        }
    }
}
