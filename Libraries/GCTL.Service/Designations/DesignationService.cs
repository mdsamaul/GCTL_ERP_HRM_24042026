using GCTL.Core.Data;
using GCTL.Core.ViewModels.Common;
using GCTL.Data.Models;

namespace GCTL.Service.Designations
{
    public class DesignationService : AppService<HrmDefDesignation>, IDesignationService
    {
        private readonly IRepository<HrmDefDesignation> designationRepository;
        private readonly IRepository<CoreAccessCode> accessCodeRepository;
        public DesignationService(IRepository<HrmDefDesignation> designationRepository,
            IRepository<CoreAccessCode> accessCodeRepository)
            : base(designationRepository)
        {
            this.designationRepository = designationRepository;
            this.accessCodeRepository = accessCodeRepository;
        }

        public List<HrmDefDesignation> GetDesignations()
        {
            return GetAll();
        }

        public HrmDefDesignation GetDesignation(string id)
        {
            return designationRepository.GetById(id);
        }

        public HrmDefDesignation SaveDesignation(HrmDefDesignation entity)
        {
            entity.BanglaDesignation = string.Empty;

            entity.BanglaShortName = string.Empty;

            entity.CompanyCode = string.Empty;
            entity.EmployeeId = string.Empty;
            entity.MobileAllowanceId = string.Empty;
            if (IsDesignationExistByCode(entity.DesignationCode))
                Update(entity);
           
           
                Add(entity);
            
                

            return entity;
        }

        public bool DeleteDesignation(string id)
        {
            var entity = GetDesignation(id);
            if (entity != null)
            {
                designationRepository.Delete(entity);
                return true;
            }
            return false;
        }

        public bool IsDesignationExistByCode(string code)
        {
            return designationRepository.All().Any(x => x.DesignationCode == code);
        }

        public bool IsDesignationExist(string name)
        {
            return designationRepository.All().Any(x => x.DesignationName == name);
        }

        public bool IsDesignationExist(string name, string typeCode)
        {
            return designationRepository.All().Any(x => x.DesignationName == name && x.DesignationCode != typeCode);
        }

        public IEnumerable<CommonSelectModel> DesignationSelection()
        {
            return designationRepository.All()
                .Select(x => new CommonSelectModel
                {
                    Code = x.DesignationCode,
                    Name = x.DesignationName
                });
        }
        public bool SavePermission(string accessCode)
        {
            return accessCodeRepository.All().Any(x => x.AccessCodeId == accessCode && x.Title == "Designation" && x.CheckAdd);
        }
        public bool UpdatePermission(string accessCode)
        {
            return accessCodeRepository.All().Any(x => x.AccessCodeId == accessCode && x.Title == "Designation" && x.CheckEdit);
        }
        public bool DeletePermission(string accessCode)
        {
            return accessCodeRepository.All().Any(x => x.AccessCodeId == accessCode && x.Title == "Designation" && x.CheckDelete);
        }
    }
}
