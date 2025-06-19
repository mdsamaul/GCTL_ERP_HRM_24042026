using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;
using GCTL.Core.ViewModels.HrmPaySalaryDeductionEntries;
using GCTL.Data.Models;
using iText.Kernel.Pdf;
using Microsoft.AspNetCore.Mvc.Rendering;
using iText.Layout.Properties;
using GCTL.Core.Data;
using Microsoft.EntityFrameworkCore;


using iText.Layout;
using iText.Layout.Element;
using Microsoft.Extensions.Configuration;
using DocumentFormat.OpenXml.Office2013.Excel;
using iText.Kernel.Colors;
using iText.Pdfa;
using OfficeOpenXml.Style;
using OfficeOpenXml;

using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Geom;

using System.IO;

using PdfDocument = iText.Kernel.Pdf.PdfDocument;
using PdfParagraph = iText.Layout.Element.Paragraph;
using PdfTable = iText.Layout.Element.Table;
using PdfText = iText.Layout.Element.Text;
using PdfTextAlignment = iText.Layout.Properties.TextAlignment;
using PdfDocument2 = iText.Layout.Document;
using iText.Layout.Borders;
using iText.Kernel.Pdf.Canvas;
using iText.IO.Image;
using GCTL.Core.ViewModels;
using GCTL.Core.Helpers;



namespace GCTL.Service.HrmPaySalaryDeductionReports
{
    public class HrmPaySalaryDeductionReportService : AppService<HrmPaySalaryDeductionEntry>, IHrmPaySalaryDeductionReportService
    {
        private readonly IRepository<GCTL_ERP_DB_DatapathContext> _context;

        private readonly IRepository<HrmPaySalaryDeductionEntry> entryRepo;
        private readonly IRepository<HrmEmployee> employeeRepo;
        private readonly IRepository<HrmEmployeeOfficialInfo> empOffRepo;
        private readonly IRepository<HrmDefDesignation> desiRepo;
        private readonly IRepository<HrmDefDepartment> depRepo;
        private readonly IRepository<CoreBranch> branchRepo;
        private readonly IRepository<CoreCompany> companyRepo;
        private readonly IRepository<HrmPayDefDeductionType> typeRepo;
        private readonly IRepository<HrmPayMonth> monthRepo;

        //private readonly string _connectionString;

        public HrmPaySalaryDeductionReportService(
            IRepository<GCTL_ERP_DB_DatapathContext> context,
            IRepository<HrmPaySalaryDeductionEntry> entryRepo,
            IRepository<HrmEmployee> employeeRepo,
            IRepository<HrmEmployeeOfficialInfo> empOffRepo,
            IRepository<HrmDefDesignation> desiRepo,
            IRepository<HrmDefDepartment> depRepo,
            IRepository<CoreBranch> branchRepo,
            IRepository<CoreCompany> companyRepo,
            IRepository<HrmPayDefDeductionType> typeRepo,
            IRepository<HrmPayMonth> monthRepo
            //,
            //IConfiguration configuration
        ) : base(entryRepo)
        {
            _context = context;

            this.entryRepo = entryRepo;
            this.employeeRepo = employeeRepo;
            this.empOffRepo = empOffRepo;
            this.desiRepo = desiRepo;
            this.depRepo = depRepo;
            this.branchRepo = branchRepo;
            this.companyRepo = companyRepo;
            this.typeRepo = typeRepo;
            this.monthRepo = monthRepo;

            //_connectionString = configuration.GetConnectionString("ApplicationDbConnection");
        }

