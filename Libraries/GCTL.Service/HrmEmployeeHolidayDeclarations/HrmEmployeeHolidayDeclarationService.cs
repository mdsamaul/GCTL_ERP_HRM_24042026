using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Data;
using GCTL.Core.ViewModels.HrmDefHolidayDeclarationTypes;
using GCTL.Core.ViewModels.HrmEmployeeHolidayDeclarations;
using GCTL.Data.Models;

using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using Org.BouncyCastle.Crypto.Agreement.JPake;
using Org.BouncyCastle.Ocsp;

namespace GCTL.Service.HrmEmployeeHolidayDeclarations
{
    public class HrmEmployeeHolidayDeclarationService : AppService<HrmEmployeeHolidayDeclaration>, IHrmEmployeeHolidayDeclarationService
    {
        private readonly IRepository<HrmEmployeeHolidayDeclaration> holidayRepository;
        private readonly IRepository<HrmEmployee> employeeRepository;
        private readonly IRepository<HrmEmployeeOfficialInfo> employeeOfficialInfoRepository;
        private readonly IRepository<HrmDefDesignation> designationRepository;
        private readonly IRepository<HrmDefHolidayDeclarationType> HrmDefHolidayDeclarationType;

        private readonly IRepository<CoreCompany> companyRepository;
        private readonly IRepository<HrmDefDivision> divisionRepository;
        private readonly IRepository<CoreAccessCode> accessCodeRepository;
        private readonly IRepository<CoreBranch> branchRepository;
        private readonly IRepository<HrmDefDepartment> departmentRepository;
        private readonly IRepository<HrmDefEmployeeStatus> employeeStatusRepository;
        private readonly IRepository<HrmDefEmpType> empTypeRepository;

        public HrmEmployeeHolidayDeclarationService(
            IRepository<HrmEmployeeHolidayDeclaration> holidayRepository,
            IRepository<HrmEmployee> employeeRepository,
            IRepository<HrmEmployeeOfficialInfo> employeeOfficialInfoRepository,
            IRepository<HrmDefDesignation> designationRepository,
            IRepository<HrmDefHolidayDeclarationType> HrmDefHolidayDeclarationType,
            IRepository<CoreCompany> companyRepository,
            IRepository<HrmDefDivision> divisionRepository,
            IRepository<CoreAccessCode> accessCodeRepository,
            IRepository<CoreBranch> branchRepository,
            IRepository<HrmDefDepartment> departmentRepository,
            IRepository<HrmDefEmployeeStatus> employeeStatusRepository,
            IRepository<HrmDefEmpType> empTypeRepository
        )
            : base(holidayRepository)
        {
            this.holidayRepository = holidayRepository;
            this.employeeRepository = employeeRepository;
            this.employeeOfficialInfoRepository = employeeOfficialInfoRepository;
            this.designationRepository = designationRepository;
            this.HrmDefHolidayDeclarationType = HrmDefHolidayDeclarationType;

            this.companyRepository = companyRepository;
            this.divisionRepository = divisionRepository;
            this.accessCodeRepository = accessCodeRepository;
            this.branchRepository = branchRepository;
            this.departmentRepository = departmentRepository;
            this.employeeStatusRepository = employeeStatusRepository;
            this.empTypeRepository = empTypeRepository;
        }


        public List<CoreCompany> GetAllCompany()
        {
            return companyRepository.GetAll().Where(e => e.CompanyCode == "001").ToList();
        }

