using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Data;
using GCTL.Core.ViewModels.RosterScheduleApproval;
using GCTL.Core.ViewModels.RosterScheduleEntry;
using GCTL.Core.ViewModels.RosterScheduleReport;
using GCTL.Data.Models;
using GCTL.Service.EmployeeWeekendDeclaration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml.Style;
using OfficeOpenXml;

namespace GCTL.Service.RosterScheduleReport
{
    public class RosterScheduleReportServices : AppService<HrmRosterScheduleEntry>, IRosterScheduleReportServices
    {
        private readonly IRepository<HrmRosterScheduleEntry> rosterApprovalRepo;
        private readonly IRepository<GCTL_ERP_DB_DatapathContext> _context;
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
        private readonly string _connectionString;

        public RosterScheduleReportServices(
            IRepository<HrmRosterScheduleEntry> RosterApprovalRepo,
            IRepository<GCTL_ERP_DB_DatapathContext> context,
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
              IConfiguration configuration
            ) : base(RosterApprovalRepo)
        {
            this.rosterApprovalRepo = RosterApprovalRepo;
            _context = context;
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
            _connectionString = configuration.GetConnectionString("ApplicationDbConnection");
        }

       
        public async Task<RosterReportFilterListDto> GetRosterDataAsync(RosterReportFilterDto filter)
        {
            var query = from rse in rosterEntryRepo.All()
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
                        join st in shiftRepo.All() on rse.ShiftCode equals st.ShiftCode into stJoin
                        from st in stJoin.DefaultIfEmpty()
                        join status in empStRepo.All() on eoi.EmployeeStatus equals status.EmployeeStatusId into statusJoin
                        from status in statusJoin.DefaultIfEmpty()
                        join rSAP in rosterApprovalRepo.All() on rse.RosterScheduleId equals rSAP.RosterScheduleId into rSAPJoin
                        from rSAP in rSAPJoin.DefaultIfEmpty()
                        where
                            (filter.FromDate == null || rse.Date >= filter.FromDate) &&
                            (filter.ToDate == null || rse.Date <= filter.ToDate) &&
                            (status.EmployeeStatusId == "01")
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
                            Date = rse.Date,
                            rosterId = rse.RosterScheduleId ?? "",
                            shiftCode = st.ShiftCode ?? "",
                            shiftName = (st.ShiftName ?? "") + " ( " + st.ShiftStartTime.ToString("hh:mm:ss tt") + " - " + st.ShiftEndTime.ToString("hh:mm:ss tt") + " )",
                            remark = rse.Remark ?? "",
                            ApprovalStatus = rse.ApprovalStatus ?? "",
                            ApprovedBy = rse.ApprovedBy ?? "",
                            ApprovalDatetime = rse.ApprovalDatetime
                        };

            // Filters
            if (filter.CompanyCodes?.Any() == true)
                query = query.Where(x => x.CompanyCode != null && filter.CompanyCodes.Contains(x.CompanyCode));

            if (filter.BranchCodes?.Any() == true)
                query = query.Where(x => x.BranchCode != null && filter.BranchCodes.Contains(x.BranchCode));

            if (filter.DepartmentCodes?.Any() == true)
                query = query.Where(x => x.DepartmentCode != null && filter.DepartmentCodes.Contains(x.DepartmentCode));

            if (filter.DesignationCodes?.Any() == true)
                query = query.Where(x => x.DesignationCode != null && filter.DesignationCodes.Contains(x.DesignationCode));

            if (filter.EmployeeStatuses?.Any() == true)
                query = query.Where(x => x.EmployeeStatusCode != null && filter.EmployeeStatuses.Contains(x.EmployeeStatusCode));

            if (filter.EmployeeIDs?.Any() == true)
                query = query.Where(x => x.EmpId != null && filter.EmployeeIDs.Contains(x.EmpId));

