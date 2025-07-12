﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Data;
using GCTL.Core.ViewModels.INV_Catagory;
using GCTL.Core.ViewModels.RMG_Prod_Def_UnitType;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace GCTL.Service.RMG_Prod_Def_UnitType
{
    public class RMG_Prod_Def_UnitTypeService:AppService<RmgProdDefUnitType>, IRMG_Prod_Def_UnitTypeService
    {
        private readonly IRepository<RmgProdDefUnitType> UnitTypeRepo;
        private readonly IRepository<CoreAccessCode> accessCodeRepository;

        public RMG_Prod_Def_UnitTypeService(
            IRepository<RmgProdDefUnitType> UnitTypeRepo,
            IRepository<CoreAccessCode> accessCodeRepository
            ) : base(UnitTypeRepo)
        {
            this.UnitTypeRepo = UnitTypeRepo;
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

            return await accessCodeRepository.All().AnyAsync(x => x.AccessCodeId == accessCode && x.Title == "Unit Type" && x.TitleCheck);

        }

        public async Task<bool> SavePermissionAsync(string accessCode)

        {

            return await accessCodeRepository.All().AnyAsync(x => x.AccessCodeId == accessCode && x.Title == "Unit Type" && x.CheckAdd);

        }

        public async Task<bool> UpdatePermissionAsync(string accessCode)

        {

            return await accessCodeRepository.All().AnyAsync(x => x.AccessCodeId == accessCode && x.Title == "Unit Type" && x.CheckEdit);

        }

        public async Task<bool> DeletePermissionAsync(string accessCode)

        {

            return await accessCodeRepository.All().AnyAsync(x => x.AccessCodeId == accessCode && x.Title == "Unit Type" && x.CheckDelete);

        }

        #endregion

        public async Task<List<RMG_Prod_Def_UnitTypeSetupViewModel>> GetAllAsync()
        {
            //await Task.Delay(100);
            try
            {
                return UnitTypeRepo.All().Select(c => new RMG_Prod_Def_UnitTypeSetupViewModel
                {
                    TC = c.Tc,
                    UnitTypId = c.UnitTypId,
                    UnitTypeName = c.UnitTypeName,
                    ShortName = c.ShortName,
                    DecimalPlaces = c.DecimalPlaces,
                }).ToList();
            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task<RMG_Prod_Def_UnitTypeSetupViewModel> GetByIdAsync(string id)
        {


            try
            {
                var entity = UnitTypeRepo.All().Where(x => x.UnitTypId == id).FirstOrDefault();
                if (entity == null) return null;

                return new RMG_Prod_Def_UnitTypeSetupViewModel
                {
                    TC = entity.Tc,
                    UnitTypId = entity.UnitTypId,
                    UnitTypeName = entity.UnitTypeName,
                    ShortName = entity.ShortName,
                    DecimalPlaces= entity.DecimalPlaces,
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

        public async Task<(bool isSuccess, string message, object data)> CreateUpdateAsync(RMG_Prod_Def_UnitTypeSetupViewModel model)
        {
            try
            {
                if (model.TC == 0)
                {
                    var entity = new RmgProdDefUnitType
                    {
                        UnitTypId = model.UnitTypId,
                        UnitTypeName = model.UnitTypeName,
                        ShortName = model.ShortName,    
                        DecimalPlaces = model.DecimalPlaces,
                        Luser = model.Luser,
                        Lip = model.Lip,
                        Ldate = model.Ldate,
                        Lmac = model.Lmac,
                    };

                    await UnitTypeRepo.AddAsync(entity);
                    return (true, CreateSuccess, entity);
                }

                var exData = await UnitTypeRepo.GetByIdAsync(model.TC);
                if (exData != null)
                {
                    // Update existing data
                    exData.UnitTypId = model.UnitTypId;
                    exData.UnitTypeName = model.UnitTypeName;
                    exData.ShortName = model.ShortName;
                    exData.DecimalPlaces = model.DecimalPlaces;
                    exData.ModifyDate = DateTime.Now;
                    await UnitTypeRepo.UpdateAsync(exData);
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
                    var entity = await UnitTypeRepo.GetByIdAsync(int.Parse(id));
                    if (entity == null)
                    {
                        continue;
                    }

                    await UnitTypeRepo.DeleteAsync(entity);
                }
                catch (Exception)
                {
                    return (true, DeleteFailed, null);
                }
            }

            return (true, DeleteSuccess, null);
        }

        public async Task<string> AutoUnitTypeIdAsync()
        {
            var unitTypeList = (await UnitTypeRepo.GetAllAsync()).ToList();

            int newUnitTypeId;

            if (unitTypeList != null && unitTypeList.Count > 0)
            {
                var lastUnitTypeId = unitTypeList
                    .OrderByDescending(x => x.Tc)
                    .Select(x => x.UnitTypId)
                    .FirstOrDefault();
                int.TryParse(lastUnitTypeId, out int lastId);
                newUnitTypeId = lastId + 1;
            }
            else
            {
                newUnitTypeId = 1;
            }

            return newUnitTypeId.ToString("D3");
        }

        public async Task<(bool isSuccess, string message, object data)> AlreadyExistAsync(string UnitTypeValue)
        {
            bool Exists = UnitTypeRepo.All().Any(x => x.UnitTypeName == UnitTypeValue);
            return (Exists, DataExists, null);
        }
    }
}
