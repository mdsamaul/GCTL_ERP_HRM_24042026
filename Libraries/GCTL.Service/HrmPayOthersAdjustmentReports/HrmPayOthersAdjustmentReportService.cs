using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Office2013.Excel;
using GCTL.Core.Data;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.HrmPayOthersAdjustmentEntries;
using GCTL.Data.Models;
using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using iText.Pdfa;
//using DocumentFormat.OpenXml.Wordprocessing;

namespace GCTL.Service.HrmPayOthersAdjustmentReports
{
    public class HrmPayOthersAdjustmentReportService:AppService<HrmPayOthersAdjustmentEntry>, IHrmPayOthersAdjustmentReportService
    {
        private readonly IRepository<HrmPayOthersAdjustmentEntry> entryRepo;
        private readonly IRepository<HrmEmployee> employeeRepo;
        private readonly IRepository<HrmEmployeeOfficialInfo> empOffRepo;
        private readonly IRepository<HrmDefDesignation> desiRepo;
        private readonly IRepository<HrmDefDepartment> depRepo;
        private readonly IRepository<CoreBranch> branchRepo;
        private readonly IRepository<CoreCompany> companyRepo;
        private readonly IRepository<HrmPayDefBenefitType> typeRepo;
        private readonly IRepository<HrmPayMonth> monthRepo;

        public HrmPayOthersAdjustmentReportService(
            IRepository<HrmPayOthersAdjustmentEntry> entryRepo,
            IRepository<HrmEmployee> employeeRepo,
            IRepository<HrmEmployeeOfficialInfo> empOffRepo,
            IRepository<HrmDefDesignation> desiRepo,
            IRepository<HrmDefDepartment> depRepo,
            IRepository<CoreBranch> branchRepo, IRepository<CoreCompany>
            companyRepo, IRepository<HrmPayDefBenefitType> typeRepo,
            IRepository<HrmPayMonth> monthRepo
        ):base(entryRepo)
        {
            this.entryRepo = entryRepo;
            this.employeeRepo = employeeRepo;
            this.empOffRepo = empOffRepo;
            this.desiRepo = desiRepo;
            this.depRepo = depRepo;
            this.branchRepo = branchRepo;
            this.companyRepo = companyRepo;
            this.typeRepo = typeRepo;
            this.monthRepo = monthRepo;
        }

        public async Task<byte[]> GenerateExcelReport(List<ReportFilterResultDto> data)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Other benefit Report");

            var months = data
                .Select(d => $"{d.Month} {d.Year}")
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct()
                .ToList();

