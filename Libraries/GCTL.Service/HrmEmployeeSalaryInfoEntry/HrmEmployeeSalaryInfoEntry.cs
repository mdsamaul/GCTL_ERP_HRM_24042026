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
    public class HrmEmployeeSalaryInfoEntry : AppService<HrmEmployeeOfficialInfo>, IHrmEmployeeSalaryInfoEntry
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

        public HrmEmployeeSalaryInfoEntry(
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
            IRepository<HrmEisDefEmploymentNature> empNatureRepo) : base(empOfficialInfoRepo)
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
        }

        public Task<bool> BulkDeleteAsync(List<decimal> autoIds)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> BulkEditAsync(HrmEmployeeSalaryInfoEntryViewModel model)
        {
            if (model == null || !model.SalaryInfoUpdate.Any())
                return false;

            await empOfficialInfoRepo.BeginTransactionAsync();

            try
            {
                foreach (var row in model.SalaryInfoUpdate)
                {
                    var exRecord = await empOfficialInfoRepo.GetByIdAsync(row.AutoId);

                    if(exRecord == null) continue;

                    exRecord.GrossSalary = row.GrossSalary ?? 0.00m;
                    exRecord.DisbursementMethodId=row.DisbursementMethodId;

                    await empOfficialInfoRepo.UpdateAsync(exRecord);
                }

                await empOfficialInfoRepo.CommitTransactionAsync();

                return true;
            }
            catch (Exception ex)
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

                return await package.GetAsByteArrayAsync();
            }
        }

        public async Task<(List<EmployeeFilterResultDto> data, int TotalRecords)> GetFilterEmployeeAsync(EmployeeFilterViewModel model, string searchValue, int page, int pageSize, string sortColumn, string sortDirection)
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
                        join sep in separationRepo.All().AsNoTracking() on e.EmployeeId equals sep.EmployeeId into separationGroup
                        from sep in separationGroup.DefaultIfEmpty()
                        join des in disbursementRepo.All().AsNoTracking() on e.DisbursementMethodId equals des.DisbursementMethodId into disbursGroup
                        from des in disbursGroup.DefaultIfEmpty()
                        select new
                        {
                            EmployeeId = e.EmployeeId,
                            PayId = e.PayId,
                            FirstName = emp.FirstName,
                            LastName = emp.LastName,
                            empName = emp.FirstName+" "+emp.LastName,
                            e.BranchCode,
                            BranchName = b.BranchName,
                            e.CompanyCode,
                            CompanyName = c.CompanyName,
                            e.DesignationCode,
                            DesignationName = ds.DesignationName,
                            e.DepartmentCode,
                            DepartmentName = d.DepartmentName,
                            e.EmpTypeCode,
                            EmpTypename = emptype.EmpTypeName,
                            e.EmployeeStatus,
                            EmployeeStatusName = status.EmployeeStatus,
                            e.EmploymentNatureId,
                            EmpNatureName = empNature.EmploymentNature,
                            e.JoiningDate,
                            sep.SeparationDate,
                            e.GrossSalary,
                            e.DisbursementMethodId,
                            DisbursementName = des.DisbursementMethod,
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
                query = query.Where(x => x.JoiningDate.Value.Date >= model.JoiningDateFrom.Value.Date);

            if (model.JoiningDateTO != null)
                query = query.Where(x => x.JoiningDate.Value.Date <= model.JoiningDateTO.Value.Date);

            if (model.EmployeeStatuses?.Any() == true)
                query = query.Where(x => model.EmployeeStatuses.Contains(x.EmployeeStatus));

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                query = query.Where(x =>
                    x.FirstName.Contains(searchValue) ||
                    x.LastName.Contains(searchValue) ||
                    x.PayId.Contains(searchValue) ||
                    x.EmployeeId.Contains(searchValue));
            }

            var totalRecords = await query.CountAsync();

            if (!string.IsNullOrWhiteSpace(sortColumn))
            {
                switch (sortColumn.ToLower())
                {
                    case "employeename":
                        query = sortDirection?.ToLower() == "desc"
                            ? query.OrderByDescending(x => x.FirstName).ThenByDescending(x => x.LastName)
                            : query.OrderBy(x => x.FirstName).ThenBy(x => x.LastName);
                        break;
                    case "payid":
                        query = sortDirection?.ToLower() == "desc"
                            ? query.OrderByDescending(x => x.PayId)
                            : query.OrderBy(x => x.PayId);
                        break;
                    case "joiningdate":
                        query = sortDirection?.ToLower() == "desc"
                            ? query.OrderByDescending(x => x.JoiningDate)
                            : query.OrderBy(x => x.JoiningDate);
                        break;
                    case "designationname":
                        query = sortDirection?.ToLower() == "desc"
                            ? query.OrderByDescending(x => x.DesignationName)
                            : query.OrderBy(x => x.DesignationName);
                        break;
                    case "departmentname":
                        query = sortDirection?.ToLower() == "desc"
                            ? query.OrderByDescending(x => x.DepartmentName)
                            : query.OrderBy(x => x.DepartmentName);
                        break;
                    case "grosssalary":
                        query = sortDirection?.ToLower() == "desc"
                            ? query.OrderByDescending(x => x.GrossSalary)
                            : query.OrderBy(x => x.GrossSalary);
                        break;
                    default:
                        query = query.OrderBy(x => x.EmployeeId); // Default sorting
                        break;
                }
            }
            else
            {
                query = query.OrderBy(x => x.EmployeeId); // Default sorting if no sort column specified
            }

            // Apply pagination
            var paginatedData = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Map to result DTOs
            var employees = paginatedData.Select(x => new EmployeeListItemViewModel
            {
                EmployeeId = x.EmployeeId,
                EmployeeName = string.Join(" ", new[] { x.FirstName, x.LastName }.Where(n => !string.IsNullOrWhiteSpace(n))),
                PayId = x.PayId,
                DesignationName = x.DesignationName,
                DepartmentName = x.DepartmentName,
                EmployeeTypeName = x.EmpTypename,
                EmploymentNature = x.EmpNatureName,
                JoiningDate = x.JoiningDate.HasValue ? x.JoiningDate.Value.ToString("dd/MM/yyyy") : "",
                SeparationDate = x.SeparationDate.ToString("dd/MM/yyyy") ?? "", // Fixed null reference
                EmployeeStatus = x.EmployeeStatus,
                GrossSalary = x.GrossSalary,
                DisbursementMethodId = x.DisbursementMethodId,
                DisbursementMethodName = x.DisbursementName
            }).ToList();

            // For lookup data, you might want to get this from a separate method or cache
            // to avoid performance issues, but here's the implementation:
            var allFilteredData = await (from e in empOfficialInfoRepo.All().AsNoTracking()
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
                                         where e.CompanyCode == "001"
                                         select new
                                         {
                                             EmployeeId = e.EmployeeId,
                                             FirstName = emp.FirstName,
                                             LastName = emp.LastName,
                                             e.BranchCode,
                                             BranchName = b.BranchName,
                                             e.CompanyCode,
                                             CompanyName = c.CompanyName,
                                             e.DesignationCode,
                                             DesignationName = ds.DesignationName,
                                             e.DepartmentCode,
                                             DepartmentName = d.DepartmentName,
                                             e.EmpTypeCode,
                                             EmpTypename = emptype.EmpTypeName,
                                             EmployeeStatusName = status.EmployeeStatus,
                                             e.EmploymentNatureId,
                                             EmpNatureName = empNature.EmploymentNature,
                                         }).ToListAsync();

            result.LookupData = new Dictionary<string, List<LookupItemDto>>();

            result.LookupData["companies"] = allFilteredData
                .Where(x => x.CompanyCode != null && x.CompanyName != null)
                .Select(x => new LookupItemDto { Code = x.CompanyCode, Name = x.CompanyName })
                .Distinct()
                .ToList();

            result.LookupData["branches"] = allFilteredData
                .Where(x => x.BranchCode != null && x.BranchName != null)
                .Select(x => new LookupItemDto { Code = x.BranchCode, Name = x.BranchName })
                .Distinct()
                .ToList();

            result.LookupData["departments"] = allFilteredData
                .Where(x => x.DepartmentCode != null && x.DepartmentName != null)
                .Select(x => new LookupItemDto { Code = x.DepartmentCode, Name = x.DepartmentName })
                .Distinct()
                .ToList();

            result.LookupData["designations"] = allFilteredData
                .Where(x => x.DesignationCode != null && x.DesignationName != null)
                .Select(x => new LookupItemDto { Code = x.DesignationCode, Name = x.DesignationName })
                .Distinct()
                .ToList();

            result.LookupData["employees"] = allFilteredData
                .Where(x => x.EmployeeId != null && x.FirstName != null)
                .Select(x => new LookupItemDto
                {
                    Code = x.EmployeeId,
                    Name = string.Join(" ", new[] { x.FirstName, x.LastName }.Where(n => !string.IsNullOrWhiteSpace(n))) + $" ({x.EmployeeId})"
                })
                .Distinct()
                .ToList();

            result.LookupData["employeeStatuses"] = allFilteredData
                .Where(x => !string.IsNullOrWhiteSpace(x.EmployeeStatusName))
                .Select(x => new LookupItemDto { Code = x.EmployeeStatusName, Name = x.EmployeeStatusName })
                .Distinct()
                .ToList();

            result.LookupData["employeeType"] = allFilteredData
                .Where(x => !string.IsNullOrWhiteSpace(x.EmpTypeCode) && !string.IsNullOrWhiteSpace(x.EmpTypename))
                .Select(x => new LookupItemDto { Code = x.EmpTypeCode, Name = x.EmpTypename })
                .Distinct()
                .ToList();

            result.LookupData["employmentNature"] = allFilteredData
                .Where(x => x.EmploymentNatureId != null && !string.IsNullOrWhiteSpace(x.EmpNatureName))
                .Select(x => new LookupItemDto { Code = x.EmploymentNatureId.ToString(), Name = x.EmpNatureName })
                .Distinct()
                .ToList();

            result.Employees = employees;

            return (new List<EmployeeFilterResultDto> { result }, totalRecords);
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
            using var package = new ExcelPackage(stream);

            var worksheet = package.Workbook.Worksheets[0];

            int rowCount = worksheet.Dimension.Rows;
            if (rowCount <= 1) return false;

            //var validationErrors = new List<string>();
            var record= new List<EmployeeListItemViewModel>();

            var methodMap = await disbursementRepo.All().ToListAsync();
            var methodLookup = methodMap.ToDictionary(t => t.DisbursementMethodId, t => t.DisbursementMethod, StringComparer.OrdinalIgnoreCase);

            var allEmp = await empOfficialInfoRepo.All().ToListAsync();
            var empLookup= new Dictionary<string,object>();

            foreach(var emp in allEmp)
            {
                if (!string.IsNullOrWhiteSpace(emp.EmployeeId))
                    empLookup[emp.EmployeeId] = emp;
                if(!string.IsNullOrWhiteSpace(emp.PayId))
                    empLookup[emp.PayId] = emp;
            }

            var empToUpdate = new List<object>();

            for(int row = 2; row <= rowCount; row++)
            {
                var empId = worksheet.Cells[row, 1].Text?.Trim();
                var payId = worksheet.Cells[row, 2].Text?.Trim();
                var rawMethod = worksheet.Cells[row, 3].Text?.Trim();
                var amount = worksheet.Cells[row, 4].Text?.Trim();

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
                if (methodLookup.TryGetValue(rawMethod, out var mappedMethod))
                {
                    empRecord.DisbursementMethodId = mappedMethod;
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
    }
}
