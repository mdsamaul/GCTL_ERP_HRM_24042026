using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.InkML;
using GCTL.Core.Data;
using GCTL.Core.ViewModels.ManualEarnLeaveEntry;
using GCTL.Core.ViewModels.PFAssignEntry;
using GCTL.Core.ViewModels.RosterScheduleEntry;
using GCTL.Data.Models;
using GCTL.Service.EmployeeWeekendDeclaration;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using Org.BouncyCastle.Ocsp;
using SixLabors.ImageSharp;
namespace GCTL.Service.RosterScheduleEntry 
{
    public class RosterScheduleEntryService : AppService<HrmRosterScheduleEntry>, IRosterScheduleEntryService
    {
        private readonly IRepository<HrmRosterScheduleEntry> rosterScheduleRepo;
        private readonly IRepository<HrmEmployee> employeeRepo;
        private readonly IRepository<HrmEmployeeOfficialInfo> empOffRepo;
        private readonly IRepository<HrmDefDesignation> desiRepo;
        private readonly IRepository<HrmDefDepartment> depRepo;
        private readonly IRepository<HrmDefDivision> divRepo;
        private readonly IRepository<CoreBranch> branchRepo;
        private readonly IRepository<CoreCompany> companyRepo;
        private readonly IRepository<HrmDefEmpType> defEmpTypeRepo;
        private readonly IRepository<HrmEisDefEmploymentNature> empNatureRepo;
        private readonly IRepository<HrmDefEmployeeStatus> empStatusRepo;
        private readonly IRepository<HrmSeparation> SeparationRepo;
        private readonly IRepository<HrmPayMonth> PayMonthRepo;
        private readonly IRepository<HrmAtdShift> shiftRepo;
        private readonly string _connectionString;

