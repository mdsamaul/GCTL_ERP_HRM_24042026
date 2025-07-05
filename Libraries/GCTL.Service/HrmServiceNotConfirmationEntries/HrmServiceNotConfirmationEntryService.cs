using GCTL.Core.Data;
using GCTL.Core.ViewModels.HrmPayMonths;
using GCTL.Core.ViewModels.HrmServiceNotConfirmEntries;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.HrmServiceNotConfirmationEntries
{
    public class HrmServiceNotConfirmationEntryService:AppService<HrmServiceNotConfirmationEntry>, IHrmServiceNotConfirmationEntryService
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
        private readonly IRepository<HrmPayMonth> payMonthRepository;

        private readonly IRepository<HrmServiceNotConfirmationEntry> entryRepository;

        public HrmServiceNotConfirmationEntryService(
            IRepository<HrmServiceNotConfirmationEntry> entryRepository,
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
            IRepository<HrmPayMonth> payMonthRepository) : base(entryRepository)
        {
            this.entryRepository = entryRepository;
            this.companyRepository = companyRepository;
            this.divisionRepository = divisionRepository;
            this.employeeOfficialInfoRepository = employeeOfficialInfoRepository;
            this.branchRepository = branchRepository;
            this.designationRepository = designationRepository;
            this.departmentRepository = departmentRepository;
            this.employeeRepository = employeeRepository;
            this.empTypeRepository = empTypeRepository;
            this.payMonthRepository = payMonthRepository;
            this.employmentNatureRepository = employmentNatureRepository;
            this.employeeStatusRepository = employeeStatusRepository;
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
                    var entries = await entryRepository.All()
                        .Where(e => batch.Contains(e.Tc))
                        .AsNoTracking()
                        .ToListAsync();

                    if (entries.Any())
                        await entryRepository.DeleteRangeAsync(entries);
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

        public Task<bool> EditAsync(HrmServiceNotConfirmViewModel model)
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> GenerateExcelSampleAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<string> GenerateSNCIdAsync()
        {
            try
            {
                var maxId = entryRepository.All()
                    .AsEnumerable() // Switch to LINQ-to-Objects
                    .Where(e => !string.IsNullOrEmpty(e.Sncid) && e.Sncid.All         (char.IsDigit))
                    .Select(e => int.Parse(e.Sncid))
                    .DefaultIfEmpty(0)
                    .Max();

                return (maxId + 1).ToString("D8");
            }
            catch
            {
                return "00000001";
            }
        }


        public Task<HrmServiceNotConfirmViewModel> GetByIdAsync(decimal id)
        {
            throw new NotImplementedException();
        }

        public async Task<EmployeeFilterResultDto> GetFilterEmployeeAsync(EmployeeFilterViewModel model)
        {
            var result = new EmployeeFilterResultDto();

            var query = from emp in employeeRepository.All().AsNoTracking()
                        join e in employeeOfficialInfoRepository.All().AsNoTracking() on emp.EmployeeId equals e.EmployeeId
                        join c in companyRepository.All().AsNoTracking() on e.CompanyCode equals c.CompanyCode
                        join b in branchRepository.All().AsNoTracking() on e.BranchCode equals b.BranchCode into branchGroup
                        from b in branchGroup.DefaultIfEmpty()
                        join dep in departmentRepository.All().AsNoTracking()
                        on e.DepartmentCode equals dep.DepartmentCode into departmentGroup
                        from dep in departmentGroup.DefaultIfEmpty()
                        join des in designationRepository.All().AsNoTracking() on e.DesignationCode equals des.DesignationCode into designationGroup
                        from des in designationGroup.DefaultIfEmpty()
                        join eType in empTypeRepository.All().AsNoTracking() on e.EmpTypeCode equals eType.EmpTypeCode into eTypeGroup
                        from eType in eTypeGroup.DefaultIfEmpty()
                        join eStatus in employeeStatusRepository.All().AsNoTracking() on e.EmployeeStatus equals eStatus.EmployeeStatusId into eStatusGroup
                        from eStatus in eStatusGroup.DefaultIfEmpty()

                        where (e.EmployeeStatus.Equals("01") && e.EmpTypeCode.Equals("02"))

                        select new
                        {
                            EmployeeId = e.EmployeeId,
                            FirstName = emp.FirstName,
                            LastName = emp.LastName,
                            e.CompanyCode,
                            CompanyName = c.CompanyName,
                            BranchName = b.BranchName,
                            DepartmentName = dep.DepartmentName,
                            DesignationName = des.DesignationName,
                            GrossSalary = e.GrossSalary,
                            e.JoiningDate,
                            ProbetionPeriod = "",
                            EndDate = "",
                            ServiceLength = e.JoiningDate.HasValue ? (DateTime.Today.Year - e.JoiningDate.Value.Year) * 12 + DateTime.Today.Month - e.JoiningDate.Value.Month + " months and " + (DateTime.Today - e.JoiningDate.Value.AddMonths(((DateTime.Today.Year - e.JoiningDate.Value.Year) * 12 + DateTime.Today.Month - e.JoiningDate.Value.Month))).Days + " days" : ""
                        };

            query = query.Where(x => x.CompanyCode == "001");

            if (!string.IsNullOrWhiteSpace(model.EmployeeID))
                query = query.Where(x => x.EmployeeId == model.EmployeeID);

            if (!string.IsNullOrWhiteSpace(model.CompanyCode))
                query = query.Where(x => x.CompanyCode == model.CompanyCode);

            var filteredData = await query.ToListAsync();

            result.LookupData["companies"] = filteredData
                .Where(x => x.CompanyCode != null && x.CompanyName != null)
                .Select(x => new LookupItemDto { Code = x.CompanyCode, Name = x.CompanyName })
                .Distinct()
                .ToList();

            result.LookupData["employees"] = filteredData
                .Where(x => x.EmployeeId != null)
                .Select(x => new LookupItemDto {
                    Code = x.EmployeeId,
                    Name = string.Join(" ", new[] { x.FirstName, x.LastName }.Where(n => !string.IsNullOrWhiteSpace(n))) + $" ({x.EmployeeId})"
                })
                .Distinct()
                .ToList();

            result.Employees = filteredData.Select(x => new EmployeeListItemViewModel
            {
                EmployeeId = x.EmployeeId,
                Name = string.Join(" ", new[] { x.FirstName, x.LastName }.Where(n => !string.IsNullOrWhiteSpace(n))),
                JoiningDate = x.JoiningDate.HasValue ? x.JoiningDate.Value.ToString("dd/MM/yyyy") : "",
                DesignationName = x.DesignationName,
                DepartmentName = x.DepartmentName,
                GrossSalary = x.GrossSalary.ToString(),
                ProbationPeriod = x.ProbetionPeriod,
                EndOn = x.EndDate,
                ServiceLength = x.ServiceLength
            }).ToList();

            return result;
        }

        public async Task<(List<HrmServiceNotConfirmViewModel> Data, int TotalRecords)> GetPaginatedDataAsync(string searchValue, int page, int pageSize, string sortColumn, string sortDirection)
        {
            var query = from e in entryRepository.All().AsNoTracking()
                        join em in employeeRepository.All().AsNoTracking() on e.EmployeeId equals em.EmployeeId
                        join ei in employeeOfficialInfoRepository.All().AsNoTracking() on em.EmployeeId equals ei.EmployeeId
                        select new HrmServiceNotConfirmViewModel
                        {
                            Tc = e.Tc,
                            Sncid = e.Sncid,
                            EmployeeId = e.EmployeeId,
                            EmployeeName = (em.FirstName ?? "") + (em.LastName != null && em.LastName != "" ? " " + em.LastName : ""),

                            EffectiveDate = e.EffectiveDate.HasValue ? e.EffectiveDate.Value.Date : null,
                            DuePaymentDate = e.DuePaymentDate.HasValue ? e.DuePaymentDate.Value.Date : null,
                            RefLetterDate = e.RefLetterDate.HasValue ? e.RefLetterDate.Value.Date : null,
                            RefLetterNo = e.RefLetterNo ?? "",
                            Remarks = e.Remarks ?? ""
                        };

            var materializedQuery = await query.ToListAsync();

            IEnumerable<HrmServiceNotConfirmViewModel> filterQuery = materializedQuery;

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                filterQuery = filterQuery.Where(d =>
                (d.EmployeeName?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ?? false) ||
                 (d.EmployeeId?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ?? false) ||
                 (d.Sncid?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ?? false) ||
                 (d.EffectiveDate?.ToString().Contains(searchValue, StringComparison.OrdinalIgnoreCase) ?? false) ||
                 (d.DuePaymentDate?.ToString().Contains(searchValue, StringComparison.OrdinalIgnoreCase) ?? false) ||
                 (d.RefLetterNo?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ?? false) ||
                 (d.RefLetterDate?.ToString().Contains(searchValue, StringComparison.OrdinalIgnoreCase) ?? false) ||
                 (d.Remarks?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ?? false)
                );
            }

            var totalRecords = filterQuery.Count();

            if(!string.IsNullOrWhiteSpace(sortColumn) && !string.IsNullOrWhiteSpace(sortDirection))
            {
                filterQuery = sortColumn.ToLower() switch
                {
                    "sncid" => sortDirection.ToLower() == "asc" ? filterQuery.OrderBy(x => x.Sncid) : filterQuery.OrderByDescending(x => x.Sncid),
                    "employeeid" => sortDirection.ToLower() == "asc" ? filterQuery.OrderBy(a => a.EmployeeId) : filterQuery.OrderByDescending(a => a.EmployeeId),
                    "employeename" => sortDirection.ToLower() == "asc" ? filterQuery.OrderBy(a => a.EmployeeName) : filterQuery.OrderByDescending(a => a.EmployeeName),
                    "effectivedate" => sortDirection.ToLower() == "asc" ?
                    filterQuery.OrderBy(a => a.EffectiveDate) : filterQuery.OrderByDescending(a => a.EffectiveDate),
                    "duepaymentdate" => sortDirection.ToLower() == "asc" ?
                    filterQuery.OrderBy(a => a.DuePaymentDate) : filterQuery.OrderByDescending(a => a.DuePaymentDate),
                    "refletterno" => sortDirection.ToLower() == "asc" ? filterQuery.OrderBy(a => a.RefLetterNo) : filterQuery.OrderByDescending(a => a.RefLetterNo),
                    "refletterdate" => sortDirection.ToLower() == "asc" ? filterQuery.OrderBy(a => a.RefLetterDate) : filterQuery.OrderByDescending(a => a.RefLetterDate),
                    "remarks" => sortDirection.ToLower() == "asc" ? filterQuery.OrderBy(a => a.Remarks) : filterQuery.OrderByDescending(a => a.Remarks),
                    _ => filterQuery.OrderBy(a => a.Tc)
                };
            }
            else
            {
                filterQuery = filterQuery.OrderBy(a => a.Tc);
            }

            var data = pageSize < 0
                ? filterQuery.ToList()
                : filterQuery.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return (data, totalRecords);
        }

        public Task<List<HrmPayMonthViewModel>> GetPayMonthsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<bool> ProcessExcelFileAsync(Stream stream, HrmServiceNotConfirmViewModel model)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> SaveAsync(HrmServiceNotConfirmViewModel model)
        {
            if (model == null || model.EmployeeId == null)
                return false;

            try
            {
                var newId = await GenerateSNCIdAsync();

                HrmServiceNotConfirmationEntry record = new HrmServiceNotConfirmationEntry
                {
                    Sncid = newId,
                    EmployeeId = model.EmployeeId,
                    EffectiveDate = model.EffectiveDate,
                    DuePaymentDate = model.DuePaymentDate,
                    RefLetterNo = model.RefLetterNo ?? "",  // "" if you don't want null
                    RefLetterDate = model.RefLetterDate,
                    Remarks = model.Remarks ?? "",
                    Ldate = model.Ldate,
                    Lip = model.Lip,
                    Lmac = model.Lmac,
                    Luser = model.Luser,
                    CompanyCode = model.CompanyCode,
                };

                await entryRepository.AddAsync(record);
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
