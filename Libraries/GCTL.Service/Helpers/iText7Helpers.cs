using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;

namespace GCTL.Service.Helpers
{
    public class iText7Helpers
    {
        public void WarmUpPdfGeneration()
        {
            using var ms = new MemoryStream();
            using var writer = new PdfWriter(ms);
            using var pdf = new PdfDocument(writer);
            var doc = new Document(pdf);
            doc.Add(new Paragraph("Warm-up"));
            doc.Close();
        }
    }
}
