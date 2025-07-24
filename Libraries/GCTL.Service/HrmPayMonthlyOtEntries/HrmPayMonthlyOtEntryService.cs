using GCTL.Core.Data;
using GCTL.Core.ViewModels.HrmPayMonthlyOtEntries;
using GCTL.Core.ViewModels.HrmPayMonths;
using GCTL.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.HrmPayMonthlyOtEntries
{
    public class HrmPayMonthlyOtEntryService:AppService<HrmPayMonthlyOtentry>, IHrmPayMonthlyOtEntryService
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
        private readonly IRepository<HrmPayMonth> payMonthRepository;
        private readonly IRepository<HrmPayMonthlyOtentry> entryRepository;
        
        public HrmPayMonthlyOtEntryService(
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
            IRepository<HrmPayMonthlyOtentry> entryRepository,
            IRepository<HrmPayMonth> payMonthRepository,
            IRepository<HrmEisDefEmploymentNature> employmentNatureRepository) : base(entryRepository)
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
            this.entryRepository = entryRepository;
            this.payMonthRepository = payMonthRepository;
            this.employmentNatureRepository = employmentNatureRepository;
        }

        public async Task<bool> BulkDeleteAsync(List<decimal> tcs)
        {
            const int batchSize = 1000;
            await entryRepository.BeginTransactionAsync();

            try
            {
                for (int i = 0; i < tcs.Count; i += batchSize)
                {
                    var batch = tcs.Skip(i).Take(batchSize).ToList();

                    var otEntries = await entryRepository.All()
                        .Where(c => batch.Contains(c.Tc))
                        .AsNoTracking()
                        .ToListAsync();

                    if (otEntries.Any())
                    {
                        await entryRepository.DeleteRangeAsync(otEntries);
                    }
                }

                await entryRepository.CommitTransactionAsync();
                return true;
            }
            catch (Exception ex)
            {
                await entryRepository.RollbackTransactionAsync();
                return false;
            }
        }

        public async Task<bool> EditAsync(HrmPayMonthlyOtEntryViewModel model)
        {
            if (model == null || model.Tc == 0 || model.MonthlyOtid == null)
            {
                return false;
            }
            await entryRepository.BeginTransactionAsync();

            try
            {
                var exRecord = await entryRepository.GetByIdAsync(model.Tc);

                if (exRecord == null) 
                {
                    return false;
                }

                var duplicateRecord = await entryRepository.All()
                    .Where(e=>e.EmployeeId == model.EmployeeId && e.Month==model.Month && e.Year==model.Year && e.MonthlyOtid != model.MonthlyOtid).ToListAsync();

                if ( duplicateRecord!=null && duplicateRecord.Any()) 
                {
                    await entryRepository.DeleteRangeAsync(duplicateRecord);
                }

                exRecord.Ot = model.Ot;
                exRecord.Otamount = model.Otamount;
                exRecord.Month = model.Month;
                exRecord.Year = model.Year;
                exRecord.Remarks = model.Remarks;

                exRecord.ModifyDate = model.ModifyDate;
                exRecord.Lip = model.Lip;
                exRecord.Lmac = model.Lmac;
                exRecord.Luser = model.Luser;
                exRecord.CompanyCode = model.CompanyCode;

                await entryRepository.UpdateAsync(exRecord);

                await entryRepository.CommitTransactionAsync();

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
                var worksheet = package.Workbook.Worksheets.Add("Monthly OT");

                worksheet.Cells[1, 1].Value = "Employee ID";
                worksheet.Cells[1, 2].Value = "OT";
                worksheet.Cells[1, 3].Value = "Amount";
                worksheet.Cells[1, 4].Value = "Month";
                worksheet.Cells[1, 5].Value = "Year";
                //worksheet.Cells[1, 6].Value = "Remarks";

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
                worksheet.Cells[2, 2].Value = 10;
                worksheet.Cells[2, 3].Value = 100;
                worksheet.Cells[2, 4].Value = DateTime.Now.ToString("MMMM");
                worksheet.Cells[2, 5].Value = DateTime.Now.ToString("yyyy");
                worksheet.Cells.AutoFitColumns();

                return package.GetAsByteArray();
            }
        }

        public async Task<string> GenerateMonthlyOtIdAsync()
        {
            var lastEntry = await entryRepository.All()
                .OrderByDescending(x => x.Tc)
                .Select(e => e.MonthlyOtid).FirstOrDefaultAsync();

            int lastNumber = 0;
            if(lastEntry!=null && !string.IsNullOrEmpty(lastEntry))
            {
                int.TryParse(lastEntry, out lastNumber);
            }
            int newNumber = lastNumber + 1;
            string newId = newNumber.ToString("D8");

            return newId;
        }


        public async Task<HrmPayMonthlyOtEntryViewModel> GetByIdAsync(decimal id)
        {
            var entry = await entryRepository.GetByIdAsync(id);

            if(entry == null)  return null;

            var viewModel = new HrmPayMonthlyOtEntryViewModel()
            {
                Tc = entry.Tc,
                MonthlyOtid = entry.MonthlyOtid,
                EmployeeId = entry.EmployeeId,
                Ot = entry.Ot,
                Otamount = entry.Otamount,
                Month = entry.Month,
                Year = entry.Year,
                Remarks = entry.Remarks,
            };
            return viewModel;
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
                        join di in divisionRepository.All().AsNoTracking() on e.DivisionCode equals di.DivisionCode into diGroup
                        from di in diGroup.DefaultIfEmpty()
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
                            DivisionName = di.DivisionName,
                            EmployeeTypeName = emptype.EmpTypeName,
                            EmployeeStatusId = e.EmployeeStatus,
                            EmployeeStatusName = status.EmployeeStatus,
                            EmpNatureCode = e.EmploymentNatureId,
                            EmpNatureName = empNature.EmploymentNature
                        };

            query = query.Where(x => x.CompanyCode == "001");

            //if (filter.CompanyCodes == null || !filter.CompanyCodes.Any())
            //   return new EmployeeFilterResultDto();

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
                CompanyName = x.CompanyName,
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

            result.LookupData["divisions"] = filteredData
                .Where(x => x.DivisionCode != null && x.DivisionName != null)
                .Select(x => new LookupItemDto { Code = x.DivisionCode, Name = x.DivisionName })
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

        public async Task<(List<HrmPayMonthlyOtEntryViewModel> Data, int TotalRecords)> GetPaginatedDataAsync(string searchValue, int page, int pageSize, string sortColumn, string sortDirection)
        {
            var query = from d in entryRepository.All().AsNoTracking()
                        join e in employeeRepository.All().AsNoTracking() on d.EmployeeId equals e.EmployeeId
                        join ei in employeeOfficialInfoRepository.All().AsNoTracking() on e.EmployeeId equals ei.EmployeeId into eiJoin
                        from ei in eiJoin.DefaultIfEmpty()
                        join de in designationRepository.All() on ei.DesignationCode equals de.DesignationCode into deJoin
                        from de in deJoin.DefaultIfEmpty()
                        join m in payMonthRepository.All() on d.Month equals m.MonthId.ToString() into mJoin
                        from m in mJoin.DefaultIfEmpty()
                        select new HrmPayMonthlyOtEntryViewModel
                        {
                            Tc = d.Tc,
                            MonthlyOtid = d.MonthlyOtid,
                            EmployeeId = d.EmployeeId,
                            EmployeeName=e.FirstName+" "+e.LastName,
                            Designation=de.DesignationName,
                            Ot=d.Ot,
                            Otamount=d.Otamount ?? 0,
                            Month=m.MonthName,
                            Year=d.Year,
                            Remarks=d.Remarks,
                            Luser=d.Luser,
                            Ldate=d.Ldate.Value.Date,
                        };

            var materializedQuery = await query.ToListAsync();

            IEnumerable<HrmPayMonthlyOtEntryViewModel> filteredQuery = materializedQuery;

            if (!string.IsNullOrEmpty(searchValue))
            {
                filteredQuery = filteredQuery.Where(d =>
                    (d.EmployeeName?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (d.MonthlyOtid?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (d.EmployeeId?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (d.Designation?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    d.Ot.ToString().Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                    d.Otamount.ToString().Contains(searchValue, StringComparison.OrdinalIgnoreCase) ||
                    (d.Remarks?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (d.Luser?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (d.Ldate?.ToString().Contains(searchValue, StringComparison.OrdinalIgnoreCase) ?? false)
                );
            }

            var totalRecords = filteredQuery.Count();

            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortDirection))
            {
                filteredQuery = sortColumn.ToLower() switch
                {
                    "id" => sortDirection.ToLower() == "asc" ? filteredQuery.OrderBy(a => a.Id) : filteredQuery.OrderByDescending(a => a.Id),
                    "employeeid" => sortDirection.ToLower() == "asc" ? filteredQuery.OrderBy(a => a.EmployeeId) : filteredQuery.OrderByDescending(a => a.EmployeeId),
                    "employeename" => sortDirection.ToLower() == "asc" ? filteredQuery.OrderBy(a => a.EmployeeName) : filteredQuery.OrderByDescending(a => a.EmployeeName),
                    "designation" => sortDirection.ToLower() == "asc" ? filteredQuery.OrderBy(a => a.Designation) : filteredQuery.OrderByDescending(a => a.Designation),
                    "ot" => sortDirection.ToLower() == "asc" ? filteredQuery.OrderBy(a => a.Ot) : filteredQuery.OrderByDescending(a => a.Ot),
                    "otamount" => sortDirection.ToLower() == "asc" ? filteredQuery.OrderBy(a => a.Otamount) : filteredQuery.OrderByDescending(a => a.Otamount),
                    "month" => sortDirection.ToLower() == "asc" ? filteredQuery.OrderBy(a => a.Month) : filteredQuery.OrderByDescending(a => a.Month),
                    "year" => sortDirection.ToLower() == "asc" ? filteredQuery.OrderBy(a => a.Year) : filteredQuery.OrderByDescending(a => a.Year),
                    "remarks" => sortDirection.ToLower() == "asc" ? filteredQuery.OrderBy(a => a.Remarks) : filteredQuery.OrderByDescending(a => a.Remarks),
                    "luser" => sortDirection.ToLower() == "asc" ? filteredQuery.OrderBy(a => a.Luser) : filteredQuery.OrderByDescending(a => a.Luser),
                    "ldate"=>sortDirection.ToLower() =="asc"?filteredQuery.OrderBy(a=>a.Ldate):filteredQuery.OrderByDescending(a=>a.Ldate),
                    _ => filteredQuery.OrderBy(a => a.Tc)
                };
            }
            else
            {
                filteredQuery = filteredQuery.OrderBy(a => a.Tc);
            }

            var data = pageSize < 0
                        ? filteredQuery.ToList()
                        : filteredQuery.Skip((page - 1) * pageSize).Take(pageSize).ToList();


            return (data, totalRecords);
        }

        public async Task<List<HrmPayMonthViewModel>> GetPayMonthsAsync()
        {
            var result = await payMonthRepository.All().Select(x => new HrmPayMonthViewModel
            {
                MonthId = x.MonthId,
                MonthName = x.MonthName,
            }).ToListAsync();

            return result;
        }

        public async Task<bool> ProcessExcelFileAsync(Stream fileStream, HrmPayMonthlyOtEntryViewModel model)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            try
            {
                using var package = new ExcelPackage(fileStream);
                var worksheet = package.Workbook.Worksheets[0];

                int rowCount = worksheet.Dimension.Rows;
                if (rowCount <= 1) return false;

                var validationErrors = new List<string>();
                var records= new List<HrmPayMonthlyOtentry>();

                var lastRecord = await entryRepository.All()
                    .OrderByDescending(e => e.Tc)
                    .FirstOrDefaultAsync();
                int nextId = lastRecord != null && int.TryParse(lastRecord.MonthlyOtid, out int lastNumber)
                    ? lastNumber + 1
                    : 1;

                var monthMap = await payMonthRepository.All().ToListAsync();
                var monthLookup = monthMap.ToDictionary(t=>t.MonthName, t=>t.MonthId.ToString().Trim(), StringComparer.OrdinalIgnoreCase);

                foreach(var month in monthMap)
                {
                    var monthIdStr = month.MonthId.ToString().Trim();
                    if (!monthLookup.ContainsKey(monthIdStr))
                    {
                        monthLookup[monthIdStr] = monthIdStr;
                    }
                }

                for(int row = 2;row<=rowCount;row++) 
                {
                    var employeeId = worksheet.Cells[row, 1].Text?.Trim();
                    var rawMonth = worksheet.Cells[row, 4].Text?.Trim();
                    var year = worksheet.Cells[row,5].Text?.Trim();
                    var otStr = worksheet.Cells[row,2].Text?.Trim();
                    var otAmountStr = worksheet.Cells[row,3].Text?.Trim();

                    if(string.IsNullOrWhiteSpace(employeeId)||
                        string.IsNullOrWhiteSpace(otStr) ||
                        string.IsNullOrWhiteSpace(year) ||
                        string.IsNullOrWhiteSpace(rawMonth))
                    {
                        validationErrors.Add($"Row {row}: All fields except remarks are required.");
                        continue;
                    }

                    string payMonth = null;
                    if (!string.IsNullOrWhiteSpace(rawMonth) && monthLookup.TryGetValue(rawMonth.Trim(),out var mappedCode)) 
                    {
                    payMonth = mappedCode;
                    }
                    else
                    {
                        continue;
                    }

                    if(!decimal.TryParse(otStr,out decimal ot))
                    {
                        validationErrors.Add($"Row {row}: Invalid ot.");
                        continue;
                    }

                    if(!decimal.TryParse(otAmountStr,out decimal otAmount))
                    {
                        validationErrors.Add($"Row {row}: Invalid otAmount.");
                        continue;
                    }

                    var alreadyExists = await entryRepository.All()
                        .Where(e=>e.EmployeeId == employeeId && e.Month == payMonth && e.Year == year)
                        .ToListAsync();

                    if (alreadyExists != null && alreadyExists.Any()) 
                    {
                        await entryRepository.DeleteRangeAsync(alreadyExists);
                    }

                    string newId = $"{nextId:D8}";
                    nextId++;

                    records.Add(new HrmPayMonthlyOtentry
                    {
                        MonthlyOtid = newId,
                        EmployeeId = employeeId,
                        Month = payMonth,
                        Year = year,
                        Ot = ot,
                        Otamount = otAmount,
                        Ldate = model.Ldate,
                        Lip = model.Lip,
                        Lmac = model.Lmac,
                        Luser = model.Luser,
                        CompanyCode = model.CompanyCode
                    });
                }

                if (!records.Any() && validationErrors.Any()) 
                {
                    return false;
                }

                if (records.Any())
                {
                    await entryRepository.AddRangeAsync(records);
                    string message = $"{records.Count} record(s) added successfully.";

                    if (validationErrors.Any())
                        message += $" {validationErrors.Count} record(s) failed validation.";

                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> SaveAsync(HrmPayMonthlyOtEntryViewModel model)
        {
            if (model == null || model.EmployeeIds == null || !model.EmployeeIds.Any() || model.Ot==null)
                return false;

            try
            {
                List<(string Month, string Year)> monthsToProcess = new();
                if (!string.IsNullOrWhiteSpace(model.Month) && !string.IsNullOrWhiteSpace(model.Year))
                {
                    monthsToProcess.Add((model.Month, model.Year));
                }
                else if (model.DateForm != default && model.DateTo != default) 
                {
                    DateTime start = new DateTime(model.DateForm.Year, model.DateForm.Month, 1);
                    DateTime end = new DateTime(model.DateTo.Year, model.DateTo.Month, 1).AddMonths(1);
                    while (start < end) 
                    {
                        monthsToProcess.Add((start.Month.ToString(),start.Year.ToString()));
                        start = start.AddMonths(1);
                    }
                }
                else
                {
                    return false;
                }

                int nextId = 1;
                var last = await entryRepository.All().OrderByDescending(e => e.Tc).FirstOrDefaultAsync();
                if (last != null && int.TryParse(last.MonthlyOtid, out int lastNumber)) nextId = lastNumber + 1;

                var empIds = model.EmployeeIds;
                var months = monthsToProcess.Select(m => m.Month).ToList();
                var years = monthsToProcess.Select(m => m.Year).ToList();

                var entriesToDelete = await entryRepository.All()
                    .Where(e => empIds.Contains(e.EmployeeId) && months.Contains(e.Month) && years.Contains(e.Year))
                    .ToListAsync();

                if (entriesToDelete != null && entriesToDelete.Any())
                    await entryRepository.DeleteRangeAsync(entriesToDelete);

                List<HrmPayMonthlyOtentry> records = new();

                foreach (var (month, year) in monthsToProcess) 
                {
                    foreach(var empId in model.EmployeeIds)
                    {
                        string id = $"{nextId:D8}";
                        nextId++;

                        records.Add(new HrmPayMonthlyOtentry
                        {
                            MonthlyOtid = id,
                            EmployeeId=empId,
                            Month=month,
                            Year=year,
                            Ot=model.Ot,
                            Otamount=model.Otamount,
                            Remarks=model.Remarks,
                            Ldate=model.Ldate,
                            Lip=model.Lip,
                            Lmac=model.Lmac,
                            Luser=model.Luser,
                            CompanyCode=model.CompanyCode,
                        });
                    }
                }

                const int batchSize = 10000;
                for(int i = 0; i < records.Count; i += batchSize)
                {
                    var batch = records.Skip(i).Take(batchSize).ToList();
                    await entryRepository.AddRangeAsync(batch);
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