        public async Task<ReportFilterListViewModel> GetDataAsync(ReportFilterViewModel filter)
        {
            //var test = filter;
            //Console.WriteLine(test);

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
                        join pm in monthRepo.All() on rse.SalaryMonth equals pm.MonthId.ToString() into pmJoin
                        from pm in pmJoin.DefaultIfEmpty()
                        join dt in typeRepo.All() on rse.DeductionTypeId equals dt.DeductionTypeId into typeJoin
                        from dt in typeJoin.DefaultIfEmpty()
                        select new
                        {
                            EmpId = e.EmployeeId,
                            EmpName = (e.FirstName ?? "") + " " + (e.LastName ?? ""),
                            CompanyCode = eoi.CompanyCode ?? "",
                            BranchCode = cb.BranchCode ?? "",
                            DesignationCode = dg.DesignationCode ?? "",
                            DesignationName = dg.DesignationName ?? "",
                            DepartmentCode = dp.DepartmentCode ?? "",
                            DepartmentName = dp.DepartmentName ?? "",
                            BranchName = cb.BranchName ?? "",
                            CompanyName = cp.CompanyName ?? "",
                            EmployeeStatusCode = eoi.EmployeeStatus ?? "",
                            deductionId = rse.Id ?? "",
                            deductionTypeCode = dt.DeductionTypeId ?? "",
                            deductionTypeName = dt.DeductionType ?? "",
                            monthId = pm.MonthId.ToString(),
                            monthName = pm.MonthName,
                            year = rse.SalaryYear,
                            deductionAmount = rse.DeductionAmount ?? 0.00m,
                            remarks = rse.Remarks ?? ""
                        };

            // Apply filters
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

            // Add filters for the new fields
            if (filter.MonthIDs?.Any() == true)
            {
                query = query.Where(x => x.monthId != null && filter.MonthIDs.Contains(x.monthId));
            }
            else
            {
                string currentMonth = DateTime.Now.Month.ToString();
                query = query.Where(x => x.monthId != null && x.monthId == currentMonth);
            }
            if (filter.DeductionTypeIDs?.Any() == true)
            {
                query = query.Where(x => x.deductionTypeCode != null && filter.DeductionTypeIDs.Contains(x.deductionTypeCode));
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
                //.Distinct()
                .ToListAsync();

            var allDeductionTypes = await typeRepo.All()
                .Where(dt => dt.DeductionTypeId != null && dt.DeductionType != null)
                .Select(dt => new ReportFilterResultDto
                {
                    Code = dt.DeductionTypeId,
                    Name = dt.DeductionType
                }).Distinct().ToListAsync();

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

                DeductionTypes = allDeductionTypes,

                Months = allMonths,

                Employees = await query.Where(x => x.EmpId != null && x.EmpName != null)
                    .Select(x => new ReportFilterResultDto
                    {
                        Code = x.EmpId,
                        Name = x.EmpName
                    }).Distinct().ToListAsync(),

                SalaryDeduction = await query.Where(x => x.EmpId != null && x.EmpName != null)
                    .Select(x => new ReportFilterResultDto
                    {
                        Code = x.EmpId,
                        Name = x.EmpName,
                        DesignationName = x.DesignationName ?? "",
                        DepartmentName = x.DepartmentName ?? "",
                        BranchName = x.BranchName ?? "",
                        CompanyName = x.CompanyName ?? "",
                        EmpId = x.EmpId,
                        SalaryDeductionId = x.deductionId,
                        Year = x.year,
                        Month = x.monthName,
                        DeductionAmount = x.deductionAmount,
                        DeductionType = x.deductionTypeName,
                        Remarks = x.remarks
                    }).Distinct().ToListAsync(),
            };

            return result;
        }

