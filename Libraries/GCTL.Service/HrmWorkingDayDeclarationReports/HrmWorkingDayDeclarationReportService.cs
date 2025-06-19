using GCTL.Core.Data;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.HrmWorkingDayDeclarations;
using GCTL.Data.Models;
using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf;
using iText.Layout.Borders;
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
using static Humanizer.In;
using iText.Layout;

namespace GCTL.Service.HrmWorkingDayDeclarationReports
{
    public class HrmWorkingDayDeclarationReportService:AppService<HrmAttWorkingDayDeclaration>, IHrmWorkingDayDeclarationReportService
    {
        private readonly IRepository<HrmPayOthersAdjustmentEntry> entryRepo;
        private readonly IRepository<HrmEmployee> employeeRepo;
        private readonly IRepository<HrmEmployeeOfficialInfo> empOffRepo;
        private readonly IRepository<HrmDefDesignation> desiRepo;
        private readonly IRepository<HrmDefDepartment> depRepo;
        private readonly IRepository<CoreBranch> branchRepo;
        private readonly IRepository<CoreCompany> companyRepo;
        private readonly IRepository<HrmPayMonth> monthRepo;
        private readonly IRepository<HrmDefEmployeeStatus> statusRepo;
        private readonly IRepository<HrmDefEmpType> eTypeRepo;

        private readonly IRepository<HrmAttWorkingDayDeclaration> repo;

        public HrmWorkingDayDeclarationReportService(
            IRepository<HrmAttWorkingDayDeclaration> repo,
            IRepository<HrmEmployee> employeeRepo,
            IRepository<HrmEmployeeOfficialInfo> empOffRepo,
            IRepository<HrmDefDesignation> desiRepo,
            IRepository<HrmDefDepartment> depRepo,
            IRepository<CoreBranch> branchRepo, 
            IRepository<CoreCompany> companyRepo,
            IRepository<HrmPayMonth> monthRepo
,
            IRepository<HrmDefEmployeeStatus> statusRepo,
            IRepository<HrmDefEmpType> eTypeRepo
        ) : base(repo)
        {
            this.repo = repo;
            this.employeeRepo = employeeRepo;
            this.empOffRepo = empOffRepo;
            this.desiRepo = desiRepo;
            this.depRepo = depRepo;
            this.branchRepo = branchRepo;
            this.companyRepo = companyRepo;
            this.monthRepo = monthRepo;
            this.statusRepo = statusRepo;
            this.eTypeRepo = eTypeRepo;
        }

        public async Task<byte[]> GenerateExcelReport(List<ReportFilterResultDto> data, bool isDate)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Working Day Declaration");

            //worksheet.View.FreezePanes(4, 0);

            string reportPeriod;

