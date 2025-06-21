using DocumentFormat.OpenXml.InkML;
using GCTL.Core.Data;
using GCTL.Core.ViewModels.HRM_EmployeeWeekendDeclaration;
using GCTL.Data.Models;
using GCTL.Service.EmployeeWeekendDeclaration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.EmployeeWeekendDeclarationReport
{
    public class EmployeeWeekendDeclarationReportServices : AppService<HrmEmployeeWeekendDeclaration>, IEmployeeWeekendDeclarationReportServices
    {
        private readonly IRepository<HrmEmployee> employeeRepo;
        private readonly IRepository<HrmEmployeeOfficialInfo> empOffRepo;
        private readonly IRepository<HrmDefDesignation> desiRepo;
        private readonly IRepository<HrmDefDepartment> depRepo;
        private readonly IRepository<HrmDefDivision> divRepo;
        private readonly IRepository<CoreBranch> branchRepo;
        private readonly IRepository<CoreCompany> companyRepo;
        private readonly IRepository<HrmDefEmployeeStatus> empStRepo;
        private readonly IRepository<HrmAtdShift> shiftRepo;
        private readonly IRepository<HrmRosterScheduleEntry> rosterEntryRepo;
        private readonly IRepository<HrmEmployeeWeekendDeclaration> empWeekDecRepo;

        public EmployeeWeekendDeclarationReportServices(
            IRepository<HrmEmployeeWeekendDeclaration> empWeekendDecRepo,
             IRepository<HrmEmployee> employeeRepo,
          IRepository<HrmEmployeeOfficialInfo> empOffRepo,
          IRepository<HrmDefDesignation> desiRepo,
          IRepository<HrmDefDepartment> depRepo,
          IRepository<HrmDefDivision> divRepo,
          IRepository<CoreBranch> branchRepo,
          IRepository<CoreCompany> companyRepo,
            IRepository<HrmDefEmployeeStatus> empStRepo,
            IRepository<HrmAtdShift> shiftRepo,
            IRepository<HrmRosterScheduleEntry> rosterEntryRepo,
            IRepository<HrmEmployeeWeekendDeclaration> empWeekDecRepo
            ) : base(empWeekendDecRepo)
        {
            this.employeeRepo = employeeRepo;
            this.empOffRepo = empOffRepo;
            this.desiRepo = desiRepo;
            this.depRepo = depRepo;
            this.divRepo = divRepo;
            this.branchRepo = branchRepo;
            this.companyRepo = companyRepo;
            this.empStRepo = empStRepo;
            this.shiftRepo = shiftRepo;
            this.rosterEntryRepo = rosterEntryRepo;
            this.empWeekDecRepo = empWeekDecRepo;
        }

       public async Task<EmployeeFilterResultDto> GetRosterDataAsync(EmployeeFilterDto FilterData)
        {
            var queary = from empWek in empWeekDecRepo.All()
                         join eoi in empOffRepo.All() on empWek.EmployeeId equals eoi.EmployeeId
                         join e in employeeRepo.All() on empWek.EmployeeId equals e.EmployeeId into empJoin
                         from e in empJoin.DefaultIfEmpty()
                         join dg in desiRepo.All() on eoi.DesignationCode equals dg.DesignationCode into dgJoin
                         from dg in dgJoin.DefaultIfEmpty()
                         join cb in branchRepo.All() on eoi.BranchCode equals cb.BranchCode into cbJoin
                         from cb in cbJoin.DefaultIfEmpty()
                         join dp in depRepo.All() on eoi.DepartmentCode equals dp.DepartmentCode into dpJoin
                         from dp in dpJoin.DefaultIfEmpty()
                         join cp in companyRepo.All() on eoi.CompanyCode equals cp.CompanyCode into cpJoin
                         from cp in cpJoin.DefaultIfEmpty()
                         where (FilterData.FromDate == null || empWek.Date >= FilterData.FromDate) &&
                         (FilterData.ToDate == null || empWek.Date <= FilterData.ToDate)
                         select new
                         {
                             empId = e.EmployeeId,
                             empName = (e.FirstName ?? " ") + " " + (e.LastName ?? " "),
                             companyName = cp.CompanyName ?? "",
                             companyCode = cp.CompanyCode ?? "",
                             branchName = cb.BranchName ?? "",
                             branchCode = cb.BranchCode ?? "",
                             desiganationName = dg.DesignationName ?? "",
                             desiganationCode = dg.DesignationCode ?? "",
                             departmentName = dp.DepartmentName ?? "",
                             departmentCode = dp.DepartmentCode ?? "",
                             date = empWek.Date,
                             remark = empWek.Remark ?? "",
                             dayName = empWek.Date.ToString("dddd") ?? ""


                         };
            if (FilterData.CompanyCodes?.Any() == true)
            {
                queary = queary.Where(x => x.companyCode != null && FilterData.CompanyCodes.Contains(x.companyCode));
            }
            if (FilterData.BranchCodes?.Any() == true)
            {
                queary = queary.Where(x => x.branchCode != null && FilterData.BranchCodes.Contains(x.branchCode));
            }
            if (FilterData.DepartmentCodes?.Any() == true)
            {
                queary = queary.Where(x => x.departmentCode != null && FilterData.DepartmentCodes.Contains(x.departmentCode));
            }
            if (FilterData.DesignationCodes?.Any() == true)
            {
                queary = queary.Where(x => x.desiganationCode != null && FilterData.DesignationCodes.Contains(x.desiganationCode));
            }
            if (FilterData.EmployeeIDs?.Any() == true)
            {
                queary = queary.Where(x => x.empId != null && FilterData.EmployeeIDs.Contains(x.empId));
            }
            var earliestDate = await queary.OrderBy(x => x.date).Select(x => x.date).FirstOrDefaultAsync();
            var lastDate = await queary.OrderByDescending(x => x.date).Select(x => x.date).FirstOrDefaultAsync();
         int skip =(FilterData.PageNumber -1)* FilterData.PageSize;
            var paginatedData = queary.OrderByDescending(x=>x.date).Skip(skip).Take(FilterData.PageSize);
            int totalCount = await queary.CountAsync();

            var result = new EmployeeFilterResultDto
            {
                Companies = await queary.Where(x => x.companyCode != null && x.companyName != null).Select(x => new CodeNameDto { Code = x.companyCode, Name = x.companyName }).Distinct().ToListAsyncSafe(),
                Branches = await queary.Where(x => x.branchCode != null && x.branchName != null).Select(x => new CodeNameDto { Code = x.branchCode, Name = x.branchName }).Distinct().ToListAsyncSafe(),
                Departments = await queary.Where(x => x.departmentCode != null && x.departmentName != null).Select(x => new CodeNameDto { Code = x.departmentCode, Name = x.departmentName }).Distinct().ToListAsyncSafe(),
                Designations = await queary.Where(x => x.desiganationCode != null && x.desiganationName != null).Select(x => new CodeNameDto { Code = x.desiganationCode, Name = x.desiganationName }).Distinct().ToListAsyncSafe(),

                Employees = await queary.Where(x => x.empId != null && x.empName != null).Select(x => new CodeNameDto
                {
                    Code = x.empId,
                    Name = x.empName,
                    DepartmentName = x.departmentName,
                    DesignationName = x.desiganationName,
                    BranchName = x.branchName,
                    CompanyName = x.companyName,
                    DayName = x.dayName,
                    ShowDate = x.date.ToString("dd/MM/yyyy"),
                    Remarks = x.remark,
                    FromDate = FilterData.FromDate.HasValue ? FilterData.FromDate.Value.ToString("dd/MM/yyyy") : earliestDate.ToString("dd/MM/yyyy"),
                    ToDate = FilterData.ToDate.HasValue ? FilterData.ToDate.Value.ToString("dd/MM/yyyy") : lastDate.ToString("dd/MM/yyyy")
                }).Distinct().ToListAsyncSafe(),

            };
            return result;

        }
        public async Task<EmployeeFilterResultDto> GetRosterDataPdfAsync(EmployeeFilterDto FilterData)
        {
            var queary = from empWek in empWeekDecRepo.All()
                         join eoi in empOffRepo.All() on empWek.EmployeeId equals eoi.EmployeeId
                         join e in employeeRepo.All() on empWek.EmployeeId equals e.EmployeeId into empJoin
                         from e in empJoin.DefaultIfEmpty()
                         join dg in desiRepo.All() on eoi.DesignationCode equals dg.DesignationCode into dgJoin
                         from dg in dgJoin.DefaultIfEmpty()
                         join cb in branchRepo.All() on eoi.BranchCode equals cb.BranchCode into cbJoin
                         from cb in cbJoin.DefaultIfEmpty()
                         join dp in depRepo.All() on eoi.DepartmentCode equals dp.DepartmentCode into dpJoin
                         from dp in dpJoin.DefaultIfEmpty()
                         join cp in companyRepo.All() on eoi.CompanyCode equals cp.CompanyCode into cpJoin
                         from cp in cpJoin.DefaultIfEmpty()
                         where (FilterData.FromDate == null || empWek.Date >= FilterData.FromDate) &&
                         (FilterData.ToDate == null || empWek.Date <= FilterData.ToDate)
                         select new
                         {
                             empId = e.EmployeeId,
                             empName = (e.FirstName ?? " ") + " " + (e.LastName ?? " "),
                             companyName = cp.CompanyName ?? "",
                             companyCode = cp.CompanyCode ?? "",
                             branchName = cb.BranchName ?? "",
                             branchCode = cb.BranchCode ?? "",
                             desiganationName = dg.DesignationName ?? "",
                             desiganationCode = dg.DesignationCode ?? "",
                             departmentName = dp.DepartmentName ?? "",
                             departmentCode = dp.DepartmentCode ?? "",
                             date = empWek.Date,
                             remark = empWek.Remark ?? "",
                             dayName = empWek.Date.ToString("dddd") ?? ""


                         };
            if (FilterData.CompanyCodes?.Any() == true)
            {
                queary = queary.Where(x => x.companyCode != null && FilterData.CompanyCodes.Contains(x.companyCode));
            }
            if (FilterData.BranchCodes?.Any() == true)
            {
                queary = queary.Where(x => x.branchCode != null && FilterData.BranchCodes.Contains(x.branchCode));
            }
            if (FilterData.DepartmentCodes?.Any() == true)
            {
                queary = queary.Where(x => x.departmentCode != null && FilterData.DepartmentCodes.Contains(x.departmentCode));
            }
            if (FilterData.DesignationCodes?.Any() == true)
            {
                queary = queary.Where(x => x.desiganationCode != null && FilterData.DesignationCodes.Contains(x.desiganationCode));
            }
            if (FilterData.EmployeeIDs?.Any() == true)
            {
                queary = queary.Where(x => x.empId != null && FilterData.EmployeeIDs.Contains(x.empId));
            }
            var earliestDate = await queary.OrderBy(x => x.date).Select(x => x.date).FirstOrDefaultAsync();
            var lastDate = await queary.OrderByDescending(x => x.date).Select(x => x.date).FirstOrDefaultAsync();
            int skip = (FilterData.PageNumber - 1) * FilterData.PageSize;
            var paginatedData = queary.OrderByDescending(x => x.date).Skip(skip).Take(FilterData.PageSize);
            int totalCount = await queary.CountAsync();

            var result = new EmployeeFilterResultDto
            {
                Companies = await queary.Where(x => x.companyCode != null && x.companyName != null).Select(x => new CodeNameDto { Code = x.companyCode, Name = x.companyName }).Distinct().ToListAsyncSafe(),
                Branches = await queary.Where(x => x.branchCode != null && x.branchName != null).Select(x => new CodeNameDto { Code = x.branchCode, Name = x.branchName }).Distinct().ToListAsyncSafe(),
                Departments = await queary.Where(x => x.departmentCode != null && x.departmentName != null).Select(x => new CodeNameDto { Code = x.departmentCode, Name = x.departmentName }).Distinct().ToListAsyncSafe(),
                Designations = await queary.Where(x => x.desiganationCode != null && x.desiganationName != null).Select(x => new CodeNameDto { Code = x.desiganationCode, Name = x.desiganationName }).Distinct().ToListAsyncSafe(),

                Employees = await queary.Where(x => x.empId != null && x.empName != null).Select(x => new CodeNameDto
                {
                    Code = x.empId,
                    Name = x.empName,
                    DepartmentName = x.departmentName,
                    DesignationName = x.desiganationName,
                    BranchName = x.branchName,
                    CompanyName = x.companyName,
                    DayName = x.dayName,
                    ShowDate = x.date.ToString("dd/MM/yyyy"),
                    Remarks = x.remark,
                    FromDate = FilterData.FromDate.HasValue ? FilterData.FromDate.Value.ToString("dd/MM/yyyy") : earliestDate.ToString("dd/MM/yyyy"),
                    ToDate = FilterData.ToDate.HasValue ? FilterData.ToDate.Value.ToString("dd/MM/yyyy") : lastDate.ToString("dd/MM/yyyy"),
                    Luser= FilterData.Luser?? ""
                }).Distinct().ToListAsyncSafe(),

            };
            return result;

        }
    }
}
