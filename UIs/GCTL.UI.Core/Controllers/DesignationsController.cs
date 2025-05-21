
using GCTL.Core.ViewModels.Designations;
using GCTL.Data.Models;
using GCTL.Service.Common;
using GCTL.Service.Designations;
using GCTL.UI.Core.ViewModels.Designations;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using GCTL.Core.Helpers;
using ClosedXML.Excel;

using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;

using iText.Kernel.Geom;
using GCTL.Core.ViewModels.Companies;




namespace GCTL.UI.Core.Controllers
{
    public class DesignationsController : BaseController
    {
        private readonly IDesignationService designationService;
        private readonly ICommonService commonService;
        private readonly IMapper mapper;
        string strMaxNO = "";
        public DesignationsController(IDesignationService designationService,
                                      ICommonService commonService,
                                      IMapper mapper)
        {
            this.designationService = designationService;
            this.commonService = commonService;
            this.mapper = mapper;
        }

        public IActionResult Index(bool child = false)
        {
            DesignationPageViewModel model = new DesignationPageViewModel()
            {
                PageUrl = Url.Action(nameof(Index))
            };
            commonService.FindMaxNo(ref strMaxNO, "DesignationCode", "HRM_Def_Designation", 3);
            model.Setup = new DesignationSetupViewModel
            {
                DesignationCode = strMaxNO
            };

            if (child)
                return PartialView(model);

            return View(model);
        }


