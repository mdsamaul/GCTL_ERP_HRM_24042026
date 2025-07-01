using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using GCTL.Core.Helpers;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.HrmPayMonthlyOtEntries;
using GCTL.Service.HrmPayMonthlyOtReports;
using GCTL.UI.Core.ViewModels.HrmPayMonthlyOts;
using Microsoft.AspNetCore.Mvc;

namespace GCTL.UI.Core.Controllers
{
    public class HrmPayMonthlyOtReportController : BaseController
    {
        private readonly IHrmPayMonthlyOtReportService reportService;

        public HrmPayMonthlyOtReportController( IHrmPayMonthlyOtReportService reportService )
        {
            this.reportService = reportService;
        }


        public IActionResult Index()
        {
            HrmPayMonthlyOtViewModel model = new HrmPayMonthlyOtViewModel()
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

        [HttpPost("HrmPayMonthlyOtReport/PreviewReport")]
        public async Task<IActionResult> PreviewReport([FromBody] ReportExportRequest request)
        {
            BaseViewModel model = new BaseViewModel();
            model.ToAudit(LoginInfo);

            var data = await reportService.GetDataAsync(request.FilterData);
            if (data.MonthlyOt == null || !data.MonthlyOt.Any())
            {
                return NotFound();
            }
            var pdfBytes = await reportService.GeneratePdfReport(data.MonthlyOt, model);
            return File(pdfBytes, "application/pdf");
        }

        [HttpPost("HrmPayMonthlyOtReport/ExportReport")]
        public async Task<IActionResult> ExportReport([FromBody] ReportExportRequest request)
        {

            try
            {
                BaseViewModel model = new BaseViewModel();
                model.ToAudit(LoginInfo);


                var data = await reportService.GetDataAsync(request.FilterData);
                if (data.MonthlyOt == null || !data.MonthlyOt.Any())
                {
                    return NotFound();
                }

                byte[] fileBytes;
                string fileName;
                string contentType;
                //bool isDate = request.FilterData.MonthlyOt.Any();
                string baseFileName = $"MonthlyOtReport_{DateTime.Now:yyyyMMdd_HHmmss}";
                switch (request.ExportFormat.ToLower())
                {
                    case "pdf":
                        fileBytes = await reportService.GeneratePdfReport(data.MonthlyOt, model);
                        fileName = $"{baseFileName}.pdf";
                        contentType = "application/pdf";
                        break;

                    case "excel":
                        fileBytes = await reportService.GenerateExcelReport(data.MonthlyOt);
                        fileName = $"{baseFileName}.xlsx";
                        contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        break;

                    case "word":
                        fileBytes = GenerateWordReport(data.MonthlyOt);
                        fileName = $"{baseFileName}.docx";
                        contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                        break;

                    case "download":
                        fileBytes = await reportService.GeneratePdfReport(data.MonthlyOt, model);
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

        private byte[] GenerateWordReport(List<ReportFilterResultDto> data)
        {
            using var stream = new MemoryStream();
            using var document = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document);

            var mainPart = document.AddMainDocumentPart();
            mainPart.Document = new Document();
            var body = new Body();

            var months = data
                .Select(d => $"{d.Month} {d.Year}")
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct()
                .ToList();
            var reportPeriod = string.Join(", ", months);

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
                    new Run(new RunProperties(new Bold(), new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman" }, new FontSize() { Val = "28" }), new Text(data.Select(e => e.CompanyName).Distinct().FirstOrDefault())), // 14pt
                    new Run(new Break()),
                    new Run(new RunProperties(new Bold(), new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman" }, new FontSize() { Val = "24" }), new Text("Monthly OT Report")), // 12pt
                    new Run(new Break()),
                    new Run(new RunProperties(new FontSize() { Val = "20" }), new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman" }, new Text($"{reportPeriod}")) // 10pt
                )
            );

            var departmentGroupedData = data
                    .GroupBy(x => x.DepartmentName ?? "Unknown Department")
                    .OrderBy(g => g.Key);

            var uniqueEmployees = new HashSet<String>();
            decimal grandTotalOT = 0;
            decimal grandTotalOTA = 0;
            int totalEmployees = 0;
            int totalEntries = 0;

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

                string[] headers = { "SN", "Employee ID", "Name", "Designation", "Branch", /*"Employee Type", "Joining Date", "Employee Status",*/"OT","Amount", "Remarks" };

                int[] columnWidths = { 500, 1150, 2600, 2600, 1650,1250,/*  1250,*/ 1650, 1750 };

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
                decimal departmentOTTotal = 0;
                decimal departmentOTATotal = 0;
              
                var orderedItems = departmentGroup.OrderBy(x => x.Code).ToList();


                foreach (var item in orderedItems)
                {
                    var ot = item.Ot ?? 0;
                    var amount = item.OtAmount ?? 0;
                    departmentOTTotal += ot;
                    departmentOTATotal += amount;
                    totalEntries++;

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
                        //item.EmployeeType??"",
                        item.BranchName ?? "",
                        (item.Ot ?? 0).ToString("G29"),
                        (item.OtAmount??0).ToString("G29"),
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

                var totalRow = new TableRow();
                for (int i = 0; i < headers.Length; i++)
                {
                    string cellText = "";
                    bool bold = false;
                    bool rightAlign = false;

                    if (i == 5)
                    {
                        cellText = $"Total: {departmentOTTotal.ToString("G29")}";
                        bold = true;
                    }

                    if (i == 6)
                    {
                        cellText = $"Total: {departmentOTATotal.ToString("G29")}";
                        bold = true;
                    }

                    var borders = new TableCellBorders(
                        new TopBorder() { Val = BorderValues.None, Size = 0 },
                        new BottomBorder() { Val = BorderValues.None, Size = 0 },
                        new LeftBorder() { Val = BorderValues.None, Size = 0 },
                        new RightBorder() { Val = BorderValues.None, Size = 0 }
                    );

                    totalRow.Append(CreateCell(cellText, columnWidths[i], bold, null, false, rightAlign));
                }
                table.Append(totalRow);
                body.Append(table);

                grandTotalOT += departmentOTTotal;
                grandTotalOTA += departmentOTATotal;

                totalEmployees += uniqueEmployees.Count;
            }

            body.Append(sectionProps);

            mainPart.Document.Append(body);
            mainPart.Document.Save();
            document.Dispose();

            return stream.ToArray();
        }
    }
}