        public async Task<byte[]> GeneratePdfReport(List<ReportFilterResultDto> data, BaseViewModel model)
        {
            using var stream = new MemoryStream();
            using var writer = new PdfWriter(stream);
            using var pdf = new PdfDocument(writer);
            using var document = new PdfDocument2(pdf, iText.Kernel.Geom.PageSize.A4);

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

                // Department header removed - it was here before

                float[] columnWidths = new float[]
                {
3.8f,   // SN
10.4f,  // Employee ID
20.8f,  // Name
20.8f,  // Designation
11.3f,  // Branch
11.3f,  // Deduction Type
7.5f,   // Amount
14.1f   // Remarks
                };

                var table = new PdfTable(UnitValue.CreatePercentArray(columnWidths));
                table.SetWidth(UnitValue.CreatePercentValue(100));
                // Remove SetKeepTogether to allow table to break across pages
                // table.SetKeepTogether(true); 

                string[] headers = { "SN", "Employee ID", "Name", "Designation", "Branch", "Deduction Type", "Amount", "Remarks" };

                // Create a custom header that includes department name for continuation pages
                var departmentHeaderRow = new Cell(1, 8)
                    .Add(new PdfParagraph($"Department: {departmentGroup.Key}"))
                    .SetFontSize(11)
                    // .SimulateBold() // If using SimulateBold(), keep yours instead
                    .SetTextAlignment(PdfTextAlignment.LEFT)
                    .SetPadding(5)
                    .SetBorderTop(iText.Layout.Borders.Border.NO_BORDER)
                    .SetBorderLeft(iText.Layout.Borders.Border.NO_BORDER)
                    .SetBorderRight(iText.Layout.Borders.Border.NO_BORDER)
                    .SetBorderBottom(iText.Layout.Borders.Border.NO_BORDER);

                // Add department header as a header cell (will repeat on new pages)
                table.AddHeaderCell(departmentHeaderRow);

                // Create column header cells with reduced border width
                foreach (var header in headers)
                {
                    var headerCell = new Cell().Add(new PdfParagraph(header).SimulateBold().SetFontSize(7))
                        .SetTextAlignment(PdfTextAlignment.CENTER)
                        .SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 0.2f)); // Reduced from 1 to 0.5
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
                    var amount = item.DeductionAmount ?? 0;
                    departmentTotal += amount;
                    totalDeductions++;

                    if (!string.IsNullOrEmpty(item.Code))
                        uniqueEmployees.Add(item.Code);

                    var values = new[]
                    {
                serialNo.ToString(),
                item.Code ?? "",
                item.Name ?? "",
                item.DesignationName ?? "",
                item.BranchName ?? "",
                item.DeductionType ?? "",
                amount.ToString("N2"),
                item.Remarks ?? ""
            };

                    for (int j = 0; j < values.Length; j++)
                    {
                        var cell = new Cell().Add(new PdfParagraph(values[j]))
                            .SetFontSize(7)
                            .SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 0.2f)); // Reduced from 1 to 0.5

                        if (j != 2 && j != 3 && j != 4)
                        {
                            cell.SetTextAlignment(PdfTextAlignment.CENTER);
                        }

                        // Right align Amount column
                        if (j == 6) // Amount column
                        {
                            cell.SetTextAlignment(PdfTextAlignment.RIGHT);
                        }