        public RosterScheduleEntryService(
            IRepository<HrmRosterScheduleEntry> rosterScheduleRepo,
            IRepository<HrmEmployee> employeeRepo,
          IRepository<HrmEmployeeOfficialInfo> empOffRepo,
          IRepository<HrmDefDesignation> desiRepo,
          IRepository<HrmDefDepartment> depRepo,
          IRepository<HrmDefDivision> divRepo,
          IRepository<CoreBranch> branchRepo,
          IRepository<CoreCompany> companyRepo,
          IRepository<HrmDefEmpType> defEmpTypeRepo,
          IRepository<HrmEisDefEmploymentNature> empNatureRepo,
          IRepository<HrmDefEmployeeStatus> empStatusRepo,
          IRepository<HrmSeparation> SeparationRepo,
          IRepository<HrmPayMonth> PayMonthRepo,
          IRepository<HrmAtdShift> ShiftRepo,
            IConfiguration configuration

            ) : base(rosterScheduleRepo)
        {
            this.rosterScheduleRepo = rosterScheduleRepo;
            this.employeeRepo = employeeRepo;
            this.empOffRepo = empOffRepo;
            this.desiRepo = desiRepo;
            this.depRepo = depRepo;
            this.divRepo = divRepo;
            this.branchRepo = branchRepo;
            this.companyRepo = companyRepo;
            this.defEmpTypeRepo = defEmpTypeRepo;
            this.empNatureRepo = empNatureRepo;
            this.empStatusRepo = empStatusRepo;
            this.SeparationRepo = SeparationRepo;
            this.PayMonthRepo = PayMonthRepo;
            this.shiftRepo = ShiftRepo;
            _connectionString = configuration.GetConnectionString("ApplicationDbConnection");
        }
        public async Task<RosterScheduleEntryFilterListDto> GetFilterDataAsync(RosterScheduleEntryFilterDto filter)
        {
            var query = from e in employeeRepo.All()
                        join eoi in empOffRepo.All() on e.EmployeeId equals eoi.EmployeeId
                        join dg in desiRepo.All() on eoi.DesignationCode equals dg.DesignationCode into dg_join
                        from dg in dg_join.DefaultIfEmpty()
                        join dp in depRepo.All() on eoi.DepartmentCode equals dp.DepartmentCode into dp_join
                        from dp in dp_join.DefaultIfEmpty()
                        join dv in divRepo.All() on eoi.DivisionCode equals dv.DivisionCode into dv_join
                        from dv in dv_join.DefaultIfEmpty()
                        join cb in branchRepo.All() on eoi.BranchCode equals cb.BranchCode into cb_join
                        from cb in cb_join.DefaultIfEmpty()
                        join cc in companyRepo.All() on eoi.CompanyCode equals cc.CompanyCode into cc_join
                        from cc in cc_join.DefaultIfEmpty()
                        join et in defEmpTypeRepo.All() on eoi.EmpTypeCode equals et.EmpTypeCode into et_join
                        from et in et_join.DefaultIfEmpty()
                        join en in empNatureRepo.All() on eoi.EmploymentNatureId equals en.EmploymentNatureId into en_join
                        from en in en_join.DefaultIfEmpty()
                        join empStatus in empStatusRepo.All() on eoi.EmployeeStatus equals empStatus.EmployeeStatusId into empStatus_join
                        from empStatus in empStatus_join.DefaultIfEmpty()
                        join sp in SeparationRepo.All() on e.EmployeeId equals sp.EmployeeId into sp_join
                        from sp in sp_join.DefaultIfEmpty()
                            //where (filter.CompanyCodes == null || filter.CompanyCodes.Contains(eoi.CompanyCode))
                            //    && (filter.BranchCodes == null || filter.BranchCodes.Contains(eoi.BranchCode))
                            //    && (filter.DivisionCodes == null || filter.DivisionCodes.Contains(eoi.DivisionCode))
                            //    && (filter.DepartmentCodes == null || filter.DepartmentCodes.Contains(eoi.DepartmentCode))
                            //    && (filter.DesignationCodes == null || filter.DesignationCodes.Contains(eoi.DesignationCode))
                            //    && (filter.EmployeeIDs == null || filter.EmployeeIDs.Contains(e.EmployeeId.ToString()))
                            //    && (filter.EmployeeStatuses == null || filter.EmployeeStatuses.Contains(eoi.EmployeeStatus))
                        select new
                        {
                            EmpID = e.EmployeeId,
                            EmployeeName = e.FirstName ?? "" + e.LastName ?? "",
                            CompanyCode = eoi.CompanyCode,
                            BranchCode = cb.BranchCode,
                            DivisionCode = dv.DivisionCode,
                            DivisionName = dv.DivisionName,
                            DesignationCode = dg.DesignationCode,
                            DesignationName = dg.DesignationName,
                            DepartmentCode = dp.DepartmentCode,
                            DepartmentName = dp.DepartmentName,
                            BranchName = cb.BranchName,
                            CompanyName = cc.CompanyName,
                            EmployeeStatusCode = eoi.EmployeeStatus,
                            EmpStatusName = empStatus.EmployeeStatus,
                            EmployeeTypeName = et.EmpTypeName,
                            EmploymentNature = en.EmploymentNature,
                            JoiningDate = eoi.JoiningDate,
                            EndDate = (eoi.EmployeeStatus == "01" ? DateTime.Now : sp.SeparationDate),
                            DaysInYear = ((((eoi.EmployeeStatus == "01" ? DateTime.Now : sp.SeparationDate).Year % 4 == 0)
                     && ((eoi.EmployeeStatus == "01" ? DateTime.Now : sp.SeparationDate).Year % 100 != 0))
                     || ((eoi.EmployeeStatus == "01" ? DateTime.Now : sp.SeparationDate).Year % 400 == 0)) ? 366 : 365,
                            ServiceDuration = (eoi.JoiningDate.HasValue ?
                       ((DateTime.Now.Year - eoi.JoiningDate.Value.Year) +
                       ((DateTime.Now.Month - eoi.JoiningDate.Value.Month) / 12.0) +
                       ((DateTime.Now.Day - eoi.JoiningDate.Value.Day) /
                       (DateTime.IsLeapYear(DateTime.Now.Year) ? 366.0 : 365.0)))
                       : 0),
                            SeparationDate = sp.SeparationDate,
                            ServiceDuration2 = eoi.JoiningDate.HasValue
                         ? (DateTime.Now.Year - eoi.JoiningDate.Value.Year).ToString() + "Y" +
                           (DateTime.Now.Month - eoi.JoiningDate.Value.Month).ToString() + "M-" +
                           (DateTime.Now.Day - eoi.JoiningDate.Value.Day).ToString() + "D"
                         : "N/A"
                        };

            if (filter.CompanyCodes?.Any() == true)
            {
                query = query.Where(x => x.CompanyCode != null && filter.CompanyCodes.Contains(x.CompanyCode));
            }
            if (filter.BranchCodes?.Any() == true)
            {
                query = query.Where(x => x.BranchCode != null & filter.BranchCodes.Contains(x.BranchCode));
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
                query = query.Where(x => x.DesignationCode != null && filter.DesignationCodes.Contains(x.DesignationCode));
            }
            if (filter.EmployeeIDs?.Any() == true)
            {
                query = query.Where(x => x.EmpID != null && filter.EmployeeIDs.Contains(x.EmpID));
            }
            if (filter.EmployeeStatuses?.Any() == true)
            {
                query = query.Where(x => x.EmployeeStatusCode != null && filter.EmployeeStatuses.Contains(x.EmployeeStatusCode));
            }

            var result = new RosterScheduleEntryFilterListDto
            {
                Companies = await query.Where(x => x.CompanyCode != null && x.CompanyName != null)
                .Select(x => new RosterScheduleEntryFilterResultDto { Code = x.CompanyCode, Name = x.CompanyName })
                .Distinct().ToListAsyncSafe(),

                Branches = await query.Where(x => x.BranchCode != null && x.BranchName != null)
                .Select(x => new RosterScheduleEntryFilterResultDto { Code = x.BranchCode, Name = x.BranchName })
                .Distinct().ToListAsyncSafe(),

                Divisions = await query.Where(x => x.DivisionCode != null && x.DivisionName != null)
                .Select(x => new RosterScheduleEntryFilterResultDto { Code = x.DivisionCode, Name = x.DivisionName })
                .Distinct().ToListAsyncSafe(),
                Departments = await query.Where(x => x.DepartmentCode != null && x.DepartmentName != null)
                .Select(x => new RosterScheduleEntryFilterResultDto { Code = x.DepartmentCode, Name = x.DepartmentName })
                .Distinct().ToListAsyncSafe(),
                Designations = await query.Where(x => x.DesignationCode != null && x.DesignationName != null)
                .Select(x => new RosterScheduleEntryFilterResultDto { Code = x.DesignationCode, Name = x.DesignationName })
                .Distinct().ToListAsyncSafe(),
                Employees = await query.Where(x => x.EmpID != null && x.EmployeeName != null)
                .Select(x => new RosterScheduleEntryFilterResultDto
                {
                    Code = x.EmpID,
                    Name = x.EmployeeName,
                    Designation = x.DesignationName,
                    Department = x.DepartmentName,
                    Branch = x.BranchName,
                    Company = x.CompanyName,
                    EmployeeType = x.EmployeeTypeName,
                    EmploymentNature = x.EmploymentNature,
                    JoiningDate = x.JoiningDate != null ? x.JoiningDate.Value.ToString("dd/MM/yyyy") : "",
                    SeparationDate = x.SeparationDate,
                    ServiceDuration = x.ServiceDuration,
                    ServiceDuration2 = x.ServiceDuration2,
                    EmployeeStatus = x.EmpStatusName

                }).Distinct().ToListAsyncSafe(),

                ActivityStatuses = await query.Where(x => x.EmployeeTypeName != null)
                .Select(x => new RosterScheduleEntryFilterResultDto { Code = x.EmployeeStatusCode, Name = x.EmployeeTypeName })
                .Distinct().ToListAsyncSafe(),
            };
            return result;
        }
        public async Task<List<PAYMonthResultDto>> getAllMonthService()
        {
            var months = await PayMonthRepo.GetAllAsync();

            var result = months.Select(m => new PAYMonthResultDto
            {
                MonthId = m.MonthId,
                MonthName = m.MonthName,
                MonthNum = m.MonthNum,
                MonthNameBangla = m.MonthNameBangla
            }).ToList();

            return result;
        }

