using GCTL.Core.Data;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.HrmEmployeePPAlertReports;
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
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

using PdfDocument2 = iText.Layout.Document;

namespace GCTL.Service.HrmEmployeePPAlertReports
{
    public class HrmEmployeePPAlertReportService : AppService<HrmEmployeeOfficialInfo>, IHrmEmployeePPAlertReportService
    {
        private readonly IRepository<HrmEmployeeOfficialInfo> empOffRepo;
        private readonly IRepository<HrmEmployee> empRepo;
        private readonly IRepository<CoreBranch> branchRepo;
        private readonly IRepository<CoreCompany> companyRepo;
        private readonly IRepository<HrmDefDepartment> departmentRepo;
        private readonly IRepository<HrmDefDesignation> designationRepo;
        private readonly IRepository<HrmDefProbationPeriodExtension> ppExtentionRepo;

        public HrmEmployeePPAlertReportService(
            IRepository<HrmEmployeeOfficialInfo> empOffReport,
            IRepository<HrmEmployee> empRepo,
            IRepository<CoreBranch> branchRepo,
            IRepository<CoreCompany> companyRepo,
            IRepository<HrmDefDepartment> departmentRepo,
            IRepository<HrmDefDesignation> designationRepo,
            IRepository<HrmDefProbationPeriodExtension> ppExtentionRepo
        ) : base(empOffReport)
        {
            this.empOffRepo = empOffReport;
            this.empRepo = empRepo;
            this.branchRepo = branchRepo;
            this.companyRepo = companyRepo;
            this.departmentRepo = departmentRepo;
            this.designationRepo = designationRepo;
            this.ppExtentionRepo = ppExtentionRepo;
        }

        public async Task<byte[]> GenerateExcelReport(List<HrmEmployeePPAlertReportViewModel> data)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();

            var worksheet = package.Workbook.Worksheets.Add("Probational Period Alert Report");

            worksheet.Cells["A1:J1"].Merge = true;
            worksheet.Cells["A1"].Value = data.Select(c => c.CompanyName);
            worksheet.Cells["A1"].Style.Font.Size = 16;
            worksheet.Cells["A1"].Style.Font.Bold = true;
            worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            worksheet.Cells["A2:J2"].Merge = true;
            worksheet.Cells["A2"].Value = "Probational Period Alert Report";
            worksheet.Cells["A2"].Style.Font.Size = 16;
            worksheet.Cells["A2"].Style.Font.Bold = true;
            worksheet.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            int currentRow = 4;
            var groupData = data.GroupBy(x => x.DepartmentName ?? "Unknown Department").OrderBy(x => x.Key);

            foreach (var dep in groupData)
            {
                worksheet.Cells[currentRow, 1, currentRow, 10].Merge = true;
                worksheet.Cells[currentRow, 1].Value = $"Department: {dep.Key}";
                worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
                worksheet.Cells[currentRow, 1].Style.Font.Size = 14;
                currentRow++;

                string[] headers = new string[]
                {
                    "SN","Employee ID", "Name", "Designation", "Department", "Gross Salary", "Joining Date", "Probation Period", "End on", "Service Length"
                };


                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cells[currentRow, i + 1];
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                currentRow++;

                int sn = 1;
                var uniqueEmp = new HashSet<string>();

                foreach (var item in dep.OrderBy(x => x.Code))
                {
                    if (!string.IsNullOrWhiteSpace(item.Code))
                        uniqueEmp.Add(item.Code);

                    
                    //currentRow++;
                    worksheet.Cells[currentRow, 1].Value = sn++;
                    worksheet.Cells[currentRow, 2].Value = item.Code;
                    worksheet.Cells[currentRow, 3].Value = item.Name;
                    worksheet.Cells[currentRow, 4].Value = item.DesingationName;
                    worksheet.Cells[currentRow, 5].Value = item.BranchName;
                    worksheet.Cells[currentRow, 6].Value = item.GrossSalary;
                    worksheet.Cells[currentRow, 7].Value = item.JoiningDate;
                    worksheet.Cells[currentRow, 8].Value = item.ProbationPeriod;
                    worksheet.Cells[currentRow, 9].Value = item.ProbationPeriodEndOn;
                    worksheet.Cells[currentRow, 10].Value = item.ServiceLength;

                    for (int i = 1; i <= headers.Length; i++)
                    {
                        var cell = worksheet.Cells[currentRow, i];
                        cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                        cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ExcelHorizontalAlignment alignment = i switch
                        {
                            3 => ExcelHorizontalAlignment.Left,
                            _ => ExcelHorizontalAlignment.Center
                        };
                        cell.Style.HorizontalAlignment = alignment;
                    }

                    currentRow++;
                    //sn++;
                }
                currentRow += 2;
            }
            worksheet.Cells.AutoFitColumns();
            return await package.GetAsByteArrayAsync();
        }

