using GCTL.Core.Helpers;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.HrmWorkingDayDeclarations;
using GCTL.Service.HrmWorkingDayDeclarationReports;
using GCTL.UI.Core.ViewModels.HrmWorkingDayDeclarations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;




using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using A = DocumentFormat.OpenXml.Drawing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;

namespace GCTL.UI.Core.Controllers
{
    public class HrmWorkingDayDeclarationReportController : BaseController
    {
        private readonly IHrmWorkingDayDeclarationReportService reportService;

        public HrmWorkingDayDeclarationReportController(IHrmWorkingDayDeclarationReportService reportService)
        {
            this.reportService = reportService;
        }

        public IActionResult Index()
        {
            HrmWorkingDayDeclarationPageViewModel model = new HrmWorkingDayDeclarationPageViewModel()
            {
                PageUrl = Url.Action(nameof(Index)),
            };
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> PreviewReport([FromBody] ReportExportRequest request)
        {

            BaseViewModel model = new BaseViewModel();
            model.ToAudit(LoginInfo);

            var data = await reportService.GetDataAsync(request.FilterData);

            if (data?.WorkingDays == null || !data.WorkingDays.Any())
            {
                return NotFound();
            }

            byte[] pdfBytes;
            if (request.FilterData.WorkingDates.Any())
            {
                 pdfBytes = await reportService.GeneratePdfReport(data.WorkingDays, model, true);
            }
            else
            {
                 pdfBytes = await reportService.GeneratePdfReport(data.WorkingDays, model);
            }


            return File(pdfBytes, "application/pdf");
        }

        [HttpPost]
        public async Task<IActionResult> GetAllFilterEmp([FromBody] ReportFilterViewModel filter)
        {
            try
            {
                var result = await reportService.GetDataAsync(filter);
                if (result != null)
                {
                    return Json(new { isSuccess = true, message = "Data loaded successfully", data = result });
                }
                return Json(new { isSuccess = false, message = "An error occurred while loading data" });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = "An error occurred while loading data" });
            }
        }