            if (isDate)
            {
                var days = data
                    .Select(d => d.WorkingDayDates.Value.ToString("dd/MM/yyyy"))
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

            try
            {
                string logoPath = "wwwroot/images/DP_logo.png";

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
            }

            worksheet.Row(1).Height = 50;
            worksheet.Cells["A1:I1"].Merge = true;
            worksheet.Cells["A1"].Value = data.Select(c=>c.CompanyName);
            worksheet.Cells["A1"].Style.Font.Size = 16;
            worksheet.Cells["A1"].Style.Font.Bold = true;
            worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            worksheet.Cells["A2:I2"].Merge = true;
            worksheet.Cells["A2"].Value = "Working Day Declaration Report";
            worksheet.Cells["A2"].Style.Font.Size = 14;
            worksheet.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            worksheet.Cells["A3:I3"].Merge = true;
            worksheet.Cells["A3"].Value = $"{reportPeriod}";
            worksheet.Cells["A3"].Style.Font.Size = 14;
            worksheet.Cells["A3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            int currentRow = 5;

            var groupedByDate = data.GroupBy(x => x.WorkingDayDates.Value.Date).OrderBy(g => g.Key);
            
            foreach (var dateGroup in groupedByDate)
            {
                if (!isDate)
                {
                    worksheet.Cells[currentRow, 1, currentRow, 9].Merge = true;
                    worksheet.Cells[currentRow, 1].Value = $"Date: {dateGroup.Key.ToString("dd/MM/yyyy")}";
                    worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
                    worksheet.Cells[currentRow, 1].Style.Font.Size = 14;
                    currentRow++;
                }

                var departmentGroups = dateGroup.GroupBy(x => x.DepartmentName ?? "Unknown Department")
                                                .OrderBy(g => g.Key);

                int dateEmployeeCount = 0;
                int dateDeductionCount = 0;

                foreach (var departmentGroup in departmentGroups)
                {
                    worksheet.Cells[currentRow, 1, currentRow, 9].Merge = true;
                    worksheet.Cells[currentRow, 1].Value = $"Department: {departmentGroup.Key}";
                    worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
                    worksheet.Cells[currentRow, 1].Style.Font.Size = 12;
                    currentRow++;

                    string[] headers = { "SL", "Employee ID", "Name", "Designation", "Branch","Employee Type", "Joining Date","Employee Status", "Remarks" };

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
                    var uniqueEmployeesInDept = new HashSet<string>();

                    foreach (var item in departmentGroup.OrderBy(x => x.Code))
                    {
                        dateDeductionCount++;

                        if (!string.IsNullOrEmpty(item.Code))
                        {
                            uniqueEmployeesInDept.Add(item.Code);
                        }

                        worksheet.Cells[currentRow, 1].Value = serialNo;
                        worksheet.Cells[currentRow, 2].Value = item.Code ?? "";
                        worksheet.Cells[currentRow, 3].Value = item.Name ?? "";
                        worksheet.Cells[currentRow, 4].Value = item.DesignationName ?? "";
                        worksheet.Cells[currentRow, 5].Value = item.BranchName ?? "";
                        worksheet.Cells[currentRow, 6].Value = item.EmployeeType ?? "";
                        worksheet.Cells[currentRow, 7].Value = item.JoiningDate ?? "";
                        worksheet.Cells[currentRow, 8].Value = item.EmployeeStatus ?? "";
                        worksheet.Cells[currentRow, 9].Value = item.Remarks ?? "";

                        for (int col = 1; col <= 9; col++)
                        {
                            worksheet.Cells[currentRow, col].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        }
                        currentRow++;
                        serialNo++;
                    }

                    dateEmployeeCount += uniqueEmployeesInDept.Count;

                    currentRow++; 
                }

                worksheet.Cells[currentRow, 1, currentRow, 9].Merge = true;
                worksheet.Cells[currentRow, 1].Value = $"Total Employee on {dateGroup.Key:d}: {dateEmployeeCount}";
                worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
                currentRow++;
                currentRow += 2;
            }

            currentRow++;

            worksheet.Cells.AutoFitColumns();

            return package.GetAsByteArray();
        }

        public async Task<byte[]> GeneratePdfReport(List<ReportFilterResultDto> data, BaseViewModel model, bool isDate)
        {
            using var stream = new MemoryStream();
            using var writer = new PdfWriter(stream);
            using var pdf = new PdfDocument(writer);
            using var document = new iText.Layout.Document(pdf, iText.Kernel.Geom.PageSize.A4.Rotate());

            var timesRegular = PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN);
            var timesBold = PdfFontFactory.CreateFont(StandardFonts.TIMES_BOLD);

            document.SetMargins(80, 36, 60, 36);

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

                        return availableHeight < (fullPageContentHeight * 0.20f);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error checking page break: {ex.Message}");
                }
                return false;
            }

