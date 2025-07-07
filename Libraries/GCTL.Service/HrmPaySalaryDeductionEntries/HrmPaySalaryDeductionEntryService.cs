using DocumentFormat.OpenXml.Drawing;
using GCTL.Core.Data;
using GCTL.Core.ViewModels.HrmPayMonths;
using GCTL.Core.ViewModels.HrmPaySalaryDeductionEntries;

using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.HRM_PAY_SalaryDeductionEntry
{
    public class HrmPaySalaryDeductionEntryService : AppService<HrmPaySalaryDeductionEntry>, IHrmPaySalaryDeductionEntryService
    {
        private readonly IRepository<CoreCompany> companyRepository;
        private readonly IRepository<HrmDefDivision> divisionRepository;
        private readonly IRepository<CoreAccessCode> accessCodeRepository;
        private readonly IRepository<HrmEmployeeOfficialInfo> employeeOfficialInfoRepository;
        private readonly IRepository<CoreBranch> branchRepository;
        private readonly IRepository<HrmDefDepartment> departmentRepository;
        private readonly IRepository<HrmDefDesignation> designationRepository;
        private readonly IRepository<HrmEmployee> employeeRepository;
        private readonly IRepository<HrmDefEmployeeStatus> employeeStatusRepository;
        private readonly IRepository<HrmDefEmpType> empTypeRepository;
        private readonly IRepository<HrmEisDefEmploymentNature> employmentNatureRepository;
        private readonly IRepository<HrmPaySalaryDeductionEntry> deductionEntryRepository;
        private readonly IRepository<HrmPayMonth> payMonthRepository;
        private readonly IRepository<HrmPayDefDeductionType> deductionTypeRepository;


        public HrmPaySalaryDeductionEntryService(
            IRepository<CoreCompany> companyRepository,
            IRepository<HrmDefDivision> divisionRepository, 
            IRepository<CoreAccessCode> accessCodeRepository,
            IRepository<HrmEmployeeOfficialInfo> employeeOfficialInfoRepository, 
            IRepository<CoreBranch> branchRepository,
            IRepository<HrmDefDepartment> departmentRepository, 
            IRepository<HrmDefDesignation> designationRepository, 
            IRepository<HrmEmployee> employeeRepository, 
            IRepository<HrmDefEmployeeStatus> employeeStatusRepository,
            IRepository<HrmDefEmpType> empTypeRepository, 
            IRepository<HrmPaySalaryDeductionEntry> deductionEntryRepository, 
            IRepository<HrmPayMonth> payMonthRepository,
            IRepository<HrmEisDefEmploymentNature> employmentNatureRepository,
            IRepository<HrmPayDefDeductionType> deductionTypeRepository) : base(deductionEntryRepository)
        {
            this.companyRepository = companyRepository;
            this.divisionRepository = divisionRepository;
            this.accessCodeRepository = accessCodeRepository;
            this.employeeOfficialInfoRepository = employeeOfficialInfoRepository;
            this.branchRepository = branchRepository;
            this.departmentRepository = departmentRepository;
            this.designationRepository = designationRepository;
            this.employeeRepository = employeeRepository;
            this.employeeStatusRepository = employeeStatusRepository;
            this.empTypeRepository = empTypeRepository;
            this.deductionEntryRepository = deductionEntryRepository;
            this.payMonthRepository = payMonthRepository;
            this.employmentNatureRepository = employmentNatureRepository;
            this.deductionTypeRepository = deductionTypeRepository;
        }

        public async Task<EmployeeFilterResultDto> GetFilterEmployeeAsync(EmployeeFilterViewModel filter)
        {
            var result = new EmployeeFilterResultDto();

            var query = from e in employeeOfficialInfoRepository.All().AsNoTracking()
                        join emp in employeeRepository.All().AsNoTracking() on e.EmployeeId equals emp.EmployeeId
                        join c in companyRepository.All().AsNoTracking() on e.CompanyCode equals c.CompanyCode 
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
                        join empNature in employmentNatureRepository.All().AsNoTracking() on e.EmploymentNatureId equals empNature.EmploymentNatureId into empNatureGroup
                            from empNature in empNatureGroup.DefaultIfEmpty()
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
                            EmployeeStatusName = status.EmployeeStatus,
                            EmpNatureCode = e.EmploymentNatureId,
                            EmpNatureName = empNature.EmploymentNature
                        };

            query = query.Where(x => x.CompanyCode == "001");

            //if (filter.CompanyCodes == null || !filter.CompanyCodes.Any())
            //    return new EmployeeFilterResultDto();

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
            {
                filter.EmployeeStatuses = new List<string> { "Active" };  // Default to "Active"
            }

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
                //CompanyName = x.CompanyName,
                EmployeeTypeName = x.EmployeeTypeName,
                EmployeeStatus = x.EmployeeStatusName,
                EmploymentNature = x.EmpNatureName ?? " "
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

            //result.LookupData["divisions"] = filteredData
            //    .Where(x => x.DivisionCode != null && x.DivisionName != null)
            //    .Select(x => new LookupItemDto { Code = x.DivisionCode, Name = x.DivisionName })
            //    .Distinct()
            //    .ToList();

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

        public async Task<List<HrmPayMonthViewModel>> GetPayMonthsAsync()
        {
            var result = await payMonthRepository.All().Select(x=>new HrmPayMonthViewModel
            {
                MonthId = x.MonthId,
                MonthName = x.MonthName,
            }).ToListAsync();

            return result;
        }

        public async Task<List<HrmPayDefDeductionType>> GetDeductionTypeAsync()
        {
            var result = await deductionTypeRepository.All().Select(x => new HrmPayDefDeductionType
            {
                DeductionTypeId = x.DeductionTypeId,
                DeductionType = x.DeductionType,
            }).ToListAsync();

            return result;
        }

        public async Task<string> GenerateDeductionIdAsync()
        {
            var lastEntry = await deductionEntryRepository.All()
                .OrderByDescending(x => x.AutoId)
                .FirstOrDefaultAsync();

            int lastNumber = 0;

            if (lastEntry != null && !string.IsNullOrEmpty(lastEntry.Id))
            {
                int.TryParse(lastEntry.Id, out lastNumber);
            }

            int newNumber = lastNumber + 1;

            string newId = newNumber.ToString("D8");

            return newId;
        }
        public async Task<(List<HrmPaySalaryDeductionEntryViewModel> Data, int TotalRecords)> GetPaginatedDataAsync(string searchValue, int page, int pageSize, string sortColumn, string sortDirection)
        {
            var query = from d in deductionEntryRepository.All().AsNoTracking()
                        join e in employeeRepository.All().AsNoTracking() on d.EmployeeId equals e.EmployeeId
                        join ei in employeeOfficialInfoRepository.All().AsNoTracking() on e.EmployeeId equals ei.EmployeeId into eiJoin
                        from ei in eiJoin.DefaultIfEmpty()
                        join de in designationRepository.All() on ei.DesignationCode equals de.DesignationCode into deJoin
                        from de in deJoin.DefaultIfEmpty()
                        join m in payMonthRepository.All() on d.SalaryMonth equals m.MonthId.ToString() into mJoin
                        from m in mJoin.DefaultIfEmpty()
                        join t in deductionTypeRepository.All() on d.DeductionTypeId equals t.DeductionTypeId into tJoin
                        from t in tJoin.DefaultIfEmpty()
                        select new HrmPaySalaryDeductionEntryViewModel
                        {
                            AutoId = d.AutoId,
                            Id = d.Id,
                            EmployeeId = d.EmployeeId,
                            EmployeeName = e.FirstName + " " + e.LastName,
                            Designation = de.DesignationName,
                            DeductionType = t.DeductionType,
                            DeductionAmount = d.DeductionAmount,
                            SalaryMonth = m.MonthName,
                            SalaryYear = d.SalaryYear,
                            Remarks = d.Remarks,
                            Luser = d.Luser
                        };

            // Force the initial query evaluation to create concrete objects
            var materializedQuery = await query.ToListAsync();

            // Now perform filtering on the in-memory collection
            IEnumerable<HrmPaySalaryDeductionEntryViewModel> filteredQuery = materializedQuery;

            if (!string.IsNullOrEmpty(searchValue))
            {
                filteredQuery = filteredQuery.Where(d =>
                    (d.EmployeeName?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (d.Id?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (d.EmployeeId?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (d.Designation?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (d.DeductionType?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    d.DeductionAmount.ToString().Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                    (d.SalaryMonth?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (d.SalaryYear?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (d.Remarks?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (d.Luser?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ?? false)
                );
            }

            var totalRecords = filteredQuery.Count();

            // Apply sorting
            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortDirection))
            {
                filteredQuery = sortColumn.ToLower() switch
                {
                    "id" => sortDirection.ToLower() == "asc" ? filteredQuery.OrderBy(a => a.Id) : filteredQuery.OrderByDescending(a => a.Id),
                    "employeeid" => sortDirection.ToLower() == "asc" ? filteredQuery.OrderBy(a => a.EmployeeId) : filteredQuery.OrderByDescending(a => a.EmployeeId),
                    "employeename" => sortDirection.ToLower() == "asc" ? filteredQuery.OrderBy(a => a.EmployeeName) : filteredQuery.OrderByDescending(a => a.EmployeeName),
                    "designation" => sortDirection.ToLower() == "asc" ? filteredQuery.OrderBy(a => a.Designation) : filteredQuery.OrderByDescending(a => a.Designation),
                    "deductiontype" => sortDirection.ToLower() == "asc" ? filteredQuery.OrderBy(a => a.DeductionType) : filteredQuery.OrderByDescending(a => a.DeductionType),
                    "deductionamount" => sortDirection.ToLower() == "asc" ? filteredQuery.OrderBy(a => a.DeductionAmount) : filteredQuery.OrderByDescending(a => a.DeductionAmount),
                    "salarymonth" => sortDirection.ToLower() == "asc" ? filteredQuery.OrderBy(a => a.SalaryMonth) : filteredQuery.OrderByDescending(a => a.SalaryMonth),
                    "salaryyear" => sortDirection.ToLower() == "asc" ? filteredQuery.OrderBy(a => a.SalaryYear) : filteredQuery.OrderByDescending(a => a.SalaryYear),
                    "remarks" => sortDirection.ToLower() == "asc" ? filteredQuery.OrderBy(a => a.Remarks) : filteredQuery.OrderByDescending(a => a.Remarks),
                    "luser" => sortDirection.ToLower() == "asc" ? filteredQuery.OrderBy(a => a.Luser) : filteredQuery.OrderByDescending(a => a.Luser),
                    _ => filteredQuery.OrderBy(a => a.AutoId)
                };
            }
            else
            {
                filteredQuery = filteredQuery.OrderBy(a => a.AutoId);
            }

            // Apply pagination
            var data = pageSize < 0
                ? filteredQuery.ToList()
                : filteredQuery.Skip((page - 1) * pageSize).Take(pageSize).ToList();


            return (data, totalRecords);
        }
        public async Task<List<HrmPaySalaryDeductionEntryViewModel>> GetAllAsync()
        {
            var query = from d in deductionEntryRepository.All().AsNoTracking()
                        join e in employeeRepository.All().AsNoTracking() on d.EmployeeId equals e.EmployeeId
                        join ei in employeeOfficialInfoRepository.All().AsNoTracking() on e.EmployeeId equals ei.EmployeeId into eiJoin
                        from ei in eiJoin.DefaultIfEmpty()
                        join de in designationRepository.All() on ei.DesignationCode equals de.DesignationCode into deJoin
                        from de in deJoin.DefaultIfEmpty()
                        join m in payMonthRepository.All() on d.SalaryMonth equals m.MonthId.ToString() into mJoin
                        from m in mJoin.DefaultIfEmpty()
                        join t in deductionTypeRepository.All() on d.DeductionTypeId equals t.DeductionTypeId into tJoin
                        from t in tJoin.DefaultIfEmpty()
                        select new HrmPaySalaryDeductionEntryViewModel
                        {
                            AutoId = d.AutoId,
                            Id = d.Id,
                            EmployeeId = d.EmployeeId,
                            EmployeeName = e.FirstName + " " + e.LastName,
                            Designation = de.DesignationName,
                            DeductionType = t.DeductionType,
                            DeductionAmount = d.DeductionAmount,
                            SalaryMonth = m.MonthName,
                            SalaryYear = d.SalaryYear,
                            Remarks = d.Remarks,
                            Luser = d.Luser
                        };

            return await query.OrderBy(d=>d.Id).ToListAsync();
        }

        public async Task<HrmPaySalaryDeductionEntryViewModel> GetByIdAsync(decimal id)
        {
            var deduction = await deductionEntryRepository.GetByIdAsync(id);

            if (deduction == null) { return null; }

            var deductionViewModel = new HrmPaySalaryDeductionEntryViewModel
            {
                AutoId = deduction.AutoId,
                Id=deduction.Id,
                EmployeeId=deduction.EmployeeId,
                DeductionTypeId = deduction.DeductionTypeId,
                DeductionAmount=deduction.DeductionAmount,
                SalaryMonth=deduction.SalaryMonth,
                SalaryYear=deduction.SalaryYear,
                Remarks=deduction.Remarks,
                
            };
            return deductionViewModel;
        }

        public async Task<bool> SaveAsync(HrmPaySalaryDeductionEntryViewModel model)
        {
            if (model == null || model.EmployeeIds == null || !model.EmployeeIds.Any())
                return false;

            try
            {
                // STEP 1: Determine month-year pairs
                List<(string Month, string Year)> monthsToProcess = new();
                if (!string.IsNullOrEmpty(model.SalaryMonth) && !string.IsNullOrEmpty(model.SalaryYear))
                {
                    monthsToProcess.Add((model.SalaryMonth, model.SalaryYear));
                }
                else if (model.DateForm != default && model.DateTo != default)
                {
                    DateTime start = new DateTime(model.DateForm.Year, model.DateForm.Month, 1);
                    DateTime end = new DateTime(model.DateTo.Value.Year, model.DateTo.Value.Month, 1).AddMonths(1);
                    while (start < end)
                    {
                        monthsToProcess.Add((start.Month.ToString(), start.Year.ToString()));
                        start = start.AddMonths(1);
                    }
                }
                else
                {
                    return false;
                }

                // STEP 2: Get the last ID only once
                int nextId = 1;
                var last = await deductionEntryRepository.All().OrderByDescending(e => e.AutoId).FirstOrDefaultAsync();
                if (last != null && int.TryParse(last.Id, out int lastNumber)) nextId = lastNumber + 1;

                // STEP 3: Delete all existing entries for the employee/month/year combinations in bulk
                var empIds = model.EmployeeIds;
                var months = monthsToProcess.Select(m => m.Month).ToList();
                var years = monthsToProcess.Select(m => m.Year).ToList();


                var entriesToDelete = await deductionEntryRepository.All()
                    .Where(e => empIds.Contains(e.EmployeeId) && months.Contains(e.SalaryMonth) && years.Contains(e.SalaryYear) && e.DeductionTypeId == model.DeductionType)
                    .ToListAsync();

                if (entriesToDelete.Any())
                    await deductionEntryRepository.DeleteRangeAsync(entriesToDelete);

                // STEP 4: Build records efficiently
                List<HrmPaySalaryDeductionEntry> records = new();

                foreach (var (month, year) in monthsToProcess)
                {
                    foreach (var empId in model.EmployeeIds)
                    {
                        string id = $"{nextId:D8}";
                        nextId++;

                        records.Add(new HrmPaySalaryDeductionEntry
                        {
                            Id = id,
                            EmployeeId = empId,
                            SalaryMonth = month,
                            SalaryYear = year,
                            DeductionAmount = model.DeductionAmount,
                            DeductionTypeId = model.DeductionType,
                            Remarks = model.Remarks,
                            Ldate = model.Ldate,
                            Lip = model.Lip,
                            Lmac = model.Lmac,
                            Luser = model.Luser,
                            CompanyCode = model.CompanyCode
                        });
                    }
                }

                // STEP 5: Insert in batches
                const int batchSize = 10000;
                for (int i = 0; i < records.Count; i += batchSize)
                {
                    var batch = records.Skip(i).Take(batchSize).ToList();
                    await deductionEntryRepository.AddRangeAsync(batch);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }

        //public async Task<bool> SaveAsync(HrmPaySalaryDeductionEntryViewModel model)
        //{
        //    if (model == null || model.EmployeeIds == null || !model.EmployeeIds.Any())
        //    { 
        //        return false;
        //    }

        //    try
        //    {
        //        var records = new List<HrmPaySalaryDeductionEntry>();


        //        int nextId = 1;
        //        var lastHoliday = await deductionEntryRepository.All().OrderByDescending(e => e.AutoId).FirstOrDefaultAsync();

        //        if (lastHoliday != null && !string.IsNullOrEmpty(lastHoliday.Id))
        //        {
        //            if (int.TryParse(lastHoliday.Id, out int lastNumber))
        //            {
        //                nextId = lastNumber + 1;
        //            }
        //            else
        //            {
        //                nextId = 1;
        //            }
        //        }

        //        List<(string month, string year)> monthsToProcess = new List<(string month, string year)>();

        //        if(!string.IsNullOrEmpty(model.SalaryMonth) && !string.IsNullOrEmpty(model.SalaryYear))
        //        {
        //            monthsToProcess.Add((model.SalaryMonth, model.SalaryYear));
        //        }
        //        else if (model.DateForm != default && model.DateTo != default)
        //        {
        //            DateTime startDate = new DateTime(model.DateForm.Year, model.DateForm.Month, 1);
        //            DateTime endDate = new DateTime(model.DateTo.Value.Year, model.DateTo.Value.Month, 1).AddMonths(1);

        //            while (startDate < endDate)
        //            {
        //                monthsToProcess.Add((startDate.Month.ToString(), startDate.Year.ToString()));
        //                startDate = startDate.AddMonths(1);
        //            }
        //        }
        //        else
        //        {
        //            return false;
        //        }

        //        foreach (var (month, year) in monthsToProcess)
        //        {
        //            foreach (var empId in model.EmployeeIds)
        //            {
        //                var alreadyExists = await deductionEntryRepository.All().Where(e => e.EmployeeId == empId && e.SalaryMonth == model.SalaryMonth && e.SalaryYear == model.SalaryYear).FirstOrDefaultAsync();

        //                if (alreadyExists != null)
        //                {
        //                    await deductionEntryRepository.DeleteAsync(alreadyExists.Id);
        //                }

        //                string Id = $"{nextId:D8}";
        //                nextId++;

        //                records.Add(new HrmPaySalaryDeductionEntry
        //                {
        //                    Id = Id,
        //                    EmployeeId = empId,
        //                    SalaryMonth = month,
        //                    SalaryYear = year,
        //                    DeductionAmount = model.DeductionAmount,
        //                    DeductionTypeId = model.DeductionType,
        //                    Remarks = model.Remarks,
        //                    Ldate = model.Ldate,
        //                    Lip = model.Lip,
        //                    Lmac = model.Lmac,
        //                    Luser = model.Luser,
        //                    CompanyCode = model.CompanyCode
        //                });
        //            }
        //        }
        //        if (!records.Any())
        //        {
        //            return false;
        //        }
        //        var abc=records.Count;
        //        await deductionEntryRepository.AddRangeAsync(records);

        //        return true;
        //    }
        //    catch (DbUpdateException dbEx)
        //    {
        //        Console.WriteLine($"Database Update Error: {dbEx.Message}");
        //        if (dbEx.InnerException != null) Console.WriteLine($"Inner Exception: {dbEx.InnerException.Message}");
        //        return false;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"General Error: {ex.Message}");
        //        if (ex.InnerException != null) Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
        //        return false;
        //    }
        //}

        public async Task<bool> EditAsync(HrmPaySalaryDeductionEntryViewModel model)
        {
            if (model == null || model.AutoId == 0 || model.Id ==null)
            {
                return false;
            }
            await deductionEntryRepository.BeginTransactionAsync();

            try
            {
                var existingRecord = await deductionEntryRepository.GetByIdAsync(model.AutoId);

                if (existingRecord == null)
                {
                    return false;
                }

                var duplicateRecord = await deductionEntryRepository.All()
                    .Where(e => e.EmployeeId == model.EmployeeId && e.SalaryMonth == model.SalaryMonth && e.SalaryYear == model.SalaryYear && e.Id != model.Id && e.DeductionTypeId == model.DeductionTypeId)
                    .ToListAsync();

                if (duplicateRecord != null)
                {
                    await deductionEntryRepository.DeleteRangeAsync(duplicateRecord);
                }

                existingRecord.SalaryMonth = model.SalaryMonth;
                existingRecord.SalaryYear = model.SalaryYear;
                existingRecord.DeductionAmount = model.DeductionAmount;
                existingRecord.DeductionTypeId = model.DeductionType;
                existingRecord.Remarks = model.Remarks;
                existingRecord.ModifyDate = model.ModifyDate ?? DateTime.Now;
                existingRecord.Lip = model.Lip;
                existingRecord.Lmac = model.Lmac;
                existingRecord.Luser = model.Luser;
                existingRecord.CompanyCode = model.CompanyCode;

                await deductionEntryRepository.UpdateAsync(existingRecord);

                await deductionEntryRepository.CommitTransactionAsync();

                return true;
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"Database Update Error: {dbEx.Message}");
                if (dbEx.InnerException != null) Console.WriteLine($"Inner Exception: {dbEx.InnerException.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Error: {ex.Message}");
                if (ex.InnerException != null) Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                return false;
            }
        }

        public async Task<bool> BulkDeleteAsync(List<decimal> ids)
        {
            const int batchSize = 1000;
            await deductionEntryRepository.BeginTransactionAsync();

            try
            {
                for (int i = 0; i < ids.Count; i += batchSize)
                {
                    var batch = ids.Skip(i).Take(batchSize).ToList();

                    var deductionEntries = await deductionEntryRepository.All()
                        .Where(c => batch.Contains(c.AutoId))
                        .AsNoTracking()
                        .ToListAsync();

                    if (deductionEntries.Any())
                    {
                        await deductionEntryRepository.DeleteRangeAsync(deductionEntries);
                    }
                }

                await deductionEntryRepository.CommitTransactionAsync();
                return true;
            }
            catch (Exception ex)
            {
                await deductionEntryRepository.RollbackTransactionAsync();
                Console.WriteLine($"Bulk delete error: {ex}");
                return false;
            }
        }

        public async Task<byte[]> GenerateExcelSampleAsync()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Salary Deduction");

                worksheet.Cells[1, 1].Value = "Employee ID";
                worksheet.Cells[1, 2].Value = "Salary Month";
                worksheet.Cells[1, 3].Value = "Salary Year";
                worksheet.Cells[1, 4].Value = "Deduction Type";
                worksheet.Cells[1, 5].Value = "Deduction Amount";
                worksheet.Cells[1, 6].Value = "Remarks";

                using (var range = worksheet.Cells[1, 1, 1, 6])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    range.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                }

                worksheet.Column(1).Style.Numberformat.Format = "@";
                worksheet.Column(2).Style.Numberformat.Format = "@";
                worksheet.Column(3).Style.Numberformat.Format = "@";
                worksheet.Column(4).Style.Numberformat.Format = "@";
                worksheet.Column(5).Style.Numberformat.Format = "@";
                worksheet.Column(6).Style.Numberformat.Format = "@";

                worksheet.Cells[2, 1].Value = "00000000000";
                worksheet.Cells[2, 2].Value = DateTime.Now.ToString("MMMM");
                worksheet.Cells[2, 3].Value = DateTime.Now.ToString("yyyy");
                worksheet.Cells[2, 4].Value = "Salary";
                worksheet.Cells[2, 5].Value = "12500";
                worksheet.Cells[2, 6].Value = "Sample remark";

                worksheet.Cells[3, 1].Value = "00000000000";
                worksheet.Cells[3, 2].Value = DateTime.Now.ToString("MMMM");
                worksheet.Cells[3, 3].Value = DateTime.Now.ToString("yyyy");
                worksheet.Cells[3, 4].Value = "ID Card";
                worksheet.Cells[3, 5].Value = "500";
                worksheet.Cells[3, 6].Value = "Sample remark";

                worksheet.Cells.AutoFitColumns();

                return package.GetAsByteArray();
            }
        }
        
        public async Task<bool> ProcessExcleFileAsync(Stream fileStream, HrmPaySalaryDeductionEntryViewModel model) 
        { 
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            try
            {
                using var package = new ExcelPackage(fileStream);
                var worksheet = package.Workbook.Worksheets[0];

                int rowCount = worksheet.Dimension.Rows;
                if (rowCount <= 1)
                    return (false);

                var validationErrors = new List<string>();
                var records = new List<HrmPaySalaryDeductionEntry>();

                var lastDeduction = await deductionEntryRepository.All()
                    .OrderByDescending(e => e.AutoId)
                    .FirstOrDefaultAsync();
                int nextId = lastDeduction != null && int.TryParse(lastDeduction.Id, out int lastNumber)
                    ? lastNumber + 1
                    : 1;

                var deductionTypeMap = await deductionTypeRepository.All().ToListAsync();
                var deductionTypeLookup = deductionTypeMap.ToDictionary(t => t.DeductionType, t => t.DeductionTypeId.Trim(),StringComparer.OrdinalIgnoreCase);

                foreach(var type in deductionTypeMap)
                {
                    if (!deductionTypeLookup.ContainsKey(type.DeductionTypeId.Trim()))
                    {
                        deductionTypeLookup[type.DeductionTypeId.Trim()]=type.DeductionTypeId.Trim();
                    };
                }

                var salaryMonthMap = await payMonthRepository.All().ToListAsync();
                var salaryMonthLookup=salaryMonthMap.ToDictionary(t=>t.MonthName,t=>t.MonthId.ToString().Trim(),StringComparer.OrdinalIgnoreCase);

                foreach (var month in salaryMonthMap)
                {
                    var monthIdStr = month.MonthId.ToString().Trim();
                    if (!salaryMonthLookup.ContainsKey(monthIdStr))
                    {
                        salaryMonthLookup[monthIdStr] = monthIdStr;
                    }
                }

                for (int row = 2; row <= rowCount; row++)
                {
                    var employeeId = worksheet.Cells[row, 1].Text?.Trim();
                    var deductionAmountStr = worksheet.Cells[row, 5].Text?.Trim();
                    var rawDeductionType = worksheet.Cells[row, 4].Text?.Trim();
                    var rawSalaryMonth = worksheet.Cells[row, 2].Value?.ToString();
                    var salaryYear = worksheet.Cells[row, 3].Text?.Trim();
                    var remarks = worksheet.Cells[row, 6].Text?.Trim();

                    if (string.IsNullOrWhiteSpace(employeeId) ||
                        string.IsNullOrWhiteSpace(deductionAmountStr) ||
                        string.IsNullOrWhiteSpace(salaryYear))
                    {
                        validationErrors.Add($"Row {row}: All fields except remarks are required.");
                        continue;
                    }

                    string payMonth = null;
                    if (!string.IsNullOrWhiteSpace(rawSalaryMonth) && salaryMonthLookup.TryGetValue(rawSalaryMonth.Trim(), out var mappedCode))
                    {
                        payMonth = mappedCode;
                    }
                    else
                    {
                        continue;
                    }

                    string deductionType = null; 
                    if(!string.IsNullOrWhiteSpace(rawDeductionType) && deductionTypeLookup.TryGetValue(rawDeductionType.Trim(), out mappedCode))
                    {
                        deductionType = mappedCode;
                    }
                    else
                    {
                        continue; 
                    }

                    if (!decimal.TryParse(deductionAmountStr, out decimal deductionAmount))
                    {
                        validationErrors.Add($"Row {row}: Invalid deduction amount.");
                        continue;
                    }

                    var alreadyExists = await deductionEntryRepository.All()
                        .Where(e => e.EmployeeId == employeeId && e.SalaryMonth == payMonth && e.SalaryYear == salaryYear && e.DeductionTypeId == deductionType)
                        .FirstOrDefaultAsync();

                    if (alreadyExists != null)
                    {
                        await deductionEntryRepository.DeleteAsync(alreadyExists.AutoId);
                    }

                    string newId = $"{nextId:D8}";
                    nextId++;

                    records.Add(new HrmPaySalaryDeductionEntry
                    {
                        Id = newId,
                        EmployeeId = employeeId,
                        SalaryMonth = payMonth,
                        SalaryYear = salaryYear,
                        DeductionAmount = deductionAmount,
                        DeductionTypeId = deductionType,
                        Remarks = remarks ?? string.Empty,
                        Ldate = DateTime.Now,
                        Lip = model.Lip,
                        Lmac = model.Lmac,
                        Luser = model.Luser,
                        CompanyCode = model.CompanyCode,
                    });
                }

                if (!records.Any() && validationErrors.Any())
                {
                    return false;
                }

                if (records.Any())
                {
                    await deductionEntryRepository.AddRangeAsync(records);
                    string message = $"{records.Count} record(s) added successfully.";

                    if (validationErrors.Any())
                        message += $" {validationErrors.Count} record(s) failed validation.";

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}



