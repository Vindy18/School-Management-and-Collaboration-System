using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.ViewModel.Report
{
    public class ClassReport
    {
        #region Declaration
        int _totalColumn = 5;
        Document _document;
        Font _fontStyle;
        iTextSharp.text.pdf.PdfPTable _pdfPTable = new PdfPTable(5);
        iTextSharp.text.pdf.PdfPCell _pdfPCell;
        MemoryStream _memoryStream = new MemoryStream();
        List<ClassReportViewModel> _classes = new List<ClassReportViewModel>();
        #endregion

        public byte[] PrepareReport(List<ClassReportViewModel> response)
        {
            _classes = response;

            #region
            _document = new Document(PageSize.A4, 0f, 0f, 0f, 0f);
            _document.SetPageSize(PageSize.A4);
            _document.SetMargins(20f, 20f, 20f, 20f);
            _pdfPTable.WidthPercentage = 100;
            _pdfPTable.HorizontalAlignment = Element.ALIGN_LEFT;
            _fontStyle = FontFactory.GetFont("TimesNewRoman", 8f, 1);

            iTextSharp.text.pdf.PdfWriter.GetInstance(_document, _memoryStream);
            _document.Open();
            _pdfPTable.SetWidths(new float[] { 50f, 50f, 50f, 50f, 50f });
            #endregion

            this.ReportHeader();
            this.ReportBody();
            _pdfPTable.HeaderRows = 5;
            _document.Add(_pdfPTable);
            _document.Close();
            return _memoryStream.ToArray();
        }

        private void ReportHeader()
        {
            _fontStyle = FontFactory.GetFont("TimesNewRoman", 18f, 1);
            _pdfPCell = new PdfPCell(new Phrase("Class Report", _fontStyle));
            _pdfPCell.Colspan = _totalColumn;
            _pdfPCell.HorizontalAlignment = Element.ALIGN_CENTER;
            _pdfPCell.Border = 0;
            _pdfPCell.BackgroundColor = BaseColor.WHITE;
            _pdfPCell.ExtraParagraphSpace = 0;
            _pdfPTable.AddCell(_pdfPCell);
            _pdfPTable.CompleteRow();

            _fontStyle = FontFactory.GetFont("TimesNewRoman", 12f, 1);
            _pdfPCell = new PdfPCell(new Phrase("Class List", _fontStyle));
            _pdfPCell.Colspan = _totalColumn;
            _pdfPCell.HorizontalAlignment = Element.ALIGN_CENTER;
            _pdfPCell.Border = 0;
            _pdfPCell.BackgroundColor = BaseColor.WHITE;
            _pdfPCell.ExtraParagraphSpace = 0;
            _pdfPTable.AddCell(_pdfPCell);
            _pdfPTable.CompleteRow();
        }

        private void ReportBody()
        {
            #region Table header
            _fontStyle = FontFactory.GetFont("TimesNewRoman", 10f, 1);
            _pdfPCell = new PdfPCell(new Phrase("Academic Year", _fontStyle));
            _pdfPCell.HorizontalAlignment = Element.ALIGN_CENTER;
            _pdfPCell.VerticalAlignment = Element.ALIGN_MIDDLE;
            _pdfPCell.BackgroundColor = BaseColor.LIGHT_GRAY;
            _pdfPTable.AddCell(_pdfPCell);

            _pdfPCell = new PdfPCell(new Phrase("Academic Level", _fontStyle));
            _pdfPCell.HorizontalAlignment = Element.ALIGN_CENTER;
            _pdfPCell.VerticalAlignment = Element.ALIGN_MIDDLE;
            _pdfPCell.BackgroundColor = BaseColor.LIGHT_GRAY;
            _pdfPTable.AddCell(_pdfPCell);

            _pdfPCell = new PdfPCell(new Phrase("Class Name", _fontStyle));
            _pdfPCell.HorizontalAlignment = Element.ALIGN_CENTER;
            _pdfPCell.VerticalAlignment = Element.ALIGN_MIDDLE;
            _pdfPCell.BackgroundColor = BaseColor.LIGHT_GRAY;
            _pdfPTable.AddCell(_pdfPCell);

            _pdfPCell = new PdfPCell(new Phrase("Name", _fontStyle));
            _pdfPCell.HorizontalAlignment = Element.ALIGN_CENTER;
            _pdfPCell.VerticalAlignment = Element.ALIGN_MIDDLE;
            _pdfPCell.BackgroundColor = BaseColor.LIGHT_GRAY;
            _pdfPTable.AddCell(_pdfPCell);

            _pdfPCell = new PdfPCell(new Phrase("Class Teacher", _fontStyle));
            _pdfPCell.HorizontalAlignment = Element.ALIGN_CENTER;
            _pdfPCell.VerticalAlignment = Element.ALIGN_MIDDLE;
            _pdfPCell.BackgroundColor = BaseColor.LIGHT_GRAY;
            _pdfPTable.AddCell(_pdfPCell);
            _pdfPTable.CompleteRow();
            #endregion

            #region Table Body
            _fontStyle = FontFactory.GetFont("TimesNewRoman", 10f, 0);
            foreach (ClassReportViewModel vm in _classes)
            {
                _pdfPCell = new PdfPCell(new Phrase(vm.AcademicYearId.ToString(), _fontStyle));
                _pdfPCell.HorizontalAlignment = Element.ALIGN_CENTER;
                _pdfPCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                //_pdfPCell.BackgroundColor = BaseColor.LIGHT_GRAY;
                _pdfPTable.AddCell(_pdfPCell);

                _pdfPCell = new PdfPCell(new Phrase(vm.AcademicYearId.ToString(), _fontStyle));
                _pdfPCell.HorizontalAlignment = Element.ALIGN_CENTER;
                _pdfPCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                //_pdfPCell.BackgroundColor = BaseColor.LIGHT_GRAY;
                _pdfPTable.AddCell(_pdfPCell);

                _pdfPCell = new PdfPCell(new Phrase(vm.ClassNameId.ToString(), _fontStyle));
                _pdfPCell.HorizontalAlignment = Element.ALIGN_CENTER;
                _pdfPCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                //_pdfPCell.BackgroundColor = BaseColor.LIGHT_GRAY;
                _pdfPTable.AddCell(_pdfPCell);

                _pdfPCell = new PdfPCell(new Phrase(vm.Name, _fontStyle));
                _pdfPCell.HorizontalAlignment = Element.ALIGN_CENTER;
                _pdfPCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                //_pdfPCell.BackgroundColor = BaseColor.LIGHT_GRAY;
                _pdfPTable.AddCell(_pdfPCell);

                _pdfPCell = new PdfPCell(new Phrase(vm.ClassTeacherId.ToString(), _fontStyle));
                _pdfPCell.HorizontalAlignment = Element.ALIGN_CENTER;
                _pdfPCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                //_pdfPCell.BackgroundColor = BaseColor.LIGHT_GRAY;
                _pdfPTable.AddCell(_pdfPCell); ;
                _pdfPTable.CompleteRow();
            }
            #endregion

        }
    }
}
