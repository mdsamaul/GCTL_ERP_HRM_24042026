using GCTL.Core.Data;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.HrmServiceNotConfirmEntries;
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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PdfDocument2 = iText.Layout.Document;

namespace GCTL.Service.HrmServiceNotConfirmationReports
{
    public class HrmServiceNotConfirmationReportService : AppService<HrmServiceNotConfirmationEntry>, IHrmServiceNotConfirmationReportService
    {
        private readonly IRepository<HrmEmployee> employeeRepo;
        private readonly IRepository<HrmEmployeeOfficialInfo> empOffRepo;
        private readonly IRepository<HrmServiceNotConfirmationEntry> sncRepo;
        private readonly IRepository<CoreBranch> branchRepo;
        private readonly IRepository<CoreCompany> companyRepo;
        private readonly IRepository<HrmDefDepartment> departmentRepo;
        public HrmServiceNotConfirmationReportService(
            IRepository<HrmServiceNotConfirmationEntry> sncRepo,
            IRepository<HrmEmployee> employeeRepo,
            IRepository<HrmEmployeeOfficialInfo> empOffRepo,
            IRepository<CoreBranch> branchRepo,
            IRepository<CoreCompany> companyRepo,
            IRepository<HrmDefDepartment> departmentRepo)
            : base(sncRepo)
        {
            this.sncRepo = sncRepo;
            this.employeeRepo = employeeRepo;
            this.empOffRepo = empOffRepo;
            this.branchRepo = branchRepo;
            this.companyRepo = companyRepo;
            this.departmentRepo = departmentRepo;
        }

