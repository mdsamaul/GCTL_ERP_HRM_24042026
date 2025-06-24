
using GCTL.Core.Data;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.HrmPayMonthlyOtEntries;
using GCTL.Data.Models;
using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PdfDocument2 = iText.Layout.Document;

namespace GCTL.Service.HrmPayMonthlyOtReports
{
    public class HrmPayMonthlyOtReportService:AppService<HrmPayMonthlyOtentry>, IHrmPayMonthlyOtReportService
    {
        private readonly IRepository<HrmPayMonthlyOtentry> entryRepo;

        private readonly IRepository<HrmEmployee> employeeRepo;
        private readonly IRepository<HrmEmployeeOfficialInfo> empOffRepo;
        private readonly IRepository<HrmDefDesignation> desiRepo;
        private readonly IRepository<HrmDefDepartment> depRepo;
        private readonly IRepository<CoreBranch> branchRepo;
        private readonly IRepository<CoreCompany> companyRepo;
        private readonly IRepository<HrmPayMonth> monthRepo;

        public HrmPayMonthlyOtReportService(
            IRepository<HrmPayMonthlyOtentry> entryRepo,
            IRepository<HrmEmployee> employeeRepo, 
            IRepository<HrmEmployeeOfficialInfo> empOffRepo,
            IRepository<HrmDefDesignation> desiRepo, 
            IRepository<HrmDefDepartment> depRepo,
            IRepository<CoreBranch> branchRepo, 
            IRepository<CoreCompany> companyRepo,
            IRepository<HrmPayMonth> monthRepo):base(entryRepo)
        {
            this.entryRepo = entryRepo;
            this.employeeRepo = employeeRepo;
            this.empOffRepo = empOffRepo;
            this.desiRepo = desiRepo;
            this.depRepo = depRepo;
            this.branchRepo = branchRepo;
            this.companyRepo = companyRepo;
            this.monthRepo = monthRepo;
        }