        public async Task<EmployeeFilterResultDto> GetFilterDataAsync(EmployeeFilterViewModel filter)
        {
            var result = new EmployeeFilterResultDto();

            var query = from e in employeeOfficialInfoRepository.All().AsNoTracking()
                        join emp in employeeRepository.All().AsNoTracking() on e.EmployeeId equals emp.EmployeeId
                        join c in companyRepository.All().AsNoTracking() on e.CompanyCode equals c.CompanyCode into companyGroup
                        from c in companyGroup.DefaultIfEmpty()
                        join b in branchRepository.All().AsNoTracking() on e.BranchCode equals b.BranchCode into branchGroup
                        from b in branchGroup.DefaultIfEmpty()
                        join d in departmentRepository.All().AsNoTracking() on e.DepartmentCode equals d.DepartmentCode into deptGroup
                        from d in deptGroup.DefaultIfEmpty()
                        join ds in designationRepository.All().AsNoTracking() on e.DesignationCode equals ds.DesignationCode into desigGroup
                        from ds in desigGroup.DefaultIfEmpty()
                        join status in employeeStatusRepository.All().AsNoTracking() on e.EmployeeStatus equals status.EmployeeStatusId into statusGroup
                        from status in statusGroup.DefaultIfEmpty()
                        join emptype in empTypeRepository.All().AsNoTracking() on e.EmpTypeCode equals emptype.EmpTypeCode into empTypeGroup
                        from emptype in empTypeGroup.DefaultIfEmpty()
                        select new
                        {
                            EmployeeId = e.EmployeeId,
                            FirstName = emp.FirstName,
                            LastName = emp.LastName,
                            e.JoiningDate,
                            e.CompanyCode,
                            CompanyName = c.CompanyName,
                            e.BranchCode,
                            BranchName = b.BranchName,
                            e.DepartmentCode,
                            DepartmentName = d.DepartmentName,
                            e.DesignationCode,
                            DesignationName = ds.DesignationName,
                            e.DivisionCode,
                            EmployeeTypeName = emptype.EmpTypeName,
                            EmployeeStatusId = e.EmployeeStatus,
                            EmployeeStatusName = status.EmployeeStatus
                        };

            if (filter.CompanyCodes == null || !filter.CompanyCodes.Any())
                return new EmployeeFilterResultDto();
            if (filter.BranchCodes?.Any() == true)
                query = query.Where(x => filter.BranchCodes.Contains(x.BranchCode));

            if (filter.DivisionCodes?.Any() == true)
                query = query.Where(x => filter.DivisionCodes.Contains(x.DivisionCode));

            if (filter.DepartmentCodes?.Any() == true)
                query = query.Where(x => filter.DepartmentCodes.Contains(x.DepartmentCode));

            if (filter.DesignationCodes?.Any() == true)
                query = query.Where(x => filter.DesignationCodes.Contains(x.DesignationCode));

            if (filter.EmployeeIDs?.Any() == true)
                query = query.Where(x => filter.EmployeeIDs.Contains(x.EmployeeId));

            if (filter.EmployeeStatuses?.Any() == true)
                query = query.Where(x => filter.EmployeeStatuses.Contains(x.EmployeeStatusName));

            var filteredData = await query.ToListAsync();

            result.Employees = filteredData.Select(x => new EmployeeListItemViewModel
            {
                EmployeeId = x.EmployeeId,
                EmployeeName = string.Join(" ", new[] { x.FirstName, x.LastName }.Where(n => !string.IsNullOrWhiteSpace(n))),
                JoiningDate = x.JoiningDate.HasValue ? x.JoiningDate.Value.ToString("dd/MM/yyyy") : "",
                DesignationName = x.DesignationName,
                DepartmentName = x.DepartmentName,
                BranchName = x.BranchName,
                CompanyName = x.CompanyName,
                EmployeeTypeName = x.EmployeeTypeName,
                EmployeeStatus = x.EmployeeStatusName
            }).ToList();

            result.LookupData["companies"] = filteredData
                .Where(x => x.CompanyCode != null && x.CompanyName != null)
                .Select(x => new LookupItemDto { Code = x.CompanyCode, Name = x.CompanyName })
                .Distinct()
                .ToList();

            result.LookupData["branches"] = filteredData
                .Where(x => x.BranchCode != null && x.BranchName != null)
                .Select(x => new LookupItemDto { Code = x.BranchCode, Name = x.BranchName })
                .Distinct()
                .ToList();

            result.LookupData["departments"] = filteredData
                .Where(x => x.DepartmentCode != null && x.DepartmentName != null)
                .Select(x => new LookupItemDto { Code = x.DepartmentCode, Name = x.DepartmentName })
                .Distinct()
                .ToList();

            result.LookupData["designations"] = filteredData
                .Where(x => x.DesignationCode != null && x.DesignationName != null)
                .Select(x => new LookupItemDto { Code = x.DesignationCode, Name = x.DesignationName })
                .Distinct()
                .ToList();

            result.LookupData["employees"] = filteredData
                .Where(x => x.EmployeeId != null && x.FirstName != null)
                .Select(x => new LookupItemDto
                {
                    Code = x.EmployeeId,
                    Name = string.Join(" ", new[] { x.FirstName, x.LastName }.Where(n => !string.IsNullOrWhiteSpace(n))) + $" ({x.EmployeeId})"
                })
                .Distinct()
                .ToList();

            result.LookupData["employeeStatuses"] = filteredData
                .Where(x => !string.IsNullOrWhiteSpace(x.EmployeeStatusName))
                .Select(x => new LookupItemDto { Code = x.EmployeeStatusName, Name = x.EmployeeStatusName })
                .Distinct()
                .ToList();

            return result;
        }