        public async Task<ReportFilterListViewModel> GetDataAsync(ReportFilterViewModel filter)
        {
            var query = from e in empOffRepo.All().AsNoTracking()
                        join emp in employeeRepo.All().AsNoTracking() on e.EmployeeId equals emp.EmployeeId
                        join snc in sncRepo.All().AsNoTracking() on e.EmployeeId equals snc.EmployeeId
                        join b in branchRepo.All().AsNoTracking() on e.BranchCode equals b.BranchCode into branchGroup
                        from b in branchGroup.DefaultIfEmpty()
                        join d in departmentRepo.All().AsNoTracking() on e.DepartmentCode equals d.DepartmentCode into depGroup
                        from d in depGroup.DefaultIfEmpty()
                        join c in companyRepo.All().AsNoTracking() on e.CompanyCode equals c.CompanyCode into companyGroup
                        from c in companyGroup.DefaultIfEmpty()

                        select new
                        {
                            e,
                            emp,
                            snc,
                            b,
                            c,
                            d
                        };

            if (filter.CompanyCode?.Any() == true)
                query = query.Where(x => x.e.CompanyCode != null && filter.CompanyCode.Contains(x.e.CompanyCode));

            if (filter.BranchCodes?.Any() == true)
                query = query.Where(x => x.e.BranchCode != null && filter.BranchCodes.Contains(x.e.BranchCode));

            if(filter.EmployeeId?.Any() == true)
                query = query.Where(x => x.e.EmployeeId != null && filter.EmployeeId.Contains(x.e.EmployeeId));

            if (!string.IsNullOrWhiteSpace(filter.JoiningDateFrom) &&
                 DateTime.TryParseExact(filter.JoiningDateFrom, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var fromDate))
            {
                query = query.Where(x => x.e.JoiningDate.HasValue && x.e.JoiningDate.Value.Date >= fromDate.Date);
            }

            if (!string.IsNullOrWhiteSpace(filter.JoiningDateTo) &&
                DateTime.TryParseExact(filter.JoiningDateTo, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var toDate))
            {
                query = query.Where(x => x.e.JoiningDate.HasValue && x.e.JoiningDate.Value.Date <= toDate.Date);
            }

            if (!string.IsNullOrWhiteSpace(filter.EffectiveDateFrom) &&
                 DateTime.TryParseExact(filter.EffectiveDateFrom, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var fromEffDate))
            {
                query = query.Where(x => x.snc.EffectiveDate.HasValue && x.snc.EffectiveDate.Value.Date >= fromEffDate.Date);
            }

            if (!string.IsNullOrWhiteSpace(filter.EffectiveDateTo) &&
                DateTime.TryParseExact(filter.EffectiveDateTo, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var toEffDate))
            {
                query = query.Where(x => x.snc.EffectiveDate.HasValue && x.snc.EffectiveDate.Value.Date <= toEffDate.Date);
            }

            if (!string.IsNullOrWhiteSpace(filter.DuePaymentDateFrom) &&
                 DateTime.TryParseExact(filter.JoiningDateFrom, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var fromDueDate))
            {
                query = query.Where(x => x.snc.DuePaymentDate.HasValue && x.snc.DuePaymentDate.Value.Date >= fromDueDate.Date);
            }

            if (!string.IsNullOrWhiteSpace(filter.DuePaymentDateTo) &&
                DateTime.TryParseExact(filter.DuePaymentDateTo, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var toDueDate))
            {
                query = query.Where(x => x.snc.DuePaymentDate.HasValue && x.snc.DuePaymentDate.Value.Date <= toDueDate.Date);
            }

            var company = await companyRepo.All().Where(c => c.CompanyCode == "001")
                .Select(c => new ReportFilterResultViewModel
                {
                    Code = c.CompanyCode,
                    Name = c.CompanyName
                }).ToListAsync();

            var allBranch = await branchRepo.All().Where(b => b.BranchCode != null && b.BranchName != null)
                .OrderBy(b => b.BranchCode)
                .Select(b => new ReportFilterResultViewModel
                {
                    Code = b.BranchCode,
                    Name = b.BranchName
                }).Distinct().ToListAsync();

            var result = new ReportFilterListViewModel
            {
                Companies = company,
                Branches = allBranch,

                EmployeeIds = query.Select(x => new ReportFilterResultViewModel
                {
                    Code = x.e.EmployeeId,
                    Name = (x.emp.FirstName ?? "") + " " + (x.emp.LastName ?? "") + " " + $"({x.e.EmployeeId})"
                }).Distinct().ToList(),

                ServiceNotConfirms = await query.Select(x => new ReportFilterResultViewModel
                {
                    Code = x.e.EmployeeId,
                    Name = (x.emp.FirstName ?? "") + " " + (x.emp.LastName ?? ""),
                    CompanyName = x.c.CompanyName,
                    DepartmentName = x.d.DepartmentName,
                    EmployeeId = x.e.EmployeeId,
                    SncId = x.snc.Sncid,
                    JoiningDate = x.e.JoiningDate.HasValue ? x.e.JoiningDate.Value.ToString("yyyy-MM-dd") : null,
                    EffectiveDate = x.snc.EffectiveDate.HasValue ? x.snc.EffectiveDate.Value.ToString("yyyy-MM-dd") : null,
                    DuePaymentDate = x.snc.DuePaymentDate.HasValue ? x.snc.DuePaymentDate.Value.ToString("yyyy-MM-dd") : null,
                    RefLetterDate = x.snc.RefLetterDate.HasValue ? x.snc.RefLetterDate.Value.ToString("yyyy-MM-dd") : null,
                    RefLetterNo = x.snc.RefLetterNo??"",
                    Remarks = x.snc.Remarks ?? ""
                }).ToListAsync()
            };
            return result;
        }

        public async Task<byte[]> GeneratePdfReport(List<ReportFilterResultViewModel> data, BaseViewModel model)
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
                3.8f, 9.4f, 15.8f, 8.4f, 7f, 8.4f, 8.4f, 8.4f, 19.8f
            };

            string[] headers =
            {
                "Sl No", "Employee ID", "Employee Name", "Joining Date", "Effective Date",
                "Due Payment Date", "Ref Letter No", "Ref Letter Date", "Remarks"
            };