        public async Task<byte[]> GenerateExcelReport(List<ReportFilterResultDto> data)
        {
            ExcelPackage.LicenseContext= LicenseContext.NonCommercial;
            using var package = new ExcelPackage();

            var worksheet = package.Workbook.Worksheets.Add("Monthly OT Report");

            var months = data
                .Select(d => $"{d.Month} {d.Year}")
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct()
                .ToList();

            var reportPeriod = string.Join(", ", months);

            worksheet.Cells["A1:H1"].Merge = true;
            worksheet.Cells["A1"].Value = data.Select(c => c.CompanyName);
            worksheet.Cells["A1"].Style.Font.Size = 16;
            worksheet.Cells["A1"].Style.Font.Bold = true;
            worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            worksheet.Cells["A2:H2"].Merge = true;
            worksheet.Cells["A2"].Value = "Monthly OT Report";
            worksheet.Cells["A2"].Style.Font.Size = 16;
            worksheet.Cells["A2"].Style.Font.Bold = true;
            worksheet.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            //worksheet.Cells["A3:J3"].Merge = true;
            worksheet.Cells["A3:H3"].Merge = true;
            worksheet.Cells["A3"].Value = $"{reportPeriod}";
            worksheet.Cells["A3"].Style.Font.Size = 14;
            worksheet.Cells["A3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            int currentRow = 5;
            var groupedData = data.GroupBy(x => x.DepartmentName ?? "Unknown Department")
                                  .OrderBy(g => g.Key);


            decimal grandOTATotal = 0;
            decimal grandOTTotal = 0;
            //int totalEmployees = 0;
            //int totalEntries = 0;


            foreach (var departmentGroup in groupedData)
            {
                worksheet.Cells[currentRow, 1, currentRow, 8].Merge = true;
                worksheet.Cells[currentRow, 1].Value = $"Department: {departmentGroup.Key}";
                worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
                worksheet.Cells[currentRow, 1].Style.Font.Size = 12;
                currentRow++;

                string[] headers = { "SL No.", "Employee ID", "Name", "Designation", "Branch", "OT", "Amount", "Remarks" };

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
                decimal departmentOTTotal = 0;
                decimal departmentOTATotal = 0;
                var uniqueEmployees = new HashSet<string>();

                foreach (var item in departmentGroup.OrderBy(x => x.Code))
                {
                    var ot = item.Ot ?? 0;
                    var amount = item.OtAmount ?? 0;
                    departmentOTTotal += ot;
                    departmentOTATotal += amount;
                    //totalEntries++;

                    if (!string.IsNullOrEmpty(item.Code))
                        uniqueEmployees.Add(item.Code);

                    worksheet.Cells[currentRow, 1].Value = serialNo;
                    worksheet.Cells[currentRow, 2].Value = item.Code ?? "";
                    worksheet.Cells[currentRow, 3].Value = item.Name ?? "";
                    worksheet.Cells[currentRow, 4].Value = item.DesignationName ?? "";
                    worksheet.Cells[currentRow, 5].Value = item.BranchName ?? "";
                    worksheet.Cells[currentRow, 6].Value = ot;
                    worksheet.Cells[currentRow, 6].Style.Numberformat.Format = "#,##0.00";
                    worksheet.Cells[currentRow, 7].Value = amount;
                    worksheet.Cells[currentRow, 7].Style.Numberformat.Format = "#,##0.00";
                    //worksheet.Cells[currentRow, 8].Value = item.Month ?? "";
                    //worksheet.Cells[currentRow, 9].Value = item.Year ?? "";
                    worksheet.Cells[currentRow, 8].Value = item.Remarks ?? "";

                    //for (int col = 1; col <= 10; col++)
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
                worksheet.Cells[currentRow, 6].Value = $"Total: {departmentOTTotal}";
                worksheet.Cells[currentRow, 6].Style.Font.Bold = true;
                worksheet.Cells[currentRow, 6].Style.Numberformat.Format = "#,##0.00";

                worksheet.Cells[currentRow, 7].Value = $"Total: { departmentOTATotal}";
                worksheet.Cells[currentRow, 7].Style.Font.Bold = true;
                worksheet.Cells[currentRow, 7].Style.Numberformat.Format = "#,##0.00";

                currentRow += 2;

                grandOTTotal += departmentOTTotal;
                grandOTATotal += departmentOTATotal;
                //totalEmployees += uniqueEmployees.Count;
            }

            //worksheet.Cells[currentRow, 1].Value = "Summary";
            //worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
            //worksheet.Cells[currentRow, 1].Style.Font.Size = 14;
            //currentRow++;
            //worksheet.Cells[currentRow, 1].Value = "Total Employees:";
            //worksheet.Cells[currentRow, 1].Style.Font.Bold = true;

            //worksheet.Cells[currentRow, 2].Value = totalEmployees;

            //worksheet.Cells[currentRow, 4].Value = $"Total Entries: {totalEntries}";
            //worksheet.Cells[currentRow, 4].Style.Font.Bold = true;

            worksheet.Cells[currentRow, 6].Value = $"Total OT:{grandOTTotal}";
            worksheet.Cells[currentRow, 6].Style.Font.Bold = true;
            worksheet.Cells[currentRow, 6].Style.Numberformat.Format = "#,##0.00";

            worksheet.Cells[currentRow, 7].Value = $"Total Amount:{grandOTATotal}";
            worksheet.Cells[currentRow, 7].Style.Font.Bold = true;
            worksheet.Cells[currentRow, 7].Style.Numberformat.Format = "#,##0.00";

            currentRow++;

            worksheet.Cells.AutoFitColumns();

            return package.GetAsByteArray();
        }

        public async Task<byte[]> GeneratePdfReport(List<ReportFilterResultDto> data, BaseViewModel model)
        {
            if (data == null || !data.Any())
                return null;

            using var stream = new MemoryStream();
            using var writer = new PdfWriter(stream);
            using var pdf = new PdfDocument(writer);
            using var document = new PdfDocument2(pdf, iText.Kernel.Geom.PageSize.A4.Rotate());


            var timesRegular = PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN);
            var timesBold = PdfFontFactory.CreateFont(StandardFonts.TIMES_BOLD);

            document.SetMargins(80, 36, 60, 36);

            var months = data
                .Select(d => $"{d.Month} {d.Year}")
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct()
                .ToList();

            var reportPeriod = string.Join(", ", months);

            var groupData = data
                .GroupBy(x => x.DepartmentName ?? "Unknown Department")
                .OrderBy(g => g.Key);

            decimal grandOtTotal = 0;
            decimal grandOtATotal= 0;
            int totalEmployees = data.Select(e=>e.Code).Distinct().Count();
            int totalEntry = 0;

            bool isFirst = true;

            bool NeedsPageBreak(iText.Layout.Document doc)
            {
                try
                {
                    var renderer = doc.GetRenderer();
                    var currentArea = renderer.GetCurrentArea();

                    if (currentArea?.GetBBox() != null)
                    {
                        float availableHeight = currentArea.GetBBox().GetHeight();
                        float pageHeight = doc.GetPdfDocument().GetDefaultPageSize().GetHeight();
                        float totalMargins = doc.GetTopMargin() + doc.GetBottomMargin();
                        float fullPageContentHeight = pageHeight - totalMargins;

                        return availableHeight < (fullPageContentHeight * 0.30f);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error checking page break: {ex.Message}");
                }
                return false;
            }

            foreach (var dpGroup in groupData) 
            {
                if (NeedsPageBreak(document))
                    document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));

                float[] columnWidths = new float[]
                {
                    3.8f,  
                    9.4f, 
                    19.8f, 
                    19.8f, 
                    11.3f, 
                    7f,
                    8.4f,  
                    8.4f,  
                    16.1f  
                };

                var table = new Table(UnitValue.CreatePercentArray(columnWidths));
                table.SetWidth(UnitValue.CreatePercentValue(100));

                string[] headers = { "SN", "Employee ID", "Name", "Designation", "Branch","Joining Date", "OT", "Amount", "Remarks" };

                var departmentHeaderRow = new Cell(1,9)
                    .Add(new Paragraph($"Department: {dpGroup.Key}"))
                    .SetFontSize(11).SetTextAlignment(TextAlignment.LEFT)
                    .SetPadding(5).SetFont(timesBold)
                    .SetBorderTop(iText.Layout.Borders.Border.NO_BORDER)
                    .SetBorderLeft(iText.Layout.Borders.Border.NO_BORDER)
                    .SetBorderRight(iText.Layout.Borders.Border.NO_BORDER)
                    .SetBorderBottom(iText.Layout.Borders.Border.NO_BORDER);

                table.AddHeaderCell(departmentHeaderRow);

                foreach(var header in headers)
                {
                    var headerCell = new Cell()
                        .Add(new Paragraph(header).SetFont(timesBold)
                        .SetFontSize(7)).SetTextAlignment(TextAlignment.CENTER)
                        .SetBorder(new iText.Layout.Borders.SolidBorder(ColorConstants.LIGHT_GRAY, 0.2f));
                    table.AddHeaderCell(headerCell);
                }

                table.SetSkipFirstHeader(false);
                table.SetSkipLastFooter(false);


                int serialNo = 1;
                decimal departmentOtTotal = 0;
                decimal departmentOtamount = 0;
                var uniqueEmployees = new HashSet<string>();
                var orderedItems = dpGroup.OrderBy(x => x.Code).ToList();

                foreach (var item in orderedItems)
                {
                    var ot = item.Ot ?? 0;
                    var otamount = item.OtAmount ?? 0;

                    departmentOtTotal += ot;
                    departmentOtamount += otamount;
                    totalEntry++;

                    //if(!string.IsNullOrEmpty(item.Code))
                    //    uniqueEmployees.Add(item.Code);

                    var values = new[]
                    {
                        serialNo.ToString(),
                        item.Code??"",
                        item.Name??"",
                        item.DesignationName??"",
                        item.BranchName??"",    
                        item.JoiningDate??"",
                        ot.ToString("G29"),
                        otamount.ToString(),
                        item.Remarks??""
                    };

                    for (int i = 0; i < values.Length; i++) 
                    {
                        var cell = new Cell().Add(new Paragraph(values[i]))
                            .SetFontSize(7).SetFont(timesRegular)
                            .SetBorder(new iText.Layout.Borders.SolidBorder(ColorConstants.LIGHT_GRAY, 0.2f)); // Reduced from 1 to 0.5

                        if (i != 2 && i != 3 && i != 4)
                        {
                            cell.SetTextAlignment(TextAlignment.CENTER);
                        }

                        if (i == 6 || i==7) 
                        {
                            cell.SetTextAlignment(TextAlignment.RIGHT);
                        }

                        table.AddCell(cell);
                    }
                    serialNo++;
                }

                for(int i = 0; i < 6; i++)
                {
                    var emptyCell = new Cell().Add(new Paragraph(""))
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                        .SetFontSize(7);
                    table.AddCell(emptyCell);
                }


                var totalLabelCell = new Cell().Add(new Paragraph($"Total:{departmentOtTotal.ToString("G29")}"))
                    .SetTextAlignment(TextAlignment.RIGHT)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .SetFontSize(8);
                table.AddCell(totalLabelCell);

                var amountCell = new Cell().Add(new Paragraph($"Total : {departmentOtamount.ToString("G29")}"))
                    .SetTextAlignment(TextAlignment.RIGHT)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .SetFontSize(8);
                table.AddCell(amountCell);

                for(int i = 0; i < 1; i++)
                {
                    var emptyCell = new Cell().Add(new Paragraph(""))
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetFontSize(7);
                    table.AddCell(emptyCell);
                }
                document.Add(table);

                grandOtTotal += departmentOtTotal;
                grandOtATotal += departmentOtamount;
            }

            //document.Add(new Paragraph("Summary")
            //    .SetFontSize(12)
            //    .SimulateBold()
            //    .SetMarginTop(20)
            //    .SetMarginBottom(10));

            var summaryTable = new Table(new float[] { 1, 1, 1, 1 });
            summaryTable.SetWidth(UnitValue.CreatePercentValue(100)).SetMarginTop(20);

            summaryTable.AddCell(new Cell().Add(new Paragraph($"No. of Employee: {totalEmployees}"))
                .SetTextAlignment(TextAlignment.LEFT)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

            summaryTable.AddCell(new Cell().Add(new Paragraph($"Total Entries: {totalEntry}"))
                .SetTextAlignment(TextAlignment.CENTER)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

            summaryTable.AddCell(new Cell().Add(new Paragraph($"Total OT: {grandOtTotal.ToString("G29")}"))
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

            summaryTable.AddCell(new Cell().Add(new Paragraph($"Total Amount: {grandOtATotal:N2}"))
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

            document.Add(summaryTable);

            document.Close();

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

                var companyFont = PdfFontFactory.CreateFont(StandardFonts.TIMES_BOLD);
                float companyFontSize = 14;
                string companyText = data.Select(c => c.CompanyName).Distinct().FirstOrDefault();
                float companyTextWidth = companyFont.GetWidth(companyText, companyFontSize);

                canvas.BeginText()
                    .SetFontAndSize(companyFont, companyFontSize)
                    .MoveText(headerStartX - (companyTextWidth / 2), headerTopY)
                    .ShowText(companyText)
                    .EndText();

                var reportFont = PdfFontFactory.CreateFont(StandardFonts.TIMES_BOLD);
                float reportFontSize = 12;
                string reportText = "Monthly OT Report";
                float reportTextWidth = reportFont.GetWidth(reportText, reportFontSize);

                canvas.BeginText()
                    .SetFontAndSize(reportFont, reportFontSize)
                    .MoveText(headerStartX - (reportTextWidth / 2), headerTopY - 18)
                    .ShowText(reportText)
                    .EndText();

                var periodFont = PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN);
                float periodFontSize = 10;
                string periodText = $"{reportPeriod}";
                float periodTextWidth = periodFont.GetWidth(periodText, periodFontSize);

                float periodTextX = headerStartX - (periodTextWidth / 2);
                float periodTextY = headerTopY - 35;

                canvas.BeginText()
                    .SetFontAndSize(periodFont, periodFontSize)
                    .MoveText(periodTextX, periodTextY)
                    .ShowText(periodText)
                    .EndText();

                //float underlineY = periodTextY - 2;
                //canvas.SetLineWidth(0.5f);
                //canvas.MoveTo(periodTextX, underlineY)
                //      .LineTo(periodTextX + periodTextWidth, underlineY)
                //      .Stroke();

                var font = PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN);
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

        public async Task<ReportFilterListViewModel> GetDataAsync(ReportFilterViewModel filter)
        {
            var query = from rse in entryRepo.All()
                        join eoi in empOffRepo.All() on rse.EmployeeId equals eoi.EmployeeId
                        join e in employeeRepo.All() on rse.EmployeeId equals e.EmployeeId into empJoin
                        from e in empJoin.DefaultIfEmpty()
                        join dg in desiRepo.All() on eoi.DesignationCode equals dg.DesignationCode into dgJoin
                        from dg in dgJoin.DefaultIfEmpty()
                        join cb in branchRepo.All() on eoi.BranchCode equals cb.BranchCode into cbJoin
                        from cb in cbJoin.DefaultIfEmpty()
                        join dp in depRepo.All() on eoi.DepartmentCode equals dp.DepartmentCode into dpJoin
                        from dp in dpJoin.DefaultIfEmpty()
                        join cp in companyRepo.All() on eoi.CompanyCode equals cp.CompanyCode into cpJoin
                        from cp in cpJoin.DefaultIfEmpty()
                        join pm in monthRepo.All() on rse.Month equals pm.MonthId.ToString() into pmJoin
                        from pm in pmJoin.DefaultIfEmpty()

                        select new
                        {
                            EmpId = e.EmployeeId,
                            EmpName = (e.FirstName ?? "") + " " + (e.LastName ?? ""),
                            CompanyCode = eoi.CompanyCode ?? "",
                            CompanyName = cp.CompanyName ?? "",
                            BranchCode = cb.BranchCode ?? "",
                            BranchName = cb.BranchName ?? "",
                            DesignationCode = dg.DesignationCode ?? "",
                            DesignationName = dg.DesignationName ?? "",
                            DepartmentCode = dp.DepartmentCode ?? "",
                            DepartmentName = dp.DepartmentName ?? "",
                            EmployeeStatusCode = eoi.EmployeeStatus ?? "",
                            EmpSatusName = eoi.EmployeeStatus ?? "",
                            monthId = pm.MonthId.ToString(),
                            monthName = pm.MonthName,
                            MonthlyOtid = rse.MonthlyOtid,
                            Ot = rse.Ot ?? 0.00m,
                            OtAmount = rse.Otamount ?? 0m,
                            year = rse.Year ?? "",
                            JoiningDate = eoi.JoiningDate.Value.ToString("dd/MM/yyyy") ?? "",
                            remarks = rse.Remarks ?? "",
                        };

            if (filter.CompanyCodes?.Any() == true)
            {
                query = query.Where(x => x.CompanyCode != null && filter.CompanyCodes.Contains(x.CompanyCode));
            }
            if (filter.BranchCodes?.Any() == true)
            {
                query = query.Where(x => x.BranchCode != null && filter.BranchCodes.Contains(x.BranchCode));
            }
            if (filter.DepartmentCodes?.Any() == true)
            {
                query = query.Where(x => x.DepartmentCode != null && filter.DepartmentCodes.Contains(x.DepartmentCode));
            }
            if (filter.DesignationCodes?.Any() == true)
            {
                query = query.Where(x => x.DesignationCode != null && filter.DesignationCodes.Contains(x.DesignationCode));
            }
            if (filter.EmployeeIDs?.Any() == true)
            {
                query = query.Where(x => x.EmpId != null && filter.EmployeeIDs.Contains(x.EmpId));
            }

            if (filter.MonthIDs?.Any() == true)
            {
                query = query.Where(x => x.monthId != null && filter.MonthIDs.Contains(x.monthId));
            }
            else
            {
                string currentMonth = DateTime.Now.Month.ToString();
                query = query.Where(x => x.monthId != null && x.monthId == currentMonth);
            }

            if (!string.IsNullOrEmpty(filter.SalaryYear))
            {
                query = query.Where(x => x.year == filter.SalaryYear);
            }

            var company = await companyRepo.All().Where(c => c.CompanyCode == "001")
                .Select(c => new ReportFilterResultDto
                {
                    Code = c.CompanyCode,
                    Name = c.CompanyName
                }).ToListAsync();

            var allBranch = await branchRepo.All().Where(b => b.BranchCode != null && b.BranchName != null)
                .OrderBy(b => b.BranchCode)
                .Select(b => new ReportFilterResultDto
                {
                    Code = b.BranchCode,
                    Name = b.BranchName
                }).Distinct().ToListAsync();


            var allMonths = await monthRepo.All()
                .Where(m => m.MonthId != null && m.MonthName != null)
                .OrderBy(m => m.MonthId)
                .Select(m => new ReportFilterResultDto
                {
                    Code = m.MonthId.ToString(),
                    Name = m.MonthName
                })
                .ToListAsync();

            var result = new ReportFilterListViewModel
            {
                Companies = company,
                Branches = allBranch,
                Departments = await query.Where(x => x.DepartmentCode != null && x.DepartmentName != null)
                    .Select(x => new ReportFilterResultDto { Code = x.DepartmentCode, Name = x.DepartmentName })
                    .Distinct().ToListAsync(),

                Designations = await query.Where(x => x.DesignationCode != null && x.DesignationName != null)
                    .Select(x => new ReportFilterResultDto { Code = x.DesignationCode, Name = x.DesignationName })
                    .Distinct().ToListAsync(),

                Months = allMonths,

                Employees = await query.Where(x => x.EmpId != null && x.EmpName != null)
                    .Select(x => new ReportFilterResultDto
                    {
                        Code = x.EmpId,
                        Name = x.EmpName
                    }).Distinct().ToListAsync(),

                MonthlyOt = await query.Where(x => x.EmpId != null && x.EmpName != null)
                    .Select(x => new ReportFilterResultDto
                    {
                        Code = x.EmpId,
                        Name = x.EmpName,
                        DesignationName = x.DesignationName ?? "",
                        DepartmentName = x.DepartmentName ?? "",
                        BranchName = x.BranchName ?? "",
                        CompanyName = x.CompanyName ?? "",
                        //EmployeeType = x.et.EmpTypeName ?? "",
                        JoiningDate = x.JoiningDate ?? "",
                        EmpId = x.EmpId,
                        MonthlyOtid = x.MonthlyOtid,
                        Year = x.year,
                        Month = x.monthName,
                        Ot = x.Ot,
                        OtAmount = x.OtAmount,
                        Remarks = x.remarks
                    }).Distinct().ToListAsync(),
            };

            return result;
        }
    }
}
