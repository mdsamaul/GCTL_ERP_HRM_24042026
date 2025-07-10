using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Data;
using GCTL.Core.ViewModels.Brand;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace GCTL.Service.HRM_Brand
{
    public class HRM_BrandService: AppService<HrmBrand>, IHRM_BrandService
    {
        private readonly IRepository<HrmBrand> hrmBrandRepo;
        private readonly IRepository<CoreAccessCode> accessCodeRepository;

        public HRM_BrandService(
            IRepository<HrmBrand> hrmBrandRepo,
            IRepository<CoreAccessCode> accessCodeRepository
            ) :base(hrmBrandRepo)
        {
            this.hrmBrandRepo = hrmBrandRepo;
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

            return await accessCodeRepository.All().AnyAsync(x => x.AccessCodeId == accessCode && x.Title == "Brand" && x.TitleCheck);

        }

        public async Task<bool> SavePermissionAsync(string accessCode)

        {

            return await accessCodeRepository.All().AnyAsync(x => x.AccessCodeId == accessCode && x.Title == "Brand" && x.CheckAdd);

        }

        public async Task<bool> UpdatePermissionAsync(string accessCode)

        {

            return await accessCodeRepository.All().AnyAsync(x => x.AccessCodeId == accessCode && x.Title == "Brand" && x.CheckEdit);

        }

        public async Task<bool> DeletePermissionAsync(string accessCode)

        {

            return await accessCodeRepository.All().AnyAsync(x => x.AccessCodeId == accessCode && x.Title == "Brand" && x.CheckDelete);

        }

        #endregion

        public async Task<List<BrandSetupViewModel>> GetAllAsync()
        {
            //await Task.Delay(100);
            try
            {
                return hrmBrandRepo.All().Select(c => new BrandSetupViewModel
                {
                    AutoId = c.AutoId,
                    BrandID= c.BrandId,
                    BrandName = c.BrandName,
                    ShortName = c.ShortName
                }).ToList();
            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task<BrandSetupViewModel> GetByIdAsync(string id)
        {
            try
            {
                var entity = hrmBrandRepo.All().Where(x => x.BrandId == id).FirstOrDefault();
                if (entity == null) return null;

                return new BrandSetupViewModel
                {
                    AutoId = entity.AutoId,
                    BrandID = entity.BrandId,
                    BrandName= entity.BrandName,
                    ShortName = entity.ShortName,
                    ShowCreateDate = entity.Ldate.HasValue ? entity.Ldate.Value.ToString("dd/MM/yyyy") : "",
                    ShowModifyDate = entity.ModifyDate.HasValue ? entity.ModifyDate.Value.ToString("dd/MM/yyyy") : "",
                    ModifyDate =entity.ModifyDate,
                    Ldate= entity.Ldate,
                };
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<(bool isSuccess, string message, object data)> CreateUpdateAsync(BrandSetupViewModel model)
        {
            try
            {
                if (model.AutoId == 0)
                {
                    var entity = new HrmBrand
                    {
                        BrandId = model.BrandID,
                        BrandName = model.BrandName,
                        ShortName = model.ShortName,
                        CompanyCode = model.CompanyCode != null ? model.CompanyCode : "001",
                        Luser = model.Luser,
                        Lip = model.Lip,
                        Ldate = model.Ldate,
                        Lmac = model.Lmac,
                        UserInfoEmployeeId = model.UserInfoEmployeeId,
                    };

                    await hrmBrandRepo.AddAsync(entity);
                    return (true, CreateSuccess, entity);
                }

                var exData = await hrmBrandRepo.GetByIdAsync(model.AutoId);
                if (exData != null)
                {
                    // Update existing data
                    exData.BrandId = model.BrandID;
                    exData.BrandName = model.BrandName;
                    exData.ShortName = model.ShortName;
                    exData.CompanyCode = model.CompanyCode != null ? model.CompanyCode : "001";
                    exData.ModifyDate = DateTime.Now;
                    await hrmBrandRepo.UpdateAsync(exData);
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
                    var entity = await hrmBrandRepo.GetByIdAsync(decimal.Parse(id));
                    if (entity == null)
                    {
                        continue;
                    }

                    await hrmBrandRepo.DeleteAsync(entity);
                }
                catch (Exception)
                {
                    return (true, DeleteFailed, null);
                }
            }

            return (true, DeleteSuccess, null);
        }

        public async Task<string> AutoBrandIdAsync()
        {
            var BrandList = (await hrmBrandRepo.GetAllAsync()).ToList();

            int newBrandId;

            if (BrandList != null && BrandList.Count > 0)
            {
                var lastBrandId = BrandList
                    .OrderByDescending(x => x.AutoId)
                    .Select(x => x.BrandId)
                    .FirstOrDefault();

                // Try parse in case BrandId is string
                int.TryParse(lastBrandId, out int lastId);
                newBrandId = lastId + 1;
            }
            else
            {
                newBrandId = 1;
            }

            return newBrandId.ToString("D3");
        }

        public async Task<(bool isSuccess, string message, object data)> AlreadyExistAsync(string BrandValue)
        {
            bool Exists = hrmBrandRepo.All().Any(x => x.BrandName == BrandValue);
            return (Exists, DataExists, null);
        }
    }
}
