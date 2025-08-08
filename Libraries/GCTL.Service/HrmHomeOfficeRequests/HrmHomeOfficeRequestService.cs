using DocumentFormat.OpenXml.Bibliography;
using GCTL.Core.Data;
using GCTL.Core.ViewModels.HrmHomeOfficeRequests;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.HrmHomeOfficeRequests
{
    public class HrmHomeOfficeRequestService : AppService<HrmHomeOfficeRequest>, IHrmHomeOfficeRequestService
    {
        private readonly IRepository<CoreCompany> companyRepository;
        private readonly IRepository<HrmDefDivision> divisionRepository;
        private readonly IRepository<HrmEmployeeOfficialInfo> employeeOfficialInfoRepository;
        private readonly IRepository<CoreBranch> branchRepository;
        private readonly IRepository<HrmDefDepartment> departmentRepository;
        private readonly IRepository<HrmDefDesignation> designationRepository;
        private readonly IRepository<HrmEmployee> employeeRepository;
        private readonly IRepository<HrmDefEmployeeStatus> employeeStatusRepository;

        private readonly IRepository<HrmHomeOfficeRequest> entryRepository;

        public HrmHomeOfficeRequestService(
            IRepository<HrmHomeOfficeRequest> entryRepository,
            IRepository<CoreCompany> companyRepository,
            IRepository<HrmDefDivision> divisionRepository,
            IRepository<HrmEmployeeOfficialInfo> employeeOfficialInfoRepository,
            IRepository<CoreBranch> branchRepository,
            IRepository<HrmDefDepartment> departmentRepository,
            IRepository<HrmDefDesignation> designationRepository,
            IRepository<HrmEmployee> employeeRepository,
            IRepository<HrmDefEmployeeStatus> employeeStatusRepository)
            : base(entryRepository)
        {
            this.entryRepository = entryRepository;
            this.companyRepository = companyRepository;
            this.divisionRepository = divisionRepository;
            this.employeeOfficialInfoRepository = employeeOfficialInfoRepository;
            this.branchRepository = branchRepository;
            this.departmentRepository = departmentRepository;
            this.designationRepository = designationRepository;
            this.employeeRepository = employeeRepository;
            this.employeeStatusRepository = employeeStatusRepository;
        }

        public async Task<bool> BulkDeleteAsync(List<decimal> id)
        {
            if (id == null || !id.Any())
                return false;

            const int batchSize = 100;

            await entryRepository.BeginTransactionAsync();

            try
            {
                for(int i = 0;i<id.Count; i += batchSize)
                {
                    var batch = id.Skip(i).Take(batchSize);
                    var entries = await entryRepository.All()
                        .Where(e => batch.Contains(e.Tc))
                        .AsNoTracking()
                        .ToListAsync();

                    if (entries.Any())
                    {
                        await entryRepository.DeleteRangeAsync(entries);
                    }
                }

                await entryRepository.CommitTransactionAsync();
                return true;
            }
            catch
            {
                await entryRepository.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<bool> EditAsync(HrmHomeOfficeRequestSetupViewModel model)
        {
            if (model == null || model.Tc == 0 )
                return false;

            await entryRepository.BeginTransactionAsync();

            try
            {
                var exRecord = await entryRepository.GetByIdAsync(model.Tc);

                if (exRecord == null)
                    return false;

                //var duplicateRecord = await entryRepository.All()
                //    .Where(e=>e.EmployeeId == model.EmployeeId && )

                exRecord.HremployeeId = model.HremployeeId;
                exRecord.StartDate = model.StartDate;
                exRecord.EndDate = model.EndDate;
                exRecord.Reason = model.Reason;
                exRecord.ApprovalStatus = model.ApprovalStatus;
                exRecord.Luser = model.Luser;
                exRecord.Lip = model.Lip;
                exRecord.ModifyDate = model.ModifyDate;
                exRecord.Lmac = model.Lmac;
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

        public async Task<HrmHomeOfficeRequestSetupViewModel> GetByIdAsync(decimal id)
        {
            var entry = await entryRepository.GetByIdAsync(id);

            if (entry == null) return null;

            var viewModel = new HrmHomeOfficeRequestSetupViewModel()
            {
                Tc = entry.Tc,
                Horid = entry.Horid,
                EmployeeId = entry.EmployeeId,
                HremployeeId = entry.HremployeeId,
                RequestDate = entry.RequestDate,
                StartDate = entry.StartDate,
                EndDate = entry.EndDate,
                Reason = entry.Reason,
                ApprovalStatus = entry.ApprovalStatus,
                CompanyCode = entry.CompanyCode
            };
            return viewModel;
        }

        public async Task<EmployeeListItemViewModel> GetDataByEmpId(string selectedEmpId)
        {
            var result = new EmployeeListItemViewModel();

            var query = from e in employeeOfficialInfoRepository.All().AsNoTracking()
                        join emp in employeeRepository.All().AsNoTracking() on e.EmployeeId equals emp.EmployeeId
                        join c in companyRepository.All().AsNoTracking() on e.CompanyCode equals c.CompanyCode
                        join dep in departmentRepository.All().AsNoTracking()
                        on e.DepartmentCode equals dep.DepartmentCode into departmentGroup
                        from dep in departmentGroup.DefaultIfEmpty()
                        join des in designationRepository.All().AsNoTracking() on e.DesignationCode equals des.DesignationCode into designationGroup
                        from des in designationGroup.DefaultIfEmpty()
                        join eStatus in employeeStatusRepository.All().AsNoTracking() on e.EmployeeStatus equals eStatus.EmployeeStatusId into eStatusGroup
                        from eStatus in eStatusGroup.DefaultIfEmpty()
                        where e.EmployeeId == selectedEmpId
                        select new
                        {
                            EmployeeId = e.EmployeeId,
                            emp.FirstName,
                            emp.LastName,
                            e.CompanyCode,
                            CompanyName = c.CompanyName,
                            DepartmentName = dep.DepartmentName,
                            DesignationName = des.DesignationName,
                            e.JoiningDate
                        };

            var data = await query.FirstOrDefaultAsync();

            if (data == null)
                return null;

            return new EmployeeListItemViewModel
            {
                EmployeeId = data.EmployeeId,
                Name = string.Join(" ", new[] { data.FirstName, data.LastName }.Where(n => !string.IsNullOrWhiteSpace(n))),
                JoiningDate = data.JoiningDate.HasValue ? data.JoiningDate.Value.ToString("dd/MM/yyyy") : "",
                DepartmentName = data.DepartmentName,
                DesignationName = data.DesignationName,
            };
        }

        public async Task<EmployeeFilterResultDto> GetFilterEmployeeAsync(EmployeeFilterViewModel model)
        {
            var result = new EmployeeFilterResultDto();

            var query = from emp in employeeRepository.All().AsNoTracking()
                        join e in employeeOfficialInfoRepository.All().AsNoTracking() on emp.EmployeeId equals e.EmployeeId
                        join c in companyRepository.All().AsNoTracking() on e.CompanyCode equals c.CompanyCode
                        join des in designationRepository.All().AsNoTracking() on e.DesignationCode equals des.DesignationCode into designationGroup
                        from des in designationGroup.DefaultIfEmpty()
                        join eStatus in employeeStatusRepository.All().AsNoTracking() on e.EmployeeStatus equals eStatus.EmployeeStatusId into eStatusGroup
                        from eStatus in eStatusGroup.DefaultIfEmpty()

                        where e.EmployeeStatus.Equals("01")

                        select new
                        {
                            EmployeeId = e.EmployeeId,
                            FirstName = emp.FirstName,
                            LastName = emp.LastName,
                            e.CompanyCode,
                            CompanyName = c.CompanyName
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
                .DistinctBy(x => new { x.Code, x.Name })
                .ToList();

            result.LookupData["employees"] = filteredData
                .Where(x => x.EmployeeId != null)
                .Select(x => new LookupItemDto
                {
                    Code = x.EmployeeId,
                    Name = string.Join(" ", new[] { x.FirstName, x.LastName }.Where(n => !string.IsNullOrWhiteSpace(n))) + $" ({x.EmployeeId})"
                })
                .Distinct()
                .ToList();

            return result;
        }

        public async Task<List<LookupItemDto>> GetHodAsync()
        {
            var result = new List<LookupItemDto>();
            var query = from e in employeeRepository.All().AsNoTracking()
                        join eoi in employeeOfficialInfoRepository.All().AsNoTracking() on e.EmployeeId equals eoi.EmployeeId
                        where eoi.EmployeeStatus == "01"
                        select new
                        {
                            e.EmployeeId,
                            e.FirstName,
                            e.LastName
                        };

            result = await query
                .Select(x => new LookupItemDto
                {
                    Code = x.EmployeeId,
                    Name = $"{x.FirstName} {x.LastName} ({x.EmployeeId})"
                })
                .ToListAsync();

            return result;
        }

        public async Task<(List<HrmHomeOfficeRequestSetupViewModel> Data, int TotalRecords, int filteredRecords)> GetPaginatedDataAsync(string searchValue, int page, int pageSize, string sortColumn, string sortDirection)
        {
            var query = from e in entryRepository.All().AsNoTracking()
                        select new HrmHomeOfficeRequestSetupViewModel
                        {
                            Tc = e.Tc,
                            Horid = e.Horid,
                            EmployeeId = e.EmployeeId,
                            StartDate = e.StartDate,
                            EndDate = e.EndDate,
                            RequestDate = e.RequestDate,
                            ApprovalStatus = e.ApprovalStatus,
                            Reason = e.Reason,
                            EntryBy = e.EntryBy,
                        };

            var materializedQuery = await query.ToListAsync();

            var totalRecords = materializedQuery.Count;

            IEnumerable<HrmHomeOfficeRequestSetupViewModel> filterQuery = materializedQuery;

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                filterQuery = filterQuery.Where(d =>
                    (d.Horid?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (d.RequestDate?.ToString("dd/MM/yyyy").Contains(searchValue, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (d.ApprovalStatus?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (d.StartDate?.ToString("dd/MM/yyyy").Contains(searchValue, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (d.EndDate?.ToString("dd/MM/yyyy").Contains(searchValue, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (d.EmployeeId?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (d.Reason?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (d.EntryBy?.Contains(searchValue, StringComparison.OrdinalIgnoreCase) ?? false)
                );
            }


            var filteredData = filterQuery.Count();

            if(!string.IsNullOrWhiteSpace(sortColumn) && !string.IsNullOrWhiteSpace(sortDirection))
            {
                filterQuery = sortColumn.ToLower() switch 
                { 
                    "horid" => sortDirection.ToLower() == "asc" ? filterQuery.OrderBy(d => d.Horid) : filterQuery.OrderByDescending(d => d.Horid),
                    "requestdate" => sortDirection.ToLower() == "asc" ? filterQuery.OrderBy(d => d.RequestDate) : filterQuery.OrderByDescending(d => d.RequestDate),
                    "approvalstatus" => sortDirection.ToLower() == "asc" ? filterQuery.OrderBy(d => d.ApprovalStatus) : filterQuery.OrderByDescending(d => d.ApprovalStatus),
                    "employeeid" => sortDirection.ToLower() == "asc" ? filterQuery.OrderBy(d => d.EmployeeId) : filterQuery.OrderByDescending(d => d.EmployeeId),
                    "startdate" => sortDirection.ToLower() == "asc" ? filterQuery.OrderBy(d => d.StartDate) : filterQuery.OrderByDescending(d => d.StartDate),
                    "enddate" => sortDirection.ToLower() == "asc" ? filterQuery.OrderBy(d => d.EndDate) : filterQuery.OrderByDescending(d => d.EndDate),
                    "reason" => sortDirection.ToLower() == "asc" ? filterQuery.OrderBy(d => d.Reason) : filterQuery.OrderByDescending(d => d.Reason),
                    "entryby" => sortDirection.ToLower() == "asc" ? filterQuery.OrderBy(d => d.EntryBy) : filterQuery.OrderByDescending(d => d.EntryBy),
                    _ => filterQuery.OrderBy(d => d.Tc) // Default sorting by Tc
                };
            }
            else
            {
                filterQuery = filterQuery.OrderBy(d => d.Tc); // Default sorting by Tc
            }

            var data = filterQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return (data, totalRecords, filteredData);
        }

        public async Task<bool> SaveAsync(HrmHomeOfficeRequestSetupViewModel model)
        {
            if (model == null || model.EmployeeId == null)
                return false;


            try
            {
                int nextId = 1;
                var lastEntry = entryRepository.All().OrderByDescending(e => e.Tc).FirstOrDefault();
                if (lastEntry != null && int.TryParse(lastEntry.Horid, out int lastNumber)) nextId = lastNumber + 1;

                List<HrmHomeOfficeRequest> records = new();

                records.Add(new HrmHomeOfficeRequest
                {
                    Horid = nextId.ToString("D8"),
                    RequestDate = model.RequestDate,
                    ApprovalStatus = "Pending",
                    HremployeeId = model.HremployeeId,
                    EmployeeId = model.EmployeeId,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    Reason = model.Reason,
                    CompanyCode = model.CompanyCode,
                    EntryBy = model.Luser,
                    Ldate = model.Ldate,
                    Lip = model.Lip,
                    Lmac = model.Lmac,
                    Luser = model.Luser
                });

                await entryRepository.AddRangeAsync(records);
                return true;
            }
            catch (Exception ex)
            {

                return false;
            }
            
        }

        public async Task<bool> HasDuplicate(string empId, DateTime? startDate, DateTime? endDate, string? hodId = null)
        {
            if (string.IsNullOrEmpty(empId))
                return false;

            try
            {
                var query = entryRepository.All()
                    .Where(x => x.EmployeeId == empId);

                if (!string.IsNullOrEmpty(hodId))
                {
                    query = query.Where(x => x.Horid != hodId);
                }

                var hasDuplicate = await query
                    .AnyAsync(x => (startDate >= x.StartDate && startDate <= x.EndDate) ||
                                  (endDate >= x.StartDate && endDate <= x.EndDate) ||    
                                  (startDate <= x.StartDate && endDate >= x.EndDate));   

                return hasDuplicate;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
