using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using GCTL.Core.Helpers;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.HrmPayOthersAdjustmentEntries;
using GCTL.Service.HrmPayOthersAdjustmentReports;
using GCTL.UI.Core.ViewModels.HrmPayOthersAdjustments;
using Microsoft.AspNetCore.Mvc;

namespace GCTL.UI.Core.Controllers
{
    public class HrmPayOthersAdjustmentReportController : BaseController
    {
        private readonly IHrmPayOthersAdjustmentReportService reportService;

        public HrmPayOthersAdjustmentReportController(IHrmPayOthersAdjustmentReportService reportService)
        {
            this.reportService = reportService;
        }

        public IActionResult Index()
        {
            HrmPayOthersAdjustmentPageViewModel model = new HrmPayOthersAdjustmentPageViewModel()
            {
                PageUrl = Url.Action(nameof(Index))
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> PreviewReport([FromBody] ReportExportRequest request)
        {
            BaseViewModel model = new BaseViewModel();
            model.ToAudit(LoginInfo);

            var data = await reportService.GetDataAsync(request.FilterData);

            var pdfBytes = await reportService.GeneratePdfReport(data.OtherBenefits, model);

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
                if (data?.OtherBenefits == null || !data.OtherBenefits.Any())
                {
                    return Json(new { isSuccess = false, message = "No data found to export" });
                }

                byte[] fileBytes;
                string fileName;
                string contentType;

                switch (request.ExportFormat.ToLower())
                {
                    case "pdf":
                        fileBytes = await reportService.GeneratePdfReport(data.OtherBenefits, model);
                        fileName = $"OtherBenefitsReport_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                        contentType = "application/pdf";
                        break;

                    case "excel":
                        fileBytes = await reportService.GenerateExcelReport(data.OtherBenefits);
                        fileName = $"OtherBenefitsReport_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                        contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        break;

                    case "word":
                        fileBytes = GenerateWordReport(data.OtherBenefits);
                        fileName = $"OtherBenefitsReport_{DateTime.Now:yyyyMMdd_HHmmss}.docx";
                        contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                        break;

                    case "download":
                        fileBytes = await reportService.GeneratePdfReport(data.OtherBenefits, model);
                        fileName = $"OtherBenefitsReport_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                        contentType = "application/pdf";
                        Response.Headers.Add("Content-Disposition", $"inline; filename=\"{fileName}\"");
                        return File(fileBytes, contentType);

                    default:
                        return Json(new { isSuccess = false, message = "Invalid export format" });

                }
                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex) 
            {
                return Json(new {isSuccess = false, message = "An error occurred while generating the report"});   
            }
        }

        private byte[] GenerateWordReport(List<ReportFilterResultDto> data)
        {
            using var stream = new MemoryStream();
            using var document = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document);

            var mainPart = document.AddMainDocumentPart();
            mainPart.Document = new Document();

            var body = new Body();

            // Add section properties only once at the end
            var sectionProps = new SectionProperties(
                new PageSize { Width = 16838, Height = 11906, Orient = PageOrientationValues.Landscape },
                new PageMargin { Top = 720, Right = 720, Bottom = 720, Left = 720 }
            );

            TableCell CreateCell(string text, int width, bool bold = false, TableCellBorders borders = null, bool centerAlign = false, bool rightAlign = false)
            {
                var props = new TableCellProperties(
                    new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = width.ToString() }
                );

                if (borders != null)
                    props.Append(borders);

                var runProps = new RunProperties(
                    new FontSize() { Val = "18" } // 10 pt font size (value is in half-points)
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

            var months = data
                .Select(d => $"{d.Month} {d.Year}")
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct()
                .ToList();

            var reportPeriod = string.Join(", ", months);
            body.Append(
                new Paragraph(
                    new ParagraphProperties(
                        new Justification() { Val = JustificationValues.Center },
                        new SpacingBetweenLines() { After = "240" }
                    ),
                    new Run(new RunProperties(new Bold(), new FontSize() { Val = "28" }), new Text(data.Select(c => c.CompanyName).Distinct().FirstOrDefault())),
                    new Run(new Break()),
                    new Run(new RunProperties(new Bold(), new FontSize() { Val = "24" }), new Text("Salary Benefit Report")),
                    new Run(new Break()),
                    new Run(new RunProperties(new FontSize() { Val = "20" }), new Text($"For the month of {reportPeriod}"))
                )
            );

            var groupedData = data
                .GroupBy(x => x.DepartmentName ?? "Unknown Department")
                .OrderBy(g => g.Key);

            decimal grandTotal = 0;
            int totalEmployees = 0;
            int totalBenefits = 0;

            foreach (var departmentGroup in groupedData)
            {
                // Department header
                body.Append(
                    new Paragraph(
                        new ParagraphProperties(
                            new SpacingBetweenLines() { Before = "300", After = "200" }
                        ),
                        new Run(new RunProperties(new Bold(), new FontSize() { Val = "20" }), new Text($"Department: {departmentGroup.Key}"))
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

                string[] headers = { "SN.", "Employee ID", "Name", "Designation", "Branch", "Benefit Type", "Amount", "Remarks" };
                int[] columnWidths = {
                    576,   // SN
                    1008,  // Employee ID  
                    2016,  // Name
                    2016,  // Designation
                    1296,  // Branch
                    1296,  // Benefit Type
                    1152,  // Amount
                    1584   // Remarks
                };

                // Header row
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

                // Data rows
                int serialNo = 1;
                decimal departmentTotal = 0;
                var uniqueEmployees = new HashSet<string>();
                var orderedItems = departmentGroup.OrderBy(x => x.Code).ToList();

                foreach (var item in orderedItems)
                {
                    var amount = item.BenefitAmount ?? 0;
                    departmentTotal += amount;
                    totalBenefits++;

                    if (!string.IsNullOrEmpty(item.Code))
                        uniqueEmployees.Add(item.Code);

                    // Fixed: Match the number of values with headers (8 elements)
                    var values = new string[]
                    {
                        serialNo.ToString(),
                        item.Code ?? "",
                        item.Name ?? "",
                        item.DesignationName ?? "",
                        item.BranchName ?? "",
                        item.BenefitType ?? "",
                        amount.ToString("N2"),
                        item.Remarks ?? "" // Combined remarks and paid days if needed
                    };

                    var dataRow = new TableRow();
                    for (int j = 0; j < values.Length; j++)
                    {
                        // Center align everything except Name, Designation, and Branch
                        bool centerAlign = j != 2 && j != 3 && j != 4;
                        // Right align Amount column
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

                // Department total row
                var totalRow = new TableRow();
                for (int j = 0; j < headers.Length; j++)
                {
                    string cellText = "";
                    bool bold = false;
                    bool rightAlign = false;

                    if (j == 6) // Amount column
                    {
                        cellText = $"Total: {departmentTotal:N2}";
                        bold = true;
                        rightAlign = true;
                    }

                    var borders = new TableCellBorders(
                        new TopBorder() { Val = BorderValues.None },
                        new BottomBorder() { Val = BorderValues.Single, Size = 4, Color = "D3D3D3" },
                        new LeftBorder() { Val = BorderValues.Single, Size = 4, Color = "D3D3D3" },
                        new RightBorder() { Val = BorderValues.Single, Size = 4, Color = "D3D3D3" }
                    );

                    totalRow.Append(CreateCell(cellText, columnWidths[j], bold, borders, false, rightAlign));
                }

                table.Append(totalRow);
                body.Append(table);

                grandTotal += departmentTotal;
                totalEmployees += uniqueEmployees.Count;
            }

            // Summary section
            body.Append(
                new Paragraph(
                    new ParagraphProperties(
                        new SpacingBetweenLines() { Before = "400", After = "200" }
                    ),
                    new Run(new RunProperties(new Bold(), new FontSize() { Val = "24" }), new Text("Summary"))
                )
            );

            var summaryTable = new Table();
            var summaryProps = new TableProperties(
                new TableWidth() { Width = "15120", Type = TableWidthUnitValues.Dxa },
                new TableBorders(
                    new TopBorder() { Val = BorderValues.None },
                    new BottomBorder() { Val = BorderValues.None },
                    new LeftBorder() { Val = BorderValues.None },
                    new RightBorder() { Val = BorderValues.None },
                    new InsideHorizontalBorder() { Val = BorderValues.None },
                    new InsideVerticalBorder() { Val = BorderValues.None }
                )
            );
            summaryTable.Append(summaryProps);

            var summaryRow = new TableRow();
            summaryRow.Append(
                CreateCell($"No. of Employee: {totalEmployees}", 5040, false, null, centerAlign: false),
               // CreateCell($"Total Benefits: {totalBenefits}", 5040, false, null, centerAlign: true),
                CreateCell($"Grand Total: {grandTotal:N2}", 5040, false, null, rightAlign: true)
            );
            summaryTable.Append(summaryRow);

            body.Append(summaryTable);

            // Add section properties only once at the end
            body.Append(sectionProps);
            mainPart.Document.Append(body);

            // CRITICAL: Save in the correct order
            mainPart.Document.Save();
            document.Save();

            // Get the byte array before disposing
            var result = stream.ToArray();

            return result;
        }
    }
}