            foreach (var dateGroup in dateGroupedData)
            {
                if (!isFirst && NeedsPageBreak(document))
                {
                    document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
                }

                if (!isDate)
                {
                    document.Add(new Paragraph($"Date: {dateGroup.Key.ToString("dd/MM/yyyy")}")
                        .SetFontSize(11)
                        .SetPaddingLeft(2)
                        .SetFont(timesBold)
                        .SetMarginTop(isFirst ? 0 : 20)
                        .SetMarginBottom(10)
                        .SetTextAlignment(TextAlignment.LEFT));
                }

                var departmentGroupedData = dateGroup
                    .GroupBy(x => x.DepartmentName ?? "Unknown Department")
                    .OrderBy(g => g.Key);

                int dateTotalEmployees = 0;
                int dateTotalDeductions = 0;
                bool isFirstDepartment = true;

                foreach (var departmentGroup in departmentGroupedData)
                {
                    if (!isFirstDepartment && NeedsPageBreak(document))
                        document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
                    
                    float[] columnWidths = new float[] { 3.5f, 11.5f, 21.0f, 21.0f, 12.5f, 11.5f, 11.5f, 11.5f, 16.0f };

                    var table = new Table(UnitValue.CreatePercentArray(columnWidths));
                    table.SetWidth(UnitValue.CreatePercentValue(100));

                    string[] headers = { "SN", "Employee ID", "Name", "Designation", "Branch", "Employee Type", "Joining Date", "Employee Status", "Remarks" };

                    var departmentHeaderRow = new Cell(1, 9)
                        .Add(new Paragraph($"Department: {departmentGroup.Key}"))
                        .SetFont(timesBold)
                        .SetFontSize(11)
                        .SetTextAlignment(TextAlignment.LEFT)
                        .SetBorderTop(iText.Layout.Borders.Border.NO_BORDER)
                        .SetBorderLeft(iText.Layout.Borders.Border.NO_BORDER)
                        .SetBorderRight(iText.Layout.Borders.Border.NO_BORDER)
                        .SetBorderBottom(iText.Layout.Borders.Border.NO_BORDER);

                    table.AddHeaderCell(departmentHeaderRow);

                    foreach (var header in headers)
                    {
                        var headerCell = new Cell().Add(new Paragraph(header).SetFont(timesBold).SetFontSize(7))
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 0.2f));
                        table.AddHeaderCell(headerCell);
                    }

                    table.SetSkipFirstHeader(false);
                    table.SetSkipLastFooter(false);

                    int serialNo = 1;
                    var uniqueEmployees = new HashSet<string>();
                    var orderedItems = departmentGroup.OrderBy(x => x.Code).ToList();

                    foreach (var item in orderedItems)
                    {
                        dateTotalDeductions++;

                        if (!string.IsNullOrEmpty(item.Code))
                            uniqueEmployees.Add(item.Code);

                        var values = new[]
                        {
                            serialNo.ToString(),
                            item.Code ?? "",
                            item.Name ?? "",
                            item.DesignationName ?? "",
                            item.BranchName ?? "",
                            item.EmployeeStatus??"",
                            item.JoiningDate??"",
                            item.EmployeeType??"",
                            item.Remarks ?? ""
                        };

                        for (int j = 0; j < values.Length; j++)
                        {
                            var cell = new Cell().Add(new Paragraph(values[j]))
                                .SetFontSize(7)
                                .SetBorder(new SolidBorder(ColorConstants.LIGHT_GRAY, 0.2f));

                            if (j == 0 || j == 1 || j == 5 || j == 6 || j == 7)
                            {
                                cell.SetTextAlignment(TextAlignment.CENTER);
                            }

                            table.AddCell(cell);
                        }

                        serialNo++;
                    }

                    for (int i = 0; i < 2; i++)
                    {
                        var emptyCell = new Cell().Add(new Paragraph(""))
                            .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                            .SetFontSize(7);
                        table.AddCell(emptyCell);
                    }

