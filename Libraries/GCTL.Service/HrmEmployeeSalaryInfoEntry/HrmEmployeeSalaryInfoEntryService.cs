using GCTL.Core.Data;
using GCTL.Core.ViewModels.HrmEmployeeSalaryInfoEntry;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.HrmEmployeeSalaryInfoEntry
{
    public class HrmEmployeeSalaryInfoEntryService : AppService<HrmEmployeeOfficialInfo>, IHrmEmployeeSalaryInfoEntryService
    {
        private readonly IRepository<CoreCompany> companyRepo;
        private readonly IRepository<HrmEmployeeOfficialInfo> empOfficialInfoRepo;
        private readonly IRepository<CoreBranch> branchRepo;
        private readonly IRepository<HrmDefDepartment> departmentRepo;
        private readonly IRepository<HrmDefDesignation> designationRepo;
        private readonly IRepository<HrmEmployee> empRepo;
        private readonly IRepository<HrmDefEmployeeStatus> empStatusRepo;
        private readonly IRepository<HrmDefEmpType> empTypeRepo;
        private readonly IRepository<HrmEisDefEmploymentNature> empNatureRepo;
        private readonly IRepository<HrmSeparation> separationRepo;
        private readonly IRepository<HrmEisDefDisbursementMethod> disbursementRepo;
        private readonly IRepository<HrmIncrement> incRepo;

        public HrmEmployeeSalaryInfoEntryService(
            IRepository<CoreCompany> companyRepo,
            IRepository<CoreBranch> branchRepo,
            IRepository<HrmDefDepartment> departmentRepo,
            IRepository<HrmDefDesignation> designationRepo,
            IRepository<HrmDefEmployeeStatus> empStatusRepo,
            IRepository<HrmDefEmpType> empTypeRepo,
            IRepository<HrmEmployeeOfficialInfo> empOfficialInfoRepo,
            IRepository<HrmSeparation> separationRepo,
            IRepository<HrmEmployee> empRepo,
            IRepository<HrmEisDefDisbursementMethod> disbursementRepo,
            IRepository<HrmEisDefEmploymentNature> empNatureRepo,
            IRepository<HrmIncrement> incRepo) : base(empOfficialInfoRepo)
        {
            this.companyRepo = companyRepo;
            this.branchRepo = branchRepo;
            this.departmentRepo = departmentRepo;
            this.designationRepo = designationRepo;
            this.empStatusRepo = empStatusRepo;
            this.empTypeRepo = empTypeRepo;
            this.empOfficialInfoRepo = empOfficialInfoRepo;
            this.separationRepo = separationRepo;
            this.empRepo = empRepo;
            this.empNatureRepo = empNatureRepo;
            this.disbursementRepo = disbursementRepo;
            this.incRepo = incRepo;
        }

        public Task<bool> BulkDeleteAsync(List<decimal> autoIds)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> BulkEditAsync(HrmEmployeeSalaryInfoEntryViewModel model)
        {
            if (model?.SalaryInfoUpdate?.Count == 0)
                return false;

            await empOfficialInfoRepo.BeginTransactionAsync();
            try
            {
                var employeeIds = model.SalaryInfoUpdate.Select(x => x.EmployeeId).ToHashSet();

                var existingRecords = await empOfficialInfoRepo.All()
                    .Where(e => employeeIds.Contains(e.EmployeeId))
                    .ToListAsync();

                var recordLookup = existingRecords.ToDictionary(e => e.EmployeeId);

                List<HrmEmployeeOfficialInfo> recordsToUpdate = new List<HrmEmployeeOfficialInfo>();

                foreach (var row in model.SalaryInfoUpdate)
                {
                    if (!recordLookup.TryGetValue(row.EmployeeId, out var exRecord))
                        continue;

                    var newGrossSalary = row.GrossSalary ?? 0.00m;

                    if (exRecord.GrossSalary != newGrossSalary ||
                        exRecord.DisbursementMethodId != row.DisbursementMethodId)
                    {
                        exRecord.GrossSalary = newGrossSalary;
                        exRecord.DisbursementMethodId = row.DisbursementMethodId;
                        exRecord.ModifyDate = row.ModifyDate;
                        exRecord.Luser = row.Luser;
                        exRecord.Lip = row.Lip;
                        exRecord.Lmac = row.Lmac;

                        recordsToUpdate.Add(exRecord);
                    }
                }

                if (recordsToUpdate.Count > 0)
                {
                    await empOfficialInfoRepo.UpdateRangeAsync(recordsToUpdate);
                }

                await empOfficialInfoRepo.CommitTransactionAsync();
                return true;
            }
            catch (Exception)
            {
                await empOfficialInfoRepo.RollbackTransactionAsync();
                return false;
            }
        }

        public async Task<byte[]> GenerateExcelSampleAsync()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("SalaryInfoEntry");

                worksheet.Cells[1, 1].Value = "Employee ID";
                worksheet.Cells[1, 2].Value = "Pay ID";
                worksheet.Cells[1, 3].Value = "Salary";
                worksheet.Cells[1, 4].Value = "Mode Of Payment";

                using(var range= worksheet.Cells[1, 1, 1, 4])
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

                worksheet.Cells[2, 1].Value = "00000000000";
                worksheet.Cells[2, 2].Value = "135";
                worksheet.Cells[2, 3].Value = 10000;
                worksheet.Cells[2, 4].Value = "BT";

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();


                return await package.GetAsByteArrayAsync();
            }
        }

        public async Task<EmployeeFilterResultDto> GetFilterEmployeeAsync(EmployeeFilterViewModel model)
        {
            var result = new EmployeeFilterResultDto();

            var query = from e in empOfficialInfoRepo.All().AsNoTracking()
                        join emp in empRepo.All().AsNoTracking() on e.EmployeeId equals emp.EmployeeId
                        join c in companyRepo.All().AsNoTracking() on e.CompanyCode equals c.CompanyCode
                        join b in branchRepo.All().AsNoTracking() on e.BranchCode equals b.BranchCode into branchGroup
                        from b in branchGroup.DefaultIfEmpty()
                        join d in departmentRepo.All().AsNoTracking() on e.DepartmentCode equals d.DepartmentCode into deptGroup
                        from d in deptGroup.DefaultIfEmpty()
                        join ds in designationRepo.All().AsNoTracking() on e.DesignationCode equals ds.DesignationCode into desigGroup
                        from ds in desigGroup.DefaultIfEmpty()
                        join status in empStatusRepo.All().AsNoTracking() on e.EmployeeStatus equals status.EmployeeStatusId into statusGroup
                        from status in statusGroup.DefaultIfEmpty()
                        join emptype in empTypeRepo.All().AsNoTracking() on e.EmpTypeCode equals emptype.EmpTypeCode into empTypeGroup
                        from emptype in empTypeGroup.DefaultIfEmpty()
                        join empNature in empNatureRepo.All().AsNoTracking() on e.EmploymentNatureId equals empNature.EmploymentNatureId into empNatureGroup
                        from empNature in empNatureGroup.DefaultIfEmpty()
                        join des in disbursementRepo.All().AsNoTracking() on e.DisbursementMethodId equals des.DisbursementMethodId into disbursGroup
                        from des in disbursGroup.DefaultIfEmpty()
                        join sep in separationRepo.All().AsNoTracking() on e.EmployeeId equals sep.EmployeeId into separationGroup
                        from sep in separationGroup.DefaultIfEmpty()
                        let latestInc = incRepo.All()
                            .AsNoTracking()
                            .Where(x => x.EmployeeId == e.EmployeeId)
                            .OrderByDescending(x => x.Wef)
                            .FirstOrDefault()

                        select new
                        {
                            EmployeeId = e.EmployeeId,
                            PayId = e.PayId,
                            FirstName = emp.FirstName,
                            LastName = emp.LastName,
                            e.BranchCode,
                            BranchName = b != null ? b.BranchName : null,
                            e.CompanyCode,
                            CompanyName = c.CompanyName,
                            e.DesignationCode,
                            DesignationName = ds != null ? ds.DesignationName : null,
                            e.DepartmentCode,
                            DepartmentName = d != null ? d.DepartmentName : null,
                            e.EmpTypeCode,
                            EmpTypename = emptype != null ? emptype.EmpTypeName : null,
                            e.EmployeeStatus,
                            EmployeeStatusName = status != null ? status.EmployeeStatus : null,
                            e.EmploymentNatureId,
                            EmpNatureName = empNature != null ? empNature.EmploymentNature : null,
                            e.JoiningDate,
                            SeparationDate = sep == null ? (DateTime?)null : sep.SeparationDate,
                            e.GrossSalary,
                            e.DisbursementMethodId,
                            DisbursementName = des != null ? des.DisbursementMethod : null,
                            LastIncrementWEF = latestInc != null ? (DateTime?)latestInc.Wef : null
                        };

            query = query.Where(x => x.CompanyCode == "001");

            if (model.BranchCodes?.Any() == true)
                query = query.Where(x => model.BranchCodes.Contains(x.BranchCode));

            if (model.DepartmentCodes?.Any() == true)
                query = query.Where(x => model.DepartmentCodes.Contains(x.DepartmentCode));

            if (model.DesignationCodes?.Any() == true)
                query = query.Where(x => model.DesignationCodes.Contains(x.DesignationCode)); // Fixed bug

            if (model.EmployeeTypes?.Any() == true)
                query = query.Where(x => model.EmployeeTypes.Contains(x.EmpTypeCode)); // Fixed bug

            if (model.EmployeeNatureCodes?.Any() == true)
                query = query.Where(x => model.EmployeeNatureCodes.Contains(x.EmploymentNatureId)); // Fixed bug

            if (model.JoiningDateFrom != null)
            {
                query = query.Where(x => x.JoiningDate.Value.Date >= model.JoiningDateFrom.Value.Date);
            }
                
            if (model.JoiningDateTO != null)
            {
                query = query.Where(x => x.JoiningDate.Value.Date <= model.JoiningDateTO.Value.Date);
            }

            if (model.EmployeeStatuses?.Any() == true)
                query = query.Where(x => model.EmployeeStatuses.Contains(x.EmployeeStatus));

            var allFilteredData = await query.ToListAsync();

            result.Employees = allFilteredData.Select(x => new EmployeeListItemViewModel
            {
                EmployeeId = x.EmployeeId,
                EmployeeName = string.Join(" ", new[] { x.FirstName, x.LastName }.Where(n => !string.IsNullOrWhiteSpace(n))),
                PayId = x.PayId ?? "",
                DesignationName = x.DesignationName,
                DepartmentName = x.DepartmentName,
                EmployeeTypeName = x.EmpTypename,
                EmploymentNature = x.EmpNatureName ??"",
                JoiningDate = x.JoiningDate.HasValue ? x.JoiningDate.Value.ToString("dd/MM/yyyy") : "",
                SeparationDate = x.SeparationDate.HasValue ? x.SeparationDate.Value.ToString("dd/MM/yyyy") : "",
                EmployeeStatus = x.EmployeeStatusName,
                GrossSalary = x.GrossSalary,
                DisbursementMethodId = x.DisbursementMethodId,
                DisbursementMethodName = x.DisbursementName,
                LastIncDate = x.LastIncrementWEF.HasValue ? x.LastIncrementWEF.Value.ToString("dd/MM/yyyy") : ""
            }).ToList();

            result.LookupData["companies"] = allFilteredData
                .Where(x => x.CompanyCode != null && x.CompanyName != null)
                .GroupBy(x => new { x.CompanyCode, x.CompanyName })
                .Select(x => new LookupItemDto { Code = x.Key.CompanyCode, Name = x.Key.CompanyName })
                //.Distinct()
                .ToList();

            result.LookupData["branches"] = allFilteredData
                .Where(x => x.BranchCode != null && x.BranchName != null)
                .GroupBy(x=> new { x.BranchCode, x.BranchName })
                .Select(x => new LookupItemDto {Code = x.Key.BranchCode, Name = x.Key.BranchName })
                //.Distinct()
                .ToList();

            result.LookupData["departments"] = allFilteredData
                .Where(x => x.DepartmentCode != null && x.DepartmentName != null)
                .GroupBy(x => new {x.DepartmentCode,x.DepartmentName})
                .Select(x => new LookupItemDto {Code = x.Key.DepartmentCode, Name = x.Key.DepartmentName })
                //.Distinct()
                .ToList();

            result.LookupData["designations"] = allFilteredData
                .Where(x => x.DesignationCode != null && x.DesignationName != null)
                .GroupBy(x => new {x.DesignationCode, x.DesignationName})
                .Select(x => new LookupItemDto { Code = x.Key.DesignationCode, Name = x.Key.DesignationName })
                //.Distinct()
                .ToList();

            result.LookupData["employeeStatuses"] = allFilteredData
                .Where(x => !string.IsNullOrWhiteSpace(x.EmployeeStatusName))
                .GroupBy(x => new {x.EmployeeStatus, x.EmployeeStatusName})
                .Select(x => new LookupItemDto { Code = x.Key.EmployeeStatus, Name = x.Key.EmployeeStatusName })
                //.Distinct()
                .ToList();

            result.LookupData["employeeType"] = allFilteredData
                .Where(x => !string.IsNullOrWhiteSpace(x.EmpTypeCode) && !string.IsNullOrWhiteSpace(x.EmpTypename))
                .GroupBy(x => new { x.EmpTypeCode, x.EmpTypename})
                .Select(x => new LookupItemDto { Code = x.Key.EmpTypeCode, Name = x.Key.EmpTypename })
                //.Distinct()
                .ToList();

            result.LookupData["employmentNature"] = allFilteredData
                .Where(x => x.EmploymentNatureId != null && !string.IsNullOrWhiteSpace(x.EmpNatureName))
                .GroupBy(x=>new { x.EmploymentNatureId, x.EmpNatureName })
                .Select(x => new LookupItemDto { Code = x.Key.EmploymentNatureId.ToString(), Name = x.Key.EmpNatureName })
                .Distinct()
                .ToList();

            return result;
        }

        public async Task<List<HrmEisDefDisbursementMethod>> GetPaymentMode()
        {
            var result = await disbursementRepo.All().Select(x=>new HrmEisDefDisbursementMethod
            {
                DisbursementMethodId = x.DisbursementMethodId,
                DisbursementMethod = x.DisbursementMethod
            }).ToListAsync();

            return result;
        }

        public async Task<bool> ProcessExcelFileAsync(Stream stream, EmployeeListItemViewModel model)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            try
            {
                using var package = new ExcelPackage(stream);

                var worksheet = package.Workbook.Worksheets[0];

                int rowCount = worksheet.Dimension.Rows;
                if (rowCount <= 1) return false;

                //var validationErrors = new List<string>();
                var record = new List<EmployeeListItemViewModel>();

                var methodMap = await disbursementRepo.All().ToListAsync();
                var methodLookup = methodMap.ToDictionary(
                                    t => t.DisbursementMethod.ToLower(),
                                    t => t.DisbursementMethodId);

                foreach (var method in methodMap)
                {
                    methodLookup[method.DisbursementMethodId.ToLower()] = method.DisbursementMethodId;
                }


                var allEmp = await empOfficialInfoRepo.All().ToListAsync();
                var empLookup = new Dictionary<string, object>();

                foreach (var emp in allEmp)
                {
                    if (!string.IsNullOrWhiteSpace(emp.EmployeeId))
                        empLookup[emp.EmployeeId] = emp;
                    if (!string.IsNullOrWhiteSpace(emp.PayId))
                        empLookup[emp.PayId] = emp;
                }

                var empToUpdate = new List<object>();

                for (int row = 2; row <= rowCount; row++)
                {
                    var empId = worksheet.Cells[row, 1].Text?.Trim();
                    var payId = worksheet.Cells[row, 2].Text?.Trim();
                    var rawMethod = worksheet.Cells[row, 4].Text?.Trim();
                    var amount = worksheet.Cells[row, 3].Text?.Trim();

                    if (string.IsNullOrWhiteSpace(empId) && string.IsNullOrWhiteSpace(payId))
                    {
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(rawMethod) && string.IsNullOrWhiteSpace(amount))
                    {
                        continue;
                    }

                    if (!decimal.TryParse(amount, out var parsedAmount))
                    {
                        continue;
                    }

                    object employee = null;
                    if (!string.IsNullOrEmpty(empId) && empLookup.TryGetValue(empId, out employee))
                    {
                    }
                    else if (!string.IsNullOrEmpty(payId) && empLookup.TryGetValue(payId, out employee))
                    {
                    }
                    else
                    {
                        continue;
                    }

                    var empRecord = (HrmEmployeeOfficialInfo)employee;
                    empRecord.GrossSalary = parsedAmount;
                    empRecord.Luser = model.Luser;
                    empRecord.ModifyDate = model.ModifyDate;
                    empRecord.Lip = model.Lip;
                    empRecord.Lmac = model.Lmac;
                    var normalized = rawMethod?.ToLower();
                    if (!string.IsNullOrWhiteSpace(normalized) && methodLookup.TryGetValue(normalized, out var mappedMethodId))
                    {
                        empRecord.DisbursementMethodId = mappedMethodId;
                    }

                    empToUpdate.Add(empRecord);
                }

                await empOfficialInfoRepo.BeginTransactionAsync();
                try
                {
                    foreach (HrmEmployeeOfficialInfo emp in empToUpdate)
                    {
                        await empOfficialInfoRepo.UpdateAsync(emp);
                    }

                    await empOfficialInfoRepo.CommitTransactionAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
