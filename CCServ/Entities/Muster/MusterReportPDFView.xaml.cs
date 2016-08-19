using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.IO;
using System.IO.Packaging;
using System.Windows.Xps.Packaging;
using System.Windows.Xps;
using System.Net.Mime;
using System.Net.Mail;

namespace CCServ.Entities.Muster
{
    /// <summary>
    /// Interaction logic for MusterReportPDFView.xaml
    /// </summary>
    public partial class MusterReportPDFView : Window
    {
        public MusterReportPDFView()
        {
            InitializeComponent();
        }

        public void GetPDF()
        {

            using (var ms = new MemoryStream())
            {
                Package package = Package.Open(ms, FileMode.Create);
                XpsDocument doc = new XpsDocument(package);
                XpsDocumentWriter writer = XpsDocument.CreateXpsDocumentWriter(doc);
                ms.Seek(0, SeekOrigin.Begin);

                writer.Write(this);

                var data = ms.ToArray();

                MailMessage mailMessage = new MailMessage();
                mailMessage.To.Add("Address1@test.com");
                mailMessage.From = new MailAddress("Address2@test.com");
                mailMessage.Subject = "Subject";
                mailMessage.Body = "Body";

                using (var attachStream = new MemoryStream(data))
                {
                    Attachment attach = new Attachment(attachStream, "test.xps", "application/vnd.ms-xpsdocument");
                    mailMessage.Attachments.Add(attach);

                    SmtpClient smtp = new SmtpClient("localhost");

                    smtp.Send(mailMessage);
                }
                    

                

                doc.Close();
                package.Close();
            }

        }
        
    }
}