            var reportPeriod = string.Join(", ", months);
            try
            {
                string logoPath = "wwwroot/images/DP_logo.png"; ;
                
                if (!string.IsNullOrEmpty(logoPath))
                {
                    var logoFileInfo = new System.IO.FileInfo(logoPath);
                    var excelImage = worksheet.Drawings.AddPicture("CompanyLogo", logoFileInfo);

                    excelImage.SetPosition(0, 5, 0, 5); 
                    excelImage.SetSize(120, 42); 
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not load logo: {ex.Message}");
                // Continue without logo if there's an error
            }
            worksheet.Row(1).Height = 50;
            worksheet.Cells["A1:H1"].Merge = true;
            worksheet.Cells["A1"].Value = data.Select(c=>c.CompanyName);
            worksheet.Cells["A1"].Style.Font.Size = 16;
            worksheet.Cells["A1"].Style.Font.Bold = true;
            worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            worksheet.Cells["A2:H2"].Merge = true;
            worksheet.Cells["A2"].Value = "Other Benefit Report";
            worksheet.Cells["A2"].Style.Font.Size = 16;
            worksheet.Cells["A2"].Style.Font.Bold = true;
            worksheet.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            worksheet.Cells["A3:H3"].Merge = true;
            worksheet.Cells["A3"].Value = $"For the month of {reportPeriod}";
            worksheet.Cells["A3"].Style.Font.Size = 14;
            worksheet.Cells["A3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            int currentRow = 5;
            var groupedData = data.GroupBy(x => x.DepartmentName ?? "Unknown Department")
                                  .OrderBy(g => g.Key);

            decimal grandTotal = 0;
            int totalEmployees = 0;
            int totalDeductions = 0;

            foreach (var departmentGroup in groupedData)
            {
                worksheet.Cells[currentRow, 1, currentRow, 8].Merge = true;
                worksheet.Cells[currentRow, 1].Value = $"Department: {departmentGroup.Key}";
                worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
                worksheet.Cells[currentRow, 1].Style.Font.Size = 12;
                currentRow++;

                string[] headers = { "SL No.", "Employee ID", "Name", "Designation", "Branch", "Benefit Type", "Amount", "Remarks" };

                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cells[currentRow, i + 1];
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                currentRow++;

                int serialNo = 1;
                decimal departmentTotal = 0;
                var uniqueEmployees = new HashSet<string>();

                foreach (var item in departmentGroup.OrderBy(x => x.Code))
                {
                    var amount = item.BenefitAmount ?? 0;
                    departmentTotal += amount;
                    totalDeductions++;

                    if (!string.IsNullOrEmpty(item.Code))
                        uniqueEmployees.Add(item.Code);

                    worksheet.Cells[currentRow, 1].Value = serialNo;
                    worksheet.Cells[currentRow, 2].Value = item.Code ?? "";
                    worksheet.Cells[currentRow, 3].Value = item.Name ?? "";
                    worksheet.Cells[currentRow, 4].Value = item.DesignationName ?? "";
                    worksheet.Cells[currentRow, 5].Value = item.BranchName ?? "";
                    worksheet.Cells[currentRow, 6].Value = item.BenefitType ?? "";
                    worksheet.Cells[currentRow, 7].Value = amount;
                    worksheet.Cells[currentRow, 7].Style.Numberformat.Format = "#,##0.00";
                    worksheet.Cells[currentRow, 8].Value = item.Remarks ?? "";

                    for (int col = 1; col <= 8; col++)
                    {
                        worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    }
                    currentRow++;
                    serialNo++;
                }
                //for (int col = 1; col <= 10; col++)
                //{
                //    worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                //}
                worksheet.Cells[currentRow, 6].Value = "Total:";
                worksheet.Cells[currentRow, 6].Style.Font.Bold = true;
                worksheet.Cells[currentRow, 7].Value = departmentTotal;
                worksheet.Cells[currentRow, 7].Style.Font.Bold = true;
                worksheet.Cells[currentRow, 7].Style.Numberformat.Format = "#,##0.00";

                currentRow += 2;

                grandTotal += departmentTotal;
                totalEmployees += uniqueEmployees.Count;
            }

            worksheet.Cells[currentRow, 1].Value = "No. of Employee:";
            worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
            //worksheet.Cells[currentRow, 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);

            worksheet.Cells[currentRow, 2].Value = totalEmployees;
            //worksheet.Cells[currentRow, 2].Style.Border.BorderAround(ExcelBorderStyle.Thin);

            worksheet.Cells[currentRow, 4].Value = $"Total Other Benefit: {totalDeductions}";
            worksheet.Cells[currentRow, 4].Style.Font.Bold = true;
            //worksheet.Cells[currentRow, 4].Style.Border.BorderAround(ExcelBorderStyle.Thin);

            worksheet.Cells[currentRow, 6].Value = "Grand Total:";
            worksheet.Cells[currentRow, 6].Style.Font.Bold = true;
            //worksheet.Cells[currentRow, 6].Style.Border.BorderAround(ExcelBorderStyle.Thin);

            worksheet.Cells[currentRow, 7].Value = grandTotal;
            worksheet.Cells[currentRow, 7].Style.Numberformat.Format = "#,##0.00";
            //worksheet.Cells[currentRow, 7].Style.Border.BorderAround(ExcelBorderStyle.Thin);

            currentRow++;

            worksheet.Cells.AutoFitColumns();

            return package.GetAsByteArray();
        }

        public async Task<byte[]> GeneratePdfReport(List<ReportFilterResultDto> data, BaseViewModel model)
        {
            using var stream = new MemoryStream();
            using var writer = new PdfWriter(stream);
            using var pdf = new PdfDocument(writer);
            using var document = new iText.Layout.Document(pdf, iText.Kernel.Geom.PageSize.A4.Rotate());

            document.SetMargins(80, 36, 60, 36);

            var months = data
                .Select(d => $"{d.Month} {d.Year}")
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct()
                .ToList();

            var reportPeriod = string.Join(", ", months);

            // Group data by department
            var groupedData = data
                .GroupBy(x => x.DepartmentName ?? "Unknown Department")
                .OrderBy(g => g.Key);

            decimal grandTotal = 0;
            int totalEmployees = 0;
            int totalDeductions = 0;

            foreach (var departmentGroup in groupedData)
            {
                // Check available space and add page break if needed
                float pageHeight = document.GetPdfDocument().GetDefaultPageSize().GetHeight();
                float topMargin = document.GetTopMargin();
                float bottomMargin = document.GetBottomMargin();
                float availableHeight;

                try
                {
                    var currentArea = document.GetRenderer().GetCurrentArea();
                    if (currentArea != null && currentArea.GetBBox() != null)
                    {
                        var bbox = currentArea.GetBBox();
                        availableHeight = bbox.GetHeight();
                    }
                    else
                    {
                        availableHeight = pageHeight - topMargin - bottomMargin;
                    }
                }
                catch
                {
                    availableHeight = pageHeight - topMargin - bottomMargin;
                }

                // If less than 30% space available and we have content, add page break
                if (availableHeight < (pageHeight - topMargin - bottomMargin) * 0.30f && grandTotal > 0)
                {
                    document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
                }

                // Updated column widths to accommodate 9 columns (added Paid Days)
                float[] columnWidths = new float[]
                {
                    3.5f,   // SN
                    9.5f,   // Employee ID
                    19.0f,  // Name
                    19.0f,  // Designation
                    10.5f,  // Branch
                    10.5f,  // Benefit Type
                    7.0f,   // Amount
                    7.0f,   // Paid Days
                    14.0f   // Remarks
                };

                var table = new Table(UnitValue.CreatePercentArray(columnWidths));
                table.SetWidth(UnitValue.CreatePercentValue(100));

                string[] headers = { "SN", "Employee ID", "Name", "Designation", "Branch", "Benefit Type", "Amount", "Paid Days", "Remarks" };

                // Create a custom header that includes department name for continuation pages
                var departmentHeaderRow = new Cell(1, 9) // Updated to span 9 columns
                    .Add(new Paragraph($"Department: {departmentGroup.Key}"))
                    .SetFontSize(11)
                    .SetTextAlignment(TextAlignment.LEFT)
                    .SetPadding(5)
                    .SetBorderTop(iText.Layout.Borders.Border.NO_BORDER)
                    .SetBorderLeft(iText.Layout.Borders.Border.NO_BORDER)
                    .SetBorderRight(iText.Layout.Borders.Border.NO_BORDER)
                    .SetBorderBottom(iText.Layout.Borders.Border.NO_BORDER);

                // Add department header as a header cell (will repeat on new pages)
                table.AddHeaderCell(departmentHeaderRow);

                // Create column header cells with thinner borders
                foreach (var header in headers)
                {
                    var headerCell = new Cell().Add(new Paragraph(header).SimulateBold().SetFontSize(7))
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 0.2f)); // Thinner border (0.1f instead of 0.2f)
                    table.AddHeaderCell(headerCell);
                }

                // Enable header repetition on new pages
                table.SetSkipFirstHeader(false);
                table.SetSkipLastFooter(false);

                int serialNo = 1;
                decimal departmentTotal = 0;
                var uniqueEmployees = new HashSet<string>();
                var orderedItems = departmentGroup.OrderBy(x => x.Code).ToList();

                foreach (var item in orderedItems)
                {
                    var amount = item.BenefitAmount ?? 0;
                    departmentTotal += amount;
                    totalDeductions++;

                    if (!string.IsNullOrEmpty(item.Code))
                        uniqueEmployees.Add(item.Code);

                    // Updated values array to include Paid Days
                    var values = new[]
                    {
                        serialNo.ToString(),
                        item.Code ?? "",
                        item.Name ?? "",
                        item.DesignationName ?? "",
                        item.BranchName ?? "",
                        item.BenefitType ?? "",
                        amount.ToString("N2"),
                        Math.Truncate(item.PaidDays ?? 0).ToString(),
                        item.Remarks ?? ""
                    };

                    for (int j = 0; j < values.Length; j++)
                    {
                        var cell = new Cell().Add(new Paragraph(values[j]))
                            .SetFontSize(7)
                            .SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 0.2f)); // Thinner border

                        // Center align for specific columns (SN, Employee ID, Benefit Type, Amount, Paid Days)
                        if (j == 0 || j == 1 || j == 5 || j == 6 || j == 7)
                        {
                            cell.SetTextAlignment(TextAlignment.CENTER);
                        }
                        // Right align Amount column
                        else if (j == 6) // Amount column
                        {
                            cell.SetTextAlignment(TextAlignment.RIGHT);
                        }
                        // Left align for Name, Designation, Branch, Remarks (j == 2, 3, 4, 8)

                        table.AddCell(cell);
                    }

