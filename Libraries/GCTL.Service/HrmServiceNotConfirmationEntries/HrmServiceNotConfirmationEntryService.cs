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
            this.employeeOfficialInfoRepository = employeeOfficialInfoRepository;
            this.empTypeRepository = empTypeRepository;
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

        public Task<string> GenerateSNCIdAsync()
        {
            throw new NotImplementedException();
        }

        public Task<HrmServiceNotConfirmViewModel> GetByIdAsync(decimal id)
        {
            throw new NotImplementedException();
        }

        public Task<EmployeeFilterResultDto> GetFilterEmployeeAsync(EmployeeFilterViewModel model)
        {
            throw new NotImplementedException();
        }

        public Task<(List<HrmServiceNotConfirmViewModel> Data, int TotalRecords)> GetPaginatedDataAsync(string searchValue, int page, int pageSize, string sortColumn, string sortDirection)
        {
            throw new NotImplementedException();
        }

        public Task<List<HrmPayMonthViewModel>> GetPayMonthsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<bool> ProcessExcelFileAsync(Stream stream, HrmServiceNotConfirmViewModel model)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SaveAsync(HrmServiceNotConfirmViewModel model)
        {
            throw new NotImplementedException();
        }
    }
}
