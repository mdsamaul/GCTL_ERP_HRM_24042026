using GCTL.Core.Data;
using GCTL.Core.ViewModels.HrmServiceBulkConfimationEntry;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.HrmServiceBulkConfirmationEntries
{
    public class HrmServiceBulkConfirmationEntryService:AppService<HrmEmployeeOfficialInfo>,IHrmServiceBulkConfirmationEntryService
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
        private readonly IRepository<CorePeriodInfo> periodInfoRepo;
        private readonly IRepository<HrmDefProbationPeriodExtension> ppExtensionRepo;

        public HrmServiceBulkConfirmationEntryService(
            IRepository<CoreCompany> companyRepo,
            IRepository<HrmEmployeeOfficialInfo> empOfficialInfoRepo,
            IRepository<CoreBranch> branchRepo,
            IRepository<HrmDefDepartment> departmentRepo,
            IRepository<HrmDefDesignation> designationRepo,
            IRepository<HrmEmployee> empRepo,
            IRepository<HrmDefEmployeeStatus> empStatusRepo,
            IRepository<HrmDefEmpType> empTypeRepo,
            IRepository<HrmEisDefEmploymentNature> empNatureRepo,
            IRepository<CorePeriodInfo> periodInfoRepo,
            IRepository<HrmDefProbationPeriodExtension> ppExtensionRepo):base(empOfficialInfoRepo)
        {
            this.companyRepo = companyRepo;
            this.empOfficialInfoRepo = empOfficialInfoRepo;
            this.branchRepo = branchRepo;
            this.departmentRepo = departmentRepo;
            this.designationRepo = designationRepo;
            this.empRepo = empRepo;
            this.empStatusRepo = empStatusRepo;
            this.empTypeRepo = empTypeRepo;
            this.empNatureRepo = empNatureRepo;
            this.periodInfoRepo = periodInfoRepo;
            this.ppExtensionRepo = ppExtensionRepo;
        }


        public Task<bool> BulkDeleteAsync(List<decimal> autoIds)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> BulkEditAsync(HrmServiceBulkConfirmationViewModel model)
        {
            if (model?.confirmInfo?.Count == 0)
                return false;

            await empOfficialInfoRepo.BeginTransactionAsync();

            try
            {
                var empIds = model.confirmInfo.Select(x => x.EmployeeId).ToHashSet();

                var existingEntries = empOfficialInfoRepo.FindBy(x => empIds.Contains(x.EmployeeId)).ToList();

                var recordLookup = existingEntries.ToDictionary(x => x.EmployeeId);

                List<HrmEmployeeOfficialInfo> entryToUpgrade = new List<HrmEmployeeOfficialInfo>();

                foreach(var row in model.confirmInfo)
                {
                    if (!recordLookup.TryGetValue(row.EmployeeId, out var exRecord))
                        continue;

                    if (model.confirmInfo != null)
                    {
                        if(DateTime.TryParse(row.ConfirmeDate, out var confirmeDate)){
                            exRecord.ConfirmeDate = confirmeDate;
                        }

                        exRecord.ConfirmationRefNo = row.RefLetterNo;

                        exRecord.ModifyDate = row.ModifyDate;
                        exRecord.Luser = row.Luser;
                        exRecord.Lip = row.Lip;
                        exRecord.Lmac = row.Lmac;

                        entryToUpgrade.Add(exRecord);
                    }
                }

                if (entryToUpgrade.Count>0)
                {
                    await empOfficialInfoRepo.UpdateRangeAsync(entryToUpgrade);
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

        public Task<EmployeeFilterResultViewModel> GetFilterEmployeeAsync(EmployeeFilterViewModel model)
        {

            var result = new EmployeeFilterResultViewModel();

            var query = from e in empOfficialInfoRepo.All().AsNoTracking()
                        join emp in empRepo.All().AsNoTracking() on e.EmployeeId equals emp.EmployeeId
                        join c in companyRepo.All().AsNoTracking() on e.CompanyCode equals c.CompanyCode 
                        join b in branchRepo.All().AsNoTracking() on e.BranchCode equals b.BranchCode into branchJoin
                        from b in branchJoin.DefaultIfEmpty()
                        join dept in departmentRepo.All().AsNoTracking() on e.DepartmentCode equals dept.DepartmentCode into deptJoin
                        from dept in deptJoin.DefaultIfEmpty()
                        join desig in designationRepo.All().AsNoTracking() on e.DesignationCode equals desig.DesignationCode into desigJoin
                        from desig in desigJoin.DefaultIfEmpty()
                        join status in empStatusRepo.All().AsNoTracking() on e.EmployeeStatus equals status.EmployeeStatus into statusJoin
                        from status in statusJoin.DefaultIfEmpty()
                        join type in empTypeRepo.All().AsNoTracking() on e.EmpTypeCode equals type.EmpTypeCode into typeJoin
                        from type in typeJoin.DefaultIfEmpty()
                        join nature in empNatureRepo.All().AsNoTracking() on e.EmploymentNatureId equals nature.EmploymentNatureId into natureJoin
                        from nature in natureJoin.DefaultIfEmpty()
                        join extend in ppExtensionRepo.All().AsNoTracking() on e.EmployeeId equals extend.EmployeeId into extendJoin
                        from extend in extendJoin.DefaultIfEmpty()
                        select new 
                        {
                            EmployeeId = e.EmployeeId,
                            FirstName = emp.FirstName,
                            LastName = emp.LastName,
                            e.CompanyCode,
                            CompanyName = c.CompanyName,
                            e.BranchCode,
                            BranchName = b != null ? b.BranchName : null,
                            e.DepartmentCode,
                            DepartmentName = dept.DepartmentName,
                            e.DesignationCode,
                            DesignationName = desig.DesignationName,
                            e.EmployeeStatus,
                            EmployeeStatusName = status != null ? status.EmployeeStatus : null,
                            e.EmpTypeCode,
                            EmpTypename = type != null ? type.EmpTypeName : null,
                            e.EmploymentNatureId,
                            EmpNatureName = nature != null ? nature.EmploymentNature : null,
                            ConfirmationDate = e.ConfirmeDate,
                            ConfirmationRefNo = e.ConfirmationRefNo,
                            e.GrossSalary,
                            e.JoiningSalary,
                            e.ProbationPeriod,
                            e.ProbationPeriodType,
                            extend.PeriodInfoId,
                            extend.ExtendedPeriod
                        };
            throw new NotImplementedException();
        }
    }
}