        public async Task<byte[]> GeneratePdfReport(List<HrmEmployeePPAlertReportViewModel> data, BaseViewModel model)
        {
            if (data == null || !data.Any())
            {
                return null;
            }

            using var stream = new MemoryStream();
            using var writer = new PdfWriter(stream);
            using var pdf = new PdfDocument(writer);
            using var document = new PdfDocument2(pdf, iText.Kernel.Geom.PageSize.A4.Rotate());

            string logoPath = "wwwroot/images/DP_logo.png";
            var imageData = ImageDataFactory.Create(logoPath);
            var logo = new Image(imageData);

            document.SetMargins(60, 36, 60, 36);

            var boldFont = PdfFontFactory.CreateFont(StandardFonts.TIMES_BOLD);
            var regularFont = PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN);

            var groupData = data
                .GroupBy(x => x.DepartmentName ?? "Unknown Department")
                .OrderBy(g => g.Key);

            int totalEmployees = data.Select(e => e.Code).Distinct().Count();

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

            float[] columnWidths = new float[]
            {
                3.8f, 6f, 15.8f, 8.4f, 8.4f, 7f, 8.4f, 8.4f, 8.4f, 8.8f
            };

            string[] headers = new string[]
            {
                "SN", "Employee ID", "Name", "Designation", "Branch", "Gross Salary", "Joining Date", "Probation Period", "End on", "Service Length"
            };


            foreach(var dpGroup in groupData)
            {
                if (NeedsPageBreak(document))
                {
                    document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
                }

                var table = new Table(UnitValue.CreatePercentArray(columnWidths))
                    .UseAllAvailableWidth()
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                    .SetMarginBottom(10)
                    .SetWidth(UnitValue.CreatePercentValue(100)
                    );

                var departmentHeaderRow = new Cell(1, 11)
                    .Add(new Paragraph($"Department: {dpGroup.Key}"))
                    .SetFontSize(11).SetTextAlignment(TextAlignment.LEFT)
                    .SetPadding(5).SetFont(PdfFontFactory.CreateFont(StandardFonts.TIMES_BOLD))
                    .SetBorderTop(iText.Layout.Borders.Border.NO_BORDER)
                    .SetBorderLeft(iText.Layout.Borders.Border.NO_BORDER)
                    .SetBorderRight(iText.Layout.Borders.Border.NO_BORDER)
                    .SetBorderBottom(iText.Layout.Borders.Border.NO_BORDER);

                table.AddHeaderCell(departmentHeaderRow);

                foreach (var header in headers)
                {
                    var headerCell = new Cell()
                        .Add(new Paragraph(header).SetFont(PdfFontFactory.CreateFont(StandardFonts.TIMES_BOLD))
                        .SetFontSize(9)).SetTextAlignment(TextAlignment.CENTER)
                        .SetBorder(new iText.Layout.Borders.SolidBorder(ColorConstants.LIGHT_GRAY, 0.2f));
                    table.AddHeaderCell(headerCell);
                }

                table.SetSkipFirstHeader(false);
                table.SetSkipLastFooter(false);

                int sn = 1;
                var uniqueEmp = new HashSet<string>();
                var orderedItems = dpGroup.OrderBy(x => x.Code).ToList();

                foreach(var item in orderedItems)
                {
                    var values = new[]
                    {
                        sn++.ToString(),
                        item.Code ?? "",
                        item.Name ?? "",
                        item.DesingationName ?? "",
                        item.BranchName ?? "",
                        item.GrossSalary ?? "",
                        item.JoiningDate ?? "",
                        item.ProbationPeriod ?? "",
                        item.ProbationPeriodEndOn ?? "",
                        item.ServiceLength ?? ""
                    };

                    for(int i=0; i<values.Length; i++)
                    {
                        var cell = new Cell()
                            .Add(new Paragraph(values[i]).SetFont(regularFont).SetFontSize(8))
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetBorder(new iText.Layout.Borders.SolidBorder(ColorConstants.LIGHT_GRAY, 0.2f));
                        if (i == 2) // Name column
                        {
                            cell.SetTextAlignment(TextAlignment.LEFT);
                        }
                        table.AddCell(cell);
                    }
                    //sn++;
                }
                document.Add(table);
            }