            foreach (var dpGroup in groupData)
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
                        .SetFontSize(7)).SetTextAlignment(TextAlignment.CENTER)
                        .SetBorder(new iText.Layout.Borders.SolidBorder(ColorConstants.LIGHT_GRAY, 0.2f));
                    table.AddHeaderCell(headerCell);
                }

                table.SetSkipFirstHeader(false);
                table.SetSkipLastFooter(false);

                int sn = 1;
                var uniqueEmp = new HashSet<string>();
                var orderedItems = dpGroup.OrderBy(x => x.Code).ToList();

                foreach (var item in orderedItems)
                {
                    var values = new[]
                    {
                        sn++.ToString(),
                        item.Code ?? "",
                        item.Name ?? "",
                        item.JoiningDate ?? "",
                        item.EffectiveDate ?? "",
                        item.DuePaymentDate ?? "",
                        item.RefLetterNo ?? "",
                        item.RefLetterDate ?? "",
                        item.Remarks ?? ""
                    };

                    for (int i = 0; i < values.Length; i++)
                    {
                        TextAlignment alignment = i switch
                        {
                            2 or 8 => TextAlignment.LEFT,
                            _ => TextAlignment.CENTER
                        };

                        var cell = new Cell().Add(new Paragraph(values[i]))
                            .SetFontSize(8)
                            .SetFont(PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN))
                            .SetTextAlignment(alignment)
                            .SetBorder(new iText.Layout.Borders.SolidBorder(ColorConstants.LIGHT_GRAY, 0.2f));

                        table.AddCell(cell);
                    }

                    sn++;
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
                string reportText = "Employee Service Not Confirmation Report";
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

        public async Task<byte[]> GenerateExcelReport(List<ReportFilterResultViewModel> data)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();

            var worksheet = package.Workbook.Worksheets.Add("Service Not Confirmation Report");

            worksheet.Cells["A1:K1"].Merge = true;
            worksheet.Cells["A1"].Value = data.Select(c => c.CompanyName);
            worksheet.Cells["A1"].Style.Font.Size = 16;
            worksheet.Cells["A1"].Style.Font.Bold = true;
            worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            worksheet.Cells["A2:K2"].Merge = true;
            worksheet.Cells["A2"].Value = "Service Not Confirmation Report";
            worksheet.Cells["A2"].Style.Font.Size = 16;
            worksheet.Cells["A2"].Style.Font.Bold = true;
            worksheet.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            int currentRow = 4;
            var groupData = data.GroupBy(x=>x.DepartmentName??"Unknown Department").OrderBy(g => g.Key);

            foreach (var departmentGroup in groupData)
            {
                worksheet.Cells[currentRow, 1, currentRow, 8].Merge = true;
                worksheet.Cells[currentRow, 1].Value = $"Department: {departmentGroup.Key}";
                worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
                worksheet.Cells[currentRow, 1].Style.Font.Size = 14;
                currentRow++;

                string[] headers = new string[]
                {
                    "Employee ID", "Employee Name", "Joining Date", "Effective Date", "Due Payment Date",
                    "Ref Letter No", "Ref Letter Date", "Remarks"
                };

                int sn = 1;
                var uniqueEmp = new HashSet<string>();

                foreach (var item in departmentGroup.OrderBy(x => x.Code))
                {
                    if (!string.IsNullOrEmpty(item.Code))
                        uniqueEmp.Add(item.Code);

                    worksheet.Cells[currentRow, 1].Value = sn++;
                    worksheet.Cells[currentRow, 2].Value = item.Code;
                    worksheet.Cells[currentRow, 3].Value = item.Name;
                    worksheet.Cells[currentRow, 4].Value = item.JoiningDate;
                    worksheet.Cells[currentRow, 5].Value = item.EffectiveDate;
                    worksheet.Cells[currentRow, 6].Value = item.DuePaymentDate;
                    worksheet.Cells[currentRow, 7].Value = item.RefLetterNo;
                    worksheet.Cells[currentRow, 8].Value = item.RefLetterDate;
                    worksheet.Cells[currentRow, 9].Value = item.Remarks;

                    for (int i = 1; i <= headers.Length; i++)
                    {
                        var cell = worksheet.Cells[currentRow, i];
                        cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                        cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        ExcelHorizontalAlignment alignment = i switch
                        {
                            3 or 10 => ExcelHorizontalAlignment.Left,
                            _ => ExcelHorizontalAlignment.Center
                        };
                        cell.Style.HorizontalAlignment = alignment;
                    }

                    currentRow++;
                    sn++;
                }
                currentRow+=2;
            }

            return await package.GetAsByteArrayAsync();
        }
    }
}