            // Pagination
            int skip = (filter.PageNumber - 1) * filter.PageSize;
            var paginatedData = query
                .OrderByDescending(x => x.Date)
                .Skip(skip)
                .Take(filter.PageSize);

            int totalCount = await query.CountAsync();

            var result = new RosterReportFilterListDto
            {
              
                Companies = await query.Where(x => x.CompanyCode != null && x.CompanyName != null)
                .Select(x => new RosterReportFilterResultDto { Code = x.CompanyCode, Name = x.CompanyName })
                .Distinct().ToListAsyncSafe(),

                Branches = await query.Where(x => x.BranchCode != null && x.BranchName != null)
                    .Select(x => new RosterReportFilterResultDto { Code = x.BranchCode, Name = x.BranchName })
                    .Distinct()
                    .ToListAsyncSafe(),

                Departments = await query.Where(x => x.DepartmentCode != null && x.DepartmentName != null)
                    .Select(x => new RosterReportFilterResultDto { Code = x.DepartmentCode, Name = x.DepartmentName })
                    .Distinct()
                    .ToListAsyncSafe(),

                Designations = await query.Where(x => x.DesignationCode != null && x.DesignationName != null)
                    .Select(x => new RosterReportFilterResultDto { Code = x.DesignationCode, Name = x.DesignationName })
                    .Distinct()
                    .ToListAsyncSafe(),

                Employees = await query.Where(x => x.EmpId != null && x.EmpName != null)
                .Select(x => new RosterReportFilterResultDto
                {
                    Code = x.EmpId,
                    Name = x.EmpName,
                    DesignationName = x.DesignationName ?? "",
                    DepartmentName = x.DepartmentName ?? "",
                    BranchName = x.BranchName ?? "",
                    CompanyName = x.CompanyName ?? "",
                    RosterScheduleId = x.rosterId ?? "",
                    Date = x.Date,
                    DayName = x.Date.ToString("dddd"),
                    ShiftName = x.shiftName ?? "",
                    Remark = x.remark ?? "",
                    ShowDate = x.Date.ToString("dd/MM/yyyy"),
                    ApprovalStatus = x.ApprovalStatus,
                    ApprovedBy = x.ApprovedBy,
                    ShowApprovalDatetime = x.ApprovalDatetime.HasValue ? x.ApprovalDatetime.Value.ToString("dd/MM/yyyy") : ""
                }).Distinct().ToListAsyncSafe(),

                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };

            return result;
        }

