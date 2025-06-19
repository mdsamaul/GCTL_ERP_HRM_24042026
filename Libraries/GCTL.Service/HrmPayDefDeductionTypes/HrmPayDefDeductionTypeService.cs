using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Data;
using GCTL.Core.ViewModels.HrmPayDefDeductionTypes;
using GCTL.Data.Models;
using iText.StyledXmlParser.Jsoup.Nodes;
using Microsoft.EntityFrameworkCore;
namespace GCTL.Service.HrmPayDefDeductionTypes
{
    public class HrmPayDefDeductionTypeService:AppService<HrmPayDefDeductionType>, IHrmPayDefDeductionTypeService
    {
        private readonly IRepository<Data.Models.HrmPayDefDeductionType> deductionTypeRepository;
        public HrmPayDefDeductionTypeService(IRepository<Data.Models.HrmPayDefDeductionType> deductionTypeRepository):base(deductionTypeRepository)
        {
            this.deductionTypeRepository = deductionTypeRepository;
        }

        public async Task<bool> BulkDeleteAsync(List<decimal> ids)
        {
            await deductionTypeRepository.BeginTransactionAsync();
            try
            {
                var deductioinType = await deductionTypeRepository.All().Where(c => ids.Contains(c.Tc)).ToListAsync();

                if (deductioinType != null || deductioinType.Count > 0)
                {
                    await deductionTypeRepository.DeleteRangeAsync(deductioinType);

                    await deductionTypeRepository.CommitTransactionAsync();

                    return true;
                }
                await deductionTypeRepository.RollbackTransactionAsync();
                return false;
            }
            catch (Exception ex) 
            {
                await deductionTypeRepository.RollbackTransactionAsync();
                return false;
            }
        }

        public async Task<bool> EditAsync(HrmPayDefDeductionTypeViewModel model)
        {
            if(model == null ||model.Tc == 0 || model.DeductionTypeId == null)
            {
                return false;
            }

            await deductionTypeRepository.BeginTransactionAsync();

            try
            {
                var existingRecord = await deductionTypeRepository.GetByIdAsync(model.Tc);

                if (existingRecord == null)
                {
                    return false;
                }

                var duplicateRecord = await deductionTypeRepository.All().Where(e=>e.DeductionType == model.DeductionTypeId && e.Tc != model.Tc).ToListAsync();

                if (duplicateRecord != null)
                { 
                    await deductionTypeRepository.DeleteRangeAsync(duplicateRecord);
                }

                existingRecord.DeductionType = model.DeductionType;
                existingRecord.ShortName = model.ShortName ?? " ";
                existingRecord.ModifyDate = model.ModifyDate ?? DateTime.Now;
                existingRecord.Lip = model.Lip;
                existingRecord.Lmac = model.Lmac;
                existingRecord.Luser = model.Luser;
                existingRecord.CompanyCode = model.CompanyCode;

                await deductionTypeRepository.UpdateAsync(existingRecord);

                await deductionTypeRepository.CommitTransactionAsync();

                return true;
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"Database Update Error: {dbEx.Message}");
                if (dbEx.InnerException != null) Console.WriteLine($"Inner Exception: {dbEx.InnerException.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Error: {ex.Message}");
                if (ex.InnerException != null) Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                return false;
            }
        }

        public async Task<IEnumerable<HrmPayDefDeductionTypeViewModel>> GetAllAsync()
        {
            var entities = await deductionTypeRepository.GetAllAsync();

            var viewModels = entities.Select(entity => new HrmPayDefDeductionTypeViewModel
            {
                Tc = entity.Tc,
                DeductionTypeId = entity.DeductionTypeId,
                DeductionType = entity.DeductionType,
                ShortName = entity.ShortName,
                Luser = entity.Luser,
                Ldate = entity.Ldate,
                Lip = entity.Lip,
                Lmac = entity.Lmac,
                ModifyDate = entity.ModifyDate,
                CompanyCode = entity.CompanyCode
            });

            return viewModels;
        }

