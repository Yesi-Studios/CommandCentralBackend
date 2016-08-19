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

        public byte[] GetPDF()
        {
            PdfSharp.Pdf.PdfDocument documention = new PdfSharp.Pdf.PdfDocument();

            MemoryStream lMemoryStream = new MemoryStream();
            Package package = Package.Open(lMemoryStream, FileMode.Create);
            XpsDocument doc = new XpsDocument(package);
            XpsDocumentWriter writer = XpsDocument.CreateXpsDocumentWriter(doc);
            writer.Write(this);
            doc.Close();
            package.Close();

            return lMemoryStream.ToArray();

        }
        
    }
}
