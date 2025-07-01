using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Data;
using GCTL.Core.ViewModels.Companies;
using GCTL.Core.ViewModels.HrmWorkingDayDeclarations;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace GCTL.Service.HrmWorkingDayDeclarations
{
    public class HrmWorkingDayDeclarationService : AppService<HrmAttWorkingDayDeclaration>, IHrmWorkingDayDeclarationService
    {
        private readonly IRepository<HrmAttWorkingDayDeclaration> declarationRepository;
        private readonly IRepository<HrmEmployee> employeeRepository;
        private readonly IRepository<HrmEmployeeOfficialInfo> employeeOfficialInfoRepository;
        private readonly IRepository<HrmDefDesignation> designationRepository;

        private readonly IRepository<CoreCompany> companyRepository;
        private readonly IRepository<HrmDefDivision> divisionRepository;
        //private readonly IRepository<CoreAccessCode> accessCodeRepository;
        private readonly IRepository<CoreBranch> branchRepository;
        private readonly IRepository<HrmDefDepartment> departmentRepository;
        private readonly IRepository<HrmDefEmployeeStatus> employeeStatusRepository;
        private readonly IRepository<HrmDefEmpType> empTypeRepository;

        public HrmWorkingDayDeclarationService(
            IRepository<HrmAttWorkingDayDeclaration> declarationRepository,
            IRepository<HrmEmployee> employeeRepository,
            IRepository<HrmEmployeeOfficialInfo> employeeOfficialInfoRepository,
            IRepository<HrmDefDesignation> designationRepository,
            IRepository<CoreCompany> companyRepository,
            IRepository<HrmDefDivision> divisionRepository,
            //IRepository<CoreAccessCode> accessCodeRepository,
            IRepository<CoreBranch> branchRepository,
            IRepository<HrmDefDepartment> departmentRepository,
            IRepository<HrmDefEmployeeStatus> employeeStatusRepository,
            IRepository<HrmDefEmpType> empTypeRepository
        )
            : base(declarationRepository)
        {
            this.declarationRepository = declarationRepository;
            this.employeeRepository = employeeRepository;
            this.employeeOfficialInfoRepository = employeeOfficialInfoRepository;
            this.designationRepository = designationRepository;


            this.companyRepository = companyRepository;
            this.divisionRepository = divisionRepository;
            //this.accessCodeRepository = accessCodeRepository;
            this.branchRepository = branchRepository;
            this.departmentRepository = departmentRepository;
            this.employeeStatusRepository = employeeStatusRepository;
            this.empTypeRepository = empTypeRepository;
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

            query = query.Where(x => x.CompanyCode == "001");

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

            if (filter.EmployeeStatuses?.Any() != true)
                filter.EmployeeStatuses = new List<string> { "Active" };  // Default to "Active"

            if (filter.EmployeeStatuses?.Any() == true)
                query = query.Where(x => filter.EmployeeStatuses.Contains(x.EmployeeStatusName));

            var filteredData = await query.ToListAsync();

            result.Employees = filteredData.Select(x => new EmployeeListItemViewModel
            {
                EmployeeId = x.EmployeeId,
                EmployeeName = string.Join(" ", new[] { x.FirstName, x.LastName }.Where(n => !string.IsNullOrWhiteSpace(n))),
                JoiningDate = x.JoiningDate.HasValue ? x.JoiningDate.Value.ToString("dd/MM/yyyy") : "",
                DesignationName = x.DesignationName ?? "",
                DepartmentName = x.DepartmentName ?? "",
                BranchName = x.BranchName ?? "",
                CompanyName = x.CompanyName ?? "",
                EmployeeTypeName = x.EmployeeTypeName ?? "",
                EmployeeStatus = x.EmployeeStatusName ?? ""
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
            const int batchSize = 1000;
            await declarationRepository.BeginTransactionAsync();

            try
            {
                for(int i=0;i<ids.Count; i += batchSize)
                {
                    var batch= ids.Skip(i).Take(batchSize).ToList();

                    var declaration = await declarationRepository.All()
                        .Where(c => ids.Contains(c.Tc))
                        .AsNoTracking()
                        .ToListAsync();

                    if (declaration.Any())
                    {
                         await declarationRepository.DeleteRangeAsync(declaration); 
                    }
                }
                
                await declarationRepository.CommitTransactionAsync();
                return true;
            }
            catch (Exception ex)
            {
                await declarationRepository.RollbackTransactionAsync();
                Console.WriteLine($"Bulk delete error: {ex}");
                return false;
            }
        }

        public async Task<bool> DeleteDeclarationByDateAndEmployee(DateTime date, List<string> ids)
        {
            try
            {
                await declarationRepository.BeginTransactionAsync();

                var declarations = declarationRepository.All()
                    .Where(h => h.WorkingDayDate.HasValue &&
                                h.WorkingDayDate.Value.Date == date.Date &&
                                ids.Contains(h.EmployeeId))
                    .ToList();


                if (declarations == null || declarations.Count == 0)
                {
                    await declarationRepository.BeginTransactionAsync();
                    return false;
                }

                await declarationRepository.DeleteRangeAsync(declarations);

                await declarationRepository.CommitTransactionAsync();

                return true;
            }
            catch (Exception ex)
            {
                await declarationRepository.RollbackTransactionAsync();

                return false;
            }
        }

        public async Task<(bool isSuccess, string message, object data)> EditAsync(HrmWorkingDayDeclarationViewModel model)
        {
            var t = model.Tc;


            await declarationRepository.BeginTransactionAsync();

            try
            {
                var existingRecord = await declarationRepository.GetByIdAsync(model.Tc);

                if (existingRecord == null)
                {
                    return (false, $"Working Day Declaration with ID {model.Tc} not found.", null);
                }

                if (!DateTime.TryParseExact(model.WorkingDayDates.First().Trim(), "dd-MM-yyyy", null, DateTimeStyles.None, out DateTime workingDate))
                {
                    Console.WriteLine($"Warning: Invalid date format '{model.WorkingDayDates.First()}'. Skipping.");
                    return (false, $"Working Day Declaration Date {model.WorkingDayDates.First()} not found.", null);
                }

                workingDate = workingDate.Date;

                var duplicateRecord = await declarationRepository.All()
                    .Where(e => model.EmployeeIds.Contains(e.EmployeeId) && e.WorkingDayDate.Value.Date == workingDate && e.Tc != model.Tc).ToListAsync();



                if (duplicateRecord != null)
                {
                    await declarationRepository.DeleteRangeAsync(duplicateRecord);
                }


                existingRecord.WorkingDayDate = model.WorkingDayDates?.Any() == true
                    ? workingDate
                    : null;

                existingRecord.Remarks = model.Remarks ?? string.Empty;
                
                existingRecord.ModifyDate = model.ModifyDate ?? DateTime.Now;
                existingRecord.Luser = model.Luser;
                existingRecord.Lip = model.Lip;
                existingRecord.Lmac = model.Lmac;
                existingRecord.CompanyCode = model.CompanyCode;

                await declarationRepository.UpdateAsync(existingRecord);

                await declarationRepository.CommitTransactionAsync();

                return (true, "Working Day Declaration updated successfully", new
                {
                    ehdid = existingRecord.Tc,
                    employeeId = existingRecord.EmployeeId,
                    date = existingRecord.WorkingDayDate
                });
            }
            catch (DbUpdateException dbEx)
            {
                await declarationRepository.RollbackTransactionAsync();
                Console.WriteLine($"Database Update Error: {dbEx.Message}");
                if (dbEx.InnerException != null) Console.WriteLine($"Inner Exception: {dbEx.InnerException.Message}");
                return (false, "Database error occurred while updating the record.", dbEx.InnerException?.Message ?? dbEx.Message);
            }
            catch (Exception ex)
            {
                await declarationRepository.RollbackTransactionAsync();
                Console.WriteLine($"General Error: {ex.Message}");
                if (ex.InnerException != null) Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                return (false, "An unexpected error occurred while updating the record.", ex.Message);
            }
        }

        public async Task<List<HrmWorkingDayDeclarationViewModel>> GetAllAsync()
        {
            var query = from d in declarationRepository.All().AsNoTracking()
                        join e in employeeRepository.All().AsNoTracking() on d.EmployeeId equals e.EmployeeId
                        join ei in employeeOfficialInfoRepository.All().AsNoTracking() on e.EmployeeId equals ei.EmployeeId into eiJoin
                        from ei in eiJoin.DefaultIfEmpty()
                        join de in designationRepository.All() on ei.DesignationCode equals de.DesignationCode into dJoin
                        from de in dJoin.DefaultIfEmpty()
                        
                        select new HrmWorkingDayDeclarationViewModel
                        {
                            Tc = d.Tc,
                            WorkingDayCode = d.WorkingDayCode,
                            EmployeeId = d.EmployeeId,
                            EmployeeName = e.FirstName + " " + e.LastName,
                            Designation = de.DesignationName,
                            WorkingDayDate = d.WorkingDayDate,
                            Remarks = d.Remarks,
                            CompanyCode = d.CompanyCode,
                            Luser = d.Luser
                        };

            return await query.OrderBy(h => h.WorkingDayCode).ToListAsync();
        }

        public async Task<HrmWorkingDayDeclarationViewModel> GetByIdAsync(decimal id)
        {
            var holiday = await declarationRepository.GetByIdAsync(id);

            if (holiday == null)
            {
                return null;
            }

            var holidayViewModel = new HrmWorkingDayDeclarationViewModel
            {
                Tc = holiday.Tc,
                WorkingDayCode = holiday.WorkingDayCode,
                EmployeeId = holiday.EmployeeId,
                WorkingDayDate = holiday.WorkingDayDate,
                Remarks = holiday.Remarks ?? "",
            };

            return holidayViewModel;
        }

        
        public async Task<(List<HrmWorkingDayDeclarationViewModel> Data, int TotalRecords)> GetPaginatedDataAsync(string searchValue, int page, int pageSize, string sortColumn, string sortDirection)
        {
            var query = from d in declarationRepository.All().AsNoTracking()
                        join e in employeeRepository.All().AsNoTracking() on d.EmployeeId equals e.EmployeeId
                        join ei in employeeOfficialInfoRepository.All().AsNoTracking() on e.EmployeeId equals ei.EmployeeId into eiJoin
                        from ei in eiJoin.DefaultIfEmpty()
                        join de in designationRepository.All() on ei.DesignationCode equals de.DesignationCode into dJoin
                        from de in dJoin.DefaultIfEmpty()
                        join dp in departmentRepository.All() on ei.DepartmentCode equals dp.DepartmentCode into dpJoin 
                        from dp in dpJoin.DefaultIfEmpty()

                        select new HrmWorkingDayDeclarationViewModel
                        {
                            Tc = d.Tc,
                            WorkingDayCode = d.WorkingDayCode,
                            Code = d.EmployeeId,
                            EmployeeId = d.EmployeeId,
                            EmployeeName = e.FirstName + " " + e.LastName,
                            Designation = de.DesignationName,
                            Department = dp.DepartmentName,
                            WorkingDayDate = d.WorkingDayDate,
                            Remarks = d.Remarks,
                            CompanyCode = d.CompanyCode,
                            Luser = d.Luser
                        };

            var materializedQuery = await query.ToListAsync();

            IEnumerable<HrmWorkingDayDeclarationViewModel> list = materializedQuery;


            if (!string.IsNullOrEmpty(searchValue))
            {
                list = list.Where(d =>
                    (d.EmployeeName != null && d.EmployeeName.Contains(searchValue, StringComparison.OrdinalIgnoreCase)) ||
                    (d.EmployeeId != null && d.Code.Contains(searchValue, StringComparison.OrdinalIgnoreCase)) ||
                    (d.Designation != null && d.Designation.Contains(searchValue, StringComparison.OrdinalIgnoreCase)) ||
                    (d.Department != null && d.Department.Contains(searchValue, StringComparison.OrdinalIgnoreCase)) ||
                    (d.WorkingDayDate.HasValue && d.WorkingDayDate.Value.ToString("dd/MM/yyyy").Contains(searchValue, StringComparison.OrdinalIgnoreCase)) ||
                    (d.CompanyCode != null && d.CompanyCode.Contains(searchValue, StringComparison.OrdinalIgnoreCase)) ||
                    (d.Remarks != null && d.Remarks.Contains(searchValue, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }


            var totalRecords = list.Count();

            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortDirection))
            {
                list = sortColumn.ToLower() switch
                {
                    "workingdaycode" => sortDirection.ToLower() == "asc" ? list.OrderBy(a => a.WorkingDayCode) : list.OrderByDescending(a => a.WorkingDayCode),
                    "employeeid" => sortDirection.ToLower() == "asc" ? list.OrderBy(e => e.EmployeeId) : list.OrderByDescending(e => e.EmployeeId),
                    "employeename" => sortDirection.ToLower() == "asc" ? list.OrderBy(a => a.EmployeeName) : list.OrderByDescending(a => a.EmployeeName),
                    "designation" => sortDirection.ToLower() == "asc" ? list.OrderBy(a => a.Designation) : list.OrderByDescending(a => a.Designation),
                    "department" => sortDirection.ToLower() == "asc" ? list.OrderBy(a => a.Department) : list.OrderByDescending(h => h.Department),
                    "workingdaydate" => sortDirection.ToLower() == "asc" ? list.OrderBy(i => i.WorkingDayDate) : list.OrderByDescending(i => i.WorkingDayDate),
                    "remarks" => sortDirection.ToLower() == "asc" ? list.OrderBy(r => r.Remarks) : list.OrderByDescending(i => i.Remarks),
                    "luser" => sortDirection.ToLower() == "asc" ? list.OrderBy(l => l.Luser) : list.OrderByDescending(l => l.Luser),
                    _ => list.OrderBy(a => a.Tc)
                };
            }
            else
            {
                list = list.OrderBy(a => a.Tc);
            }

            var data = pageSize < 0
                ? list.ToList()
                : list.Skip((page - 1) * pageSize).Take(pageSize).ToList();


            return (data, totalRecords);

        }

        public async Task<(bool isSuccess, string message, object data)> SaveAsync(HrmWorkingDayDeclarationViewModel model)
        {
            if (model == null || !model.WorkingDayDates.Any() || model.EmployeeIds == null || !model.EmployeeIds.Any())
            {
                return (false, "Invalid data received: Missing dates or employee IDs.", null);
            }

            try
            {
                var parsedDates = new List<DateTime>();
                foreach (var dateStr in model.WorkingDayDates)
                {
                    if (DateTime.TryParseExact(dateStr.Trim(), "dd-MM-yyyy", null, DateTimeStyles.None, out DateTime workingDate))
                    {
                        parsedDates.Add(workingDate.Date);
                    }
                    else
                    {
                        Console.WriteLine($"Warning: Invalid date format '{dateStr}'. Skipping.");
                    }
                }

                if (!parsedDates.Any())
                {
                    return (false, "No valid dates found.", null);
                }

                var lastRecord = await declarationRepository.All()
                    .OrderByDescending(e => e.WorkingDayCode)
                    .Select(e => e.WorkingDayCode)
                    .FirstOrDefaultAsync();

                int nextId = 1;
                if (lastRecord != null && int.TryParse(lastRecord, out int lastNumber))
                {
                    nextId = lastNumber + 1;
                }

                var minDate = parsedDates.Min();
                var maxDate = parsedDates.Max();

                var existingRecords = await declarationRepository.All()
                    .Where(e => model.EmployeeIds.Contains(e.EmployeeId) &&
                               e.WorkingDayDate.HasValue &&
                               e.WorkingDayDate.Value.Date >= minDate &&
                               e.WorkingDayDate.Value.Date <= maxDate)
                    .Where(e => parsedDates.Contains(e.WorkingDayDate.Value.Date))
                    .ToListAsync();

                if (existingRecords.Any())
                {
                    await declarationRepository.DeleteRangeAsync(existingRecords);
                }

                int totalRecords = model.EmployeeIds.Count * parsedDates.Count;
                var records = new List<HrmAttWorkingDayDeclaration>(totalRecords);

                // Generate all records in memory
                foreach (var empId in model.EmployeeIds)
                {
                    foreach (var workingDate in parsedDates)
                    {
                        string newCode = $"{nextId:D8}";
                        nextId++;

                        records.Add(new HrmAttWorkingDayDeclaration
                        {
                            WorkingDayCode = newCode,
                            EmployeeId = empId,
                            WorkingDayDate = workingDate, 
                            Remarks = model.Remarks,
                            Ldate = model.Ldate,
                            Luser = model.Luser,
                            Lip = model.Lip,
                            Lmac = model.Lmac,
                            CompanyCode = model.CompanyCode,
                        });
                    }
                }

                await declarationRepository.AddRangeAsync(records);

                return (true, "Saved successfully", new
                {
                    employeeCount = model.EmployeeIds.Count,
                    dateCount = parsedDates.Count,
                    totalRecordsSaved = records.Count,
                    workingDayDates = model.WorkingDayDates,
                    employeeIds = model.EmployeeIds
                });
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"Database Update Error: {dbEx.Message}");
                if (dbEx.InnerException != null)
                    Console.WriteLine($"Inner Exception: {dbEx.InnerException.Message}");

                return (false, "An error occurred while saving data to the database.",
                        dbEx.InnerException?.Message ?? dbEx.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Error: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");

                return (false, "An unexpected error occurred while saving data.", ex.Message);
            }
        }
    }
}
