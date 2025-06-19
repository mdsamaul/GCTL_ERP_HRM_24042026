using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Data;
using GCTL.Core.ViewModels.HrmPayDefBenefitTypes;
using GCTL.Data.Models;
using GCTL.Service.Common;
using Microsoft.EntityFrameworkCore;

namespace GCTL.Service.HrmPayDefBenefitTypes
{
    public class HrmPayDefBenefitTypeService:AppService<HrmPayDefBenefitType>,IHrmPayDefBenefitTypeService
    {
        private readonly IRepository<HrmPayDefBenefitType> benefitTypeRepository;
        private readonly ICommonService commonService;
        string strMaxNO = string.Empty;
        private const string TableName = "HRM_Pay_Def_BenefitType";
        private const string ColumnName = "BenefitTypeId";

        public HrmPayDefBenefitTypeService(IRepository<HrmPayDefBenefitType> benefitTypeRepository,
            ICommonService commonService) : base(benefitTypeRepository)
        {
            this.benefitTypeRepository = benefitTypeRepository;
            this.commonService = commonService;
        }

        public async Task<List<HrmPayDefBenefitTypeViewModel>> GetAllAsync()
        {
            var entity = await benefitTypeRepository.GetAllAsync();
            return entity.Select(entityVM => new HrmPayDefBenefitTypeViewModel
            {
                Tc = entityVM.Tc,
                BenefitTypeId = entityVM.BenefitTypeId,
                BenefitType = entityVM.BenefitType,
                ShortName = entityVM.ShortName,
                Ldate = entityVM.Ldate,
                ModifyDate = entityVM.ModifyDate,
                Luser = entityVM.Luser,
            }).ToList();
        }

        public async Task<HrmPayDefBenefitTypeViewModel> GetByIdAsync(string code)
        {
            var entity = await benefitTypeRepository.All().Where(e=>e.BenefitTypeId == code).FirstOrDefaultAsync();
            if (entity == null) return null;

            HrmPayDefBenefitTypeViewModel entityVM = new HrmPayDefBenefitTypeViewModel();

            entityVM.Tc = entity.Tc;
            entityVM.BenefitTypeId = entityVM.BenefitTypeId;
            entityVM.BenefitType = entityVM.BenefitType;
            entityVM.ShortName = entityVM.ShortName;
            entityVM.Ldate = entityVM.Ldate;
            entityVM.ModifyDate = entityVM.ModifyDate;
            entityVM.Luser = entityVM.Luser;

            return entityVM;
        }

        public HrmPayDefBenefitType GetBenefitType(string code)
        {
            return benefitTypeRepository.All().Where(e => e.BenefitTypeId == code).FirstOrDefault();
        }

        public async Task<bool> SaveAsync(HrmPayDefBenefitTypeViewModel entityVM)
        {
            commonService.FindMaxNo(ref strMaxNO, ColumnName, TableName, 2);
            await benefitTypeRepository.BeginTransactionAsync();
            try
            {

                HrmPayDefBenefitType entity = new HrmPayDefBenefitType();
                entity.BenefitTypeId = strMaxNO;
                entity.BenefitType = entityVM.BenefitType;
                entity.ShortName = entityVM.ShortName ?? "";
                entity.Luser = entityVM.Luser;
                entity.Lip = entityVM.Lip;
                entity.Lmac = entityVM.Lmac ?? string.Empty;
                entity.Ldate = DateTime.Now;
                entity.CompanyCode = entityVM.CompanyCode ?? "001";
                await benefitTypeRepository.AddAsync(entity);
                await benefitTypeRepository.CommitTransactionAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"error message {ex.Message}");
                await benefitTypeRepository.RollbackTransactionAsync();

                return false;
            }
        }

        public async Task<bool> UpdateAsync(HrmPayDefBenefitTypeViewModel entityVM)
        {
            await benefitTypeRepository.BeginTransactionAsync();
            try
            {

                var entity = await benefitTypeRepository.GetByIdAsync(entityVM.Tc);
                if (entity == null)
                {
                    await benefitTypeRepository.RollbackTransactionAsync();
                    return false;
                }
                //entity.BenefitType = entityVM.BenefitType;
                entity.BenefitType = entityVM.BenefitType;
                entity.ShortName = entityVM.ShortName;
                entity.Luser = entityVM.Luser;
                entity.Lip = entityVM.Lip;
                entity.Lmac = entityVM.Lmac;
                entity.ModifyDate = DateTime.Now;
                await benefitTypeRepository.UpdateAsync(entity);
                await benefitTypeRepository.CommitTransactionAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred : {ex.Message}");
                await benefitTypeRepository.RollbackTransactionAsync();
                return false;
            }
        }

        public async Task<bool> DeleteTab(List<decimal> ids)
        {
            var entity = await benefitTypeRepository.All().Where(x => ids.Contains(x.Tc)).ToListAsync();

            if (!entity.Any())
            {
                return false;
            }

            benefitTypeRepository.Delete(entity);

            return true;
        }

        public async Task<bool> IsExistByCodeAsync(string code)
        {
            return await benefitTypeRepository.All().AnyAsync(x => x.BenefitTypeId == code);
        }

        public async Task<bool> IsExistAsync(string name)
        {
            return await benefitTypeRepository.All().AnyAsync(x=>x.BenefitType == name);
        }

        public async Task<bool> IsExistAsync(string name, string typeCode)
        {
            return await benefitTypeRepository.All().AnyAsync(x=>x.BenefitType==name &&  x.BenefitTypeId != typeCode);
        }
    }
}
