using GCTL.Core.Data;
using GCTL.Core.ViewModels.Common;
using GCTL.Data.Models;

namespace GCTL.Service.Departments
{
    public class DepartmentService : AppService<HrmDefDepartment>, IDepartmentService
    {
        private readonly IRepository<HrmDefDepartment> departmentRepository;
        private readonly IRepository<CoreAccessCode> accessCodeRepository;

        public DepartmentService(IRepository<HrmDefDepartment> departmentRepository,
            IRepository<CoreAccessCode> accessCodeRepository)
            : base(departmentRepository)
        {
            this.departmentRepository = departmentRepository;
            this.accessCodeRepository = accessCodeRepository;
        }

        public List<HrmDefDepartment> GetDepartments()
        {
            return GetAll();
        }

        public HrmDefDepartment GetDepartment(string id)
        {
            return departmentRepository.GetById(id);
        }

        public HrmDefDepartment SaveDepartment(HrmDefDepartment entity)
        {
            if (IsDepartmentExistByCode(entity.DepartmentCode))

                Update(entity);
            else
                Add(entity);

            return entity;
        }
        //public HrmDefDepartment SaveDepartment(HrmDefDepartment entity)
        //{
        //    if (IsDepartmentExistByCode(entity.DepartmentCode))
        //    {
        //        entity.CompanyCode = string.Empty; // Logic when department already exists
        //        entity.EmployeeId = string.Empty;
        //        Update(entity); // Update existing entity
        //    }
        //    else
        //    {
        //        Add(entity); // Add new entity
        //    }

        //    return entity; // Return the entity after saving
        //}

        public bool DeleteDepartment(string id)
        {
            var entity = GetDepartment(id);
            if (entity != null)
            {
                departmentRepository.Delete(entity);
                return true;
            }
            return false;
        }

        public bool IsDepartmentExistByCode(string code)
        {
            return departmentRepository.All().Any(x => x.DepartmentCode == code);
        }

        public bool IsDepartmentExist(string name)
        {
            return departmentRepository.All().Any(x => x.DepartmentName == name);
        }

        public bool IsDepartmentExist(string name, string typeCode)
        {
            return departmentRepository.All().Any(x => x.DepartmentName == name && x.DepartmentCode != typeCode);
        }

        public IEnumerable<CommonSelectModel> DepartmentSelection()
        {
            return departmentRepository.All()
                .Select(x => new CommonSelectModel
                {
                    Code = x.DepartmentCode,
                    Name = x.DepartmentName
                });
        }
        public bool SavePermission(string accessCode)
        {
            return accessCodeRepository.All().Any(x => x.AccessCodeId == accessCode && x.Title == "Department" && x.CheckAdd);
        }
        public bool UpdatePermission(string accessCode)
        {
            return accessCodeRepository.All().Any(x => x.AccessCodeId == accessCode && x.Title == "Department" && x.CheckEdit);
        }
        public bool DeletePermission(string accessCode)
        {
            return accessCodeRepository.All().Any(x => x.AccessCodeId == accessCode && x.Title == "Department" && x.CheckDelete);
        }
    }
}