        public async Task<List<RosterShiftDto>> getAlllShiftService()
        {
            var Shifts = await shiftRepo.GetAllAsync();
            return Shifts.Select(entityVM => new RosterShiftDto
            {
                AutoId = entityVM.AutoId,
                ShiftCode = entityVM.ShiftCode,
                ShiftTypeId = entityVM.ShiftTypeId,
                ShiftName = entityVM.ShiftName,
                ShiftShortName = entityVM.ShiftShortName,
                Remarks = entityVM.Remarks,
                Description = entityVM.ShiftShortName,
                ShiftStartTime = entityVM.ShiftStartTime,
                ShiftEndTime = entityVM.ShiftEndTime,
                AbsentTime = entityVM.AbsentTime,
                LateTime = entityVM.LateTime,
                Ldate = entityVM.Ldate,
                ModifyDate = entityVM.ModifyDate,
                Wef = entityVM.Wef,
                Luser = entityVM.Luser,
                Lip = entityVM.Lip,
                Lmac = entityVM.Lmac,
                LunchInTime = entityVM.LunchInTime,
                LunchOutTime = entityVM.LunchOutTime,
                LunchBreakHour = entityVM.LunchBreakHour
            }).ToList();
        }

        public async Task<(bool isSuccess, string isMessage, object data)> CreateAndUpdateService(RosterScheduleEntrySetupViewModel FromModel)
        {
            //edit
            if (FromModel.RosterScheduleId != null)
            {
                if (FromModel.FromDate == null || FromModel.ShiftCode == null)
                {
                    return (false, "Update Failed", null);
                }
                var test = rosterScheduleRepo.GetAll().Where(x => x.RosterScheduleId == FromModel.RosterScheduleId).FirstOrDefault();

                var roster = await rosterScheduleRepo.All().FirstOrDefaultAsync(e => e.Tc == test.Tc);

                if (roster == null)
                {
                    return (false, "Update Failed", null);
                }

                try
                {
                    bool isDuplicate = rosterScheduleRepo.GetAll()
                        .Any(x => x.EmployeeId == roster.EmployeeId && x.Date == DateTime.Parse(FromModel.FromDate) && x.RosterScheduleId == FromModel.RosterScheduleId && x.ApprovalStatus == "Approved");
                    if (isDuplicate)
                    {
                        return (false, "Approved schedule can't be updated.", null);
                    }
                    roster.Date = DateTime.Parse(FromModel.FromDate);
                    roster.ShiftCode = FromModel.ShiftCode;
                    roster.Remark = FromModel.Remark ?? "";
                    roster.ModifyDate = FromModel.ModifyDate;
                    roster.Weekend = "";
                    // Handle the empty string cases for ApprovedBy and ApprovalStatus
                    roster.ApprovedBy = FromModel.ApprovedBy ?? "";
                    roster.ApprovalStatus = FromModel.ApprovalStatus ?? "";
                    // Handle the null DateTime case for ApprovalDatetime
                    roster.ApprovalDatetime = FromModel.ApprovalDatetime;

                    await rosterScheduleRepo.UpdateAsync(roster);
                    return (isSuccess: true, isMessage: "Update Successfully", data: roster);
                }
                catch (Exception ex)
                {
                    return (false, "Update Failed: " + ex.Message, null);
                }
            }
            else
            {
                //save
                if (FromModel == null || FromModel.FromDate == null || FromModel.ToDate == null ||
     FromModel.EmployeeListID == null || FromModel.year == null || FromModel.ShiftCode == null)
                {
                    return (false, "Save Failed: Missing required fields", null);
                }

                if (!DateTime.TryParse(FromModel.FromDate, out DateTime startDate) ||
                    !DateTime.TryParse(FromModel.ToDate, out DateTime endDate) ||
                    !int.TryParse(FromModel.year, out int year) ||
                    year < 1900 || year > 2100 ||
                    startDate.Year < 1900 || startDate.Year > 2100 ||
                    endDate.Year < 1900 || endDate.Year > 2100)
                {
                    return (false, "Save Failed: Invalid date or year", null);
                }

                var allEmployees = employeeRepo.GetAll().Select(e => e.EmployeeId).ToHashSet();

                foreach (var empId in FromModel.EmployeeListID)
                {
                    if (!allEmployees.Contains(empId))
                        return (false, $"Employee Not found: {empId}", null);
                }

                var oldSchedules = rosterScheduleRepo.GetAll()
                    .Where(x => FromModel.EmployeeListID.Contains(x.EmployeeId) &&
                                x.Date >= startDate && x.Date <= endDate &&
                                x.ApprovalStatus != "Approved")
                    .ToList();

                foreach (var old in oldSchedules)
                {
                    rosterScheduleRepo.Delete(old);
                }

                DataTable rosterTable = new DataTable();
                rosterTable.Columns.Add("RosterScheduleId", typeof(string));
                rosterTable.Columns.Add("EmployeeId", typeof(string));
                rosterTable.Columns.Add("Date", typeof(DateTime));
                rosterTable.Columns.Add("ShiftCode", typeof(string));
                rosterTable.Columns.Add("Remark", typeof(string));
                rosterTable.Columns.Add("CompanyCode", typeof(string));
                rosterTable.Columns.Add("Ldate", typeof(DateTime)).AllowDBNull = true;
                rosterTable.Columns.Add("Luser", typeof(string));
                rosterTable.Columns.Add("Lip", typeof(string));
                rosterTable.Columns.Add("Lmac", typeof(string));
                rosterTable.Columns.Add("EmployeeIdSao", typeof(string));
                rosterTable.Columns.Add("Weekend", typeof(string));
                rosterTable.Columns.Add("ApprovalStatus", typeof(string));
                rosterTable.Columns.Add("ApprovedBy", typeof(string));
                rosterTable.Columns.Add("ModifyBy", typeof(string));
                rosterTable.Columns.Add("ApprovalDatetime", typeof(DateTime)).AllowDBNull = true;

                var lastRoster = rosterScheduleRepo.GetAll()
                    .OrderByDescending(x => x.RosterScheduleId)
                    .FirstOrDefault();

                int nextIdNumber = 1;
                if (lastRoster != null && int.TryParse(lastRoster.RosterScheduleId, out int lastNumber))
                {
                    nextIdNumber = lastNumber + 1;
                }
                //string newId = $"{nextIdNumber:D8}";
                //newId = newId + 1;
                var existingShifts = rosterScheduleRepo.GetAll()
                    .Where(x => FromModel.EmployeeListID.Contains(x.EmployeeId) &&
                                x.Date >= startDate && x.Date <= endDate)
                    .ToList();

                foreach (var empId in FromModel.EmployeeListID)
                {
                    DateTime currentDate = startDate;
                    while (currentDate <= endDate)
                    {
                        if (existingShifts.Any(x => x.EmployeeId == empId && x.Date == currentDate && x.ApprovalStatus == "Approved"))
                        {
                            currentDate = currentDate.AddDays(1);
                            continue;
                        }

                        object approvalDatetimeValue = DBNull.Value;

                        if (FromModel.ApprovalDatetime.HasValue)
                        {
                            approvalDatetimeValue = FromModel.ApprovalDatetime.Value;
                        }
                        else if (!string.IsNullOrEmpty(FromModel.ApprovalDatetimeShow) &&
                                 DateTime.TryParse(FromModel.ApprovalDatetimeShow, out DateTime parsedDate))
                        {
                            approvalDatetimeValue = parsedDate;
                        }
                        string newId = $"{nextIdNumber:D8}";
                        nextIdNumber++;
                        rosterTable.Rows.Add(
                            newId,
                            empId,
                            currentDate,
                            FromModel.ShiftCode,
                            FromModel.Remark ?? "",
                            FromModel.CompanyCode ?? "",
                            FromModel.Ldate == default ? DBNull.Value : (object)FromModel.Ldate,
                            FromModel.Luser ?? "",
                            FromModel.Lip ?? "",
                            FromModel.Lmac ?? "",
                            "", // EmployeeIdSao
                            "", // Weekend
                            FromModel.ApprovalStatus ?? "",
                            FromModel.ApprovedBy ?? "",
                            FromModel.ModifyBy ?? "",
                            approvalDatetimeValue
                        );
                        currentDate = currentDate.AddDays(1);                    
                    }
                   
                }

                try
                {
                    using (SqlConnection conn = new SqlConnection(_connectionString))
                    {
                        conn.Open();
                        using (SqlBulkCopy bulkCopy = new SqlBulkCopy(conn))
                        {
                            bulkCopy.DestinationTableName = "HRM_RosterScheduleEntry";
                            bulkCopy.BatchSize = 10000;
                            bulkCopy.BulkCopyTimeout = 600;

                            bulkCopy.ColumnMappings.Add("RosterScheduleId", "RosterScheduleId");
                            bulkCopy.ColumnMappings.Add("EmployeeId", "EmployeeID");
                            bulkCopy.ColumnMappings.Add("Date", "Date");
                            bulkCopy.ColumnMappings.Add("ShiftCode", "ShiftCode");
                            bulkCopy.ColumnMappings.Add("Remark", "Remark");
                            bulkCopy.ColumnMappings.Add("CompanyCode", "CompanyCode");
                            bulkCopy.ColumnMappings.Add("Ldate", "LDate");
                            bulkCopy.ColumnMappings.Add("Luser", "LUser");
                            bulkCopy.ColumnMappings.Add("Lip", "LIP");
                            bulkCopy.ColumnMappings.Add("Lmac", "LMAC");
                            bulkCopy.ColumnMappings.Add("EmployeeIdSao", "EmployeeID_SAO");
                            bulkCopy.ColumnMappings.Add("Weekend", "Weekend");
                            bulkCopy.ColumnMappings.Add("ApprovalStatus", "ApprovalStatus");
                            bulkCopy.ColumnMappings.Add("ApprovalDatetime", "ApprovalDatetime");
                            bulkCopy.ColumnMappings.Add("ApprovedBy", "ApprovedBy");
                            bulkCopy.ColumnMappings.Add("ModifyBy", "ModifyBy");

                            int totalRows = rosterTable.Rows.Count;
                            int batchSize = 10000;

                            for (int i = 0; i < totalRows; i += batchSize)
                            {
                                DataTable batchTable = rosterTable.AsEnumerable()
                                    .Skip(i).Take(Math.Min(batchSize, totalRows - i))
                                    .CopyToDataTable();

                                bulkCopy.WriteToServer(batchTable);
                            }
                        }
                    }

                    var resultList = new List<Dictionary<string, object>>();
                    foreach (DataRow row in rosterTable.Rows)
                    {
                        var dict = new Dictionary<string, object>();
                        foreach (DataColumn col in rosterTable.Columns)
                        {
                            dict[col.ColumnName] = row[col];
                        }
                        resultList.Add(dict);
                    }

                    return (true, "Save Success", resultList);
                }
                catch (Exception ex)
                {
                    return (false, "Save Failed: " + ex.Message, null);
                }
            }

            }

