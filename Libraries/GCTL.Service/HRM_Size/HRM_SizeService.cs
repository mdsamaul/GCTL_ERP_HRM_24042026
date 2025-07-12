using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Data;
using GCTL.Core.ViewModels.HRM_Size;
using GCTL.Core.ViewModels.INV_Catagory;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace GCTL.Service.HRM_Size
{
    public class HRM_SizeService:AppService<HrmSize>, IHRM_SizeService
    {
        private readonly IRepository<HrmSize> hrmSizeRepo;
        private readonly IRepository<CoreAccessCode> accessCodeRepository;

        public HRM_SizeService(
            IRepository<HrmSize> hrmSizeRepo,
            IRepository<CoreAccessCode> accessCodeRepository
            ) : base(hrmSizeRepo)
        {
            this.hrmSizeRepo = hrmSizeRepo;
            this.accessCodeRepository = accessCodeRepository;
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

            return await accessCodeRepository.All().AnyAsync(x => x.AccessCodeId == accessCode && x.Title == "Size" && x.TitleCheck);

        }

        public async Task<bool> SavePermissionAsync(string accessCode)

        {

            return await accessCodeRepository.All().AnyAsync(x => x.AccessCodeId == accessCode && x.Title == "Size" && x.CheckAdd);

        }

        public async Task<bool> UpdatePermissionAsync(string accessCode)

        {

            return await accessCodeRepository.All().AnyAsync(x => x.AccessCodeId == accessCode && x.Title == "Size" && x.CheckEdit);

        }

        public async Task<bool> DeletePermissionAsync(string accessCode)

        {

            return await accessCodeRepository.All().AnyAsync(x => x.AccessCodeId == accessCode && x.Title == "Size" && x.CheckDelete);

        }

        #endregion

        public async Task<List<HRM_SizeSetupViewModel>> GetAllAsync()
        {
            //await Task.Delay(100);
            try
            {
                return hrmSizeRepo.All().Select(c => new HRM_SizeSetupViewModel
                {
                    AutoId = c.AutoId,
                    SizeID = c.SizeId,
                    SizeName = c.SizeName,
                    ShortName = c.ShortName
                }).ToList();
            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task<HRM_SizeSetupViewModel> GetByIdAsync(string id)
        {


            try
            {
                var entity = hrmSizeRepo.All().Where(x => x.SizeId == id).FirstOrDefault();
                if (entity == null) return null;

                return new HRM_SizeSetupViewModel
                {
                    AutoId = entity.AutoId,
                    SizeName = entity.SizeName,
                    SizeID = entity.SizeId,
                    ShortName = entity.ShortName,
                    ShowCreateDate = entity.Ldate.HasValue ? entity.Ldate.Value.ToString("dd/MM/yyyy") : "",
                    ShowModifyDate = entity.ModifyDate.HasValue ? entity.ModifyDate.Value.ToString("dd/MM/yyyy") : "",
                    ModifyDate = entity.ModifyDate
                };
            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task<(bool isSuccess, string message, object data)> CreateUpdateAsync(HRM_SizeSetupViewModel model)
        {
            try
            {
                if (model.AutoId == 0)
                {
                    var entity = new HrmSize
                    {
                        SizeId = model.SizeID,
                        SizeName = model.SizeName??"",
                        ShortName = model.ShortName ?? "",
                        CompanyCode = model.CompanyCode != null ? model.CompanyCode : "001",
                        Luser = model.Luser,
                        Lip = model.Lip,
                        Ldate = model.Ldate,
                        Lmac = model.Lmac,
                        UserInfoEmployeeId = model.UserInfoEmployeeId,
                    };

                    await hrmSizeRepo.AddAsync(entity);
                    return (true, CreateSuccess, entity);
                }

                var exData = await hrmSizeRepo.GetByIdAsync(model.AutoId);
                if (exData != null)
                {
                    // Update existing data
                    exData.SizeId = model.SizeID;
                    exData.SizeName = model.SizeName;
                    exData.ShortName = model.ShortName;
                    exData.CompanyCode = model.CompanyCode != null ? model.CompanyCode : "001";
                    exData.ModifyDate = DateTime.Now;
                    await hrmSizeRepo.UpdateAsync(exData);
                    return (true, UpdateSuccess, exData);
                }

                return (false, UpdateFailed, null);
            }
            catch (Exception)
            {
                return (false, CreateFailed, null);
            }
        }

        public async Task<(bool isSuccess, string message, object data)> DeleteAsync(List<string> ids)
        {
            foreach (var id in ids)
            {

                try
                {
                    var entity = await hrmSizeRepo.GetByIdAsync(decimal.Parse(id));
                    if (entity == null)
                    {
                        continue;
                    }

                    await hrmSizeRepo.DeleteAsync(entity);
                }
                catch (Exception)
                {
                    return (true, DeleteFailed, null);
                }
            }

            return (true, DeleteSuccess, null);
        }

        public async Task<string> AutoSizeyIdAsync()
        {
            var SizeList = (await hrmSizeRepo.GetAllAsync()).ToList();

            int newSizeId;

            if (SizeList != null && SizeList.Count > 0)
            {
                var lastCatagoryId = SizeList
                    .OrderByDescending(x => x.AutoId)
                    .Select(x => x.SizeId)
                    .FirstOrDefault();

                // Try parse in case CatagoryId is string
                int.TryParse(lastCatagoryId, out int lastId);
                newSizeId = lastId + 1;
            }
            else
            {
                newSizeId = 1;
            }

            return newSizeId.ToString("D3");
        }

        public async Task<(bool isSuccess, string message, object data)> AlreadyExistAsync(string SizeValue)
        {
            bool Exists = hrmSizeRepo.All().Any(x => x.SizeName == SizeValue);
            return (Exists, DataExists, null);
        }
    }
}