        [HttpPost]
        public async Task<IActionResult> ExportReport([FromBody] ReportExportRequest request)
        {
            try
            {
                BaseViewModel model = new BaseViewModel();
                model.ToAudit(LoginInfo);


                var data = await reportService.GetDataAsync(request.FilterData);
                if (data?.WorkingDays == null || !data.WorkingDays.Any())
                {
                    return NotFound();
                }

                byte[] fileBytes;
                string fileName;
                string contentType;
                bool isDate = request.FilterData.WorkingDates.Any();
                string baseFileName = $"WorkingDaysReport_{DateTime.Now:yyyyMMdd_HHmmss}";
                switch (request.ExportFormat.ToLower())
                {
                    case "pdf":
                        fileBytes = await reportService.GeneratePdfReport(data.WorkingDays, model, isDate);
                        fileName = $"{baseFileName}.pdf";
                        contentType = "application/pdf";
                        break;

                    case "excel":
                        fileBytes = await reportService.GenerateExcelReport(data.WorkingDays, isDate);
                        fileName = $"{baseFileName}.xlsx";
                        contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        break;

                    case "word":
                        fileBytes = GenerateWordReport(data.WorkingDays, isDate);
                        fileName = $"{baseFileName}.docx";
                        contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                        break;

                    case "download":
                        fileBytes = await reportService.GeneratePdfReport(data.WorkingDays, model, isDate);
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

        private byte[] GenerateWordReport(List<ReportFilterResultDto> data, bool isDate)
        {
            using var stream = new MemoryStream();
            using var document = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document);

            var mainPart = document.AddMainDocumentPart();
            mainPart.Document = new Document();
            var body = new Body();

            string reportPeriod;

            if (isDate)
            {
                var days = data
                .Select(d => d.WorkingDayDates.Value.ToString("MMMM dd, yyyy"))
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct()
                .ToList();
                reportPeriod = string.Join(", ", days);
            }
            else
            {
                var months = data
                .Select(d => $"{d.WorkingDayDates.Value.ToString("MMMM")} {d.WorkingDayDates.Value.Year}")
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct()
                .ToList();
                reportPeriod = string.Join(", ", months);
            }

            var dateGroupedData = data
                .GroupBy(x => x.WorkingDayDates.Value.Date)
                .OrderBy(g => g.Key);

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
                    new Run(new RunProperties(new Bold(),new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman" }, new FontSize() { Val = "28" }), new Text(data.Select(e=>e.CompanyName).Distinct().FirstOrDefault())), // 14pt
                    new Run(new Break()),
                    new Run(new RunProperties(new Bold(), new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman" }, new FontSize() { Val = "24" }), new Text("Working Day Report")), // 12pt
                    new Run(new Break()),
                    new Run(new RunProperties(new FontSize() { Val = "20" }), new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman" }, new Text($"{reportPeriod}")) // 10pt
                )
            );

            foreach (var dateGroup in dateGroupedData)
            {
                body.Append(
                    new Paragraph(
                        new ParagraphProperties(
                            new SpacingBetweenLines() { Before = "200", After = "200" }),

                        new Run(
                            new RunProperties(new Bold(), new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman" }, 
                            new FontSize() { Val = "22" }), 
                            new Text($"Date: {dateGroup.Key.ToString("dd/MM/yyyy")}"))
                    )
                );

                var departmentGroupedData = dateGroup
                    .GroupBy(x => x.DepartmentName ?? "Unknown Department")
                    .OrderBy(g => g.Key);

                var uniqueEmployees = new HashSet<String>();

                foreach (var departmentGroup in departmentGroupedData)
                {
                    body.Append(
                        new Paragraph(
                            new ParagraphProperties(
                                new SpacingBetweenLines() { Before = "100", After = "50" }
                            ),
                            new Run(new RunProperties(new Bold(), new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman" }, new FontSize() { Val = "20" }), new Text($"Department: {departmentGroup.Key}"))
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

                    string[] headers = { "SN", "Employee ID", "Name", "Designation", "Branch", "Employee Type",  "Joining Date", "Employee Status", "Remarks" };

                    int[] columnWidths = { 500, 1150, 2100, 2100, 1250, 1250, 1250, 1250, 1250 };

                    var headerRow = new TableRow();

                    for (int i = 0; i < headers.Length; i++)
                    {
                        var borders = new TableCellBorders(
                            new TopBorder() { Val = BorderValues.Single, Size = 4, Color = "D3D3D3" },
                            new BottomBorder() { Val = BorderValues.Single, Size = 4, Color = "D3D3D3" },
                            new LeftBorder() { Val = BorderValues.Single, Size = 4, Color = "D3D3D3" },
                            new RightBorder() { Val = BorderValues.Single, Size = 4, Color = "D3D3D3" }
                        );
                        headerRow.Append(CreateCell(headers[i], columnWidths[i], bold: true, borders, centerAlign: true));
                    }

                    table.Append(headerRow);

                    int serialNo = 1;
                    var orderedItems = departmentGroup.OrderBy(x => x.Code).ToList();

                    foreach (var item in orderedItems)
                    {
                        if (!string.IsNullOrEmpty(item.Code))
                        {
                            uniqueEmployees.Add(item.Code);
                        }

                        var values = new[]
                        {
                            serialNo.ToString(),
                            item.Code ?? "",
                            item.Name ?? "",
                            item.DesignationName ?? "",
                            item.BranchName ?? "",
                            item.EmployeeType ?? "",
                            item.JoiningDate ?? "",
                            item.EmployeeStatus ?? "",
                            item.Remarks ?? ""
                        };

                        var dataRow = new TableRow();

                        for (int j = 0; j < values.Length; j++)
                        {
                            bool centerAlign = j != 2 && j != 3 && j != 4;
                            bool rightAlign = j == 6;

                            var borders = new TableCellBorders(
                                new TopBorder() { Val = BorderValues.Single, Size = 4, Color = "D3D3D3" },
                                new BottomBorder() { Val = BorderValues.Single, Size = 4, Color = "D3D3D3" },
                                new LeftBorder() { Val = BorderValues.Single, Size = 4, Color = "D3D3D3" },
                                new RightBorder() { Val = BorderValues.Single, Size = 4, Color = "D3D3D3" }
                            );

                            dataRow.Append(CreateCell(values[j], columnWidths[j], false, borders, centerAlign, rightAlign));
                        }

                        table.Append(dataRow);
                        serialNo++;
                    }

                    body.Append(table);
                }

                int dateTotalEmployee = uniqueEmployees.Count;

                body.Append(
                    new Paragraph(
                        new ParagraphProperties(
                            new SpacingBetweenLines() { Before = "0", After = "200" }
                        ),
                        new Run(new RunProperties(new FontSize() { Val = "20" }), new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman" }, new TabChar(),
                            new Text($"Total Employee on {dateGroup.Key.ToString("dd/MM/yyyy")}: ")
                        ),
                        new Run(new RunProperties(new Bold(), new FontSize() { Val = "20" }), new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman" },
                            new Text(dateTotalEmployee.ToString())
                        )
                    )
                );
            }

            body.Append(sectionProps);

            mainPart.Document.Append(body);
            mainPart.Document.Save();
            document.Dispose();

            return stream.ToArray();
        }
    }
}
