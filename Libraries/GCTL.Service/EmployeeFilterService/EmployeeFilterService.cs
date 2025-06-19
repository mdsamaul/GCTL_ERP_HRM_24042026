//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using GCTL.Core.Data;
//using GCTL.Core.ViewModels.EmployeeFilter;
//using GCTL.Data.Models;

//using Microsoft.EntityFrameworkCore;
//namespace GCTL.Service.EmployeeFilterService
//{
//    public class EmployeeFilterService : AppService<HrmEmployeeOfficialInfo>, IEmployeeFilterService
//    {
//        private readonly IRepository<CoreCompany> companyRepository;
//        private readonly IRepository<HrmDefDivision> divisionRepository;
//        private readonly IRepository<CoreAccessCode> accessCodeRepository;
//        private readonly IRepository<HrmEmployeeOfficialInfo> employeeOfficialInfoRepository;
//        private readonly IRepository<CoreBranch> branchRepository;
//        private readonly IRepository<HrmDefDepartment> departmentRepository;
//        private readonly IRepository<HrmDefDesignation> designationRepository;
//        private readonly IRepository<HrmEmployee> employeeRepository;
//        private readonly IRepository<HrmDefEmployeeStatus> employeeStatusRepository;
//        private readonly IRepository<HrmDefEmpType> empTypeRepository;

//        public EmployeeFilterService(
//            IRepository<CoreCompany> companyRepository,
//            IRepository<HrmDefDivision> divisionRepository,
//            IRepository<CoreAccessCode> accessCodeRepository,
//            IRepository<HrmEmployeeOfficialInfo> employeeOfficialInfoRepository,
//            IRepository<CoreBranch> branchRepository,
//            IRepository<HrmDefDepartment> departmentRepository,
//            IRepository<HrmDefDesignation> designationRepository,
//            IRepository<HrmEmployee> employeeRepository,
//            IRepository<HrmDefEmployeeStatus> employeeStatusRepository,
//            IRepository<HrmDefEmpType> empTypeRepository
//            )
//            : base(employeeOfficialInfoRepository)
//        {
//            this.companyRepository = companyRepository;
//            this.divisionRepository = divisionRepository;
//            this.accessCodeRepository = accessCodeRepository;
//            this.employeeOfficialInfoRepository = employeeOfficialInfoRepository;
//            this.branchRepository = branchRepository;
//            this.departmentRepository = departmentRepository;
//            this.designationRepository = designationRepository;
//            this.employeeRepository = employeeRepository;
//            this.employeeStatusRepository = employeeStatusRepository;
//            this.empTypeRepository = empTypeRepository;
//        }

//        public List<CoreCompany> GetAllCompany()
//        {
//            return companyRepository.GetAll().Where(e=>e.CompanyCode=="001").ToList();
//        }

//        public async Task<EmployeeFilterResultDto> GetFilterDataAsync(EmployeeFilterViewModel filter)
//        {
//            var result = new EmployeeFilterResultDto();

//            var query = from e in employeeOfficialInfoRepository.All().AsNoTracking()
//                        join emp in employeeRepository.All().AsNoTracking() on e.EmployeeId equals emp.EmployeeId 
//                        join c in companyRepository.All().AsNoTracking() on e.CompanyCode equals c.CompanyCode into companyGroup
//                        from c in companyGroup.DefaultIfEmpty()
//                        join b in branchRepository.All().AsNoTracking() on e.BranchCode equals b.BranchCode into branchGroup
//                        from b in branchGroup.DefaultIfEmpty()
//                        join d in departmentRepository.All().AsNoTracking() on e.DepartmentCode equals d.DepartmentCode into deptGroup
//                        from d in deptGroup.DefaultIfEmpty()
//                        join ds in designationRepository.All().AsNoTracking() on e.DesignationCode equals ds.DesignationCode into desigGroup
//                        from ds in desigGroup.DefaultIfEmpty()
//                        join status in employeeStatusRepository.All().AsNoTracking() on e.EmployeeStatus equals status.EmployeeStatusId into statusGroup
//                        from status in statusGroup.DefaultIfEmpty()
//                        join emptype in empTypeRepository.All().AsNoTracking() on e.EmpTypeCode equals emptype.EmpTypeCode into empTypeGroup
//                        from emptype in empTypeGroup.DefaultIfEmpty()
//                        select new
//                        {
//                            EmployeeId = e.EmployeeId,
//                            FirstName = emp.FirstName,
//                            LastName = emp.LastName,
//                            e.JoiningDate,
//                            e.CompanyCode,
//                            CompanyName = c.CompanyName,
//                            e.BranchCode,
//                            BranchName = b.BranchName,
//                            e.DepartmentCode,
//                            DepartmentName = d.DepartmentName,
//                            e.DesignationCode,
//                            DesignationName = ds.DesignationName,
//                            e.DivisionCode,
//                            EmployeeTypeName = emptype.EmpTypeName,
//                            EmployeeStatusId = e.EmployeeStatus,
//                            EmployeeStatusName = status.EmployeeStatus
//                        };

