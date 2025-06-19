using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels.Common;
using GCTL.Core.ViewModels.HrmPayDefBenefitTypes;
using GCTL.Data.Models;

namespace GCTL.Service.HrmPayDefBenefitTypes
{
    public interface IHrmPayDefBenefitTypeService
    {
        Task<List<HrmPayDefBenefitTypeViewModel>> GetAllAsync();
        Task<HrmPayDefBenefitTypeViewModel> GetByIdAsync(string code);
        HrmPayDefBenefitType GetBenefitType(string code);
        Task<bool> SaveAsync(HrmPayDefBenefitTypeViewModel entityVM);
        Task<bool> UpdateAsync(HrmPayDefBenefitTypeViewModel entityVM);
        Task<bool> DeleteTab(List<decimal> ids);
        Task<bool> IsExistByCodeAsync(string code);
        Task<bool> IsExistAsync(string name);
        Task<bool> IsExistAsync(string name, string typeCode);
        //Task<IEnumerable<CommonSelectModel>> SelectionNationalityAsync();

    }
}