                    document.Add(table);
                    dateTotalEmployees += uniqueEmployees.Count;
                    isFirstDepartment = false;
                }

                document.Add(new Paragraph($"Total Employees on {dateGroup.Key.ToString("dd/MM/yyyy")}: ")
                    .SetFontSize(9)
                    .SetFont(timesRegular)
                    .SetMarginTop(10)
                    .SetMarginBottom(5).Add(new Text(dateTotalEmployees.ToString()).SetFont(timesBold)));
                
                isFirst = false;
            }

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
                float companyFontSize = 16;
                string companyText = data.Select(c => c.CompanyName).Distinct().FirstOrDefault();
                float companyTextWidth = timesBold.GetWidth(companyText, companyFontSize);

                canvas.BeginText()
                    .SetFontAndSize(companyFont, companyFontSize)
                    .MoveText(headerStartX - (companyTextWidth / 2), headerTopY)
                    .ShowText(companyText)
                    .EndText();

                var reportFont = PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN);
                float reportFontSize = 12;
                string reportText = "Working Day Declaration Report";
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
            var query = from r in repo.All().AsNoTracking()
                        join eoi in empOffRepo.All().AsNoTracking() on r.EmployeeId equals eoi.EmployeeId
                        join e in employeeRepo.All().AsNoTracking() on r.EmployeeId equals e.EmployeeId into empJoin
                        from e in empJoin.DefaultIfEmpty()
                        join dg in desiRepo.All().AsNoTracking() on eoi.DesignationCode equals dg.DesignationCode into dgJoin
                        from dg in dgJoin.DefaultIfEmpty()
                        join cb in branchRepo.All().AsNoTracking() on eoi.BranchCode equals cb.BranchCode into cbJoin
                        from cb in cbJoin.DefaultIfEmpty()
                        join dp in depRepo.All().AsNoTracking() on eoi.DepartmentCode equals dp.DepartmentCode into dpJoin
                        from dp in dpJoin.DefaultIfEmpty()
                        join cp in companyRepo.All().AsNoTracking() on eoi.CompanyCode equals cp.CompanyCode into cpJoin
                        from cp in cpJoin.DefaultIfEmpty()
                        join es in statusRepo.All().AsNoTracking() on eoi.EmployeeStatus equals es.EmployeeStatusId into stJoin
                        from es in stJoin.DefaultIfEmpty()
                        join et in eTypeRepo.All().AsNoTracking() on eoi.EmpTypeCode equals et.EmpTypeCode into etJoin
                        from et in etJoin.DefaultIfEmpty()
                        join pm in monthRepo.All().AsNoTracking() on r.WorkingDayDate.Value.Month equals pm.MonthId into pmJoin
                        from pm in pmJoin.DefaultIfEmpty()
                        select new
                        {
                            r, eoi, e, dg, cb, dp, cp, pm, es, et,

                            empFirstName = (e.FirstName + " ") ?? "",
                            empLastName = (e.LastName) ?? ""
                        };


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

            if (!string.IsNullOrEmpty(filter.SalaryYear))
                query = query.Where(x => x.r.WorkingDayDate.Value.Year.ToString() == filter.SalaryYear);

            if (filter.WorkingDates?.Any() == true)
                query = query.Where(x => filter.WorkingDates.Contains(x.r.WorkingDayDate.Value.ToString()));

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


                Months = await monthRepo.All().AsNoTracking()
                    .OrderBy(m => m.MonthId)
                    .Select(m => new ReportFilterResultDto { Code = m.MonthId.ToString(), Name = m.MonthName })
                    .ToListAsync(),

                Employees = await query.Where(x => x.e.EmployeeId != null)
                    .Select(x => new ReportFilterResultDto { Code = x.e.EmployeeId, Name = x.empFirstName + x.empLastName })
                    .Distinct().ToListAsync(),

                WorkingDayDates = await query.Where(x=>x.r.WorkingDayDate!=null)
                .Select(x=>new ReportFilterResultDto
                {
                    Code= x.r.WorkingDayDate.Value.ToString(), Name=x.r.WorkingDayDate.Value.ToString("dd/MM/yyyy")
                })
                .Distinct().ToListAsync(),

                WorkingDays = await query.Where(x => x.r.WorkingDayCode != null).Select(x => new ReportFilterResultDto
                {
                    Code = x.e.EmployeeId,
                    Name = x.empFirstName + x.empLastName,
                    DesignationName = x.dg.DesignationName ?? "",
                    DepartmentName = x.dp.DepartmentName ?? "",
                    BranchName = x.cb.BranchName ?? "",
                    CompanyName = x.cp.CompanyName ?? "",
                    EmpId = x.e.EmployeeId,
                    JoiningDate = x.eoi.JoiningDate.Value.ToString("dd/MM/yyyy") ?? "",
                    EmployeeStatus=x.es.EmployeeStatus ?? "",
                    EmployeeType=x.et.EmpTypeName ?? "",
                    WorkingDayCode = x.r.WorkingDayCode,
                    WorkingDayDates = x.r.WorkingDayDate
                }).ToListAsync()
            };

            return result;

        }
    }
}