            //get roster sechudel 
        public async Task<List<RosterScheduleEntrySetupViewModel>> GetRosterScheduleGridService()
        {
            var query = from rs in rosterScheduleRepo.All()
                        join eoi in empOffRepo.All() on rs.EmployeeId equals eoi.EmployeeId
                        join e in employeeRepo.All() on rs.EmployeeId equals e.EmployeeId into empGroup
                        from e in empGroup.DefaultIfEmpty()
                        join d in desiRepo.All() on eoi.DesignationCode equals d.DesignationCode into desigGroup
                        from d in desigGroup.DefaultIfEmpty()
                        join s in shiftRepo.All() on rs.ShiftCode equals s.ShiftCode into shiftGroup
                        from s in shiftGroup.DefaultIfEmpty()
                        select new RosterScheduleEntrySetupViewModel
                        {
                            TC = rs.Tc,
                            RosterScheduleId = rs.RosterScheduleId,
                            EmployeeID = eoi.EmployeeId,
                            Name = (e != null ? e.FirstName + " " + e.LastName : ""),
                            DesignationName = d != null ? d.DesignationName : "",
                            ShiftName = s != null ? s.ShiftName +" ("+ s.ShiftStartTime.ToString("hh:MM:tt") +" - "+ s.ShiftEndTime.ToString("hh:MM:tt") + ")": "",
                            DateShow = rs.Date.ToString("dd/MM/yyyy"),
                            DayName = rs.Date.ToString("dddd"),
                            Remark = rs.Remark,
                            Luser = rs.Luser,
                            ApprovalStatus = rs.ApprovalStatus ?? "",
                            ApprovedBy = rs.ApprovedBy ?? "",
                            ApprovalDatetimeShow = rs.ApprovalDatetime.HasValue? rs.ApprovalDatetime.Value.ToString("dd/MM/yyyy"): ""

                        };

            return await query.ToListAsync();
        }


