using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;
using GCTL.Core.ViewModels.HrmPaySalaryDeductionEntries;
using GCTL.Service.HrmPaySalaryDeductionReports;
using GCTL.UI.Core.ViewModels.HrmPaySalaryDeductionReport;
using GCTL.UI.Core.ViewModels.HrmPaySalaryDeductions;
using Microsoft.AspNetCore.Mvc;
using AspNetCore.Reporting;
using GCTL.Core.ViewModels;
using GCTL.Core.Helpers;



namespace GCTL.UI.Core.Controllers
{
    public class HrmPaySalaryDeductionReportController : BaseController
    {
        private readonly IHrmPaySalaryDeductionReportService reportService;
        private readonly ILogger<HrmPaySalaryDeductionReportController> logger;

        public HrmPaySalaryDeductionReportController(
            IHrmPaySalaryDeductionReportService reportService,
            ILogger<HrmPaySalaryDeductionReportController> logger)
        {
            this.reportService = reportService;
            this.logger = logger;
        }

        public IActionResult Index()
        {
            HrmPaySalaryDeductionViewModel model = new HrmPaySalaryDeductionViewModel()
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
                logger.LogError(ex, "Error loading filter data");
                return Json(new { isSuccess = false, message = "An error occurred while loading data" });
            }
        }

        [HttpPost("HrmPaySalaryDeductionReport/PreviewReport")]
        public async Task<IActionResult> PreviewReport([FromBody] ReportExportRequest request)
        {
            BaseViewModel model = new BaseViewModel();
            model.ToAudit(LoginInfo);

            var data = await reportService.GetDataAsync(request.FilterData);

            var pdfBytes = await reportService.GeneratePdfReport(data.SalaryDeduction,model);
            return File(pdfBytes, "application/pdf");
        }

        [HttpPost]
        [Route("HrmPaySalaryDeductionReport/ExportReport")]
        public async Task<IActionResult> ExportReport([FromBody] ReportExportRequest request)
        {
            try
            {
                BaseViewModel model = new BaseViewModel();
                model.ToAudit(LoginInfo);

                var data = await reportService.GetDataAsync(request.FilterData);
                if (data?.SalaryDeduction == null || !data.SalaryDeduction.Any())
                {
                    return Json(new { isSuccess = false, message = "No data found to export" });
                }

                byte[] fileBytes;
                string fileName;
                string contentType;

                switch (request.ExportFormat.ToLower())
                {
                    case "pdf":
                        fileBytes = await reportService.GeneratePdfReport(data.SalaryDeduction, model);
                        fileName = $"SalaryDeductionReport_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                        contentType = "application/pdf";
                        break;

                    case "excel":
                        fileBytes = await reportService.GenerateExcelReport(data.SalaryDeduction);
                        fileName = $"SalaryDeductionReport_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                        contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        break;

                    case "word":
                        fileBytes = GenerateWordReport(data.SalaryDeduction);
                        fileName = $"SalaryDeductionReport_{DateTime.Now:yyyyMMdd_HHmmss}.docx";
                        contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                        break;

                    case "download":
                        fileBytes = await reportService.GeneratePdfReport(data.SalaryDeduction, model);
                        fileName = $"SalaryDeductionReport_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                        contentType = "application/pdf";
                        Response.Headers.Add("Content-Disposition", $"inline; filename=\"{fileName}\"");
                        return File(fileBytes, contentType);

                    default:
                        return Json(new { isSuccess = false, message = "Invalid export format" });
                }

                //Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");

                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error generating report");
                return Json(new { isSuccess = false, message = "An error occurred while generating the report" });
            }
        }
        private byte[] GenerateWordReport(List<ReportFilterResultDto> data)
        {
            using var stream = new MemoryStream();
            using var document = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document);

            var mainPart = document.AddMainDocumentPart();
            mainPart.Document = new Document();
            var body = new Body();

            // Set A4 landscape (matching PDF)
            var sectionProps = new SectionProperties(
                new PageSize { Width = 16838, Height = 11906, Orient = PageOrientationValues.Landscape },
                new PageMargin { Top = 518, Right = 518, Bottom = 518, Left = 518 } // 80pt top, 36pt sides, 60pt bottom
            );

