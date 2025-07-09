using GCTL.Core.Data;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.HrmServiceNotConfirmEntries;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.HrmServiceNotConfirmationReports
{
    public class HrmServiceNotConfirmationReportService : AppService<HrmServiceNotConfirmationEntry>, IHrmServiceNotConfirmationReportService
    {
        private readonly IRepository<HrmEmployee> employeeRepo;
        private readonly IRepository<HrmEmployeeOfficialInfo> empOffRepo;
        public readonly IRepository<HrmServiceNotConfirmationEntry> sncRepo;
        public readonly IRepository<CoreBranch> branchRepo;
        public readonly IRepository<CoreCompany> companyRepo;
        public HrmServiceNotConfirmationReportService(
            IRepository<HrmServiceNotConfirmationEntry> sncRepo,
            IRepository<HrmEmployee> employeeRepo,
            IRepository<HrmEmployeeOfficialInfo> empOffRepo,
            IRepository<CoreBranch> branchRepo,
            IRepository<CoreCompany> companyRepo)
            : base(sncRepo)
        {
            this.sncRepo = sncRepo;
            this.employeeRepo = employeeRepo;
            this.empOffRepo = empOffRepo;
            this.branchRepo = branchRepo;
            this.companyRepo = companyRepo;
        }

        public async Task<ReportFilterListViewModel> GetDataAsync(ReportFilterViewModel filter)
        {
            var query = from e in empOffRepo.All().AsNoTracking()
                        join emp in employeeRepo.All().AsNoTracking() on e.EmployeeId equals emp.EmployeeId
                        join snc in sncRepo.All().AsNoTracking() on e.EmployeeId equals snc.EmployeeId
                        join b in branchRepo.All().AsNoTracking() on e.BranchCode equals b.BranchCode into branchGroup
                        from b in branchGroup.DefaultIfEmpty()
                        join c in companyRepo.All().AsNoTracking() on e.CompanyCode equals c.CompanyCode into companyGroup
                        from c in companyGroup.DefaultIfEmpty()

                        select new
                        {
                            e,
                            emp,
                            snc,
                            b,
                            c
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
                    Name = x.e.EmployeeId
                }).Distinct().ToList(),
                ServiceNotConfirms = await query.Select(x => new ReportFilterResultViewModel
                {
                    Code = x.e.EmployeeId,
                    Name = (x.emp.FirstName ?? "") + " " + (x.emp.LastName ?? ""),
                    CompanyName = x.c.CompanyName,
                    EmployeeId = x.e.EmployeeId,
                    SncId = x.snc.Sncid,
                    JoiningDate = x.e.JoiningDate.HasValue ? x.e.JoiningDate.Value.ToString("yyyy-MM-dd") : null,
                    EffectiveDate = x.snc.EffectiveDate.HasValue ? x.snc.EffectiveDate.Value.ToString("yyyy-MM-dd") : null,
                    DuePaymentDate = x.snc.DuePaymentDate.HasValue ? x.snc.DuePaymentDate.Value.ToString("yyyy-MM-dd") : null,
                    RefLetterDate = x.snc.RefLetterDate.HasValue ? x.snc.RefLetterDate.Value.ToString("yyyy-MM-dd") : null,,
                    RefLetterNo = x.snc.RefLetterNo??"",
                    Remarks = x.snc.Remarks ?? ""
                }).ToListAsync()
            };
            return result;
        }

        public Task<byte[]> GeneratePdfReport(List<ReportFilterResultViewModel> data, BaseViewModel model)
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> GenerateExcelReport(List<ReportFilterResultViewModel> data)
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
                    if(!string.IsNullOrWhiteSpace(in=))
                }
            }
            throw new NotImplementedException();
        }
    }
}