//            // Apply filters (for cascading effect)
//            //if (filter.CompanyCodes?.Any() == true)
//            //    query = query.Where(x => filter.CompanyCodes.Contains(x.CompanyCode));
//            if (filter.CompanyCodes == null || !filter.CompanyCodes.Any())
//                return new EmployeeFilterResultDto();
//            if (filter.BranchCodes?.Any() == true)
//                query = query.Where(x => filter.BranchCodes.Contains(x.BranchCode));

//            if (filter.DivisionCodes?.Any() == true)
//                query = query.Where(x => filter.DivisionCodes.Contains(x.DivisionCode));

//            if (filter.DepartmentCodes?.Any() == true)
//                query = query.Where(x => filter.DepartmentCodes.Contains(x.DepartmentCode));

//            if (filter.DesignationCodes?.Any() == true)
//                query = query.Where(x => filter.DesignationCodes.Contains(x.DesignationCode));

//            if (filter.EmployeeIDs?.Any() == true)
//                query = query.Where(x => filter.EmployeeIDs.Contains(x.EmployeeId));

//            if (filter.EmployeeStatuses?.Any() == true)
//                query = query.Where(x => filter.EmployeeStatuses.Contains(x.EmployeeStatusName));

//            // Materialize the filtered result once
//            var filteredData = await query.ToListAsync();

//            // Populate filtered Employees
//            result.Employees = filteredData.Select(x => new EmployeeListItemViewModel
//            {
//                EmployeeId = x.EmployeeId,
//                EmployeeName = string.Join(" ", new[] { x.FirstName, x.LastName }.Where(n => !string.IsNullOrWhiteSpace(n))),
//                JoiningDate = x.JoiningDate.HasValue ? x.JoiningDate.Value.ToString("dd/MM/yyyy") : "",
//                DesignationName = x.DesignationName,
//                DepartmentName = x.DepartmentName,
//                BranchName = x.BranchName,
//                CompanyName = x.CompanyName,
//                EmployeeTypeName = x.EmployeeTypeName,
//                EmployeeStatus = x.EmployeeStatusName
//            }).ToList();

//            // Filtered lookup values (cascading effect)
//            result.LookupData["companies"] = filteredData
//                .Where(x => x.CompanyCode != null && x.CompanyName != null)
//                .Select(x => new LookupItemDto { Code = x.CompanyCode, Name = x.CompanyName })
//                .Distinct()
//                .ToList();

//            result.LookupData["branches"] = filteredData
//                .Where(x => x.BranchCode != null && x.BranchName != null)
//                .Select(x => new LookupItemDto { Code = x.BranchCode, Name = x.BranchName })
//                .Distinct()
//                .ToList();

//            //result.LookupData["divisions"] = filteredData
//            //    .Where(x => x.DivisionCode != null && x.DivisionName != null)
//            //    .Select(x => new LookupItemDto { Code = x.DivisionCode, Name = x.DivisionName })
//            //    .Distinct()
//            //    .ToList();

//            result.LookupData["departments"] = filteredData
//                .Where(x => x.DepartmentCode != null && x.DepartmentName != null)
//                .Select(x => new LookupItemDto { Code = x.DepartmentCode, Name = x.DepartmentName })
//                .Distinct()
//                .ToList();

//            result.LookupData["designations"] = filteredData
//                .Where(x => x.DesignationCode != null && x.DesignationName != null)
//                .Select(x => new LookupItemDto { Code = x.DesignationCode, Name = x.DesignationName })
//                .Distinct()
//                .ToList();

//            result.LookupData["employees"] = filteredData
//                .Where(x => x.EmployeeId != null && x.FirstName != null)
//                .Select(x => new LookupItemDto
//                {
//                    Code = x.EmployeeId,
//                    Name = string.Join(" ", new[] { x.FirstName, x.LastName }.Where(n => !string.IsNullOrWhiteSpace(n))) + $" ({x.EmployeeId})"
//                })
//                .Distinct()
//                .ToList();

//            result.LookupData["employeeStatuses"] = filteredData
//                .Where(x => !string.IsNullOrWhiteSpace(x.EmployeeStatusName))
//                .Select(x => new LookupItemDto { Code = x.EmployeeStatusName, Name = x.EmployeeStatusName })
//                .Distinct()
//                .ToList();

//            return result;
//        }

//    }
//}
