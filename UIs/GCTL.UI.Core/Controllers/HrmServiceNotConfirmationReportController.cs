using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using GCTL.Core.Helpers;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.HrmServiceNotConfirmEntries;
using GCTL.Service.HrmServiceNotConfirmationReports;
using GCTL.Service.Reports;
using GCTL.UI.Core.ViewModels.HrmServiceNotConfirmationEntry;
using Microsoft.AspNetCore.Mvc;

namespace GCTL.UI.Core.Controllers
{
    public class HrmServiceNotConfirmationReportController : BaseController
    {
        private readonly IHrmServiceNotConfirmationReportService reportService;
        public HrmServiceNotConfirmationReportController(IHrmServiceNotConfirmationReportService reportService)
        {
            this.reportService = reportService;
        }
        public IActionResult Index()
        {
            HrmServiceNotConfirmationPageViewModel model = new HrmServiceNotConfirmationPageViewModel
            {
                PageUrl = Url.Action(nameof(Index))
            };

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> GetAllFilterEmp([FromBody] ReportFilterViewModel filterDto)
        {
            try
            {
                var result = await reportService.GetDataAsync(filterDto);
                if (result != null)
                {
                    return Json(new { isSuccess = true, message = "Data loaded successfully", data = result });
                }
                return Json(new { isSuccess = false, message = "Data load failed" });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = "An error occurred while loading data" });
            }
        }

        [HttpPost("HrmServiceNotConfirmationReport/PreviewReport")]
        public async Task<IActionResult> PreviewReport([FromBody] ReportExportRequest request)
        {
            BaseViewModel model = new BaseViewModel();
            model.ToAudit(LoginInfo);

            var data = await reportService.GetDataAsync(request.FilterData);
            if (data.ServiceNotConfirms == null || !data.ServiceNotConfirms.Any())
            {
                return NotFound();
            }
            var pdfBytes = await reportService.GeneratePdfReport(data.ServiceNotConfirms, model);
            return File(pdfBytes, "application/pdf");
        }

        [HttpPost("HrmServiceNotConfirmationReport/ExportReport")]
        public async Task<IActionResult> ExportReport([FromBody] ReportExportRequest request)
        {
            try
            {
                BaseViewModel model = new BaseViewModel();
                model.ToAudit(LoginInfo);

                var data = await reportService.GetDataAsync(request.FilterData);

                byte[] fileBytes;
                string fileName;
                string contentType;

                string baseFileName = $"EmployeeServiceNotConfirmationReport_{DateTime.Now:yyyyMMdd_HHmmss}";

                switch (request.ExportFormat.ToLower())
                {
                    case "pdf":
                        fileBytes = await reportService.GeneratePdfReport(data.ServiceNotConfirms, model);
                        fileName = $"{baseFileName}.pdf";
                        contentType = "application/pdf";
                        break;

                    case "excel":
                        fileBytes = await reportService.GenerateExcelReport(data.ServiceNotConfirms);
                        fileName = $"{baseFileName}.xlsx";
                        contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        break;

                    case "word":
                        fileBytes = GenerateWordReport(data.ServiceNotConfirms);
                        fileName = $"{baseFileName}.docx";
                        contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                        break;

                    case "download":
                        fileBytes = await reportService.GeneratePdfReport(data.ServiceNotConfirms, model);
                        fileName = $"{baseFileName}.pdf";
                        contentType = "application/pdf";
                        Response.Headers.Add("Content-Disposition", $"inline; filename=\"{fileName}\"");
                        return File(fileBytes, contentType);

                    default:
                        return BadRequest();

                }
                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }

        private byte[] GenerateWordReport(List<ReportFilterResultViewModel> serviceNotConfirms)
        {
            using var stream = new MemoryStream();
            using var document = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document);

            var mainPart = document.AddMainDocumentPart();
            mainPart.Document = new Document();
            var body = new Body();

            var sectionProps = new SectionProperties(
                new PageSize { Width = 16838, Height = 11906, Orient = PageOrientationValues.Landscape },
                new PageMargin { Top = 518, Right = 518, Bottom = 518, Left = 518 }
            );

            TableCell CreateCell(string text, int width, bool bold = false, TableCellBorders borders = null, bool centerAlign = false, bool rightAlign = false)
            {
                var props = new TableCellProperties(
                    new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = width.ToString() }
                );

                if (borders != null)
                    props.Append(borders);

                var runProps = new RunProperties(
                    new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman" },
                    new FontSize() { Val = "18" }
                );

                if (bold)
                    runProps.Append(new Bold());

                var run = new Run(runProps, new Text(text));

                var paragraphProps = new ParagraphProperties(
                    new SpacingBetweenLines()
                    {
                        Before = "0",
                        After = "0",
                        Line = "240",
                        LineRule = LineSpacingRuleValues.Exact
                    }
                );

                if (centerAlign)
                    paragraphProps.Append(new Justification() { Val = JustificationValues.Center });
                else if (rightAlign)
                    paragraphProps.Append(new Justification() { Val = JustificationValues.Right });

                return new TableCell(props, new Paragraph(paragraphProps, run));
            }


            body.Append(
                new Paragraph(
                    new ParagraphProperties(
                        new Justification() { Val = JustificationValues.Center },
                        new SpacingBetweenLines() { After = "240" } // Add space after header
                    ),
                    new Run(new RunProperties(new Bold(), new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman" }, new FontSize() { Val = "28" }), new Text(serviceNotConfirms.Select(e => e.CompanyName).Distinct().FirstOrDefault())), // 14pt
                    new Run(new Break()),
                    new Run(new RunProperties(new Bold(), new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman" }, new FontSize() { Val = "24" }), new Text("Employee Service Not Confirm Report")) // 12pt
                )
            );

            var departmentGroupedData = serviceNotConfirms
                .GroupBy(x => x.DepartmentName ?? "Unknown Department")
                .OrderBy(x => x.Key);

            var uniqueEmployees = new HashSet<String>();
            int totalEmployees = 0;

            foreach (var item in departmentGroupedData)
            {
                body.Append(
                    new Paragraph(
                        new ParagraphProperties(
                            new SpacingBetweenLines() { Before = "100", After = "50" }
                        ),
                        new Run(new RunProperties(new Bold(), new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman" }, new FontSize() { Val = "20" }), new Text($"Department: {item.Key}"))
                    )
                );

                var table = new Table();

                var tableProps = new TableProperties(
                    new TableWidth() { Width = "15120", Type = TableWidthUnitValues.Dxa },
                    new TableBorders(
                        new TopBorder() { Val = BorderValues.Single, Size = 4, Color = "D3D3D3" },
                        new BottomBorder() { Val = BorderValues.Single, Size = 4, Color = "D3D3D3" },
                        new LeftBorder() { Val = BorderValues.Single, Size = 4, Color = "D3D3D3" },
                        new RightBorder() { Val = BorderValues.Single, Size = 4, Color = "D3D3D3" },
                        new InsideHorizontalBorder() { Val = BorderValues.Single, Size = 4, Color = "D3D3D3" },
                        new InsideVerticalBorder() { Val = BorderValues.Single, Size = 4, Color = "D3D3D3" }
                    )
                );

                table.AppendChild(tableProps);

                string[] headers =
                {
                    "Sl No", "Employee ID", "Employee Name", "Joining Date", "Effective Date", "Due Payment Date", "Ref Letter No", "Ref Letter Date", "Remarks"
                };
            }

            throw new NotImplementedException();
        }
    }
}
