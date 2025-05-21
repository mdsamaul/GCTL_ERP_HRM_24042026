using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Data;
using GCTL.Core.ViewModels.PFAssignEntry;
using GCTL.Core.ViewModels.RosterScheduleApproval;
using GCTL.Core.ViewModels.RosterScheduleEntry;
using GCTL.Data.Models;
using GCTL.Service.EmployeeWeekendDeclaration;
using GCTL.UI.Core.Views.RosterScheduleApproval;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace GCTL.Service.RosterScheduleApproval
{
    public class RosterScheduleApprovalService : AppService<HrmRosterScheduleEntry>, IRosterScheduleApprovalService
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

        public RosterScheduleApprovalService(
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
        public async Task<RosterFilterListDto> GetRosterDataAsync(RosterFilterDto filter)
        {
            var query = from rse in rosterEntryRepo.All()
                        join eoi in empOffRepo.All() on rse.EmployeeId equals eoi.EmployeeId
                        join e in employeeRepo.All() on rse.EmployeeId equals e.EmployeeId into empJoin
                        from e in empJoin.DefaultIfEmpty()
                        join dg in desiRepo.All() on eoi.DesignationCode equals dg.DesignationCode into dgJoin
                        from dg in dgJoin.DefaultIfEmpty()
                        join cb in branchRepo.All() on eoi.BranchCode equals cb.BranchCode into cbJoin
                        from cb in cbJoin.DefaultIfEmpty()
                        join dv in divRepo.All() on eoi.DivisionCode equals dv.DivisionCode into dvJoin
                        from dv in dvJoin.DefaultIfEmpty()
                        join dp in depRepo.All() on eoi.DepartmentCode equals dp.DepartmentCode into dpJoin
                        from dp in dpJoin.DefaultIfEmpty()
                        join empSt in empStRepo.All() on eoi.EmployeeStatus equals empSt.EmployeeStatusId into empStJoin
                        from empSt in empStJoin.DefaultIfEmpty()
                        join cp in companyRepo.All() on eoi.CompanyCode equals cp.CompanyCode into cpJoin
                        from cp in cpJoin.DefaultIfEmpty()
                        join st in shiftRepo.All() on rse.ShiftCode equals st.ShiftCode into stJoin
                        from st in stJoin.DefaultIfEmpty()
                        join rSAP in rosterApprovalRepo.All() on rse.RosterScheduleId equals rSAP.RosterScheduleId into rSAPJoin
                        from rSAP in rSAPJoin.DefaultIfEmpty()
                        where
                            (filter.FromDate == null || rse.Date >= filter.FromDate) &&
                                (filter.ToDate == null || rse.Date <= filter.ToDate) &&
                                (rse.ApprovalStatus != "Approved")
                        select new {
                            EmpId = e.EmployeeId,
                            EmpName = e.FirstName ?? "" + " " + e.LastName ?? "",
                            CompanyCode = eoi.CompanyCode ?? "",
                            BranchCode = cb.BranchCode ?? "",
                            DivisionCode = dv.DivisionCode ?? "",
                            DivisionName = dv.DivisionName ?? "",
                            DesignationCode = dg.DesignationCode ?? "",
                            DesignationName = dg.DesignationName ?? "",
                            DepartmentCode = dp.DepartmentCode ?? "",
                            DepartmentName = dp.DepartmentName ?? "",
                            BranchName = cb.BranchName ?? "",
                            CompanyName = cp.CompanyName ?? "",
                            EmployeeStatusCode = eoi.EmployeeStatus ?? "",
                            Date = rse.Date,
                            rosterId = rse.RosterScheduleId ?? "",
                            shiftCode = st.ShiftCode  ?? "",
                            shiftName = st.ShiftName + "( " + st.ShiftStartTime.ToString("hh:mm:tt") + " - " + st.ShiftEndTime.ToString("hh:mm:tt") + " )" ?? "",
                            remark = rse.Remark ?? "",
                            empStatus = empSt.EmployeeStatus ?? "",
                        };
            if (filter.CompanyCodes?.Any() == true) {
                query = query.Where(x => x.CompanyCode != null && filter.CompanyCodes.Contains(x.CompanyCode));
            }
            if (filter.BranchCodes?.Any() == true)
            {
                query = query.Where(x => x.BranchCode != null && filter.BranchCodes.Contains(x.BranchCode));
            }
            if (filter.DivisionCodes?.Any() == true)
            {
                query = query.Where(x => x.DivisionCode != null && filter.DivisionCodes.Contains(x.DivisionCode));
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
            var result = new RosterFilterListDto
            {
                Companies = await query.Where(x => x.CompanyCode != null && x.CompanyName != null)
                .Select(x => new RosterFilterResultDto { Code = x.CompanyCode, Name = x.CompanyName })
                .Distinct().ToListAsyncSafe(),

                Branches = await query.Where(x => x.BranchCode != null && x.BranchName != null)
                .Select(x => new RosterFilterResultDto { Code = x.BranchCode, Name = x.BranchName })
                .Distinct().ToListAsyncSafe(),

                Divisions = await query.Where(x => x.DivisionCode != null && x.DivisionName != null)
                .Select(x => new RosterFilterResultDto { Code = x.DivisionCode, Name = x.DivisionName })
                .Distinct().ToListAsyncSafe(),
                Departments = await query.Where(x => x.DepartmentCode != null && x.DepartmentName != null)
                .Select(x => new RosterFilterResultDto { Code = x.DepartmentCode, Name = x.DepartmentName })
                .Distinct().ToListAsyncSafe(),
                Designations = await query.Where(x => x.DesignationCode != null && x.DesignationName != null)
                .Select(x => new RosterFilterResultDto { Code = x.DesignationCode, Name = x.DesignationName })
                .Distinct().ToListAsyncSafe(),
                Employees = await query.Where(x => x.EmpId != null && x.EmpName != null)
                .Select(x => new RosterFilterResultDto
                {
                    Code = x.EmpId,
                    Name = x.EmpName,
                    DesignationName = x.DesignationName ?? "",
                    DepartmentName = x.DepartmentName ?? "",
                    BranchName = x.BranchName ?? "",
                    DivisionName = x.DivisionName ?? "",
                    CompanyName = x.CompanyName ?? "",
                    RosterScheduleId = x.rosterId ?? "",
                    Date = x.Date,
                    DayName = x.Date.ToString("dddd"),
                    ShiftName = x.shiftName ?? "",
                    Remark = x.remark ?? "",
                    ShowDate = x.Date.ToString("dd/MM/yyyy")
                }).Distinct().ToListAsyncSafe(),

                ActivityStatuses = await query.Where(x => x.empStatus != null)
                .Select(x => new RosterFilterResultDto { Code = x.EmployeeStatusCode, Name = x.empStatus })
                .Distinct().ToListAsyncSafe(),
            };

            return result;
        }
      


        public async Task<(bool isSuccess, string isMessage)> ApprovalRosterServices(ApprovalRequest modelData)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    await conn.OpenAsync();

                    foreach (var rosterId in modelData.CheckedApprovalList)
                    {
                       
                        var rosterData = rosterEntryRepo.GetAll().Where(x => x.RosterScheduleId == rosterId).FirstOrDefault();
                        rosterData.ApprovalStatus = "Approved";
                            rosterData.ApprovalDatetime= DateTime.Now;
                        rosterData.Remark = modelData.Remark??"";
                        rosterData.ApprovedBy = modelData.Luser;
                        rosterEntryRepo.Update(rosterData);

                    }
                    Console.WriteLine("Loop completed, returning success.");
                    return (true, "Update successful.");
                }
            }
            catch (Exception ex)
            {
                return (false, "Update failed: " + ex.Message);
            }
        }



        public async Task<List<RosterScheduleEntrySetupViewModel>> GetRosterScheduleGridService()
        {
            var query = from ras in rosterEntryRepo.All()
                        join eoi in empOffRepo.All() on ras.EmployeeId equals eoi.EmployeeId
                        join e in employeeRepo.All() on ras.EmployeeId equals e.EmployeeId into empGroup
                        from e in empGroup.DefaultIfEmpty()
                        join d in desiRepo.All() on eoi.DesignationCode equals d.DesignationCode into desiGroup
                        from d in desiGroup.DefaultIfEmpty()
                        join s in shiftRepo.All() on ras.ShiftCode equals s.ShiftCode into shiftGroup
                        from s in shiftGroup.DefaultIfEmpty()
                        where
                        (ras.ApprovalStatus == "Approved")
                        select new RosterScheduleEntrySetupViewModel
                        {
                            TC = ras.Tc,
                            RosterScheduleId = ras.RosterScheduleId,
                            EmployeeID = ras.EmployeeId,
                            Name = (e != null ? e.FirstName + " " + e.LastName : ""),
                            DesignationName = d != null ? d.DesignationName : "",
                            ShiftName = s.ShiftName + "( " + s.ShiftStartTime.ToString("hh:mm:tt") + " - " + s.ShiftEndTime.ToString("hh:mm:tt") + " )" ?? "",
                            Remark = ras.Remark ?? "",
                            ApprovalDatetime = ras.ApprovalDatetime,
                            ApprovalStatus = ras.ApprovalStatus ?? "",
                            ApprovedBy = ras.ApprovedBy ?? "",
                            Date = ras.Date,
                            Luser= ras.Luser,
                            ApprovalDatetimeShow = ras.Date.ToString("dd/MM/yyyy hh:mm tt")

                            //DayName = ras.ScheduleDate.HasValue ? ras.ScheduleDate.Value.DayOfWeek.ToString() : "",
                            //luser = ras.LastModifiedBy ?? "" // You can rename it
                        };

            return await query.ToListAsync();
        }      
    }
}