            // Helper to create table cells with width, border, and bold option
            TableCell CreateCell(string text, int width, bool bold = false, TableCellBorders borders = null, bool centerAlign = false, bool rightAlign = false)
            {
                var props = new TableCellProperties(
                    new TableCellWidth() { Type = TableWidthUnitValues.Dxa, Width = width.ToString() }
                );

                if (borders != null)
                    props.Append(borders);

                var runProps = new RunProperties(
                    new FontSize() { Val = "14" } // 7pt font size for table content
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

            // Single centered header (matching PDF style)
            body.Append(
                new Paragraph(
                    new ParagraphProperties(
                        new Justification() { Val = JustificationValues.Center },
                        new SpacingBetweenLines() { After = "240" } // Add space after header
                    ),
                    new Run(new RunProperties(new Bold(), new FontSize() { Val = "28" }), new Text(data.Select(c => c.CompanyName).Distinct().FirstOrDefault())), // 14pt
                    new Run(new Break()),
                    new Run(new RunProperties(new Bold(), new FontSize() { Val = "24" }), new Text("Salary Deduction Report")), // 12pt
                    new Run(new Break()),
                    new Run(new RunProperties(new FontSize() { Val = "20" }), new Text($"For the month of {reportPeriod}")) // 10pt
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
                // Department header with proper spacing
                body.Append(
                    new Paragraph(
                        new ParagraphProperties(
                            new SpacingBetweenLines() { Before = "300", After = "200" } // Add spacing
                        ),
                        new Run(new RunProperties(new Bold(), new FontSize() { Val = "20" }), new Text($"Department: {departmentGroup.Key}"))
                    )
                );

                var table = new Table();

                // Table properties for landscape layout
                var tableProps = new TableProperties(
                    new TableWidth() { Width = "15120", Type = TableWidthUnitValues.Dxa }, // Full width landscape
                    new TableBorders(
                        new TopBorder() { Val = BorderValues.Single, Size = 4, Color = "D3D3D3" }, // Light gray borders
                        new BottomBorder() { Val = BorderValues.Single, Size = 4, Color = "D3D3D3" },
                        new LeftBorder() { Val = BorderValues.Single, Size = 4, Color = "D3D3D3" },
                        new RightBorder() { Val = BorderValues.Single, Size = 4, Color = "D3D3D3" },
                        new InsideHorizontalBorder() { Val = BorderValues.Single, Size = 4, Color = "D3D3D3" },
                        new InsideVerticalBorder() { Val = BorderValues.Single, Size = 4, Color = "D3D3D3" }
                    )
                );
                table.AppendChild(tableProps);

                // Headers matching PDF exactly
                string[] headers = {
            "SN", "Employee ID", "Name", "Designation", "Branch",
            "Deduction Type", "Amount",/* "Month", "Year",*/ "Remarks"
        };

                // Column widths for landscape (matching PDF proportions)
                int[] columnWidths = {
                    576,   // SN
                    1008,  // Employee ID  
                    2016,  // Name
                    2016,  // Designation
                    1296,  // Branch
                    1296,  // Deduction Type
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
                    var amount = item.DeductionAmount ?? 0; // Changed from DeductionAmount to BenefitAmount
                    departmentTotal += amount;
                    totalBenefits++;

                    if (!string.IsNullOrEmpty(item.Code))
                        uniqueEmployees.Add(item.Code);

                    // All columns including Month and Year
                    var values = new[]
                    {
                serialNo.ToString(),
                item.Code ?? "",
                item.Name ?? "",
                item.DesignationName ?? "",
                item.BranchName ?? "",
                item.DeductionType ?? "", // Changed from DeductionType to BenefitType
                amount.ToString("N2"),
                //item.Month ?? "",
                //item.Year ?? "",
                item.Remarks ?? ""
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

                // Total row (matching PDF style)
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
                        new TopBorder() { Val = BorderValues.None }, // No top border for total row
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

            // Summary section (matching PDF)
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
                CreateCell($"Total Employee: {totalEmployees}", 5040, false, null, centerAlign: false), // Left-align
                CreateCell($"Total Deductions: {totalBenefits}", 5040, false, null, centerAlign: true), // Center-align
                CreateCell($"Grand Total: {grandTotal:N2}", 5040, false, null, rightAlign: true) // Right-align
            );
            summaryTable.Append(summaryRow);

            body.Append(summaryTable);
            body.Append(sectionProps); // Add section properties at the end

            mainPart.Document.Append(body);
            mainPart.Document.Save();
            document.Dispose();

            return stream.ToArray();
        }

        //private byte[] GenerateWordReport(List<ReportFilterResultDto> data)
        //{
        //    using var stream = new MemoryStream();
        //    using var document = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document);

        //    var mainPart = document.AddMainDocumentPart();
        //    mainPart.Document = new Document();
        //    var body = mainPart.Document.AppendChild(new Body());

        //    // Title
        //    var titleParagraph = new Paragraph();
        //    var titleRun = new Run();
        //    var titleText = new Text("Data Path - Salary Deduction Report");
        //    titleRun.AppendChild(new RunProperties(
        //        new Bold(),
        //        new FontSize() { Val = "32" }
        //    ));
        //    titleRun.AppendChild(titleText);
        //    titleParagraph.AppendChild(new ParagraphProperties(
        //        new Justification() { Val = JustificationValues.Center }
        //    ));
        //    titleParagraph.AppendChild(titleRun);
        //    body.AppendChild(titleParagraph);

        //    // Add spacing
        //    body.AppendChild(new Paragraph());

        //    var groupedData = data.GroupBy(x => x.DepartmentName ?? "Unknown Department")
        //                         .OrderBy(g => g.Key);

        //    decimal grandTotal = 0;
        //    int totalEmployees = 0;

        //    foreach (var departmentGroup in groupedData)
        //    {
        //        // Department header
        //        var deptParagraph = new Paragraph();
        //        var deptRun = new Run();
        //        deptRun.AppendChild(new RunProperties(new Bold()));
        //        deptRun.AppendChild(new Text($"Department: {departmentGroup.Key}"));
        //        deptParagraph.AppendChild(deptRun);
        //        body.AppendChild(deptParagraph);

        //        // Create table
        //        var table = new Table();

        //        // Table properties
        //        var tableProps = new TableProperties(
        //            new TableBorders(
        //                new TopBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 12 },
        //                new BottomBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 12 },
        //                new LeftBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 12 },
        //                new RightBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 12 },
        //                new InsideHorizontalBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
        //                new InsideVerticalBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 }
        //            )
        //        );
        //        table.AppendChild(tableProps);

        //        // Header row
        //        var headerRow = new TableRow();
        //        string[] headers = { "SL", "Employee ID", "Name", "Designation", "Branch",
        //       "Deduction Type", "Amount", "Month", "Year", "Remarks" };

        //        foreach (var header in headers)
        //        {
        //            var cell = new TableCell();
        //            var cellProps = new TableCellProperties(
        //                new Shading() { Val = ShadingPatternValues.Clear, Fill = "D3D3D3" }
        //            );
        //            cell.AppendChild(cellProps);

        //            var paragraph = new Paragraph();
        //            var run = new Run();
        //            run.AppendChild(new RunProperties(new Bold()));
        //            run.AppendChild(new Text(header));
        //            paragraph.AppendChild(run);
        //            cell.AppendChild(paragraph);
        //            headerRow.AppendChild(cell);
        //        }
        //        table.AppendChild(headerRow);

        //        // Data rows
        //        int serialNo = 1;
        //        decimal departmentTotal = 0;

        //        foreach (var item in departmentGroup)
        //        {
        //            var amount = item.DeductionAmount ?? 0;
        //            departmentTotal += amount;

        //            var dataRow = new TableRow();
        //            string[] values = {
        //        serialNo.ToString(),
        //        item.Code ?? "",
        //        item.Name ?? "",
        //        item.DesignationName ?? "",
        //        item.BranchName ?? "",
        //        item.DeductionType ?? "",
        //        amount.ToString("N2"),
        //        item.Month ?? "",
        //        item.Year ?? "",
        //        item.Remarks ?? ""
        //    };

        //            foreach (var value in values)
        //            {
        //                var cell = new TableCell();
        //                var paragraph = new Paragraph();
        //                var run = new Run();
        //                run.AppendChild(new Text(value));
        //                paragraph.AppendChild(run);
        //                cell.AppendChild(paragraph);
        //                dataRow.AppendChild(cell);
        //            }
        //            table.AppendChild(dataRow);
        //            serialNo++;
        //        }

        //        // Department total row
        //        var totalRow = new TableRow();
        //        for (int i = 0; i < 6; i++)
        //        {
        //            var cell = new TableCell();
        //            var paragraph = new Paragraph();
        //            var run = new Run();
        //            if (i == 5)
        //            {
        //                run.AppendChild(new RunProperties(new Bold()));
        //                run.AppendChild(new Text("Department Total:"));
        //            }
        //            else
        //            {
        //                run.AppendChild(new Text(""));
        //            }
        //            paragraph.AppendChild(run);
        //            cell.AppendChild(paragraph);
        //            totalRow.AppendChild(cell);
        //        }

        //        var totalCell = new TableCell();
        //        var totalCellProps = new TableCellProperties(
        //            new Shading() { Val = ShadingPatternValues.Clear, Fill = "FFFF00" }
        //        );
        //        totalCell.AppendChild(totalCellProps);
        //        var totalParagraph = new Paragraph();
        //        var totalRun = new Run();
        //        totalRun.AppendChild(new RunProperties(new Bold()));
        //        totalRun.AppendChild(new Text(departmentTotal.ToString("N2")));
        //        totalParagraph.AppendChild(totalRun);
        //        totalCell.AppendChild(totalParagraph);
        //        totalRow.AppendChild(totalCell);

        //        // Add remaining empty cells
        //        for (int i = 0; i < 3; i++)
        //        {
        //            var cell = new TableCell();
        //            cell.AppendChild(new Paragraph());
        //            totalRow.AppendChild(cell);
        //        }

        //        table.AppendChild(totalRow);
        //        body.AppendChild(table);
        //        body.AppendChild(new Paragraph()); // Spacing

        //        grandTotal += departmentTotal;
        //        totalEmployees += departmentGroup.Count();
        //    }

        //    // Summary
        //    var summaryParagraph = new Paragraph();
        //    var summaryRun = new Run();
        //    summaryRun.AppendChild(new RunProperties(new Bold()));
        //    summaryRun.AppendChild(new Text("Summary"));
        //    summaryParagraph.AppendChild(summaryRun);
        //    body.AppendChild(summaryParagraph);

        //    body.AppendChild(new Paragraph(new Run(new Text($"Total Departments: {groupedData.Count()}"))));
        //    body.AppendChild(new Paragraph(new Run(new Text($"Total Employees: {totalEmployees}"))));

        //    var grandTotalParagraph = new Paragraph();
        //    var grandTotalRun = new Run();
        //    grandTotalRun.AppendChild(new RunProperties(new Bold()));
        //    grandTotalRun.AppendChild(new Text($"Grand Total Deduction: {grandTotal:N2}"));
        //    grandTotalParagraph.AppendChild(grandTotalRun);
        //    body.AppendChild(grandTotalParagraph);

        //    mainPart.Document.Save();
        //    document.Dispose();

        //    return stream.ToArray();
        //}

        //private async Task<byte[]> GeneratePdfReport(List<ReportFilterResultDto> data)
        //{
        //    using var stream = new MemoryStream();
        //    using var writer = new PdfWriter(stream);
        //    using var pdf = new PdfDocument(writer);
        //    using var document = new PdfDocument2(pdf, iText.Kernel.Geom.PageSize.A4.Rotate());

        //    // Add title
        //    document.Add(new PdfParagraph("Data Path")
        //        .SetTextAlignment(PdfTextAlignment.CENTER)
        //        .SetFontSize(18)
        //        .SimulateBold());

        //    document.Add(new PdfParagraph("Salary Deduction Report")
        //        .SetTextAlignment(PdfTextAlignment.CENTER)
        //        .SetFontSize(16)
        //        .SimulateBold()
        //        .SetMarginBottom(20));

        //    // Group data by department
        //    var groupedData = data.GroupBy(x => x.DepartmentName ?? "Unknown Department")
        //                         .OrderBy(g => g.Key);

        //    decimal grandTotal = 0;
        //    int totalEmployees = 0;

        //    foreach (var departmentGroup in groupedData)
        //    {
        //        // Department header
        //        document.Add(new PdfParagraph($"Department: {departmentGroup.Key}")
        //            .SetFontSize(14)
        //            .SimulateBold()
        //            .SetMarginTop(15)
        //            .SetMarginBottom(10));

        //        // Create table
        //        var table = new PdfTable(new float[] { 1, 2, 3, 2, 2, 2, 2, 1.5f, 1, 3 });
        //        table.SetWidth(UnitValue.CreatePercentValue(100));

        //        // Add headers
        //        string[] headers = { "SL", "Employee ID", "Name", "Designation", "Branch",
        //                       "Deduction Type", "Amount", "Month", "Year", "Remarks" };

        //        foreach (var header in headers)
        //        {
        //            table.AddHeaderCell(new Cell().Add(new PdfParagraph(header).SimulateBold())
        //                .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
        //                .SetTextAlignment(PdfTextAlignment.CENTER));
        //        }

        //        // Add data rows
        //        int serialNo = 1;
        //        decimal departmentTotal = 0;

        //        foreach (var item in departmentGroup)
        //        {
        //            var amount = item.DeductionAmount ?? 0;
        //            departmentTotal += amount;

        //            table.AddCell(new Cell().Add(new PdfParagraph(serialNo.ToString()))
        //                .SetTextAlignment(PdfTextAlignment.CENTER));
        //            table.AddCell(new Cell().Add(new PdfParagraph(item.Code ?? "")));
        //            table.AddCell(new Cell().Add(new PdfParagraph(item.Name ?? "")));
        //            table.AddCell(new Cell().Add(new PdfParagraph(item.DesignationName ?? "")));
        //            table.AddCell(new Cell().Add(new PdfParagraph(item.BranchName ?? "")));
        //            table.AddCell(new Cell().Add(new PdfParagraph(item.DeductionType ?? "")));
        //            table.AddCell(new Cell().Add(new PdfParagraph(amount.ToString("N2")))
        //                .SetTextAlignment(PdfTextAlignment.RIGHT));
        //            table.AddCell(new Cell().Add(new PdfParagraph(item.Month ?? "")));
        //            table.AddCell(new Cell().Add(new PdfParagraph(item.Year ?? "")));
        //            table.AddCell(new Cell().Add(new PdfParagraph(item.Remarks ?? "")));

        //            serialNo++;
        //        }

        //        // Add department total row
        //        table.AddCell(new Cell(1, 6).Add(new PdfParagraph("Department Total:").SimulateBold())
        //            .SetTextAlignment(PdfTextAlignment.RIGHT)
        //            .SetBackgroundColor(ColorConstants.YELLOW));
        //        table.AddCell(new Cell().Add(new PdfParagraph(departmentTotal.ToString("N2")).SimulateBold())
        //            .SetTextAlignment(PdfTextAlignment.RIGHT)
        //            .SetBackgroundColor(ColorConstants.YELLOW));
        //        table.AddCell(new Cell(1, 3).Add(new PdfParagraph(""))
        //            .SetBackgroundColor(ColorConstants.YELLOW));

        //        document.Add(table);

        //        grandTotal += departmentTotal;
        //        totalEmployees += departmentGroup.Count();
        //    }

        //    // Add summary
        //    document.Add(new PdfParagraph($"Summary")
        //        .SetFontSize(14)
        //        .SimulateBold()
        //        .SetMarginTop(20));

        //    document.Add(new PdfParagraph($"Total Departments: {groupedData.Count()}")
        //        .SetMarginBottom(5));
        //    document.Add(new PdfParagraph($"Total Employees: {totalEmployees}")
        //        .SetMarginBottom(5));
        //    document.Add(new PdfParagraph($"Grand Total: {grandTotal:N2}")
        //        .SimulateBold()
        //        .SetMarginBottom(5));

        //    document.Close();
        //    return stream.ToArray();
        //}

        //private async Task<byte[]> GenerateExcelReport(List<ReportFilterResultDto> data)
        //{
        //    using var workbook = new XLWorkbook();
        //    var worksheet = workbook.Worksheets.Add("Salary Deduction Report");

        //    // Title
        //    var titleRange = worksheet.Range("A1:J2");
        //    titleRange.Merge().Value = "Data Path - Salary Deduction Report";
        //    titleRange.Style.Font.FontSize = 16;
        //    titleRange.Style.Font.Bold = true;
        //    titleRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        //    int currentRow = 4;
        //    var groupedData = data.GroupBy(x => x.DepartmentName ?? "Unknown Department")
        //                         .OrderBy(g => g.Key);

        //    decimal grandTotal = 0;
        //    int totalEmployees = 0;

        //    foreach (var departmentGroup in groupedData)
        //    {
        //        // Department header
        //        worksheet.Cell(currentRow, 1).Value = $"Department: {departmentGroup.Key}";
        //        worksheet.Range(currentRow, 1, currentRow, 10).Merge();
        //        worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
        //        worksheet.Cell(currentRow, 1).Style.Fill.BackgroundColor = XLColor.LightBlue;
        //        currentRow += 2;

        //        // Headers
        //        string[] headers = { "SL", "Employee ID", "Name", "Designation", "Branch",
        //                       "Deduction Type", "Amount", "Month", "Year", "Remarks" };

        //        for (int i = 0; i < headers.Length; i++)
        //        {
        //            var cell = worksheet.Cell(currentRow, i + 1);
        //            cell.Value = headers[i];
        //            cell.Style.Font.Bold = true;
        //            cell.Style.Fill.BackgroundColor = XLColor.Gray;
        //            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        //        }
        //        currentRow++;

        //        // Data rows
        //        int serialNo = 1;
        //        decimal departmentTotal = 0;

        //        foreach (var item in departmentGroup)
        //        {
        //            var amount = item.DeductionAmount ?? 0;
        //            departmentTotal += amount;

        //            worksheet.Cell(currentRow, 1).Value = serialNo;
        //            worksheet.Cell(currentRow, 2).Value = item.Code ?? "";
        //            worksheet.Cell(currentRow, 3).Value = item.Name ?? "";
        //            worksheet.Cell(currentRow, 4).Value = item.DesignationName ?? "";
        //            worksheet.Cell(currentRow, 5).Value = item.BranchName ?? "";
        //            worksheet.Cell(currentRow, 6).Value = item.DeductionType ?? "";
        //            worksheet.Cell(currentRow, 7).Value = amount;
        //            worksheet.Cell(currentRow, 7).Style.NumberFormat.Format = "#,##0.00";
        //            worksheet.Cell(currentRow, 8).Value = item.Month ?? "";
        //            worksheet.Cell(currentRow, 9).Value = item.Year ?? "";
        //            worksheet.Cell(currentRow, 10).Value = item.Remarks ?? "";

        //            currentRow++;
        //            serialNo++;
        //        }

        //        // Department total
        //        worksheet.Cell(currentRow, 6).Value = "Department Total:";
        //        worksheet.Cell(currentRow, 6).Style.Font.Bold = true;
        //        worksheet.Cell(currentRow, 7).Value = departmentTotal;
        //        worksheet.Cell(currentRow, 7).Style.Font.Bold = true;
        //        worksheet.Cell(currentRow, 7).Style.NumberFormat.Format = "#,##0.00";
        //        worksheet.Cell(currentRow, 7).Style.Fill.BackgroundColor = XLColor.Yellow;

        //        currentRow += 3;
        //        grandTotal += departmentTotal;
        //        totalEmployees += departmentGroup.Count();
        //    }

        //    // Summary
        //    currentRow += 2;
        //    worksheet.Cell(currentRow, 1).Value = "Summary";
        //    worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
        //    worksheet.Cell(currentRow, 1).Style.Font.FontSize = 14;
        //    currentRow++;

        //    worksheet.Cell(currentRow, 1).Value = $"Total Departments: {groupedData.Count()}";
        //    currentRow++;
        //    worksheet.Cell(currentRow, 1).Value = $"Total Employees: {totalEmployees}";
        //    currentRow++;
        //    worksheet.Cell(currentRow, 1).Value = $"Grand Total Deduction: {grandTotal:N2}";
        //    worksheet.Cell(currentRow, 1).Style.Font.Bold = true;

        //    // Auto-fit columns
        //    worksheet.Columns().AdjustToContents();

        //    using var stream = new MemoryStream();
        //    workbook.SaveAs(stream);
        //    return stream.ToArray();
        //}

        //private async Task<byte[]> GenerateWordReport(List<ReportFilterResultDto> data)
        //{
        //    using var stream = new MemoryStream();
        //    using var document = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document);

        //    var mainPart = document.AddMainDocumentPart();
        //    mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document();
        //    var body = mainPart.Document.AppendChild(new Body());

        //    // Title
        //    var titleParagraph = new WordParagraph();
        //    var titleRun = new Run();
        //    var titleText = new WordText("Data Path - Salary Deduction Report");
        //    titleRun.AppendChild(new RunProperties(
        //        new Bold(),
        //        new FontSize() { Val = "32" }
        //    ));
        //    titleRun.AppendChild(titleText);
        //    titleParagraph.AppendChild(new ParagraphProperties(
        //        new Justification() { Val = JustificationValues.Center }
        //    ));
        //    titleParagraph.AppendChild(titleRun);
        //    body.AppendChild(titleParagraph);

        //    // Add spacing
        //    body.AppendChild(new WordParagraph());

        //    var groupedData = data.GroupBy(x => x.DepartmentName ?? "Unknown Department")
        //                         .OrderBy(g => g.Key);

        //    decimal grandTotal = 0;
        //    int totalEmployees = 0;

        //    foreach (var departmentGroup in groupedData)
        //    {
        //        // Department header
        //        var deptParagraph = new WordParagraph();
        //        var deptRun = new Run();
        //        deptRun.AppendChild(new RunProperties(new Bold()));
        //        deptRun.AppendChild(new WordText($"Department: {departmentGroup.Key}"));
        //        deptParagraph.AppendChild(deptRun);
        //        body.AppendChild(deptParagraph);

        //        // Create table
        //        var table = new WordTable();

        //        // Table properties
        //        var tableProps = new TableProperties(
        //            new TableBorders(
        //                new TopBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 12 },
        //                new BottomBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 12 },
        //                new LeftBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 12 },
        //                new RightBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 12 },
        //                new InsideHorizontalBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
        //                new InsideVerticalBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 }
        //            )
        //        );
        //        table.AppendChild(tableProps);

        //        // Header row
        //        var headerRow = new TableRow();
        //        string[] headers = { "SL", "Employee ID", "Name", "Designation", "Branch",
        //                       "Deduction Type", "Amount", "Month", "Year", "Remarks" };

        //        foreach (var header in headers)
        //        {
        //            var cell = new TableCell();
        //            var cellProps = new TableCellProperties(
        //                new Shading() { Val = ShadingPatternValues.Clear, Fill = "D3D3D3" }
        //            );
        //            cell.AppendChild(cellProps);

        //            var paragraph = new WordParagraph();
        //            var run = new Run();
        //            run.AppendChild(new RunProperties(new Bold()));
        //            run.AppendChild(new WordText(header));
        //            paragraph.AppendChild(run);
        //            cell.AppendChild(paragraph);
        //            headerRow.AppendChild(cell);
        //        }
        //        table.AppendChild(headerRow);

        //        // Data rows
        //        int serialNo = 1;
        //        decimal departmentTotal = 0;

        //        foreach (var item in departmentGroup)
        //        {
        //            var amount = item.DeductionAmount ?? 0;
        //            departmentTotal += amount;

        //            var dataRow = new TableRow();
        //            string[] values = {
        //            serialNo.ToString(),
        //            item.Code ?? "",
        //            item.Name ?? "",
        //            item.DesignationName ?? "",
        //            item.BranchName ?? "",
        //            item.DeductionType ?? "",
        //            amount.ToString("N2"),
        //            item.Month ?? "",
        //            item.Year ?? "",
        //            item.Remarks ?? ""
        //        };

        //            foreach (var value in values)
        //            {
        //                var cell = new TableCell();
        //                var paragraph = new WordParagraph();
        //                var run = new Run();
        //                run.AppendChild(new WordText(value));
        //                paragraph.AppendChild(run);
        //                cell.AppendChild(paragraph);
        //                dataRow.AppendChild(cell);
        //            }
        //            table.AppendChild(dataRow);
        //            serialNo++;
        //        }

        //        // Department total row
        //        var totalRow = new TableRow();
        //        for (int i = 0; i < 6; i++)
        //        {
        //            var cell = new TableCell();
        //            var paragraph = new WordParagraph();
        //            var run = new Run();
        //            if (i == 5)
        //            {
        //                run.AppendChild(new RunProperties(new Bold()));
        //                run.AppendChild(new WordText("Department Total:"));
        //            }
        //            else
        //            {
        //                run.AppendChild(new WordText(""));
        //            }
        //            paragraph.AppendChild(run);
        //            cell.AppendChild(paragraph);
        //            totalRow.AppendChild(cell);
        //        }

        //        var totalCell = new TableCell();
        //        var totalCellProps = new TableCellProperties(
        //            new Shading() { Val = ShadingPatternValues.Clear, Fill = "FFFF00" }
        //        );
        //        totalCell.AppendChild(totalCellProps);
        //        var totalParagraph = new WordParagraph();
        //        var totalRun = new Run();
        //        totalRun.AppendChild(new RunProperties(new Bold()));
        //        totalRun.AppendChild(new WordText(departmentTotal.ToString("N2")));
        //        totalParagraph.AppendChild(totalRun);
        //        totalCell.AppendChild(totalParagraph);
        //        totalRow.AppendChild(totalCell);

        //        // Add remaining empty cells
        //        for (int i = 0; i < 3; i++)
        //        {
        //            var cell = new TableCell();
        //            cell.AppendChild(new WordParagraph());
        //            totalRow.AppendChild(cell);
        //        }

        //        table.AppendChild(totalRow);
        //        body.AppendChild(table);
        //        body.AppendChild(new WordParagraph()); // Spacing

        //        grandTotal += departmentTotal;
        //        totalEmployees += departmentGroup.Count();
        //    }

        //    // Summary
        //    var summaryParagraph = new WordParagraph();
        //    var summaryRun = new Run();
        //    summaryRun.AppendChild(new RunProperties(new Bold()));
        //    summaryRun.AppendChild(new WordText("Summary"));
        //    summaryParagraph.AppendChild(summaryRun);
        //    body.AppendChild(summaryParagraph);

        //    body.AppendChild(new WordParagraph(new Run(new WordText($"Total Departments: {groupedData.Count()}"))));
        //    body.AppendChild(new WordParagraph(new Run(new WordText($"Total Employees: {totalEmployees}"))));

        //    var grandTotalParagraph = new WordParagraph();
        //    var grandTotalRun = new Run();
        //    grandTotalRun.AppendChild(new RunProperties(new Bold()));
        //    grandTotalRun.AppendChild(new WordText($"Grand Total Deduction: {grandTotal:N2}"));
        //    grandTotalParagraph.AppendChild(grandTotalRun);
        //    body.AppendChild(grandTotalParagraph);

        //    mainPart.Document.Save();
        //    document.Dispose();

        //    return stream.ToArray();
        //}
    }
}