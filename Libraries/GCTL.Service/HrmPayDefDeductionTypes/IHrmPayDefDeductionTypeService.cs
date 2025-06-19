using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels.HrmPayDefDeductionTypes;
using GCTL.Data.Models;

namespace GCTL.Service.HrmPayDefDeductionTypes
{
    public interface IHrmPayDefDeductionTypeService
    {
        List<HrmPayDefDeductionType> GetDeductionTypes();
        HrmPayDefDeductionType GetDeductionType(decimal code);
        HrmPayDefDeductionType GetDefDeductionTypeByCode(string code);
        bool DeleteDeductionType(decimal id);
        HrmPayDefDeductionType SaveDeductionType(HrmPayDefDeductionType entity);
        bool IsDeductionTypeExistByCode(string code);
        bool IsDeductionTypeExist(string name);
        bool IsDeductionTypeExist(string name, string typeCode);


        Task<bool> SaveAsync(HrmPayDefDeductionTypeViewModel model);
        Task<bool> EditAsync(HrmPayDefDeductionTypeViewModel model);
        Task<bool> BulkDeleteAsync(List<decimal> ids);
        Task<HrmPayDefDeductionTypeViewModel> GetByIdAsync(decimal id);

        Task<IEnumerable<HrmPayDefDeductionTypeViewModel>> GetAllAsync();
        Task<string> GenarateDeductionTypeIdAsync();
    }
}