        public async Task<bool> BulkDeleteAsync(List<decimal> ids)
        {
            await holidayRepository.BeginTransactionAsync();

            try
            {
                var holidayDeclaration = await holidayRepository.All().Where(c => ids.Contains(c.AutoId)).ToListAsync();

                if (holidayDeclaration != null || holidayDeclaration.Count > 0)
                {
                    await holidayRepository.DeleteRangeAsync(holidayDeclaration);

                    await holidayRepository.CommitTransactionAsync();

                    return true;
                }

                await holidayRepository.RollbackTransactionAsync();
                return false;
            }
            catch (Exception ex)
            {
                await holidayRepository.RollbackTransactionAsync();
                Console.WriteLine($"Bulk delete error: {ex}");
                return false;
            }
        }

        public async Task<bool> DeleteHolidayDeclarationsByDateAndEmployees(DateTime date, List<string> employeeIds, string companyCode)
        {
            try
            {
                await holidayRepository.BeginTransactionAsync();

                var holidayDeclarations = holidayRepository.All()
                    .Where(h => h.Date.Date == date.Date &&
                                employeeIds.Contains(h.EmployeeId) &&
                                h.CompanyCode == companyCode)
                    .ToList();

                if (holidayDeclarations == null || holidayDeclarations.Count == 0)
                {
                    await holidayRepository.BeginTransactionAsync();
                    return false;
                }

                await holidayRepository.DeleteRangeAsync(holidayDeclarations);

                await holidayRepository.CommitTransactionAsync();

                return true;
            }
            catch (Exception ex)
            {
                await holidayRepository.RollbackTransactionAsync();

                return false;
            }
        }