        //edit get
        public Task<RosterScheduleEntrySetupViewModel> EditGetServices(string id)
        {
            var emp = rosterScheduleRepo.GetAll().FirstOrDefault(x => x.RosterScheduleId == id);
            if (emp == null)
            {
                return Task.FromResult<RosterScheduleEntrySetupViewModel>(null);
            }
            var shift = shiftRepo.All().Where(x => x.ShiftCode == emp.ShiftCode).FirstOrDefault();
            return Task.FromResult(new RosterScheduleEntrySetupViewModel
            {
                RosterScheduleId = emp.RosterScheduleId,
                TC= emp.Tc,
                EmployeeID= emp.EmployeeId,
                Date=emp.Date,
                DateShow=emp.Date.ToString("dd/MM/yyyy"),
                ShiftCode= emp.ShiftCode,
                Remark=emp.Remark,
                ShiftName = shift.ShiftName
            });
        }
        

        //download excel
        public async Task<byte[]> GenerateEmpRosterExcelDownload()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var rosters = rosterScheduleRepo.GetAll().ToList();
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Employees");

                worksheet.Cells[1, 1].Value = "EmployeeId";
                worksheet.Cells[1, 2].Value = "Date (dd/MM/yyyy)";
                worksheet.Cells[1, 3].Value = "Shift";
                worksheet.Cells[1, 4].Value = "Remarks";
                worksheet.Cells[1, 5].Value = "Company Code";

