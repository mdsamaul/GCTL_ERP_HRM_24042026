using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Data;
using GCTL.Core.ViewModels.INV_Catagory;
using GCTL.Data.Models;

namespace GCTL.Service.INV_Catagory
{
    public class INV_CatagoryService:AppService<InvCatagory>, IINV_CatagoryService
    {
        private readonly IRepository<InvCatagory> invCatRepo;

        public INV_CatagoryService(
            IRepository<InvCatagory> invCatRepo
            ):base(invCatRepo)
        {
            this.invCatRepo = invCatRepo;
        }

        private readonly string CreateSuccess = "Data saved successfully.";
        private readonly string CreateFailed = "Data insertion failed.";
        private readonly string UpdateSuccess = "Data updated successfully.";
        private readonly string UpdateFailed = "Data update failed.";
        private readonly string DeleteSuccess = "Data deleted successfully.";
        private readonly string DeleteFailed = "Data deletion failed.";
        private readonly string DataExists = "Data already exists.";
        public async Task<List<INV_CatagorySetupViewModel>> GetAllAsync()
        {
            //await Task.Delay(100);
            try
            {
                return invCatRepo.All().Select(c => new INV_CatagorySetupViewModel
                {
                    AutoId = c.AutoId,
                    CatagoryID = c.CatagoryId,
                    CatagoryName = c.CatagoryName,
                    ShortName = c.ShortName
                }).ToList();
            }
            catch (Exception)
            {

                throw;
            }
           
        }

        public async Task<INV_CatagorySetupViewModel> GetByIdAsync(string id)        
        {
           
           
            try
            {
                var entity = invCatRepo.All().Where(x => x.CatagoryId == id).FirstOrDefault();
                if (entity == null) return null;

                return new INV_CatagorySetupViewModel
                {
                    AutoId = entity.AutoId,
                    CatagoryID = entity.CatagoryId,
                    CatagoryName = entity.CatagoryName,
                    ShortName = entity.ShortName
                };
            }
            catch (Exception)
            {

                throw;
            }
            
        }

        public async Task<(bool isSuccess, string message, object data)> CreateUpdateAsync(INV_CatagorySetupViewModel model)
        {
            var entity = new InvCatagory
            {
                CatagoryId = model.CatagoryID,
                CatagoryName = model.CatagoryName,
                ShortName = model.ShortName,
                CompanyCode = model.CompanyCode
            };
            try
            {
                await invCatRepo.AddAsync(entity);
                return (true, CreateSuccess, entity);
            }
            catch (Exception)
            {

                return (false, CreateFailed, null);
            }

          
        }

        public async Task<bool> UpdateAsync(INV_CatagorySetupViewModel model)
        {
            var entity = await invCatRepo.GetByIdAsync(model.AutoId);
            if (entity == null) return false;

            entity.CatagoryId = model.CatagoryID;
            entity.CatagoryName = model.CatagoryName;
            entity.ShortName = model.ShortName;
            entity.CompanyCode = model.CompanyCode;

            try
            {
                invCatRepo.Update(entity);
                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var entity = await invCatRepo.GetByIdAsync(id);
            if (entity == null) return false;

            try
            {
                await invCatRepo.DeleteAsync(entity);
                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }
        public async Task<string> AutoCatagoryIdAsync()
        {
            var catagoryList = (await invCatRepo.GetAllAsync()).ToList();

            int newCatagoryId;

            if (catagoryList != null && catagoryList.Count > 0)
            {
                var lastCatagoryId = catagoryList
                    .OrderByDescending(x => x.AutoId)
                    .Select(x => x.CatagoryId)
                    .FirstOrDefault();

                // Try parse in case CatagoryId is string
                int.TryParse(lastCatagoryId, out int lastId);
                newCatagoryId = lastId + 1;
            }
            else
            {
                newCatagoryId = 1;
            }

            return newCatagoryId.ToString("D3"); 
        }


    }
}