        public IActionResult Setup(string id)
        {
            DesignationSetupViewModel model = new DesignationSetupViewModel();
            commonService.FindMaxNo(ref strMaxNO, "DesignationCode", "HRM_Def_Designation", 3);
            var result = designationService.GetDesignation(id);
            if (result != null)
            {
                model = mapper.Map<DesignationSetupViewModel>(result);
                model.Code = id;
                model.Id = (int)result.AutoId;
            }
            else
            {
                model.DesignationCode = strMaxNO;
            }

            return PartialView($"_{nameof(Setup)}", model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Setup(DesignationSetupViewModel model)
        {
            if (designationService.IsDesignationExist(model.DesignationName, model.DesignationCode))
            {
                return Json(new { isSuccess = false, message = "Already Exists" });
            }

            if (ModelState.IsValid)
            {
                if (designationService.IsDesignationExistByCode(model.DesignationCode))
                {
                    var hasPermission = designationService.UpdatePermission(LoginInfo.AccessCode);
                    if (hasPermission)
                    {
                        HrmDefDesignation designation = designationService.GetDesignation(model.DesignationCode) ?? new HrmDefDesignation();
                        model.ToAudit(LoginInfo, model.AutoId > 0);
                        mapper.Map(model, designation);
                        model. BanglaShortName  = string.Empty;
                        model.BanglaDesignation = string.Empty;
                        model.CompanyCode  = string.Empty;
                        model.EmployeeId  = string.Empty;
                        model.MobileAllowanceId = string.Empty;
                        designationService.SaveDesignation(designation);
                        return Json(new { isSuccess = true, message = "Saved Successfully", lastCode = designation.DesignationCode });
                    }
                    else
                    {

                        return Json(new { isSuccess = false, message = "You have no access" });
                    }

                }
                else
                {
                    var hasPermission = designationService.SavePermission(LoginInfo.AccessCode);
                    if (hasPermission)
                    {
                        HrmDefDesignation designation = designationService.GetDesignation(model.DesignationCode) ?? new HrmDefDesignation();
                        model.ToAudit(LoginInfo, model.Id > 0);
                        mapper.Map(model, designation);
                        model.BanglaShortName = string.Empty;
                        model.BanglaDesignation = string.Empty;
                        model.CompanyCode = string.Empty;
                        model.EmployeeId = string.Empty;
                        model.MobileAllowanceId = string.Empty;
                        designationService.SaveDesignation(designation);
                        return Json(new { isSuccess = true, message = "Saved Successfully", lastCode = designation.DesignationCode });
                    }
                    else
                    {

                        return Json(new { isSuccess = false, message = "You have no access" });
                    }
                }

               
            }

            return Json(new { success = false, message = ModelState.Values.FirstOrDefault()?.Errors.FirstOrDefault()?.ErrorMessage });
        }

        public ActionResult Grid()
        {
            var resutl = designationService.GetDesignations();
            return Json(new { data = resutl });
        }


        [HttpPost]
        public ActionResult Delete(string id)
        {
            var hasPermission = designationService.DeletePermission(LoginInfo.AccessCode);
            if (hasPermission)
            {
                bool success = false;
                foreach (var item in id.Split(",", StringSplitOptions.RemoveEmptyEntries))
                {
                    success = designationService.DeleteDesignation(item);
                }

                return Json(new { success = success, message = "Deleted Successfully" });
            }
            else
            {
                return Json(new { isSuccess = false, message = "You have no access" });
            }
            
        }


        [HttpPost]
        public JsonResult CheckAvailability(string name, string code)
        {
            if (designationService.IsDesignationExist(name, code))
            {
                return Json(new { isSuccess = true, message = "Already Exists" });
            }

            return Json(new { isSuccess = false });
        }


        #region Xls
        public async Task<IActionResult> ExportToExcel()
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Designations");

                // Add title
                worksheet.Cell(1, 1).Value = "Designation Information";
                worksheet.Range(1, 1, 1, 4).Merge(); // Merge cells across the header columns
                worksheet.Row(1).Style.Font.Bold = true;
                worksheet.Row(1).Style.Font.FontSize = 14;
                worksheet.Row(1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Row(1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                worksheet.Range(2, 1, 2, 4).Merge();
                // Leave an empty row
                int dataStartRow = 3;

                // Add headers
                worksheet.Cell(dataStartRow, 1).Value = "Designation Id";
                worksheet.Cell(dataStartRow, 2).Value = "Designation Name";
                worksheet.Cell(dataStartRow, 3).Value = "Short Name";
                worksheet.Cell(dataStartRow, 4).Value = "Designation (বাংলা)";
                worksheet.Row(dataStartRow).Style.Font.Bold = true;
                worksheet.Row(dataStartRow).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Row(dataStartRow).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                // Add data
                var designations = designationService.GetDesignations();
                int row = dataStartRow + 1;
                foreach (var designation in designations)
                {

                   
                    worksheet.Cell(row, 1).Value = "'" + designation.DesignationCode.PadLeft(2, '0');
                    worksheet.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    worksheet.Cell(row, 2).Value = designation.DesignationName;
                    worksheet.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                    worksheet.Cell(row, 3).Value = designation.DesignationShortName;
                    worksheet.Cell(row, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    worksheet.Cell(row, 4).Value = designation.BanglaDesignation;
                    worksheet.Cell(row, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                    row++;
                }

                worksheet.Columns().AdjustToContents();

                // Save to a stream
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;
                    return File(stream.ToArray(),
                                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                                "Designations.xlsx");
                }
            }
        }
        #endregion
     

        #region Pdf

        public async Task<IActionResult> ExportToPdf()
        {
           
            using (var stream = new MemoryStream())
            {
             
                PdfWriter writer = new PdfWriter(stream);
                PdfDocument pdf = new PdfDocument(writer);
               // pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, new FooterEventHandler());

                Document document = new Document(pdf);

                // Add Title
                Paragraph title = new Paragraph("Designation Information")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(14)
                    .SimulateBold();
                document.Add(title);
                //

                //
                // Add some space
                document.Add(new Paragraph("\n"));

                // Create a table with 4 columns
                Table table = new Table(4, true);
                table.SetWidth(UnitValue.CreatePercentValue(100)); // Full width table

                // Add headers
                table.AddHeaderCell(new Cell().Add(new Paragraph("Designation Id").SimulateBold()).SetTextAlignment(TextAlignment.CENTER));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Designation Name").SimulateBold()).SetTextAlignment(TextAlignment.CENTER));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Short Name").SimulateBold()).SetTextAlignment(TextAlignment.CENTER));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Designation (বাংলা)").SimulateBold()).SetTextAlignment(TextAlignment.CENTER));

                // Add data
                var designations = designationService.GetDesignations();
                foreach (var designation in designations)
                {
                    table.AddCell(new Cell().Add(new Paragraph(designation.DesignationCode?.PadLeft(2, '0') ?? "")).SetTextAlignment(TextAlignment.CENTER));
                    table.AddCell(new Cell().Add(new Paragraph(designation.DesignationName ?? "")).SetTextAlignment(TextAlignment.LEFT));
                    table.AddCell(new Cell().Add(new Paragraph(designation.DesignationShortName ?? "")).SetTextAlignment(TextAlignment.CENTER));
                    table.AddCell(new Cell().Add(new Paragraph(designation.BanglaDesignation ?? "")).SetTextAlignment(TextAlignment.LEFT));
                }

                document.Add(table);

                // Close the document
                document.Close();

                // Return the PDF as a file
                return File(stream.ToArray(), "application/pdf", "Designations.pdf");
            }
        }


        //public class FooterEventHandler : IEventHandler
        //{
        //    public void HandleEvent(Event @event)
        //    {
        //        PdfDocumentEvent pdfEvent = (PdfDocumentEvent)@event;
        //        PdfDocument pdf = pdfEvent.GetDocument();
        //        PdfPage page = pdfEvent.GetPage();
        //        int pageNumber = pdf.GetPageNumber(page);
        //        PdfCanvas canvas = new PdfCanvas(page);

        //        // Footer content
        //        string footerText = $"Page {pageNumber}";
        //        Rectangle pageSize = page.GetPageSize();
        //        float x = (pageSize.GetLeft() + pageSize.GetRight()) / 2; // Center of the page
        //        float y = pageSize.GetBottom() + 15; // Position at the bottom
        //        canvas.BeginText()
        //              .SetFontAndSize(PdfFontFactory.CreateFont(StandardFonts.HELVETICA), 10) // Font and size
        //              .MoveText(x, y) // Position the text
        //              .ShowText(footerText) // Add the footer text
        //              .EndText()
        //              .Release(); // Finish drawing
        //    }
        //}


     



        #endregion

    }
}