        public async Task<List<HrmEmployeeHolidayDeclarationViewModel>> GetAllAsync()
        {

            var query = from h in holidayRepository.All().AsNoTracking()
                        join e in employeeRepository.All().AsNoTracking() on h.EmployeeId equals e.EmployeeId
                        join ei in employeeOfficialInfoRepository.All().AsNoTracking() on e.EmployeeId equals ei.EmployeeId into eiJoin
                        from ei in eiJoin.DefaultIfEmpty()
                        join d in designationRepository.All() on ei.DesignationCode equals d.DesignationCode into dJoin
                        from d in dJoin.DefaultIfEmpty()
                        join ht in HrmDefHolidayDeclarationType.All() on h.Hdtcode equals ht.Hdtcode into htJoin
                        from ht in htJoin.DefaultIfEmpty()
                        select new HrmEmployeeHolidayDeclarationViewModel
                        {
                            AutoId = h.AutoId,
                            Ehdid = h.Ehdid,
                            EmployeeId = h.EmployeeId,
                            EmployeeName = e.FirstName + " " + e.LastName,
                            Designation = d.DesignationName,
                            HolidayDecType = ht.Name,
                            Date = h.Date,
                            InPlaceofDate = h.InPlaceofDate,
                            IsDayoffDuty = h.IsDayoffDuty,
                            Remark = h.Remark,
                            CompanyCode = h.CompanyCode,
                            EntryUser = h.Luser
                        };

            return await query.OrderBy(h => h.Ehdid).ToListAsync();
        }
        public async Task<(List<HrmEmployeeHolidayDeclarationViewModel> Data, int TotalRecords)> GetPaginatedDataAsync(string searchValue, int page, int pageSize, string sortColumn, string sortDirection)
        {
            var query = from h in holidayRepository.All().AsNoTracking()
                        join e in employeeRepository.All().AsNoTracking() on h.EmployeeId equals e.EmployeeId
                        join ei in employeeOfficialInfoRepository.All().AsNoTracking() on e.EmployeeId equals ei.EmployeeId into eiJoin
                        from ei in eiJoin.DefaultIfEmpty()
                        join d in designationRepository.All() on ei.DesignationCode equals d.DesignationCode into dJoin
                        from d in dJoin.DefaultIfEmpty()
                        join ht in HrmDefHolidayDeclarationType.All() on h.Hdtcode equals ht.Hdtcode into htJoin
                        from ht in htJoin.DefaultIfEmpty()
                        select new HrmEmployeeHolidayDeclarationViewModel
                        {
                            AutoId = h.AutoId,
                            Ehdid = h.Ehdid,
                            EmployeeId = h.EmployeeId,
                            EmployeeName = e.FirstName + " " + e.LastName,
                            Designation = d.DesignationName,
                            HolidayDecType = ht.Name,
                            Date = h.Date,
                            InPlaceofDate = h.InPlaceofDate,
                            IsDayoffDuty = h.IsDayoffDuty,
                            Remark = h.Remark,
                            CompanyCode = h.CompanyCode,
                            EntryUser = h.Luser
                        };

            var materializedQuery = await query.ToListAsync();

            IEnumerable<HrmEmployeeHolidayDeclarationViewModel> list = materializedQuery.ToList();

            if (!string.IsNullOrEmpty(searchValue))
            {
                list = list.Where(d => d.EmployeeName.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                                       d.Ehdid.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                                       d.Designation.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                                       d.HolidayDecType.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                                       d.Date.ToShortDateString().Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                                       d.CompanyCode.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                                       d.Remark.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                                       d.InPlaceofDate.Value.ToShortDateString().Contains(searchValue, StringComparison.OrdinalIgnoreCase)
                );
            }

            var totalRecords = list.Count();

            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortDirection))
            {
                list = sortColumn.ToLower() switch
                {
                    "ehdid" => sortDirection.ToLower() == "asc" ? list.OrderBy(a => a.Ehdid) : list.OrderByDescending(a => a.Ehdid),
                    "employeeid" => sortDirection.ToLower() == "asc" ? list.OrderBy(e => e.EmployeeId) : list.OrderByDescending(e => e.EmployeeId),
                    "employeename" => sortDirection.ToLower() == "asc" ? list.OrderBy(a => a.EmployeeName) : list.OrderByDescending(a => a.EmployeeName),
                    "designation" => sortDirection.ToLower() == "asc" ? list.OrderBy(a => a.Designation) : list.OrderByDescending(a => a.Designation),
                    "date" => sortDirection.ToLower() == "asc" ? list.OrderBy(a => a.Date) : list.OrderByDescending(h => h.Date),
                    "inplaceofdate" => sortDirection.ToLower() == "asc" ? list.OrderBy(i => i.InPlaceofDate) : list.OrderByDescending(i => i.InPlaceofDate),
                    "isdayoffduty" => sortDirection.ToLower() == "asc" ? list.OrderBy(i => i.IsDayoffDuty) : list.OrderByDescending(i => i.IsDayoffDuty),
                    "holidaydectype" => sortDirection.ToLower() == "asc" ? list.OrderBy(i => i.HolidayDecType) : list.OrderByDescending(i => i.HolidayDecType),
                    "remark" => sortDirection.ToLower() == "asc" ? list.OrderBy(r => r.Remark) : list.OrderByDescending(i => i.Remark),
                    "luser" => sortDirection.ToLower() == "asc" ? list.OrderBy(l => l.Luser) : list.OrderByDescending(l => l.Luser),
                    _ => list.OrderBy(a => a.AutoId)
                };
            }
            else
            {
                list = list.OrderBy(a => a.AutoId);
            }