                    serialNo++;
                }

                // Add total row - updated for 9 columns
                for (int i = 0; i < 5; i++) // 6 empty cells before "Total:"
                {
                    var emptyCell = new Cell().Add(new Paragraph(""))
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                        .SetFontSize(7);
                    table.AddCell(emptyCell);
                }

                var totalLabelCell = new Cell().Add(new Paragraph("Total:"))
                    .SetTextAlignment(TextAlignment.RIGHT)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .SetFontSize(8)
                    .SimulateBold();
                table.AddCell(totalLabelCell);

                var amountCell = new Cell().Add(new Paragraph($"{departmentTotal:N2}"))
                    .SetTextAlignment(TextAlignment.RIGHT)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .SetFontSize(8);
                table.AddCell(amountCell);

                // One empty cell at the end
                for (int i = 0; i < 2; i++)
                {
                    var emptyCell = new Cell().Add(new Paragraph(""))
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                        .SetFontSize(7);
                    table.AddCell(emptyCell);
                }

                document.Add(table);

                grandTotal += departmentTotal;
                totalEmployees += uniqueEmployees.Count;
            }

            document.Add(new Paragraph("Summary")
                .SetFontSize(12)
                .SimulateBold()
                .SetMarginTop(20)
                .SetMarginBottom(10));

            var summaryTable = new Table(new float[] { 1, 1, 1 });
            summaryTable.SetWidth(UnitValue.CreatePercentValue(100));

            summaryTable.AddCell(new Cell().Add(new Paragraph($"No. of Employee: {totalEmployees}"))
                .SetTextAlignment(TextAlignment.LEFT)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER));
                    
            summaryTable.AddCell(new Cell().Add(new Paragraph($"Grand Total: {grandTotal:N2}"))
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

            document.Add(summaryTable);

            document.Close();

            // Rest of the code for adding headers and footers remains the same
            using var readerStream = new MemoryStream(stream.ToArray());
            using var reader = new PdfReader(readerStream);
            using var outputStream = new MemoryStream();
            using var outputWriter = new PdfWriter(outputStream);
            using var outputPdf = new PdfDocument(reader, outputWriter);

            var totalPages = outputPdf.GetNumberOfPages();

            for (int i = 1; i <= totalPages; i++)
            {
                var page = outputPdf.GetPage(i);
                var pageSize = page.GetPageSize();
                var canvas = new PdfCanvas(page);

                try
                {
                    string logoPath = "wwwroot/images/DP_logo.png";
                    var imageData = ImageDataFactory.Create(logoPath);
                    var logo = new Image(imageData);

                    float logoWidth = 90;
                    float logoHeight = 25;
                    float logoX = 30; // Left margin
                    float logoY = pageSize.GetTop() - 35;

                    logo.SetFixedPosition(i, logoX, logoY);
                    logo.ScaleAbsolute(logoWidth, logoHeight);

                    var logoCanvas = new Canvas(canvas, pageSize);
                    logoCanvas.Add(logo);
                    logoCanvas.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not load logo: {ex.Message}");
                }

                float headerStartX = pageSize.GetWidth() / 2;
                float headerTopY = pageSize.GetTop() - 25;

                var companyFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                float companyFontSize = 14;
                string companyText = data.Select(c => c.CompanyName).Distinct().FirstOrDefault();
                float companyTextWidth = companyFont.GetWidth(companyText, companyFontSize);

                canvas.BeginText()
                    .SetFontAndSize(companyFont, companyFontSize)
                    .MoveText(headerStartX - (companyTextWidth / 2), headerTopY)
                    .ShowText(companyText)
                    .EndText();

                var reportFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                float reportFontSize = 12;
                string reportText = "Other Benefit Report";
                float reportTextWidth = reportFont.GetWidth(reportText, reportFontSize);

                canvas.BeginText()
                    .SetFontAndSize(reportFont, reportFontSize)
                    .MoveText(headerStartX - (reportTextWidth / 2), headerTopY - 18)
                    .ShowText(reportText)
                    .EndText();

                var periodFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                float periodFontSize = 10;
                string periodText = $"For the month of {reportPeriod}";
                float periodTextWidth = periodFont.GetWidth(periodText, periodFontSize);

                float periodTextX = headerStartX - (periodTextWidth / 2);
                float periodTextY = headerTopY - 35;

                canvas.BeginText()
                    .SetFontAndSize(periodFont, periodFontSize)
                    .MoveText(periodTextX, periodTextY)
                    .ShowText(periodText)
                    .EndText();

                float underlineY = periodTextY - 2;
                canvas.SetLineWidth(0.5f);
                canvas.MoveTo(periodTextX, underlineY)
                      .LineTo(periodTextX + periodTextWidth, underlineY)
                      .Stroke();

                var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                float fontSize = 8;
                float margin = 36;
                float yPosition = pageSize.GetBottom() + 20;
                float pageWidth = pageSize.GetWidth();

                canvas.BeginText()
                    .SetFontAndSize(font, fontSize)
                    .MoveText(margin, yPosition)
                    .ShowText($"Print Datetime: {(model.Ldate.HasValue ? model.Ldate.Value.ToString("dd/MM/yyyy hh:mm:ss tt") : DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt"))}")
                    .EndText();

                string userText = $"Printed By: {model.Luser}";
                float userTextWidth = font.GetWidth(userText, fontSize);
                canvas.BeginText()
                    .SetFontAndSize(font, fontSize)
                    .MoveText((pageWidth - userTextWidth) / 2, yPosition)
                    .ShowText(userText)
                    .EndText();

                string pageText = $"Page {i} of {totalPages}";
                float pageTextWidth = font.GetWidth(pageText, fontSize);
                canvas.BeginText()
                    .SetFontAndSize(font, fontSize)
                    .MoveText(pageWidth - margin - pageTextWidth, yPosition)
                    .ShowText(pageText)
                    .EndText();

                canvas.Release();
            }

            outputPdf.Close();
            return outputStream.ToArray();
        }


        //public async Task<byte[]> GeneratePdfReport(List<ReportFilterResultDto> data, BaseViewModel model)
        //{
        //    using var stream = new MemoryStream();
        //    using var writer = new PdfWriter(stream);
        //    using var pdf = new PdfDocument(writer);
        //    using var document = new Document(pdf, iText.Kernel.Geom.PageSize.A4/*.Rotate()*/);

        //    document.SetMargins(80, 36, 60, 36);

        //    var months = data
        //        .Select(d => $"{d.Month} {d.Year}")
        //        .Where(s => !string.IsNullOrEmpty(s))
        //        .Distinct()
        //        .ToList();

        //    var reportPeriod = string.Join(", ", months);

        //    var groupData = data
        //        .GroupBy(x => x.DepartmentName ?? "Unknown Department")
        //        .OrderBy(g => g.Key);

        //    decimal grandTotal = 0;
        //    int totalEmployees = 0;
        //    int totalBenefits = 0;

        //    foreach (var departmentGroup in groupData)
        //    {
        //        var departmentContainer = new Div();

        //        float pageHeight = document.GetPdfDocument().GetDefaultPageSize().GetHeight();
        //        float topMargin = document.GetTopMargin();
        //        float bottomMargin = document.GetBottomMargin();
        //        float availableHeight;
        //        try
        //        {
        //            var currentArea = document.GetRenderer().GetCurrentArea();
        //            if (currentArea != null && currentArea.GetBBox() != null)
        //            {
        //                var bbox = currentArea.GetBBox();
        //                availableHeight = bbox.GetHeight();
        //            }
        //            else
        //            {
        //                availableHeight = pageHeight - topMargin - bottomMargin;
        //            }
        //        }
        //        catch
        //        {
        //            availableHeight = pageHeight - topMargin - bottomMargin;

        //        }

        //        if (availableHeight < (pageHeight - topMargin - bottomMargin) * 0.2f && grandTotal > 0)
        //        {
        //            document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
        //        }

        //        var departmentHeader = new Paragraph($"Department: {departmentGroup.Key}")
        //            .SetFontSize(10)
        //            .SimulateBold()
        //            .SetMarginTop(15)
        //            .SetMarginBottom(10)
        //            .SetKeepWithNext(true);

        //        departmentContainer.Add(departmentHeader);

        //        float[] columnWidths = new float[]
        //        {
        //            30f,   // SL
        //            60f,   // Employee ID
        //            130f,  // Name
        //            130f,  // Designation
        //            80f,   // Branch
        //            80f,   // Benefit Type
        //            70f,   // Amount
        //            //40f,   // Month
        //            //40f,   // Year
        //            100f   // Remarks
        //        };

        //        var table = new Table(UnitValue.CreatePointArray(columnWidths));
        //        table.SetWidth(UnitValue.CreatePointValue(columnWidths.Sum()));
        //        table.SetKeepTogether(true);

        //        string[] headers = { "SN", "Employee ID", "Name", "Designation", "Branch", "Benefit Type", "Amount",/* "Month", "Year", */"Remarks" };

        //        foreach (var header in headers) 
        //        {
        //            table.AddHeaderCell(new Cell().Add(new Paragraph(header).SimulateBold().SetFontSize(7))
        //                .SetTextAlignment(TextAlignment.CENTER)
        //                .SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
        //        }

        //        int serialNo = 1;
        //        decimal departmentTotal = 0;
        //        var uniqueEmployees = new HashSet<string>();
        //        var orderedItems = departmentGroup.OrderBy(x => x.Code).ToList();

        //        foreach (var item in orderedItems)
        //        {
        //            var amount = item.BenefitAmount ?? 0;
        //            departmentTotal += amount;
        //            totalBenefits++;

        //            if (!string.IsNullOrEmpty(item.Code))
        //                uniqueEmployees.Add(item.Code);

        //            var values = new[]
        //            {
        //                serialNo.ToString(),
        //                item.Code ?? "",
        //                item.Name ?? "",
        //                item.DesignationName ?? "",
        //                item.BranchName ?? "",
        //                item.BenefitType ?? "",
        //                amount.ToString("N2"),
        //                item.Month ?? "",
        //                item.Year ?? "",
        //                item.Remarks ?? ""
        //            };

        //            for (int j = 0; j < values.Length; j++)
        //            {
        //                var cell = new Cell().Add(new Paragraph(values[j]))
        //                    .SetFontSize(7)
        //                    .SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1));

        //                // Center align everything except Name and Designation (matching Word logic)
        //                if (j != 2 && j != 3 && j != 4) // Not Name or Designation
        //                {
        //                    cell.SetTextAlignment(TextAlignment.CENTER);
        //                }

        //                // Right align Amount column
        //                if (j == 6) // Amount column
        //                {
        //                    cell.SetTextAlignment(TextAlignment.RIGHT);
        //                }

        //                table.AddCell(cell);
        //            }

        //            serialNo++;
        //        }

        //        for (int i = 0; i < 6; i++)
        //        {
        //            var emptyCell = new Cell().Add(new Paragraph(""))
        //                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
        //                .SetFontSize(7);
        //            table.AddCell(emptyCell);
        //        }

        //        var totalCell = new Cell().Add(new Paragraph($"Total: {departmentTotal:N2}").SimulateBold())
        //            .SetTextAlignment(TextAlignment.RIGHT)
        //            .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
        //            .SetFontSize(8);
        //        table.AddCell(totalCell);

        //        // Empty cells for remaining columns after Amount
        //        for (int i = 0; i < 3; i++)
        //        {
        //            var emptyCell = new Cell().Add(new Paragraph(""))
        //                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
        //                .SetFontSize(7);
        //            table.AddCell(emptyCell);
        //        }

        //        //document.Add(table);
        //        departmentContainer.Add(table);
        //        document.Add(departmentContainer);

        //        grandTotal += departmentTotal;
        //        totalEmployees += uniqueEmployees.Count;
        //    }

        //    document.Add(new Paragraph("Summary")
        //        .SetFontSize(12)
        //        .SimulateBold()
        //    .SetMarginTop(20)
        //        .SetMarginBottom(10));

        //    // Create summary table with 3 columns (no borders, matching Word)
        //    var summaryTable = new Table(new float[] { 1, 1, 1 });
        //    summaryTable.SetWidth(UnitValue.CreatePercentValue(100));

        //    // Add summary row with specific alignments
        //    summaryTable.AddCell(new Cell().Add(new Paragraph($"Total Employee: {totalEmployees}"))
        //        .SetTextAlignment(TextAlignment.LEFT)
        //        .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

        //    summaryTable.AddCell(new Cell().Add(new Paragraph($"Total Benefits: {totalBenefits}"))
        //        .SetTextAlignment(TextAlignment.CENTER)
        //        .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

        //    summaryTable.AddCell(new Cell().Add(new Paragraph($"Grand Total: {grandTotal:N2}"))
        //        .SetTextAlignment(TextAlignment.RIGHT)
        //        .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

        //    document.Add(summaryTable);

        //    document.Close();

        //    // Add headers and footers to all pages after document is closed
        //    using var readerStream = new MemoryStream(stream.ToArray());
        //    using var reader = new PdfReader(readerStream);
        //    using var outputStream = new MemoryStream();
        //    using var outputWriter = new PdfWriter(outputStream);
        //    using var outputPdf = new PdfDocument(reader, outputWriter);

        //    var totalPages = outputPdf.GetNumberOfPages();

        //    for (int i = 1; i <= totalPages; i++)
        //    {
        //        var page = outputPdf.GetPage(i);
        //        var pageSize = page.GetPageSize();
        //        var canvas = new PdfCanvas(page);

        //        // Add logo in top left corner
        //        try
        //        {
        //            string logoPath = "wwwroot/images/DP_logo.png"; // Update this path
        //            var imageData = ImageDataFactory.Create(logoPath);
        //            var logo = new Image(imageData);

        //            // Position and size the logo
        //            float logoWidth = 90;
        //            float logoHeight = 25;
        //            float logoX = 30; // Left margin
        //            float logoY = pageSize.GetTop() - 35; // Top position

        //            // Get image width and height as floats
        //            float imageWidth = logo.GetImageWidth();  // Correct method to get image width
        //            float imageHeight = logo.GetImageHeight(); // Correct method to get image height

        //            // Scale the logo without maintaining aspect ratio
        //            logo.SetFixedPosition(i, logoX, logoY);

        //            // Scale the image based on the width and height
        //            logo.ScaleAbsolute(logoWidth, logoHeight);  // Adjust scale with respect to both width and height

        //            // Add logo to the page
        //            var logoCanvas = new Canvas(canvas, pageSize);
        //            logoCanvas.Add(logo);
        //            logoCanvas.Close();
        //        }
        //        catch (Exception ex)
        //        {
        //            // Handle logo loading error - continue without logo
        //            Console.WriteLine($"Could not load logo: {ex.Message}");
        //        }

        //        // Three-line header starting after logo space
        //        float headerStartX = pageSize.GetWidth() / 2;
        //        float headerTopY = pageSize.GetTop() - 25;

        //        // Line 1: Company Name (centered)
        //        var companyFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
        //        float companyFontSize = 14;
        //        string companyText = "DataPath Limited";
        //        float companyTextWidth = companyFont.GetWidth(companyText, companyFontSize);

        //        canvas.BeginText()
        //            .SetFontAndSize(companyFont, companyFontSize)
        //            .MoveText(headerStartX - (companyTextWidth / 2), headerTopY)
        //            .ShowText(companyText)
        //            .EndText();

        //        // Line 2: Report Name (centered)
        //        var reportFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
        //        float reportFontSize = 12;
        //        string reportText = "Other Benefit Report";
        //        float reportTextWidth = reportFont.GetWidth(reportText, reportFontSize);

        //        canvas.BeginText()
        //            .SetFontAndSize(reportFont, reportFontSize)
        //            .MoveText(headerStartX - (reportTextWidth / 2), headerTopY - 18)
        //            .ShowText(reportText)
        //            .EndText();

        //        // Line 3: Time Period (centered dynamically based on content length)
        //        var periodFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
        //        float periodFontSize = 10;
        //        string periodText = $"For the month of {reportPeriod}";
        //        float periodTextWidth = periodFont.GetWidth(periodText, periodFontSize);

        //        float periodTextX = headerStartX - (periodTextWidth / 2);
        //        float periodTextY = headerTopY - 35;

        //        canvas.BeginText()
        //            .SetFontAndSize(periodFont, periodFontSize)
        //            .MoveText(periodTextX, periodTextY)
        //            .ShowText(periodText)
        //            .EndText();

        //        // 🔽 Add underline below "For the month of..." line
        //        float underlineY = periodTextY - 2; // 2 points below the text baseline
        //        canvas.SetLineWidth(0.5f); // Thin line
        //        canvas.MoveTo(periodTextX, underlineY)
        //              .LineTo(periodTextX + periodTextWidth, underlineY)
        //              .Stroke();

        //        var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
        //        float fontSize = 8;
        //        float margin = 36;
        //        float yPosition = pageSize.GetBottom() + 20;
        //        float pageWidth = pageSize.GetWidth();

        //        // Left-aligned: Time
        //        canvas.BeginText()
        //            .SetFontAndSize(font, fontSize)
        //            .MoveText(margin, yPosition)
        //            .ShowText($"Print DateTime: {(model.Ldate.HasValue ? model.Ldate.Value.ToString("dd/MM/yyyy hh:mm tt") : DateTime.Now.ToString("dd/MM/yyyy hh:mm tt"))}")
        //            .EndText();

        //        // Center-aligned: User
        //        string userText = $"Printed By: {model.Luser}";
        //        float userTextWidth = font.GetWidth(userText, fontSize);
        //        canvas.BeginText()
        //            .SetFontAndSize(font, fontSize)
        //            .MoveText((pageWidth - userTextWidth) / 2, yPosition)
        //            .ShowText(userText)
        //            .EndText();

        //        // Right-aligned: Page number
        //        string pageText = $"Page {i} of {totalPages}";
        //        float pageTextWidth = font.GetWidth(pageText, fontSize);
        //        canvas.BeginText()
        //            .SetFontAndSize(font, fontSize)
        //            .MoveText(pageWidth - margin - pageTextWidth, yPosition)
        //            .ShowText(pageText)
        //            .EndText();


        //        canvas.Release();
        //    }

        //    outputPdf.Close();
        //    return outputStream.ToArray();
        //}

        public async Task<ReportFilterListViewModel> GetDataAsync(ReportFilterViewModel filter)
        {
            var query = from obe in entryRepo.All().AsNoTracking()
                        join eoi in empOffRepo.All().AsNoTracking() on obe.EmployeeId equals eoi.EmployeeId
                        join e in employeeRepo.All().AsNoTracking() on obe.EmployeeId equals e.EmployeeId into empJoin
                        from e in empJoin.DefaultIfEmpty()
                        join dg in desiRepo.All().AsNoTracking() on eoi.DesignationCode equals dg.DesignationCode into dgJoin
                        from dg in dgJoin.DefaultIfEmpty()
                        join cb in branchRepo.All().AsNoTracking() on eoi.BranchCode equals cb.BranchCode into cbJoin
                        from cb in cbJoin.DefaultIfEmpty()
                        join dp in depRepo.All().AsNoTracking() on eoi.DepartmentCode equals dp.DepartmentCode into dpJoin
                        from dp in dpJoin.DefaultIfEmpty()
                        join cp in companyRepo.All().AsNoTracking() on eoi.CompanyCode equals cp.CompanyCode into cpJoin
                        from cp in cpJoin.DefaultIfEmpty()
                        join pm in monthRepo.All().AsNoTracking() on obe.SalaryMonth equals pm.MonthId.ToString() into pmJoin
                        from pm in pmJoin.DefaultIfEmpty()
                        join dt in typeRepo.All().AsNoTracking() on obe.BenefitTypeId equals dt.BenefitTypeId into typeJoin
                        from dt in typeJoin.DefaultIfEmpty()
                        select new
                        {
                            obe,
                            eoi,
                            e,
                            dg,
                            cb,
                            dp,
                            cp,
                            pm,
                            dt,
                            empFirstName = (e.FirstName + " ") ?? "",
                            empLastName = (e.LastName) ?? ""
                            //,

                            //EmpId = e.EmployeeId,
                            //EmpName = $"{e.FirstName ?? ""} {e.LastName ?? ""}",
                            //CompanyCode = eoi.CompanyCode ?? "",
                            //BranchCode = cb.BranchCode ?? "",
                            //DesignationCode = dg.DesignationCode ?? "",
                            //DesignationName = dg.DesignationName ?? "",
                            //DepartmentCode = dp.DepartmentCode ?? "",
                            //DepartmentName = dp.DepartmentName ?? "",
                            //BranchName = cb.BranchName ?? "",
                            //CompanyName = cp.CompanyName ?? "",
                            //EmployeeStatusCode = eoi.EmployeeStatus ?? "",
                            //benefitId = obe.BenefitTypeId ?? "",
                            //benefitTypeCode = dt.BenefitTypeId ?? "",
                            //benefitTypeName = dt.BenefitType ?? "",
                            //monthId = pm.MonthId.ToString(),
                            //monthName = pm.MonthName,
                            //year = obe.SalaryYear,
                            //benefitAmount = obe.BenefitAmount ?? 0.00m,
                            //payDay = obe.PaidDays ?? 0,
                            //remarks = obe.Remarks ?? ""
                        };

            // Apply filters early to optimize query execution
            if (filter.CompanyCodes?.Any() == true)
                query = query.Where(x => filter.CompanyCodes.Contains(x.cp.CompanyCode));

            if (filter.BranchCodes?.Any() == true)
                query = query.Where(x => filter.BranchCodes.Contains(x.cb.BranchCode));

            if (filter.DepartmentCodes?.Any() == true)
                query = query.Where(x => filter.DepartmentCodes.Contains(x.dp.DepartmentCode));

            if (filter.DesignationCodes?.Any() == true)
                query = query.Where(x => filter.DesignationCodes.Contains(x.dg.DesignationCode));

            if (filter.EmployeeIDs?.Any() == true)
                query = query.Where(x => filter.EmployeeIDs.Contains(x.e.EmployeeId));

            if (filter.MonthIDs?.Any() == true)
            {
                query = query.Where(x => filter.MonthIDs.Contains(x.pm.MonthId.ToString()));
            }
            else
            {
                string currentMonth = DateTime.Now.Month.ToString();
                query = query.Where(x => x.pm.MonthId.ToString() != null && x.pm.MonthId.ToString() == currentMonth);
            }
                

            if (filter.BenefitTypeIDs?.Any() == true)
                query = query.Where(x => filter.BenefitTypeIDs.Contains(x.dt.BenefitTypeId));

            if (!string.IsNullOrEmpty(filter.SalaryYear))
                query = query.Where(x => x.obe.SalaryYear == filter.SalaryYear);

            // Fetch necessary data efficiently
            var result = new ReportFilterListViewModel
            {
                Companies = await companyRepo.All().AsNoTracking()
                    .Where(c => c.CompanyCode == "001")
                    .Select(b => new ReportFilterResultDto { Code = b.CompanyCode, Name = b.CompanyName })
                    .ToListAsync(),

                Branches = await branchRepo.All().AsNoTracking()
                    .Where(b => b.BranchCode != null && b.BranchName != null)
                    .OrderBy(b => b.BranchCode)
                    .Select(b => new ReportFilterResultDto { Code = b.BranchCode, Name = b.BranchName })
                    .Distinct().ToListAsync(),

                Departments = await query.Where(x => x.dp.DepartmentCode != null && x.dp.DepartmentName != null)
                    .Select(x => new ReportFilterResultDto { Code = x.dp.DepartmentCode, Name = x.dp.DepartmentName })
                    .Distinct().ToListAsync(),

                Designations = await query.Where(x => x.dg.DesignationCode != null && x.dg.DesignationName != null)
                    .Select(x => new ReportFilterResultDto { Code = x.dg.DesignationCode, Name = x.dg.DesignationName })
                    .Distinct().ToListAsync(),

                BenefitTypes = await typeRepo.All().AsNoTracking()
                    .Where(dt => dt.BenefitTypeId != null && dt.BenefitType != null)
                    .Select(dt => new ReportFilterResultDto { Code = dt.BenefitTypeId, Name = dt.BenefitType })
                    .Distinct().ToListAsync(),

                Months = await monthRepo.All().AsNoTracking()
                    .OrderBy(m => m.MonthId)
                    .Select(m => new ReportFilterResultDto { Code = m.MonthId.ToString(), Name = m.MonthName })
                    .ToListAsync(),

                Employees = await query.Where(x => x.e.EmployeeId != null)
                    .Select(x => new ReportFilterResultDto { Code = x.e.EmployeeId, Name = x.empFirstName + x.empLastName })
                    .Distinct().ToListAsync(),

                OtherBenefits = await query.Where(x => x.obe.OtherBenefitId != null)
                    .Select(x => new ReportFilterResultDto
                    {
                        Code = x.e.EmployeeId,
                        Name = x.empFirstName+x.empLastName,
                        DesignationName = x.dg.DesignationName ?? "",
                        DepartmentName = x.dp.DepartmentName ?? "",
                        BranchName = x.cb.BranchName ?? "",
                        CompanyName = x.cp.CompanyName ?? "",
                        EmpId = x.e.EmployeeId,
                        OtherBenefitId = x.obe.OtherBenefitId,
                        Year = x.obe.SalaryYear,
                        Month = x.pm.MonthName,
                        BenefitAmount = x.obe.BenefitAmount,
                        BenefitType = x.dt.BenefitType,
                        Remarks = x.obe.Remarks,
                        PaidDays = x.obe.PaidDays
                    }).ToListAsync(),
            };

            return result;
        }

    }
}
