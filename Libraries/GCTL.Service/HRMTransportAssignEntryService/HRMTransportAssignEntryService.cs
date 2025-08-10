using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Data;
using GCTL.Core.ViewModels.HRMTransportAssignEntry;
using GCTL.Core.ViewModels.SalesDefVehicleType;
using GCTL.Data.Models;
using GCTL.Service.SalesDefVehicleTypeService;
using Microsoft.EntityFrameworkCore;

namespace GCTL.Service.HRMTransportAssignEntryService
{
    public class HRMTransportAssignEntryService : AppService<HrmTransportAssignEntry>, IHRMTransportAssignEntryService
    {
        private readonly IRepository<HrmTransportAssignEntry> transportAssignRepo;
        private readonly IRepository<CoreAccessCode> accessCodeRepository;
        private readonly IRepository<HrmEmployeeOfficialInfo> offiEmpRepo;
        private readonly IRepository<HrmEmployee> empRepo;
        private readonly IRepository<HrmDefDepartment> depRepo;
        private readonly IRepository<HrmDefDesignation> desiRepo;
        private readonly IRepository<SalesDefVehicle> transportRepo;
        private readonly IRepository<SalesDefVehicleType> transportTypeRepo;

        public HRMTransportAssignEntryService(
            IRepository<HrmTransportAssignEntry> transportAssignRepo,
            IRepository<CoreAccessCode> accessCodeRepository,
             IRepository<HrmEmployeeOfficialInfo> offiEmpRepo,
            IRepository<HrmEmployee> empRepo,
            IRepository<HrmDefDepartment> depRepo,
            IRepository<HrmDefDesignation> desiRepo,
            IRepository<SalesDefVehicle> transportRepo,
           IRepository<SalesDefVehicleType> transportTypeRepo
            ) : base(transportAssignRepo)
        {
            this.transportAssignRepo = transportAssignRepo;
            this.accessCodeRepository = accessCodeRepository;
            this.offiEmpRepo = offiEmpRepo;
            this.empRepo = empRepo;
            this.depRepo = depRepo;
            this.desiRepo = desiRepo;
            this.transportRepo = transportRepo;
            this.transportTypeRepo = transportTypeRepo;
        }

        private readonly string CreateSuccess = "Data saved successfully.";
        private readonly string CreateFailed = "Data insertion failed.";
        private readonly string UpdateSuccess = "Data updated successfully.";
        private readonly string UpdateFailed = "Data update failed.";
        private readonly string DeleteSuccess = "Data deleted successfully.";
        private readonly string DeleteFailed = "Data deletion failed.";
        private readonly string DataExists = "Data already exists.";

        #region Permission all type

        public async Task<bool> PagePermissionAsync(string accessCode)

        {

            return await accessCodeRepository.All().AnyAsync(x => x.AccessCodeId == accessCode && x.Title == "HRM Transport Assign Entry" && x.TitleCheck);

        }

        public async Task<bool> SavePermissionAsync(string accessCode)

        {

            return await accessCodeRepository.All().AnyAsync(x => x.AccessCodeId == accessCode && x.Title == "HRM Transport Assign Entry" && x.CheckAdd);

        }

        public async Task<bool> UpdatePermissionAsync(string accessCode)

        {

            return await accessCodeRepository.All().AnyAsync(x => x.AccessCodeId == accessCode && x.Title == "HRM Transport Assign Entry" && x.CheckEdit);

        }

        public async Task<bool> DeletePermissionAsync(string accessCode)

        {

            return await accessCodeRepository.All().AnyAsync(x => x.AccessCodeId == accessCode && x.Title == "HRM Transport Assign Entry" && x.CheckDelete);

        }

        #endregion