            var data = pageSize < 0
                ? list.ToList()
                : list.Skip((page - 1) * pageSize).Take(pageSize).ToList();


            return (data, totalRecords);
        }
        public Task<List<HrmDefHolidayDeclarationTypeViewModel>> GetAllHolidayType()
        {
            var query = HrmDefHolidayDeclarationType.All().AsNoTracking()
                .Select(x => new HrmDefHolidayDeclarationTypeViewModel
                {
                    Hdtcode = x.Hdtcode,
                    Name = x.Name,
                });

            return query.ToListAsync();
        }

        public Task<bool> PagePermissionAsync(string accessCode)
        {
            throw new NotImplementedException();
        }

        public async Task<(bool isSuccess, string message, object data)> SaveAsync(HrmEmployeeHolidayDeclarationViewModel model)
        {
            if (model == null || model.Date == null || model.EmployeeIds == null || !model.EmployeeIds.Any() || string.IsNullOrWhiteSpace(model.HolidayDecType))
            {
                return (false, "Invalid data received: Missing dates or employee IDs.", null);
            }

            try
            {
                string isDayOff;
                string input = model.IsDayoffDuty?.Trim().ToLower();

                if (input == "true" || input == "yes" || input == "1")
                {
                    isDayOff = "Yes";
                }
                else
                {
                    isDayOff = "No";
                }

                var records = new List<HrmEmployeeHolidayDeclaration>();

                var lastHoliday = await holidayRepository.All().OrderByDescending(e => e.AutoId).FirstOrDefaultAsync();

                int nextId = 1;
                if (lastHoliday != null && !string.IsNullOrEmpty(lastHoliday.Ehdid))
                {
                    if (int.TryParse(lastHoliday.Ehdid, out int lastNumber))
                    {
                        nextId = lastNumber + 1;
                    }
                    else
                    {
                        Console.WriteLine($"Warning: Could not parse numeric part of last EWDId '{lastHoliday.Ehdid}'. Starting next ID from 1.");
                        nextId = 1;
                    }
                }

                foreach (var empId in model.EmployeeIds)
                {
                    var alreadyExists = await holidayRepository.All().Where(e => e.EmployeeId == empId && e.Date == model.Date).FirstOrDefaultAsync();

                    if (alreadyExists != null)
                    {
                        await holidayRepository.DeleteAsync(alreadyExists.Ehdid);
                    }

                    string newEhdId = $"{nextId:D8}";
                    nextId++;

                    records.Add(new HrmEmployeeHolidayDeclaration
                    {
                        Ehdid = newEhdId,
                        EmployeeId = empId,
                        Date = model.Date,
                        Remark = model.Remark ?? "",
                        Ldate = model.Ldate,
                        Luser = model.Luser,
                        Lip = model.Lip,
                        Lmac = model.Lmac,
                        CompanyCode = model.CompanyCode,
                        InPlaceofDate = model.InPlaceofDate != null
                            && model.InPlaceofDate > new DateTime(1971, 1, 1)
                            ? model.InPlaceofDate
                            : null,
                        IsDayoffDuty = isDayOff,
                        Hdtcode = model.HolidayDecType
                    });
                }
                if (!records.Any())
                {
                    return (false, "No valid records to save.", null);
                }

                await holidayRepository.AddRangeAsync(records);

                return (true, "Data saved successfully", new
                {
                    employeeCount = model.EmployeeIds.Count,
                    totalRecordsSaved = records.Count,
                    date = model.Date,
                    inPlaceOfDate = model.InPlaceofDate,
                    employeeIds = model.EmployeeIds,
                });
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"Database Update Error: {dbEx.Message}");
                if (dbEx.InnerException != null) Console.WriteLine($"Inner Exception: {dbEx.InnerException.Message}");
                return (false, "An error occurred while saving data to the database.", dbEx.InnerException?.Message ?? dbEx.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Error: {ex.Message}");
                if (ex.InnerException != null) Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                return (false, "An unexpected error occurred while saving data.", ex.Message);
            }
        }

        public async Task<(bool isSuccess, string message, object data)> EditAsync(HrmEmployeeHolidayDeclarationViewModel model)
        {
            if (model == null || model.Date == null || string.IsNullOrWhiteSpace(model.HolidayDecType))
            {
                return (false, "Invalid request: Missing required fields.", null);
            }

            await holidayRepository.BeginTransactionAsync();

            try
            {
                var existingRecord = await holidayRepository.GetByIdAsync(model.AutoId);

                if (existingRecord == null)
                {
                    return (false, $"Holiday declaration with ID {model.AutoId} not found.", null);
                }

                var duplicateRecord = await holidayRepository.All()
                    .Where(e => e.EmployeeId == model.EmployeeId && e.Date == model.Date && e.AutoId != model.AutoId)
                    .FirstOrDefaultAsync();

                if (duplicateRecord != null)
                {
                    await holidayRepository.DeleteAsync(duplicateRecord);
                }

                string isDayOff = model.IsDayoffDuty?.ToLower() == "true" ? "Yes" : "No";

                existingRecord.Date = model.Date;
                existingRecord.Remark = model.Remark ?? string.Empty;
                existingRecord.IsDayoffDuty = isDayOff;
                existingRecord.Hdtcode = model.HolidayDecType;
                existingRecord.InPlaceofDate = model.InPlaceofDate != null && model.InPlaceofDate > new DateTime(1971, 1, 1)
                    ? model.InPlaceofDate
                    : null;

                existingRecord.ModifyDate = model.ModifyDate ?? DateTime.Now;
                existingRecord.Luser = model.Luser;
                existingRecord.Lip = model.Lip;
                existingRecord.Lmac = model.Lmac;
                existingRecord.CompanyCode = model.CompanyCode;

                await holidayRepository.UpdateAsync(existingRecord);

                await holidayRepository.CommitTransactionAsync();

                return (true, "Holiday declaration updated successfully", new
                {
                    ehdid = existingRecord.Ehdid,
                    employeeId = existingRecord.EmployeeId,
                    date = existingRecord.Date,
                    inPlaceOfDate = existingRecord.InPlaceofDate,
                    holidayType = existingRecord.Hdtcode,
                    isDayOff = existingRecord.IsDayoffDuty
                });
            }
            catch (DbUpdateException dbEx)
            {
                await holidayRepository.RollbackTransactionAsync();
                Console.WriteLine($"Database Update Error: {dbEx.Message}");
                if (dbEx.InnerException != null) Console.WriteLine($"Inner Exception: {dbEx.InnerException.Message}");
                return (false, "Database error occurred while updating the record.", dbEx.InnerException?.Message ?? dbEx.Message);
            }
            catch (Exception ex)
            {
                await holidayRepository.RollbackTransactionAsync();
                Console.WriteLine($"General Error: {ex.Message}");
                if (ex.InnerException != null) Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                return (false, "An unexpected error occurred while updating the record.", ex.Message);
            }
        }


        public async Task<(bool isSuccess, string message, object data)> ProcessExcelFileAsync(Stream fileStream, string userName, string ip, string mac, string companyCode)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            try
            {
                using var package = new ExcelPackage(fileStream);
                var worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension.Rows;

                if (rowCount <= 1)
                    return (false, "The Excel file contains no data rows.", null);

                var validationErrors = new List<string>();
                var records = new List<HrmEmployeeHolidayDeclaration>();
                var processedEmployeeDates = new Dictionary<string, HrmEmployeeHolidayDeclaration>();

                var lastHoliday = await holidayRepository.All()
                    .OrderByDescending(e => e.AutoId)
                    .FirstOrDefaultAsync();
                int nextId = lastHoliday != null && int.TryParse(lastHoliday.Ehdid, out int lastNumber)
                    ? lastNumber + 1
                    : 1;

                var holidayTypeMap = await HrmDefHolidayDeclarationType.All()
                    .ToListAsync();

                var holidayTypeLookup = holidayTypeMap
                    .ToDictionary(t => t.Name.Trim(), t => t.Hdtcode.Trim(), StringComparer.OrdinalIgnoreCase);

                foreach (var type in holidayTypeMap)
                {
                    if (!holidayTypeLookup.ContainsKey(type.Hdtcode.Trim()))
                    {
                        holidayTypeLookup[type.Hdtcode.Trim()] = type.Hdtcode.Trim();
                    }
                }

                for (int row = 2; row <= rowCount; row++)
                {
                    var employeeId = worksheet.Cells[row, 1].Text?.Trim();
                    var dateStr = worksheet.Cells[row, 2].Text?.Trim();
                    var inPlaceOfDateStr = worksheet.Cells[row, 4].Text?.Trim();
                    var isDayOffDutyStr = worksheet.Cells[row, 3].Text?.Trim().ToLower();
                    var rawHolidayDecType = worksheet.Cells[row, 5].Text?.Trim();
                    var remark = worksheet.Cells[row, 6].Text?.Trim();

                    if (string.IsNullOrWhiteSpace(employeeId) && string.IsNullOrWhiteSpace(dateStr))
                        continue;

                    if (string.IsNullOrWhiteSpace(employeeId))
                    {
                        validationErrors.Add($"Row {row}: Employee ID is required.");
                        continue;
                    }

                    if (!DateTime.TryParse(dateStr, out DateTime date))
                    {
                        validationErrors.Add($"Row {row}: Invalid date format. Use YYYY-MM-DD format.");
                        continue;
                    }

                    DateTime? inPlaceOfDate = null;
                    if (!string.IsNullOrWhiteSpace(inPlaceOfDateStr))
                    {
                        if (DateTime.TryParse(inPlaceOfDateStr, out var parsedInPlace))
                            inPlaceOfDate = parsedInPlace;
                        else
                        {
                            validationErrors.Add($"Row {row}: Invalid in-place-of date format. Use YYYY-MM-DD format.");
                            continue;
                        }
                    }

                    bool isDayOffDuty = isDayOffDutyStr switch
                    {
                        "true" or "t" or "yes" or "y" or "1" => true,
                        _ => false
                    };

                    var alreadyExists = await holidayRepository.All().Where(e => e.EmployeeId == employeeId && e.Date.Date == date.Date)
                        .ToListAsync();
                    if (alreadyExists.Any())
                    {
                        await holidayRepository.DeleteRangeAsync(alreadyExists);
                    }

                    string holidayDecType = null;

                    if (!string.IsNullOrWhiteSpace(rawHolidayDecType) &&
                        holidayTypeLookup.TryGetValue(rawHolidayDecType.Trim(), out var mappedCode))
                    {
                        holidayDecType = mappedCode;
                    }
                    else
                    {
                        validationErrors.Add($"Row {row}: Holiday Type Code or Name '{rawHolidayDecType}' is invalid or missing.");
                        continue;
                    }

                    string newEhdId = $"{nextId:D8}";
                    nextId++;

                    var holidayRecord = (new HrmEmployeeHolidayDeclaration
                    {
                        Ehdid = newEhdId,
                        EmployeeId = employeeId,
                        Date = date,
                        Remark = remark ?? string.Empty,
                        Ldate = DateTime.Now,
                        Luser = userName,
                        Lip = ip,
                        Lmac = mac,
                        CompanyCode = companyCode,
                        InPlaceofDate = inPlaceOfDate,
                        IsDayoffDuty = isDayOffDuty ? "Yes" : "No",
                        Hdtcode = holidayDecType,
                    });

                    string uniqueKey = $"{employeeId}_{date:yyyy-MM-dd}";

                    if (processedEmployeeDates.ContainsKey(uniqueKey))
                    {
                        processedEmployeeDates[uniqueKey] = holidayRecord;
                    }
                    else
                    {
                        processedEmployeeDates.Add(uniqueKey, holidayRecord);
                    }
                }

                records = processedEmployeeDates.Values.ToList();

                if (!records.Any() && validationErrors.Any())
                {
                    return (false, "All records failed validation in Excel file.", new
                    {
                        totalValid = 0,
                        totalInvalid = validationErrors.Count,
                        validationErrors
                    });
                }

                if (records.Any())
                {
                    await holidayRepository.AddRangeAsync(records);
                    string message = $"{records.Count} record(s) added successfully.";

                    if (validationErrors.Any())
                        message += $" {validationErrors.Count} record(s) failed validation.";

                    return (true, message, new
                    {
                        totalValid = records.Count,
                        totalInvalid = validationErrors.Count,
                        validationErrors
                    });
                }

                return (false, "No valid records found in the Excel file.", null);
            }
            catch (Exception ex)
            {
                return (false, "An error occurred while processing the Excel file.", ex.Message);
            }
        }

        public async Task<byte[]> GenerateEmployeeHolidayDeclarationExcelAsync()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Holiday Declarations");

                worksheet.Cells[1, 1].Value = "Employee ID";
                worksheet.Cells[1, 2].Value = "Date (YYYY-MM-DD)";
                worksheet.Cells[1, 3].Value = "Is Day off Duty";
                worksheet.Cells[1, 4].Value = "In Place of Date (YYYY-MM-DD)";
                worksheet.Cells[1, 5].Value = "Holiday Type";
                worksheet.Cells[1, 6].Value = "Remarks";

                using (var range = worksheet.Cells[1, 1, 1, 6])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    range.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                }

                worksheet.Column(1).Style.Numberformat.Format = "@";
                worksheet.Column(5).Style.Numberformat.Format = "@";
                worksheet.Column(6).Style.Numberformat.Format = "@";

                worksheet.Cells[2, 1].Value = "00000000000";
                worksheet.Cells[2, 2].Value = DateTime.Now.ToString("yyyy-MM-dd");
                worksheet.Cells[2, 3].Value = "true";
                worksheet.Cells[2, 4].Value = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");
                worksheet.Cells[2, 5].Value = "Company Holiday";
                worksheet.Cells[2, 6].Value = "Sample remark";

                worksheet.Cells[3, 1].Value = "00000000000";
                worksheet.Cells[3, 2].Value = DateTime.Now.ToString("yyyy-MM-dd");
                worksheet.Cells[3, 3].Value = "false";
                worksheet.Cells[3, 4].Value = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");
                worksheet.Cells[3, 5].Value = "Roster Holiday";
                worksheet.Cells[3, 6].Value = "Another sample remark";

                worksheet.Cells.AutoFitColumns();

                return package.GetAsByteArray();
            }
        }

        public async Task<HrmEmployeeHolidayDeclarationViewModel> GetByIdAsync(decimal id)
        {
            var holiday = await holidayRepository.GetByIdAsync(id);

            if (holiday == null)
            {
                return null;
            }

            var holidayViewModel = new HrmEmployeeHolidayDeclarationViewModel
            {
                Ehdid = holiday.Ehdid,
                EmployeeId = holiday.EmployeeId,
                Date = holiday.Date,
                Remark = holiday.Remark ?? "",
                InPlaceofDate = holiday.InPlaceofDate,
                IsDayoffDuty = holiday.IsDayoffDuty,
                HolidayDecType = holiday.Hdtcode,
            };

            return holidayViewModel;
        }
    }
}