                        table.AddCell(cell);
                    }

                    serialNo++;
                }

                // Add total row
                for (int i = 0; i < 5; i++)
                {
                    var emptyCell = new Cell().Add(new PdfParagraph(""))
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                        .SetFontSize(7);
                    table.AddCell(emptyCell);
                }

                var totalLabelCell = new Cell().Add(new PdfParagraph("Total:"))
                    .SetTextAlignment(PdfTextAlignment.RIGHT)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .SetFontSize(8)
                    .SimulateBold();
                table.AddCell(totalLabelCell);

                var amountCell = new Cell().Add(new PdfParagraph($"{departmentTotal:N2}"))
                    .SetTextAlignment(PdfTextAlignment.RIGHT)
                    .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                    .SetFontSize(8);
                table.AddCell(amountCell);

                for (int i = 0; i < 1; i++)
                {
                    var emptyCell = new Cell().Add(new PdfParagraph(""))
                        .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                        .SetFontSize(7);
                    table.AddCell(emptyCell);
                }

                document.Add(table);

                grandTotal += departmentTotal;
                totalEmployees += uniqueEmployees.Count;
            }

            document.Add(new PdfParagraph("Summary")
                .SetFontSize(12)
                .SimulateBold()
                .SetMarginTop(20)
                .SetMarginBottom(10));

            var summaryTable = new PdfTable(new float[] { 1, 1, 1 });
            summaryTable.SetWidth(UnitValue.CreatePercentValue(100));

            summaryTable.AddCell(new Cell().Add(new PdfParagraph($"Total Employee: {totalEmployees}"))
                .SetTextAlignment(PdfTextAlignment.LEFT)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

            summaryTable.AddCell(new Cell().Add(new PdfParagraph($"Total Deduction: {totalDeductions}"))
                .SetTextAlignment(PdfTextAlignment.CENTER)
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

            summaryTable.AddCell(new Cell().Add(new PdfParagraph($"Grand Total: {grandTotal:N2}"))
                .SetTextAlignment(PdfTextAlignment.RIGHT)
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
                string reportText = "Salary Deduction Report";
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
        //    using var document = new PdfDocument2(pdf, iText.Kernel.Geom.PageSize.A4);

        //    document.SetMargins(80, 36, 60, 36);

        //    var months = data
        //        .Select(d => $"{d.Month} {d.Year}")
        //        .Where(s => !string.IsNullOrWhiteSpace(s))
        //        .Distinct()
        //        .ToList();

        //    var reportPeriod = string.Join(", ", months);

        //    // Group data by department
        //    var groupedData = data
        //        .GroupBy(x => x.DepartmentName ?? "Unknown Department")
        //        .OrderBy(g => g.Key);

        //    decimal grandTotal = 0;
        //    int totalEmployees = 0;
        //    int totalDeductions = 0;

        //    foreach (var departmentGroup in groupedData)
        //    {
        //        var departmentContainer = new iText.Layout.Element.Div();

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

        //        if (availableHeight < (pageHeight - topMargin - bottomMargin) * 0.20f && grandTotal > 0)
        //        {
        //            document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
        //        }

        //        var departmentHeader = new PdfParagraph($"Department: {departmentGroup.Key}")
        //            .SetFontSize(10)
        //            .SimulateBold()
        //            .SetMarginTop(15)
        //            .SetMarginBottom(10)
        //            .SetKeepWithNext(true);

        //        departmentContainer.Add(departmentHeader);

        //        float[] columnWidths = new float[]
        //        {
        //            3.8f,   // SN
        //            10.4f,  // Employee ID
        //            20.8f,  // Name
        //            20.8f,  // Designation
        //            11.3f,  // Branch
        //            11.3f,  // Deduction Type
        //            7.5f,   // Amount
        //            14.1f   // Remarks
        //        };

        //        var table = new PdfTable(UnitValue.CreatePercentArray(columnWidths));
        //        table.SetWidth(UnitValue.CreatePercentValue(100)); // Table takes full width
        //        table.SetKeepTogether(true);

        //        string[] headers = { "SN", "Employee ID", "Name", "Designation", "Branch", "Deduction Type", "Amount", /*"Month", "Year",*/ "Remarks" };

        //        foreach (var header in headers)
        //        {
        //            table.AddHeaderCell(new Cell().Add(new PdfParagraph(header).SimulateBold().SetFontSize(7))
        //                .SetTextAlignment(PdfTextAlignment.CENTER)
        //                .SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
        //        }

        //        int serialNo = 1;
        //        decimal departmentTotal = 0;
        //        var uniqueEmployees = new HashSet<string>();
        //        var orderedItems = departmentGroup.OrderBy(x => x.Code).ToList();

        //        foreach (var item in orderedItems)
        //        {
        //            var amount = item.DeductionAmount ?? 0;
        //            departmentTotal += amount;
        //            totalDeductions++;

        //            if (!string.IsNullOrEmpty(item.Code))
        //                uniqueEmployees.Add(item.Code);

        //            var values = new[]
        //            {
        //                serialNo.ToString(),
        //                item.Code ?? "",
        //                item.Name ?? "",
        //                item.DesignationName ?? "",
        //                item.BranchName ?? "",
        //                item.DeductionType ?? "",
        //                amount.ToString("N2"),
        //                item.Remarks ?? ""
        //            };

        //            for (int j = 0; j < values.Length; j++)
        //            {
        //                var cell = new Cell().Add(new PdfParagraph(values[j]))
        //                    .SetFontSize(7)
        //                    .SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 1));

        //                if (j != 2 && j != 3 && j !=4) 
        //                {
        //                    cell.SetTextAlignment(PdfTextAlignment.CENTER);
        //                }

        //                // Right align Amount column
        //                if (j == 6) // Amount column
        //                {
        //                    cell.SetTextAlignment(PdfTextAlignment.RIGHT);
        //                }

        //                table.AddCell(cell);
        //            }

        //            serialNo++;
        //        }

        //        for (int i = 0; i < 5; i++)
        //        {
        //            var emptyCell = new Cell().Add(new PdfParagraph(""))
        //                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
        //                .SetFontSize(7);
        //            table.AddCell(emptyCell);
        //        }

        //        var totalLabelCell = new Cell().Add(new PdfParagraph("Total:"))
        //            .SetTextAlignment(PdfTextAlignment.RIGHT)
        //            .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
        //            .SetFontSize(8)
        //            .SimulateBold();
        //        table.AddCell(totalLabelCell);

        //        var amountCell = new Cell().Add(new PdfParagraph($"{departmentTotal:N2}"))
        //            .SetTextAlignment(PdfTextAlignment.RIGHT)
        //            .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
        //            .SetFontSize(8);
        //        table.AddCell(amountCell);

        //        for (int i = 0; i < 1; i++)
        //        {
        //            var emptyCell = new Cell().Add(new PdfParagraph(""))
        //                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
        //                .SetFontSize(7);
        //            table.AddCell(emptyCell);
        //        }

        //        departmentContainer.Add(table);
        //        document.Add(departmentContainer);

        //        grandTotal += departmentTotal;
        //        totalEmployees += uniqueEmployees.Count;
        //    }

        //    document.Add(new PdfParagraph("Summary")
        //        .SetFontSize(12)
        //        .SimulateBold()
        //        .SetMarginTop(20)
        //        .SetMarginBottom(10));

        //    var summaryTable = new PdfTable(new float[] { 1, 1, 1 });
        //    summaryTable.SetWidth(UnitValue.CreatePercentValue(100));

        //    summaryTable.AddCell(new Cell().Add(new PdfParagraph($"Total Employee: {totalEmployees}"))
        //        .SetTextAlignment(PdfTextAlignment.LEFT)
        //        .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

        //    summaryTable.AddCell(new Cell().Add(new PdfParagraph($"Total Deduction: {totalDeductions}"))
        //        .SetTextAlignment(PdfTextAlignment.CENTER)
        //        .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

        //    summaryTable.AddCell(new Cell().Add(new PdfParagraph($"Grand Total: {grandTotal:N2}"))
        //        .SetTextAlignment(PdfTextAlignment.RIGHT)
        //        .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

        //    document.Add(summaryTable);

        //    document.Close();

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

        //        try
        //        {
        //            string logoPath = "wwwroot/images/DP_logo.png";
        //            var imageData = ImageDataFactory.Create(logoPath);
        //            var logo = new Image(imageData);

        //            float logoWidth = 90;
        //            float logoHeight = 25;
        //            float logoX = 30; // Left margin
        //            float logoY = pageSize.GetTop() -  35; 

        //            float imageWidth = logo.GetImageWidth(); 
        //            float imageHeight = logo.GetImageHeight();

        //            logo.SetFixedPosition(i, logoX, logoY);

        //            logo.ScaleAbsolute(logoWidth, logoHeight); 

        //            var logoCanvas = new Canvas(canvas, pageSize);
        //            logoCanvas.Add(logo);
        //            logoCanvas.Close();
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine($"Could not load logo: {ex.Message}");
        //        }

        //        float headerStartX = pageSize.GetWidth() / 2;
        //        float headerTopY = pageSize.GetTop() - 25;

        //        var companyFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
        //        float companyFontSize = 14;
        //        string companyText = "DataPath Limited";
        //        float companyTextWidth = companyFont.GetWidth(companyText, companyFontSize);

        //        canvas.BeginText()
        //            .SetFontAndSize(companyFont, companyFontSize)
        //            .MoveText(headerStartX - (companyTextWidth / 2), headerTopY)
        //            .ShowText(companyText)
        //            .EndText();

        //        var reportFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
        //        float reportFontSize = 12;
        //        string reportText = "Salary Deduction Report";
        //        float reportTextWidth = reportFont.GetWidth(reportText, reportFontSize);

        //        canvas.BeginText()
        //            .SetFontAndSize(reportFont, reportFontSize)
        //            .MoveText(headerStartX - (reportTextWidth / 2), headerTopY - 18)
        //            .ShowText(reportText)
        //            .EndText();

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

        //        float underlineY = periodTextY - 2; 
        //        canvas.SetLineWidth(0.5f);
        //        canvas.MoveTo(periodTextX, underlineY)
        //              .LineTo(periodTextX + periodTextWidth, underlineY)
        //              .Stroke();

        //        var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
        //        float fontSize = 8;
        //        float margin = 36;
        //        float yPosition = pageSize.GetBottom() + 20;
        //        float pageWidth = pageSize.GetWidth();

        //        canvas.BeginText()
        //            .SetFontAndSize(font, fontSize)
        //            .MoveText(margin, yPosition)
        //            .ShowText($"Print Datetime: {(model.Ldate.HasValue ?model.Ldate.Value.ToString ("dd/MM/yyyy hh:mm tt") : DateTime.Now.ToString("dd/MM/yyyy hh:mm tt"))}")
        //            .EndText();

        //        string userText = $"Printed By: {model.Luser}";
        //        float userTextWidth = font.GetWidth(userText, fontSize);
        //        canvas.BeginText()
        //            .SetFontAndSize(font, fontSize)
        //            .MoveText((pageWidth - userTextWidth) / 2, yPosition)
        //            .ShowText(userText)
        //            .EndText();

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

        public async Task<byte[]> GenerateExcelReport(List<ReportFilterResultDto> data)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Salary Deduction Report");

            var months = data
                .Select(d => $"{d.Month} {d.Year}")
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct()
                .ToList();

            var reportPeriod = string.Join(", ", months);

            //worksheet.Cells["A1:J1"].Merge = true;
            worksheet.Cells["A1:H1"].Merge = true;
            worksheet.Cells["A1"].Value = data.Select(c => c.CompanyName);
            worksheet.Cells["A1"].Style.Font.Size = 16;
            worksheet.Cells["A1"].Style.Font.Bold = true;
            worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            //worksheet.Cells["A2:J2"].Merge = true;
            worksheet.Cells["A2:H2"].Merge = true;
            worksheet.Cells["A2"].Value = "Salary Deduction Report";
            worksheet.Cells["A2"].Style.Font.Size = 16;
            worksheet.Cells["A2"].Style.Font.Bold = true;
            worksheet.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            //worksheet.Cells["A3:J3"].Merge = true;
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

                string[] headers = { "SL No.", "Employee ID", "Name", "Designation", "Branch",
                             "Deduction Type", "Amount", /*"Month", "Year",*/ "Remarks" };

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
                    var amount = item.DeductionAmount ?? 0;
                    departmentTotal += amount;
                    totalDeductions++;

                    if (!string.IsNullOrEmpty(item.Code))
                        uniqueEmployees.Add(item.Code);

                    worksheet.Cells[currentRow, 1].Value = serialNo;
                    worksheet.Cells[currentRow, 2].Value = item.Code ?? "";
                    worksheet.Cells[currentRow, 3].Value = item.Name ?? "";
                    worksheet.Cells[currentRow, 4].Value = item.DesignationName ?? "";
                    worksheet.Cells[currentRow, 5].Value = item.BranchName ?? "";
                    worksheet.Cells[currentRow, 6].Value = item.DeductionType ?? "";
                    worksheet.Cells[currentRow, 7].Value = amount;
                    worksheet.Cells[currentRow, 7].Style.Numberformat.Format = "#,##0.00";
                    //worksheet.Cells[currentRow, 8].Value = item.Month ?? "";
                    //worksheet.Cells[currentRow, 9].Value = item.Year ?? "";
                    worksheet.Cells[currentRow,8].Value = item.Remarks ?? "";

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
                worksheet.Cells[currentRow, 6].Value = "Total:";
                worksheet.Cells[currentRow, 6].Style.Font.Bold = true;
                worksheet.Cells[currentRow, 7].Value = departmentTotal;
                worksheet.Cells[currentRow, 7].Style.Font.Bold = true;
                worksheet.Cells[currentRow, 7].Style.Numberformat.Format = "#,##0.00";

                currentRow += 2;

                grandTotal += departmentTotal;
                totalEmployees += uniqueEmployees.Count;
            }

            worksheet.Cells[currentRow, 1].Value = "Summary";
            worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
            worksheet.Cells[currentRow, 1].Style.Font.Size = 14;
            currentRow++;
            worksheet.Cells[currentRow, 1].Value = "Total Employees:";
            worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
            //worksheet.Cells[currentRow, 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);

            worksheet.Cells[currentRow, 2].Value = totalEmployees;
            //worksheet.Cells[currentRow, 2].Style.Border.BorderAround(ExcelBorderStyle.Thin);

            worksheet.Cells[currentRow, 4].Value = $"Total Deductions: {totalDeductions}";
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

        public byte[] GenerateWordReport(List<ReportFilterResultDto> data)
        {
            using var stream = new MemoryStream();
            using var document = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document);

            var mainPart = document.AddMainDocumentPart();
            mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document();
            var body = mainPart.Document.AppendChild(new Body());

            // Title
            var titleParagraph = new DocumentFormat.OpenXml.Wordprocessing.Paragraph();
            var titleRun = new Run();
            var titleText = new DocumentFormat.OpenXml.Wordprocessing.Text("Data Path - Salary Deduction Report");
            titleRun.AppendChild(new RunProperties(
                new Bold(),
                new DocumentFormat.OpenXml.Wordprocessing.FontSize() { Val = "32" }
            ));
            titleRun.AppendChild(titleText);
            titleParagraph.AppendChild(new ParagraphProperties(
                new Justification() { Val = JustificationValues.Center }
            ));
            titleParagraph.AppendChild(titleRun);
            body.AppendChild(titleParagraph);

            // Add spacing
            body.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Paragraph());

            var groupedData = data.GroupBy(x => x.DepartmentName ?? "Unknown Department")
                                 .OrderBy(g => g.Key);

            decimal grandTotal = 0;
            int totalEmployees = 0; 

            foreach (var departmentGroup in groupedData)
            {
                // Department header
                var deptParagraph = new DocumentFormat.OpenXml.Wordprocessing.Paragraph();
                var deptRun = new Run();
                deptRun.AppendChild(new RunProperties(new Bold()));
                deptRun.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Text($"Department: {departmentGroup.Key}"));
                deptParagraph.AppendChild(deptRun);
                body.AppendChild(deptParagraph);

                // Create table
                var table = new DocumentFormat.OpenXml.Wordprocessing.Table();

                // Table properties
                var tableProps = new TableProperties(
                    new TableBorders(
                        new TopBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 12 },
                        new BottomBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 12 },
                        new LeftBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 12 },
                        new RightBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 12 },
                        new InsideHorizontalBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 },
                        new InsideVerticalBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 6 }
                    )
                );
                table.AppendChild(tableProps);

                // Header row
                var headerRow = new TableRow();
                string[] headers = { "SL", "Employee ID", "Name", "Designation", "Branch",
               "Deduction Type", "Amount", "Month", "Year", "Remarks" };

                foreach (var header in headers)
                {
                    var cell = new TableCell();
                    var cellProps = new TableCellProperties(
                        new Shading() { Val = ShadingPatternValues.Clear, Fill = "D3D3D3" }
                    );
                    cell.AppendChild(cellProps);

                    var paragraph = new DocumentFormat.OpenXml.Wordprocessing.Paragraph();
                    var run = new Run();
                    run.AppendChild(new RunProperties(new Bold()));
                    run.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Text(header));
                    paragraph.AppendChild(run);
                    cell.AppendChild(paragraph);
                    headerRow.AppendChild(cell);
                }
                table.AppendChild(headerRow);

                // Data rows
                int serialNo = 1;
                decimal departmentTotal = 0;

                foreach (var item in departmentGroup)
                {
                    var amount = item.DeductionAmount ?? 0;
                    departmentTotal += amount;

                    var dataRow = new TableRow();
                    string[] values = {
                serialNo.ToString(),
                item.Code ?? "",
                item.Name ?? "",
                item.DesignationName ?? "",
                item.BranchName ?? "",
                item.DeductionType ?? "",
                amount.ToString("N2"),
                item.Month ?? "",
                item.Year ?? "",
                item.Remarks ?? ""
            };

                    foreach (var value in values)
                    {
                        var cell = new TableCell();
                        var paragraph = new DocumentFormat.OpenXml.Wordprocessing.Paragraph();
                        var run = new Run();
                        run.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Text(value));
                        paragraph.AppendChild(run);
                        cell.AppendChild(paragraph);
                        dataRow.AppendChild(cell);
                    }
                    table.AppendChild(dataRow);
                    serialNo++;
                }

                // Department total row
                var totalRow = new TableRow();
                for (int i = 0; i < 6; i++)
                {
                    var cell = new TableCell();
                    var paragraph = new DocumentFormat.OpenXml.Wordprocessing.Paragraph();
                    var run = new Run();
                    if (i == 5)
                    {
                        run.AppendChild(new RunProperties(new Bold()));
                        run.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Text("Department Total:"));
                    }
                    else
                    {
                        run.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Text(""));
                    }
                    paragraph.AppendChild(run);
                    cell.AppendChild(paragraph);
                    totalRow.AppendChild(cell);
                }

                var totalCell = new TableCell();
                var totalCellProps = new TableCellProperties(
                    new Shading() { Val = ShadingPatternValues.Clear, Fill = "FFFF00" }
                );
                totalCell.AppendChild(totalCellProps);
                var totalParagraph = new DocumentFormat.OpenXml.Wordprocessing.Paragraph();
                var totalRun = new Run();
                totalRun.AppendChild(new RunProperties(new Bold()));
                totalRun.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Text(departmentTotal.ToString("N2")));
                totalParagraph.AppendChild(totalRun);
                totalCell.AppendChild(totalParagraph);
                totalRow.AppendChild(totalCell);

                // Add remaining empty cells
                for (int i = 0; i < 3; i++)
                {
                    var cell = new TableCell();
                    cell.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Paragraph());
                    totalRow.AppendChild(cell);
                }

                table.AppendChild(totalRow);
                body.AppendChild(table);
                body.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Paragraph()); // Spacing

                grandTotal += departmentTotal;
                totalEmployees += departmentGroup.Count();
            }

            // Summary
            var summaryParagraph = new DocumentFormat.OpenXml.Wordprocessing.Paragraph();
            var summaryRun = new Run();
            summaryRun.AppendChild(new RunProperties(new Bold()));
            summaryRun.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Text("Summary"));
            summaryParagraph.AppendChild(summaryRun);
            body.AppendChild(summaryParagraph);

            body.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(new Run(new DocumentFormat.OpenXml.Wordprocessing.Text($"Total Departments: {groupedData.Count()}"))));
            body.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(new Run(new DocumentFormat.OpenXml.Wordprocessing.Text($"Total Employees: {totalEmployees}"))));

            var grandTotalParagraph = new DocumentFormat.OpenXml.Wordprocessing.Paragraph();
            var grandTotalRun = new Run();
            grandTotalRun.AppendChild(new RunProperties(new Bold()));
            grandTotalRun.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Text($"Grand Total Deduction: {grandTotal:N2}"));
            grandTotalParagraph.AppendChild(grandTotalRun);
            body.AppendChild(grandTotalParagraph);

            mainPart.Document.Save();
            document.Dispose();

            return stream.ToArray();
        }
    }
}