        public async Task<List<HRMTransportAssignEntrySetupViewModel>> GetAllAsync()
        {
            //await Task.Delay(100);
            try
            {
                return transportAssignRepo.All().Select(c => new HRMTransportAssignEntrySetupViewModel
                {
                    AutoId = c.AutoId,
                    TAID = c.Taid,
                   TransportNoId = c.TransportNoId,
                   ShowTransportNoId = transportRepo.All().Where(x=> x.VehicleId== c.TransportNoId).Select(c=> c.VehicleNo).FirstOrDefault(),
                    TransportTypeId = c.TransportTypeId,
                    ShowTransportTypeId = transportTypeRepo.All().Where(x => x.VehicleTypeId == c.TransportTypeId).Select(c => c.VehicleType).FirstOrDefault(),
                    ShowEffectiveDate = c.EffectiveDate.HasValue? c.EffectiveDate.Value.ToString("dd/MM/yyyy"):"",
                    Active = c.Active,
                    CompanyCode = c.CompanyCode,
                    TransportUser = c.TransportUser,
                    ShowTransportUser = empRepo.All().Where(x=> x.EmployeeId == c.TransportTypeId).Select(x=> x.FirstName +" "+ x.LastName).FirstOrDefault(),
                    EmployeeID = c.EmployeeId,
                    ShowEmployeeID = empRepo.All().Where(x => x.EmployeeId == c.EmployeeId).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault(),
                    EntryUserEmployeeID= empRepo.All().Where(x => x.EmployeeId == c.EntryUserEmployeeId).Select(x => x.FirstName + " " + x.LastName).FirstOrDefault()??"",
                    
                }).ToList();
            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task<HRMTransportAssignEntrySetupViewModel> GetByIdAsync(string id)
        {
            try
            {
                var entity = transportAssignRepo.All().Where(x => x.Taid == id).FirstOrDefault();
                if (entity == null) return null;

                return new HRMTransportAssignEntrySetupViewModel
                {
                    AutoId = entity.AutoId,
                    EmployeeID = entity.EmployeeId,
                    TAID = entity.Taid,
                    TransportNoId = entity.TransportNoId,
                    TransportTypeId = entity.TransportTypeId,
                    EffectiveDate = entity.EffectiveDate,
                    ShowEffectiveDate = entity.EffectiveDate.HasValue ? entity.EffectiveDate.Value.ToString("dd/MM/yyyy") : null,
                    TransportUser = entity.TransportUser,
                    Active = entity.Active,
                    CompanyCode = entity.CompanyCode,
                    
                    ShowCreateDate = entity.Ldate.HasValue ? entity.Ldate.Value.ToString("dd/MM/yyyy") : "",
                    ShowModifyDate = entity.ModifyDate.HasValue ? entity.ModifyDate.Value.ToString("dd/MM/yyyy") : "",
                };
            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task<(bool isSuccess, string message, object data)> CreateUpdateAsync(HRMTransportAssignEntrySetupViewModel model, string companyCode, string employeeId)
        {
            try
            {
                if (model.AutoId == 0)
                {
                    var entity = new HrmTransportAssignEntry
                    {
                        Taid = model.TAID,
                        TransportNoId = model.TransportNoId,
                        TransportTypeId = model.TransportTypeId,
                        EffectiveDate = model.EffectiveDate.HasValue ? model.EffectiveDate.Value : (DateTime?)null,
                        EmployeeId = model.EmployeeID,
                        EntryUserEmployeeId = employeeId,
                        Active = model.Active,
                        CompanyCode = companyCode,
                        TransportUser = model.TransportUser,
                        Luser = model.Luser,
                        Lip = model.Lip,
                        Ldate = model.Ldate,
                        Lmac = model.Lmac,
                    };
                   

                    await transportAssignRepo.AddAsync(entity);
                    return (true, CreateSuccess, entity);
                }

                var exData = await transportAssignRepo.GetByIdAsync(model.AutoId);
                if (exData != null)
                {
                    // Update existing data
                   exData.Taid = model.TAID;
                    exData.TransportNoId = model.TransportNoId;
                    exData.TransportTypeId = model.TransportTypeId;
                    exData.EffectiveDate = model.EffectiveDate.HasValue ? model.EffectiveDate.Value : (DateTime?)null;
                    exData.Active = model.Active;
                    exData.CompanyCode = companyCode;
                    exData.TransportUser = model.TransportUser;


                    exData.ModifyDate = DateTime.Now;
                    await transportAssignRepo.UpdateAsync(exData);
                    return (true, UpdateSuccess, exData);
                }

                return (false, UpdateFailed, null);
            }
            catch (Exception)
            {
                return (false, CreateFailed, null);
            }
        }

        public async Task<(bool isSuccess, string message, object data)> DeleteAsync(List<int> ids)
        {
            foreach (var id in ids)
            {

                try
                {
                    var entity = await transportAssignRepo.GetByIdAsync(id);
                    if (entity == null)
                    {
                        continue;
                    }

                    await transportAssignRepo.DeleteAsync(entity);
                }
                catch (Exception)
                {
                    return (false, DeleteFailed, null);
                }
            }

            return (true, DeleteSuccess, null);
        }

        public async Task<string> AutoIdAsync()
        {
            var VehicleTypeList = (await transportAssignRepo.GetAllAsync()).ToList();

            int newVehicleTypeId;

            if (VehicleTypeList != null && VehicleTypeList.Count > 0)
            {
                var lastVehicleTypeId = VehicleTypeList
                    .OrderByDescending(x => x.AutoId)
                    .Select(x => x.Taid)
                    .FirstOrDefault();

                // Try parse in case VehicleTypeCode is string
                int.TryParse(lastVehicleTypeId, out int lastId);
                newVehicleTypeId = lastId + 1;
            }
            else
            {
                newVehicleTypeId = 1;
            }

            return newVehicleTypeId.ToString("D8");
        }

        public async Task<(bool isSuccess, string message, object data)> AlreadyExistAsync(string VehicleTypeValue)
        {
            bool Exists = transportAssignRepo.All().Any(x => x.TransportUser == VehicleTypeValue);
            return (Exists, DataExists, null);
        }

        public async Task<HRMTransportDetails> GetEmpDetailsIdAsync(string emp)
        {
            try
            {
                var queary = from empOffi in offiEmpRepo.All()
                             join ep in empRepo.All() on empOffi.EmployeeId equals ep.EmployeeId into empJoin
                             from ep in empJoin.DefaultIfEmpty()
                             join dp in depRepo.All() on empOffi.DepartmentCode equals dp.DepartmentCode into dpJoin
                             from dp in dpJoin.DefaultIfEmpty()
                             join desi in desiRepo.All() on empOffi.DesignationCode equals desi.DesignationCode into desiJoin
                             from desi in desiJoin.DefaultIfEmpty()
                             where ep.EmployeeId == emp
                             select new HRMTransportDetails
                             {
                                 EmpId = ep.EmployeeId,
                                 EmpName = ep.FirstName + " " + ep.LastName,
                                 Department = dp.DepartmentName,
                                 Designation = desi.DesignationName,
                                 Phone = ep.Telephone ?? ""
                             };
                return await queary.FirstOrDefaultAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