                worksheet.Column(1).Style.Numberformat.Format = "@";
                worksheet.Column(2).Style.Numberformat.Format = "dd/MM/yyyy";

                int row = 2;
                int count = 0;

                foreach (var roster in rosters)
                {
                    worksheet.Cells[row, 1].Value = roster.EmployeeId;

                    if (roster.Date != null)
                        worksheet.Cells[row, 2].Value = Convert.ToDateTime(roster.Date); // Ensure it's a DateTime

                    worksheet.Cells[row, 3].Value = roster.ShiftCode;
                    worksheet.Cells[row, 4].Value = roster.Remark;
                    worksheet.Cells[row, 5].Value = roster.CompanyCode;

                    row++;
                    count++;

                    if (count >= 2) break;
                }

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                return package.GetAsByteArray();
            }
        }

        public async Task<(bool isSuccess, string message, object data)> SaveRosterExcel(RosterScheduleEntrySetupViewModel model)
        {
            if (model == null || model.EmployeeListID == null || model.DateList == null || model.ShiftCodeList == null || !model.DateList.Any() || !model.EmployeeListID.Any() || !model.ShiftCodeList.Any())
            {
                return (false, "Invalid data received: Missing data", null);
            }

            try
            {
                var records = new List<HrmRosterScheduleEntry>();
                var lastDeclaration = await rosterScheduleRepo.All().OrderByDescending(x => x.Tc).FirstOrDefaultAsync();

                int nextIdNumber = 1;
                if (lastDeclaration != null && !string.IsNullOrEmpty(lastDeclaration.RosterScheduleId))
                {
                    if (int.TryParse(lastDeclaration.RosterScheduleId, out int lastNumber))
                    {
                        nextIdNumber = lastNumber + 1;
                    }
                }

                int totalRows = new[]
                {
            model.EmployeeListID.Count,
            model.DateList.Count,
            model.ShiftCodeList.Count,
            model.RemarkList?.Count ?? 0 ,
        }.Min();

                var existingEmployeeIds = employeeRepo.GetAll().Select(x => x.EmployeeId).ToHashSet();

                for (int i = 0; i < totalRows; i++)
                {
                    var empId = model.EmployeeListID[i];
                    //var date = DateTime.Parse( model.DateList[i]);
                    var date = DateTime.ParseExact(model.DateList[i], "dd/MM/yyyy", CultureInfo.InvariantCulture);

                    var shiftCode = model.ShiftCodeList[i];
                    var remark = (model.RemarkList != null && i < model.RemarkList.Count) ? model.RemarkList[i] : "";


                    if (existingEmployeeIds.Contains(empId))
                    {
                        // check if employee record already exists for same year
                        bool existsEmployee = rosterScheduleRepo.GetAll().Any(x => x.EmployeeId == empId && x.Date == date);
                        if (existsEmployee)
                        {
                            var employeesToDelete = rosterScheduleRepo.GetAll().Where(x => x.EmployeeId == empId && x.Date == date&& x.ApprovalStatus != "Approved").ToList();
                            await rosterScheduleRepo.DeleteRangeAsync(employeesToDelete);
                        }

                        string newRosterID = $"{nextIdNumber:D8}";
                        nextIdNumber++;

                        records.Add(new HrmRosterScheduleEntry
                        {
                            RosterScheduleId = newRosterID,
                            EmployeeId = empId,
                            Date = date,
                            ShiftCode = shiftCode,
                            Remark = remark ?? "",
                            CompanyCode = model.CompanyCode ?? "",
                            Ldate = model.Ldate,
                            Luser = model.Luser,
                            Lip = model.Lip,
                            Lmac = model.Lmac,
                            EmployeeIdSao="",
                            Weekend="",
                        });
                    }

                }

                if (!records.Any())
                {
                    return (false, "Save Failed: No valid employees found", null);
                }

                await rosterScheduleRepo.AddRangeAsync(records);
                return (true, "Save Successfully", records);
            }
            catch (DbUpdateException dbEx)
            {
                return (false, "An error occurred while saving data to the database.", dbEx.InnerException?.Message ?? dbEx.Message);
            }
            catch (Exception ex)
            {
                return (false, "An unexpected error occurred while saving data.", ex.Message);
            }
        }
    }
}