        public async Task<HrmPayDefDeductionTypeViewModel> GetByIdAsync(decimal id)
        {
            var type = await deductionTypeRepository.GetByIdAsync(id);
            
            if (type == null) return null;

            var typeViewModel = new HrmPayDefDeductionTypeViewModel
            {
                Tc = type.Tc,
                DeductionTypeId = type.DeductionTypeId,
                DeductionType = type.DeductionType,
                ShortName = type.ShortName,
                Luser = type.Luser,
                Ldate = type.Ldate,
                Lip = type.Lip,
                Lmac = type.Lmac,
                ModifyDate = type.ModifyDate,
                CompanyCode = type.CompanyCode
            };

            return typeViewModel;
        }

        public async Task<bool> SaveAsync(HrmPayDefDeductionTypeViewModel model)
        {
            if (model == null || model.DeductionType == null)
            {
                return false;
            }

            try
            {
                //var records = new List<HrmPayDefDeductionType>();

                int nextId = 1;
                var lastType = await deductionTypeRepository.All()
                    .OrderByDescending(e => e.Tc)
                    .Select(e => e.DeductionTypeId)
                    .FirstOrDefaultAsync();

                if (lastType != null && !string.IsNullOrEmpty(lastType))
                {
                    if(int.TryParse(lastType, out int lastNumber))
                    {
                        nextId = lastNumber + 1;
                    }
                    else
                    {
                        nextId = 1;
                    }
                }

                var newEntity = new HrmPayDefDeductionType
                {
                    //Tc = model.Tc,
                    DeductionTypeId = nextId.ToString("D2"),
                    DeductionType = model.DeductionType,
                    ShortName = model.ShortName,
                    Luser = model.Luser,
                    Ldate = model.Ldate,
                    Lip = model.Lip,
                    Lmac = model.Lmac,
                    CompanyCode = model.CompanyCode
                };

                await deductionTypeRepository.AddAsync(newEntity);

                return true;

            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"Database Update Error: {dbEx.Message}");
                if (dbEx.InnerException != null) Console.WriteLine($"Inner Exception: {dbEx.InnerException.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Error: {ex.Message}");
                if (ex.InnerException != null) Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                return false;
            }
        }

        public async Task<string> GenarateDeductionTypeIdAsync()
        {
            var lastId = await deductionTypeRepository.All().OrderByDescending(x=>x.Tc).Select(x=>x.DeductionTypeId).FirstOrDefaultAsync();

            int lastNumber = 0;

            if (!string.IsNullOrEmpty(lastId)) 
            {
                int.TryParse(lastId, out lastNumber);
            }

            int newNumber = lastNumber + 1;

            return newNumber.ToString("D2");
        }

        public List<HrmPayDefDeductionType> GetDeductionTypes()
        {
            return GetAll();
        }

        public HrmPayDefDeductionType GetDeductionType(decimal code)
        {
            return deductionTypeRepository.GetById(code);
        }

        public HrmPayDefDeductionType GetDefDeductionTypeByCode(string code)
        {
            return deductionTypeRepository.All().FirstOrDefault(x => x.DeductionTypeId == code);
        }

        public bool DeleteDeductionType(decimal id)
        {
            var entity = GetDeductionType(id);
            if (entity != null)
            {
                deductionTypeRepository.Delete(entity);
                return true;
            }
            return false;
        }

        public HrmPayDefDeductionType SaveDeductionType(HrmPayDefDeductionType entity)
        {
            if (string.IsNullOrWhiteSpace(entity.ShortName))
            {
                entity.ShortName = "";
            }

            if (IsDeductionTypeExistByCode(entity.DeductionTypeId))
            {
                Update(entity);
                
            }
            else
            {
                // For new entities, just add
                Add(entity);
            }
            return entity;
        }

        public bool IsDeductionTypeExistByCode(string code)
        {
            return deductionTypeRepository.All().Any(x=>x.DeductionTypeId== code);
        }

        public bool IsDeductionTypeExist(string name)
        {
            return deductionTypeRepository.All().Any(x=>x.DeductionType== name);
        }

        public bool IsDeductionTypeExist(string name, string typeCode)
        {
            return deductionTypeRepository.All().Any(x => x.DeductionType == name && x.DeductionTypeId != typeCode);
        }
    }
}