        public async Task<RosterReportFilterListDto> GetRosterDataPdfAsync(RosterReportFilterDto filter)
        {
            var query = from rse in rosterEntryRepo.All()
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
                        join st in shiftRepo.All() on rse.ShiftCode equals st.ShiftCode into stJoin
                        from st in stJoin.DefaultIfEmpty()
                        join status in empStRepo.All() on eoi.EmployeeStatus equals status.EmployeeStatusId into statusJoin
                        from status in statusJoin.DefaultIfEmpty()
                        join rSAP in rosterApprovalRepo.All() on rse.RosterScheduleId equals rSAP.RosterScheduleId into rSAPJoin
                        from rSAP in rSAPJoin.DefaultIfEmpty()
                        where
                            (filter.FromDate == null || rse.Date >= filter.FromDate) &&
                                (filter.ToDate == null || rse.Date <= filter.ToDate) 
                        select new
                        {
                            EmpId = e.EmployeeId,
                            EmpName = e.FirstName ?? "" + " " + e.LastName ?? "",
                            CompanyCode = eoi.CompanyCode ?? "",
                            BranchCode = cb.BranchCode ?? "",
                            DesignationCode = dg.DesignationCode ?? "",
                            DesignationName = dg.DesignationName ?? "",
                            DepartmentCode = dp.DepartmentCode ?? "",
                            DepartmentName = dp.DepartmentName ?? "",
                            BranchName = cb.BranchName ?? "",
                            CompanyName = cp.CompanyName ?? "",
                            EmployeeStatusCode = eoi.EmployeeStatus ?? "",
                            Date = rse.Date,
                            rosterId = rse.RosterScheduleId ?? "",
                            shiftCode = st.ShiftCode ?? "",
                            shiftName = st.ShiftName + "( " + st.ShiftStartTime.ToString("hh:mm:ss tt") + " - " + st.ShiftEndTime.ToString("hh:mm:ss tt") + " )" ?? "",
                            remark = rse.Remark ?? "",
                            ApprovalStatus = rse.ApprovalStatus ?? "",
                            ApprovedBy = rse.ApprovedBy ?? "",
                            ApprovalDatetime = rse.ApprovalDatetime,
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
                query = query.Where(x => x.DesignationCode != null & filter.DesignationCodes.Contains(x.DesignationCode));
            }
            if (filter.EmployeeStatuses?.Any() == true)
            {
                query = query.Where(x => x.EmployeeStatusCode != null && filter.EmployeeStatuses.Contains(x.EmployeeStatusCode));
            }
            if (filter.EmployeeIDs?.Any() == true)
            {
                query = query.Where(x => x.EmpId != null && filter.EmployeeIDs.Contains(x.EmpId));
            }
            var earliestDate = await query.OrderBy(x => x.Date).Select(x => x.Date).FirstOrDefaultAsync();
            var latestDate = await query.OrderByDescending(x => x.Date).Select(x => x.Date).FirstOrDefaultAsync();
            var result = new RosterReportFilterListDto
            {
                Companies = await query.Where(x => x.CompanyCode != null && x.CompanyName != null)
                .Select(x => new RosterReportFilterResultDto { Code = x.CompanyCode, Name = x.CompanyName })
                .Distinct().ToListAsyncSafe(),

                Branches = await query.Where(x => x.BranchCode != null && x.BranchName != null)
                .Select(x => new RosterReportFilterResultDto { Code = x.BranchCode, Name = x.BranchName })
                .Distinct().ToListAsyncSafe(),

                Departments = await query.Where(x => x.DepartmentCode != null && x.DepartmentName != null)
                .Select(x => new RosterReportFilterResultDto { Code = x.DepartmentCode, Name = x.DepartmentName })
                .Distinct().ToListAsyncSafe(),
                Designations = await query.Where(x => x.DesignationCode != null && x.DesignationName != null)
                .Select(x => new RosterReportFilterResultDto { Code = x.DesignationCode, Name = x.DesignationName })
                .Distinct().ToListAsyncSafe(),
                Employees = await query.Where(x => x.EmpId != null && x.EmpName != null)
                .Select(x => new RosterReportFilterResultDto
                {
                    Code = x.EmpId,
                    Name = x.EmpName,
                    DesignationName = x.DesignationName ?? "",
                    DepartmentName = x.DepartmentName ?? "",
                    BranchName = x.BranchName ?? "",
                    CompanyName = x.CompanyName ?? "",
                    RosterScheduleId = x.rosterId ?? "",
                    Date = x.Date,
                    DayName = x.Date.ToString("dddd"),
                    ShiftName = x.shiftName ?? "",
                    Remark = x.remark ?? "",
                    ShowDate = x.Date.ToString("dd/MM/yyyy"),
                    ApprovalStatus = x.ApprovalStatus,
                    ApprovedBy = x.ApprovedBy,
                    ShowApprovalDatetime = x.ApprovalDatetime.HasValue ? x.ApprovalDatetime.Value.ToString("dd/MM/yyyy") : "",
                    Luser = filter.Luser,
                    FromDate = filter.FromDate.HasValue? filter.FromDate.Value.ToString("dd/MM/yyyy"): earliestDate.ToString("dd/MM/yyyy"),
                    ToDate = filter.ToDate.HasValue? filter.ToDate.Value.ToString("dd/MM/yyyy"): latestDate.ToString("dd/MM/yyyy"),

                }).Distinct().ToListAsyncSafe(),
            };

            return result;
        }
    }
}
