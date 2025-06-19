using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Data;
using GCTL.Core.ViewModels.HrmPayDefBenefitTypes;
using GCTL.Core.ViewModels.HrmPayMonths;
using GCTL.Core.ViewModels.HrmPayOthersAdjustmentEntries;
using GCTL.Data.Models;

using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace GCTL.Service.HrmPayOthersAdjustmentEntries
{
    public class HrmPayOthersAdjustmentEntryService:AppService<HrmPayOthersAdjustmentEntry>,IHrmPayOthersAdjustmentEntryService
    {
        private readonly IRepository<CoreCompany> companyRepository;
        private readonly IRepository<HrmDefDivision> divisionRepository;
        private readonly IRepository<HrmEmployeeOfficialInfo> employeeOfficialInfoRepository;
        private readonly IRepository<CoreBranch> branchRepository;
        private readonly IRepository<HrmDefDepartment> departmentRepository;
        private readonly IRepository<HrmDefDesignation> designationRepository;
        private readonly IRepository<HrmEmployee> employeeRepository;
        private readonly IRepository<HrmDefEmployeeStatus> employeeStatusRepository;
        private readonly IRepository<HrmDefEmpType> empTypeRepository;
        private readonly IRepository<HrmEisDefEmploymentNature> employmentNatureRepository;

        private readonly IRepository<HrmPayOthersAdjustmentEntry> adjustmentRepository;
        private readonly IRepository<HrmPayDefBenefitType> benefitTypeRepository;
        private readonly IRepository<HrmPayMonth> monthRepository;

        public HrmPayOthersAdjustmentEntryService(
            IRepository<CoreCompany> companyRepository,
            IRepository<HrmDefDivision> divisionRepository, 
            IRepository<HrmEmployeeOfficialInfo> employeeOfficialInfoRepository, 
            IRepository<CoreBranch> branchRepository, 
            IRepository<HrmDefDepartment> departmentRepository, 
            IRepository<HrmDefDesignation> designationRepository,
            IRepository<HrmEmployee> employeeRepository, 
            IRepository<HrmDefEmployeeStatus> employeeStatusRepository, 
            IRepository<HrmDefEmpType> empTypeRepository,
            IRepository<HrmEisDefEmploymentNature> employmentNatureRepository,
            IRepository<HrmPayOthersAdjustmentEntry> adjustmentRepository,
            IRepository<HrmPayDefBenefitType> benefitTypeRepository,
            IRepository<HrmPayMonth> monthRepository
            ) : base(adjustmentRepository)
        {
            this.companyRepository = companyRepository;
            this.divisionRepository = divisionRepository;
            this.employeeOfficialInfoRepository = employeeOfficialInfoRepository;
            this.branchRepository = branchRepository;
            this.departmentRepository = departmentRepository;
            this.designationRepository = designationRepository;
            this.employeeRepository = employeeRepository;
            this.employeeStatusRepository = employeeStatusRepository;
            this.empTypeRepository = empTypeRepository;
            this.employmentNatureRepository = employmentNatureRepository;
            this.adjustmentRepository = adjustmentRepository;
            this.benefitTypeRepository = benefitTypeRepository;
            this.monthRepository = monthRepository;
        }

        public async Task<bool> BulkDeleteAsync(List<decimal> ids)
        {
            const int batchSize = 1000;
            await adjustmentRepository.BeginTransactionAsync();

            try
            {
                for (int i = 0; i < ids.Count; i += batchSize)
                {
                    var batch = ids.Skip(i).Take(batchSize).ToList();

                    var entries = await adjustmentRepository.All()
                        .Where(c => batch.Contains(c.Tc))
                        .AsNoTracking()
                        .ToListAsync();

                    if (entries.Any())
                    {
                        await adjustmentRepository.DeleteRangeAsync(entries);
                    }
                }

                await adjustmentRepository.CommitTransactionAsync();
                return true;
            }
            catch (Exception ex)
            {
                await adjustmentRepository.RollbackTransactionAsync();
                Console.WriteLine($"Bulk delete error: {ex}");
                return false;
            }
        }

        public async Task<bool> EditAsync(HrmPayOthersAdjustmentEntryViewModel model)
        {
            if (model == null || model.Tc == 0 || model.Id == null)
            {
                return false;
            }
            await adjustmentRepository.BeginTransactionAsync();

            try
            {
                var existingRecord = await adjustmentRepository.GetByIdAsync(model.Tc);

                if (existingRecord == null)
                {
                    return false;
                }

                var duplicateRecord = await adjustmentRepository.All().Where(e => e.EmployeeId == model.EmployeeId && e.SalaryMonth == model.SalaryMonth && e.SalaryYear == model.SalaryYear && e.BenefitTypeId == model.BenefitTypeId && e.OtherBenefitId != model.OtherBenefitId).ToListAsync();

                if (duplicateRecord != null)
                {
                    await adjustmentRepository.DeleteRangeAsync(duplicateRecord);
                }

                existingRecord.SalaryMonth = model.SalaryMonth;
                existingRecord.SalaryYear = model.SalaryYear;
                existingRecord.Remarks = model.Remarks;
                existingRecord.BenefitAmount = model.BenefitAmount;
                existingRecord.BenefitTypeId = model.BenefitTypeId;

                existingRecord.ModifyDate = model.ModifyDate ?? DateTime.Now;
                existingRecord.Lip = model.Lip;
                existingRecord.Lmac = model.Lmac;
                existingRecord.Luser = model.Luser;
                existingRecord.CompanyCode = model.CompanyCode;

                await adjustmentRepository.UpdateAsync(existingRecord);

                await adjustmentRepository.CommitTransactionAsync();

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

        public async Task<byte[]> GenerateExcelSampleAsync()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Other Benefit");

                worksheet.Cells[1, 1].Value = "Employee ID";
                worksheet.Cells[1, 4].Value = "Salary Month";
                worksheet.Cells[1, 5].Value = "Salary Year";
                worksheet.Cells[1, 2].Value = "Benefit Type";
                worksheet.Cells[1, 3].Value = "Benefit Amount";
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
                worksheet.Cells[2, 4].Value = DateTime.Now.ToString("MMMM");
                worksheet.Cells[2, 5].Value = DateTime.Now.ToString("yyyy");
                worksheet.Cells[2, 2].Value = "Compensation";
                worksheet.Cells[2, 3].Value = "12500";
                worksheet.Cells[2, 6].Value = "Sample remark";

                worksheet.Cells[3, 1].Value = "00000000000";
                worksheet.Cells[3, 4].Value = DateTime.Now.ToString("MMMM");
                worksheet.Cells[3, 5].Value = DateTime.Now.ToString("yyyy");
                worksheet.Cells[3, 2].Value = "Others";
                worksheet.Cells[3, 3].Value = "500";
                worksheet.Cells[3, 6].Value = "Sample remark";

                worksheet.Cells.AutoFitColumns();

                return package.GetAsByteArray();
            }
        }

        public async Task<List<HrmPayDefBenefitTypeViewModel>> GetBenefitTypeAsync()
        {
            var result = await benefitTypeRepository.All().Select(x => new HrmPayDefBenefitTypeViewModel
            {
                BenefitTypeId = x.BenefitTypeId,
                BenefitType = x.BenefitType,
            }).ToListAsync();

            return result;
        }

        public async Task<HrmPayOthersAdjustmentEntryViewModel> GetByIdAsync(decimal id)
        {
            var deduction = await adjustmentRepository.GetByIdAsync(id);

            if (deduction == null) { return null; }

            var deductionViewModel = new HrmPayOthersAdjustmentEntryViewModel
            {
                Tc = deduction.Tc,
                OtherBenefitId = deduction.OtherBenefitId,
                EmployeeId = deduction.EmployeeId,
                BenefitTypeId = deduction.BenefitTypeId,
                BenefitAmount = deduction.BenefitAmount,
                SalaryMonth = deduction.SalaryMonth,
                SalaryYear = deduction.SalaryYear,
                Remarks = deduction.Remarks,

            };

            return deductionViewModel;
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

        public async Task<string> GetOthersAdjustmentIdAsync()
        {
            var lastEntry = await adjustmentRepository.All()
                .OrderByDescending(x => x.Tc)
                .FirstOrDefaultAsync();

            int lastNumber = 0;

            if (lastEntry != null && !string.IsNullOrEmpty(lastEntry.BenefitTypeId))
            {
                int.TryParse(lastEntry.BenefitTypeId, out lastNumber);
            }

            int newNumber = lastNumber + 1;

            string newId = newNumber.ToString("D8");

            return newId;
        }

        public async Task<(List<HrmPayOthersAdjustmentEntryViewModel> Data, int TotalRecords)> GetPaginatedDataAsync(string searchValue, int page, int pageSize, string sortColumn, string sortDirection)
        {
            var query = from a in adjustmentRepository.All().AsNoTracking()
                        join e in employeeRepository.All().AsNoTracking() on a.EmployeeId equals e.EmployeeId
                        join ei in employeeOfficialInfoRepository.All().AsNoTracking() on e.EmployeeId equals ei.EmployeeId into eiJoin
                        from ei in eiJoin.DefaultIfEmpty()
                        join de in designationRepository.All().AsNoTracking() on ei.DesignationCode equals de.DesignationCode into deJoin
                        from de in deJoin.DefaultIfEmpty()
                        join dp in departmentRepository.All().AsNoTracking() on ei.DepartmentCode equals dp.DepartmentCode into dpJoin
                        from dp in dpJoin.DefaultIfEmpty()
                        join bt in benefitTypeRepository.All().AsNoTracking() on a.BenefitTypeId equals bt.BenefitTypeId into btJoin
                        from bt in btJoin.DefaultIfEmpty()
                        join m in monthRepository.All().AsNoTracking() on a.SalaryMonth equals m.MonthId.ToString() into mJion
                        from m in mJion.DefaultIfEmpty()
                        select new HrmPayOthersAdjustmentEntryViewModel
                        {
                            Tc = a.Tc,
                            OtherBenefitId = a.OtherBenefitId,
                            EmployeeId = a.EmployeeId,
                            EmployeeName = e.FirstName + " " + e.LastName,
                            Designation = de.DesignationName,
                            Department = dp.DepartmentName,
                            BenefitType = bt.BenefitType,
                            BenefitAmount = a.BenefitAmount,
                            SalaryMonth = m.MonthName,
                            SalaryYear = a.SalaryYear,
                            Remarks = a.Remarks,
                            Ldate = a.Ldate,
                            Luser = a.Luser
                        };

            var materializedQuery = await query.ToListAsync();

            IEnumerable<HrmPayOthersAdjustmentEntryViewModel> filteredQuery = materializedQuery;

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                filteredQuery = filteredQuery.Where(d =>
                    d.OtherBenefitId.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                    d.EmployeeId.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                    d.EmployeeName.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                    d.Designation.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                    d.Department.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                    d.BenefitType.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                    d.BenefitAmount.ToString().Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                    d.SalaryMonth.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                    d.SalaryYear.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                    d.Remarks.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                    d.Luser.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                    d.Ldate.HasValue && d.Ldate.Value.ToString("yyyy-MM-dd").Contains(searchValue, StringComparison.OrdinalIgnoreCase)
                );
            }

            var totalRecords = filteredQuery.Count();

            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortDirection))
            {
                filteredQuery = sortColumn.ToLower() switch
                {
                    "otherbenefitid" => sortDirection.ToLower() == "asc" ? filteredQuery.OrderBy(a => a.OtherBenefitId) : filteredQuery.OrderByDescending(a => a.OtherBenefitId),
                    "employeeid" => sortDirection.ToLower() == "asc" ? filteredQuery.OrderBy(a => a.EmployeeId) : filteredQuery.OrderByDescending(a => a.EmployeeId),
                    "employeename" => sortDirection.ToLower() == "asc" ? filteredQuery.OrderBy(a => a.EmployeeName) : filteredQuery.OrderByDescending(a => a.EmployeeName),
                    "designation" => sortDirection.ToLower() == "asc" ? filteredQuery.OrderBy(a => a.Designation) : filteredQuery.OrderByDescending(a => a.Designation),
                    "department" => sortDirection.ToLower() == "asc" ? filteredQuery.OrderBy(a => a.Department) : filteredQuery.OrderByDescending(a => a.Department),
                    "benefittype" => sortDirection.ToLower() == "asc" ? filteredQuery.OrderBy(a => a.BenefitType) : filteredQuery.OrderByDescending(a => a.BenefitType),
                    "benefitamount" => sortDirection.ToLower() == "asc" ? filteredQuery.OrderBy(a => a.BenefitAmount) : filteredQuery.OrderByDescending(a => a.BenefitAmount),
                    "salarymonth" => sortDirection.ToLower() == "asc" ? filteredQuery.OrderBy(a => a.SalaryMonth) : filteredQuery.OrderByDescending(a => a.SalaryMonth),
                    "salaryyear" => sortDirection.ToLower() == "asc" ? filteredQuery.OrderBy(a => a.SalaryYear) : filteredQuery.OrderByDescending(a => a.SalaryYear),
                    "remarks" => sortDirection.ToLower() == "asc" ? filteredQuery.OrderBy(a => a.Remarks) : filteredQuery.OrderByDescending(a => a.Remarks),
                    "luser" => sortDirection.ToLower() == "asc" ? filteredQuery.OrderBy(a => a.Luser) : filteredQuery.OrderByDescending(a => a.Luser),
                    "ldate" => sortDirection.ToLower() == "asc" ? filteredQuery.OrderBy(a => a.Ldate) : filteredQuery.OrderByDescending(a => a.Ldate),
                    _ => filteredQuery.OrderBy(a => a.Tc)
                };
            }
            else
            {
                filteredQuery = filteredQuery.OrderBy(a => a.Tc);
            }

            var data = filteredQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return (data, totalRecords);
        }

        public async Task<List<HrmPayMonthViewModel>> GetPayMonthsAsync()
        {
            var result = await monthRepository.All().Select(x => new HrmPayMonthViewModel
            {
                MonthId = x.MonthId,
                MonthName = x.MonthName,
            }).ToListAsync();

            return result;
        }

        public async Task<bool> ProcessExcleFileAsync(Stream fileStream, HrmPayOthersAdjustmentEntryViewModel model)
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
                var records = new List<HrmPayOthersAdjustmentEntry>();

                var lastBenefit = await adjustmentRepository.All()
                    .OrderByDescending(e => e.Tc)
                    .FirstOrDefaultAsync();

                int nextId = lastBenefit != null && int.TryParse(lastBenefit.OtherBenefitId, out int lastNumber)
                    ? lastNumber + 1
                    : 1;

                var benefitTypeMap = await benefitTypeRepository.All().ToListAsync();
                var benefitTypeLookup = benefitTypeMap.ToDictionary(t => t.BenefitType, t => t.BenefitTypeId.Trim(), StringComparer.OrdinalIgnoreCase);

                foreach (var type in benefitTypeMap)
                {
                    if (!benefitTypeLookup.ContainsKey(type.BenefitTypeId.Trim()))
                    {
                        benefitTypeLookup[type.BenefitTypeId.Trim()] = type.BenefitTypeId.Trim();
                    };
                }

                var salaryMonthMap = await monthRepository.All().ToListAsync();
                var salaryMonthLookup = salaryMonthMap.ToDictionary(t => t.MonthName, t => t.MonthId.ToString().Trim(), StringComparer.OrdinalIgnoreCase);

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
                    var benefitAmountStr = worksheet.Cells[row, 3].Text?.Trim();
                    var rawBenefitType = worksheet.Cells[row, 2].Text?.Trim();
                    var rawSalaryMonth = worksheet.Cells[row, 4].Value?.ToString();
                    var salaryYear = worksheet.Cells[row, 5].Text?.Trim();
                    var remarks = worksheet.Cells[row, 6].Text?.Trim();

                    if (string.IsNullOrWhiteSpace(employeeId) ||
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

                    string benefitType = null;
                    if (!string.IsNullOrWhiteSpace(rawBenefitType) && benefitTypeLookup.TryGetValue(rawBenefitType.Trim(), out mappedCode))
                    {
                        benefitType = mappedCode;
                    }
                    else
                    {
                        continue;
                    }

                    if (!decimal.TryParse(benefitAmountStr, out decimal benefitAmount))
                    {
                        validationErrors.Add($"Row {row}: Invalid benefit amount.");
                        continue;
                    }

                    var alreadyExists = await adjustmentRepository.All()
                        .Where(e => e.EmployeeId == employeeId && e.SalaryMonth == payMonth && e.SalaryYear == salaryYear && e.BenefitTypeId == benefitType)
                        .FirstOrDefaultAsync();

                    if (alreadyExists != null)
                    {
                        await adjustmentRepository.DeleteAsync(alreadyExists.Tc);
                    }

                    string newId = $"{nextId:D8}";
                    nextId++;

                    records.Add(new HrmPayOthersAdjustmentEntry
                    {
                        OtherBenefitId = newId,
                        EmployeeId = employeeId,
                        SalaryMonth = payMonth,
                        SalaryYear = salaryYear,
                        BenefitAmount = benefitAmount,
                        BenefitTypeId = benefitType,
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
                    await adjustmentRepository.AddRangeAsync(records);
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

        public async Task<bool> SaveAsync(HrmPayOthersAdjustmentEntryViewModel model)
        {
            if (model == null || model.EmployeeIds == null || !model.EmployeeIds.Any())
                return false;

            try
            {
                if (string.IsNullOrEmpty(model.SalaryMonth) || string.IsNullOrEmpty(model.SalaryYear))
                    return false;

                var monthsToProcess = new List<(string Month, string Year)>
                {
                    (model.SalaryMonth, model.SalaryYear)
                };

                int nextId = 1;
                var last = await adjustmentRepository.All()
                    .OrderByDescending(e => e.Tc)
                    .FirstOrDefaultAsync();

                if (last != null && int.TryParse(last.OtherBenefitId, out int lastNumber))
                    nextId = lastNumber + 1;

                var empIds = model.EmployeeIds;
                var months = monthsToProcess.Select(m => m.Month).ToList();
                var years = monthsToProcess.Select(m => m.Year).ToList();

                var entriesToDelete = await adjustmentRepository.All()
                    .Where(e =>
                        empIds.Contains(e.EmployeeId) &&
                        months.Contains(e.SalaryMonth) &&
                        years.Contains(e.SalaryYear) &&
                        e.BenefitTypeId == model.BenefitTypeId)
                    .ToListAsync();

                if (entriesToDelete.Any())
                    await adjustmentRepository.DeleteRangeAsync(entriesToDelete);

                List<HrmPayOthersAdjustmentEntry> records = new();

                foreach (var empId in model.EmployeeIds)
                {
                    string id = $"{nextId:D8}";
                    nextId++;

                    records.Add(new HrmPayOthersAdjustmentEntry
                    {
                        OtherBenefitId = id,
                        EmployeeId = empId,
                        SalaryMonth = model.SalaryMonth,
                        SalaryYear = model.SalaryYear,
                        BenefitTypeId = model.BenefitTypeId,
                        BenefitAmount = model.BenefitAmount,
                        Remarks = model.Remarks,
                        Ldate = model.Ldate,
                        Lip = model.Lip,
                        Lmac = model.Lmac,
                        Luser = model.Luser,
                        CompanyCode = model.CompanyCode
                    });
                }

                const int batchSize = 10000;
                for (int i = 0; i < records.Count; i += batchSize)
                {
                    var batch = records.Skip(i).Take(batchSize).ToList();
                    await adjustmentRepository.AddRangeAsync(batch);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }
    }
}