            var summaryTable = new Table(new float[] { 1 });
            summaryTable.SetWidth(UnitValue.CreatePercentValue(100)).SetMarginTop(10);

            summaryTable.AddCell(new Cell().Add(new Paragraph($"Total Employees: {totalEmployees}"))
              .SetTextAlignment(TextAlignment.LEFT)
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
                string reportText = "Employee Probation Period Alert Report";
                float reportTextWidth = reportFont.GetWidth(reportText, reportFontSize);

                canvas.BeginText()
                    .SetFontAndSize(reportFont, reportFontSize)
                    .MoveText(headerStartX - (reportTextWidth / 2), headerTopY - 18)
                    .ShowText(reportText)
                    .EndText();

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
        public async Task<EmployeeFilterResultViewModel> GetDataAsync(EmployeeFilterViewModel filter)
        {
            var results = new EmployeeFilterResultViewModel();

            var query = from e in empOffRepo.All().AsNoTracking()
                        join emp in empRepo.All().AsNoTracking() on e.EmployeeId equals emp.EmployeeId
                        join c in companyRepo.All().AsNoTracking() on emp.CompanyCode equals c.CompanyCode
                        join b in branchRepo.All().AsNoTracking() on e.BranchCode equals b.BranchCode into branchGroup
                        from b in branchGroup.DefaultIfEmpty()
                        join des in designationRepo.All().AsNoTracking() on e.DesignationCode equals des.DesignationCode into designationGroup
                        from des in designationGroup.DefaultIfEmpty()
                        join dep in departmentRepo.All().AsNoTracking() on e.DepartmentCode equals dep.DepartmentCode into departmentGroup
                        from dep in departmentGroup.DefaultIfEmpty()

                        where ((e.ConfirmeDate == null || e.ConfirmeDate == new DateTime(1900, 1, 1)) && e.EmployeeStatus.Equals("01") && e.EmpTypeCode.Equals("02"))

                        select new { e, emp, c, b, des, dep };

            if (filter.CompanyCodes?.Any() == true)
                query = query.Where(x => x.e.CompanyCode != null && filter.CompanyCodes.Contains(x.e.CompanyCode));

            // FIX 1: Changed from x.e.CompanyCode to x.e.DepartmentCode
            if (filter.DepartmentCodes?.Any() == true)
                query = query.Where(x => x.e.DepartmentCode != null && filter.DepartmentCodes.Contains(x.e.DepartmentCode));

            // FIX 2: Changed from x.e.CompanyCode to x.e.DesignationCode
            if (filter.DesignationCodes?.Any() == true)
                query = query.Where(x => x.e.DesignationCode != null && filter.DesignationCodes.Contains(x.e.DesignationCode));

            if (filter.ProbationEndDays != null && int.TryParse(filter.ProbationEndDays, out int probationEndDays))
            {
                var cutoffDate = DateTime.Today.AddDays(-probationEndDays);

                query = query.Where(x => (x.e.JoiningDate != null && x.e.JoiningDate != new DateTime(1900, 1, 1)) && x.e.JoiningDate.Value <= cutoffDate
                );
            }

            // FIX 3: Updated property names and date format handling
            if (!string.IsNullOrWhiteSpace(filter.DateFrom) &&
                 DateTime.TryParseExact(filter.DateFrom, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var fromDate))
            {
                query = query.Where(x => x.e.JoiningDate.HasValue && x.e.JoiningDate.Value.Date >= fromDate.Date);
            }

            // FIX 4: Updated property name and consistent date format
            if (!string.IsNullOrWhiteSpace(filter.DateTo) &&
                DateTime.TryParseExact(filter.DateTo, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var toDate))
            {
                query = query.Where(x => x.e.JoiningDate.HasValue && x.e.JoiningDate.Value.Date <= toDate.Date);
            }

            var allFilter = await query.ToListAsync();
            var empIds = allFilter.Select(x => x.e.EmployeeId).ToList();

            var extData = await ppExtentionRepo.All().AsNoTracking()
                .Where(x => empIds.Contains(x.EmployeeId))
                .OrderBy(x => x.EmployeeId)
                .ThenByDescending(x => x.Ppeid)
                .ToListAsync();

            var extensionsByEmp = extData
                .GroupBy(x => x.EmployeeId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var latestExtDict = extensionsByEmp.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.FirstOrDefault()
            );

            var company = await companyRepo.All().Where(x => x.CompanyCode == "001")
                .Select(x => new LookupItemDto
                {
                    Code = x.CompanyCode,
                    Name = x.CompanyName
                }).Distinct().ToListAsync();

            var branch = await branchRepo.All().Where(x => x.CompanyCode == "001" && x.BranchCode != null && x.BranchName != null)
                .Select(x => new LookupItemDto
                {
                    Code = x.BranchCode,
                    Name = x.BranchName
                }).Distinct().ToListAsync();

            results.Employees = allFilter.Select(x =>
            {
                string serviceLength = "";
                string pPeriod = "";
                string ePeriod = "";
                DateTime? ePeriodEndOn = null;
                DateTime? pPeriodEndOn = null;

                var latestExt = latestExtDict.ContainsKey(x.e.EmployeeId) ? latestExtDict[x.e.EmployeeId] : null;

                var allExt = extensionsByEmp.ContainsKey(x.e.EmployeeId) ? extensionsByEmp[x.e.EmployeeId] : new List<HrmDefProbationPeriodExtension>();

                if (x.e.JoiningDate.HasValue && x.e.JoiningDate.Value != new DateTime(1900, 1, 1))
                {
                    serviceLength = CalculateDateLengthInDays(x.e.JoiningDate.Value, DateTime.Today);

                    if (x.e.ProbationPeriod != null && x.e.ProbationPeriodType != null)
                    {
                        pPeriodEndOn = CalculateProbationEndDate(x.e.JoiningDate.Value, x.e.ProbationPeriod, x.e.ProbationPeriodType);
                        if (pPeriodEndOn != null)
                            pPeriod = CalculateDateLengthInDays(x.e.JoiningDate.Value, pPeriodEndOn.Value);
                    }
                }

                // FIX 5: Fixed extension calculation logic
                if (latestExt != null && allExt.Any())
                {
                    DateTime? currentEndDate = pPeriodEndOn;
                    DateTime? finalExtensionEndDate = currentEndDate;

                    // Process extensions in chronological order
                    foreach (var item in allExt.OrderBy(ext => ext.Ppeid))
                    {
                        if (item.ExtendedPeriod != null && !string.IsNullOrEmpty(item.PeriodInfoId) && finalExtensionEndDate.HasValue)
                        {
                            if (int.TryParse(item.ExtendedPeriod, out int extendedPeriodValue))
                            {
                                switch (item.PeriodInfoId)
                                {
                                    case "01":
                                        finalExtensionEndDate = finalExtensionEndDate.Value.AddYears(extendedPeriodValue);
                                        break;
                                    case "02":
                                        finalExtensionEndDate = finalExtensionEndDate.Value.AddMonths(extendedPeriodValue * 6);
                                        break;
                                    case "03":
                                        finalExtensionEndDate = finalExtensionEndDate.Value.AddMonths(extendedPeriodValue * 3);
                                        break;
                                    case "04":
                                        finalExtensionEndDate = finalExtensionEndDate.Value.AddMonths(extendedPeriodValue);
                                        break;
                                    case "05":
                                        finalExtensionEndDate = finalExtensionEndDate.Value.AddDays(extendedPeriodValue * 7);
                                        break;
                                    case "06":
                                        finalExtensionEndDate = finalExtensionEndDate.Value.AddDays(extendedPeriodValue);
                                        break;
                                }
                            }
                        }
                    }

                    if (finalExtensionEndDate.HasValue && currentEndDate.HasValue)
                    {
                        ePeriod = CalculateDateLengthInDays(currentEndDate.Value, finalExtensionEndDate.Value);
                        ePeriodEndOn = finalExtensionEndDate;
                    }
                }

                // FIX 6: Calculate total probation period including extensions
                string totalProbationPeriod = pPeriod;
                DateTime? totalProbationEndDate = pPeriodEndOn;

                if (ePeriodEndOn.HasValue)
                {
                    totalProbationEndDate = ePeriodEndOn;
                    if (x.e.JoiningDate.HasValue)
                    {
                        totalProbationPeriod = CalculateDateLengthInDays(x.e.JoiningDate.Value, ePeriodEndOn.Value);
                    }
                }

                return new HrmEmployeePPAlertReportViewModel
                {
                    Code = x.e.EmployeeId,
                    Name = string.Join(" ", new[] { x.emp.FirstName, x.emp.LastName }.Where(n => !string.IsNullOrWhiteSpace(n))),
                    DesingationName = x.des?.DesignationName ?? "", // FIX 7: Fixed typo and added null check
                    DepartmentName = x.dep?.DepartmentName ?? "", // FIX 8: Added null check
                    CompanyName = x.c?.CompanyName ?? "", // FIX 9: Added null check
                    GrossSalary = (x.e.GrossSalary == 0.00m ? "0" : x.e.GrossSalary.ToString("N2")), // FIX 10: Improved null handling
                    JoiningDate = x.e.JoiningDate.HasValue ? x.e.JoiningDate.Value.ToString("dd/MM/yyyy") : "",
                    ProbationPeriod = totalProbationPeriod, // FIX 11: Use total probation period
                    ProbationPeriodEndOn = totalProbationEndDate.HasValue ? totalProbationEndDate.Value.ToString("dd/MM/yyyy") : "", // FIX 12: Use total end date
                    BranchName = x.b.BranchName,
                    ServiceLength = serviceLength
                };

            }).ToList();

            results.LookupData["companies"] = company;

            results.LookupData["branches"] = branch;

            results.LookupData["departments"] = allFilter
                .Where(x => x.dep?.DepartmentCode != null && x.dep.DepartmentName != null)
                .GroupBy(x => new { x.dep.DepartmentCode, x.dep.DepartmentName })
                .Select(x => new LookupItemDto { Code = x.Key.DepartmentCode, Name = x.Key.DepartmentName })
                .ToList();

            results.LookupData["designations"] = allFilter
                .Where(x => x.des?.DesignationCode != null && x.des.DesignationName != null)
                .GroupBy(x => new { x.des.DesignationCode, x.des.DesignationName })
                .Select(x => new LookupItemDto { Code = x.Key.DesignationCode, Name = x.Key.DesignationName })
                .ToList();

            return results;
        }

        private DateTime? CalculateProbationEndDate(DateTime? value, string probationPeriod, string probationPeriodType)
        {
            if (value == null || string.IsNullOrEmpty(probationPeriod) || !int.TryParse(probationPeriod, out int period))
                return null;

            switch (probationPeriodType)
            {
                case "01": // Years
                    return value.Value.AddYears(period);
                case "02": // Half-Years
                    return value.Value.AddMonths(period * 6);
                case "03": // Quarter-Years
                    return value.Value.AddMonths(period * 3);
                case "04": // Months
                    return value.Value.AddMonths(period);
                case "05": // Weeks
                    return value.Value.AddDays(period * 7);
                case "06": // Days
                    return value.Value.AddDays(period);
                default:
                    return null;
            }
        }

        private string CalculateDateLengthInDays(DateTime startDate, DateTime endDate)
        {
            if (startDate == new DateTime(1900, 1, 1) || endDate == new DateTime(1900, 1, 1))
                return "";

            if (startDate <= endDate)
            {
                var totalDays = (endDate - startDate).Days;
                return $"{totalDays} {(totalDays == 1 ? "day" : "days")}";
            }
            else
            {
                return "";
            }
        }

        // FIX 13: Additional helper method for better date calculation
        private string CalculateDateLengthDetailed(DateTime startDate, DateTime endDate)
        {
            if (startDate == new DateTime(1900, 1, 1) || endDate == new DateTime(1900, 1, 1))
                return "";

            if (startDate > endDate)
                return "";

            var years = endDate.Year - startDate.Year;
            var months = endDate.Month - startDate.Month;
            var days = endDate.Day - startDate.Day;

            if (days < 0)
            {
                months--;
                days += DateTime.DaysInMonth(startDate.Year, startDate.Month);
            }

            if (months < 0)
            {
                years--;
                months += 12;
            }

            var parts = new List<string>();

            if (years > 0)
                parts.Add($"{years} {(years == 1 ? "year" : "years")}");

            if (months > 0)
                parts.Add($"{months} {(months == 1 ? "month" : "months")}");

            if (days > 0)
                parts.Add($"{days} {(days == 1 ? "day" : "days")}");

            return parts.Any() ? string.Join(", ", parts) : "0 days";
        }
    }
}

