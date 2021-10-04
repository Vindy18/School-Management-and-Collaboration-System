using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Util.PDFGenerator
{
    public class StudentList
    {
        #region Declaration
        int totalColumns = 3;
        Document document;
        Font fontStyle;
        PdfPTable pdfTable = new PdfPTable(3);
        PdfPCell pdfCell;
        MemoryStream memoryStream = new MemoryStream();
        //List<>
        #endregion

        public byte[] PrepareReport(/*list*/)
        {
            //list = list from method

            #region
            document = new Document(PageSize.A4, 0f, 0f, 0f, 0f);
            document.SetPageSize(PageSize.A4);
            document.SetMargins(20f, 20f, 20f, 20f);
            pdfTable.WidthPercentage = 100;
            pdfTable.HorizontalAlignment = Element.ALIGN_LEFT;
            fontStyle = FontFactory.GetFont("Tahoma", 8f, 1);
            PdfWriter.GetInstance(document, memoryStream);
            document.Open();
            pdfTable.SetWidths(new float[] { 20f, 150f, 100f });
            #endregion

            this.ReportHeader();
            this.ReportBody();
            pdfTable.HeaderRows = 2;
            document.Add(pdfTable);
            document.Close();
            return memoryStream.ToArray();
        }

        private void ReportHeader()
        {
            fontStyle = FontFactory.GetFont("Tahoma", 11f, 1);
            pdfCell = new PdfPCell(new Phrase("School Name", fontStyle));
            pdfCell.Colspan = totalColumns;
            pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
            pdfCell.Border = 0;
            pdfCell.BackgroundColor = BaseColor.WHITE;
            pdfCell.ExtraParagraphSpace = 0;
            pdfTable.AddCell(pdfCell);
            pdfTable.CompleteRow();

            fontStyle = FontFactory.GetFont("Tahoma", 9f, 1);
            pdfCell = new PdfPCell(new Phrase("Students List", fontStyle));
            pdfCell.Colspan = totalColumns;
            pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
            pdfCell.Border = 0;
            pdfCell.BackgroundColor = BaseColor.WHITE;
            pdfCell.ExtraParagraphSpace = 0;
            pdfTable.AddCell(pdfCell);
            pdfTable.CompleteRow();
        }

        private void ReportBody()
        {
            #region Table header
            fontStyle = FontFactory.GetFont("Tahoma", 8f, 1);
            pdfCell = new PdfPCell(new Phrase("Admission no", fontStyle));
            pdfCell.Colspan = totalColumns;
            pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
            pdfCell.VerticalAlignment = Element.ALIGN_MIDDLE;
            pdfCell.Border = 0;
            pdfCell.BackgroundColor = BaseColor.LIGHT_GRAY;
            pdfCell.ExtraParagraphSpace = 0;
            pdfTable.AddCell(pdfCell);

            pdfCell = new PdfPCell(new Phrase("Student name", fontStyle));
            pdfCell.Colspan = totalColumns;
            pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
            pdfCell.VerticalAlignment = Element.ALIGN_MIDDLE;
            pdfCell.Border = 0;
            pdfCell.BackgroundColor = BaseColor.LIGHT_GRAY;
            pdfCell.ExtraParagraphSpace = 0;
            pdfTable.AddCell(pdfCell);

            pdfCell = new PdfPCell(new Phrase("Address", fontStyle));
            pdfCell.Colspan = totalColumns;
            pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
            pdfCell.VerticalAlignment = Element.ALIGN_MIDDLE;
            pdfCell.Border = 0;
            pdfCell.BackgroundColor = BaseColor.LIGHT_GRAY;
            pdfCell.ExtraParagraphSpace = 0;
            pdfTable.AddCell(pdfCell);
            #endregion

            #region Table body
            fontStyle = FontFactory.GetFont("Tahoma", 8f, 0);
            
            #endregion
        }
    }
}
