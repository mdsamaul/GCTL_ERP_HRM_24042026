using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels.INV_Catagory;
using GCTL.Core.ViewModels.RMG_Prod_Def_UnitType;
using GCTL.Data.Models;

namespace GCTL.Service.RMG_Prod_Def_UnitType
{
    public interface IRMG_Prod_Def_UnitTypeService
    {
        Task<List<RMG_Prod_Def_UnitTypeSetupViewModel>> GetAllAsync();
        Task<RMG_Prod_Def_UnitTypeSetupViewModel> GetByIdAsync(string id);
        Task<(bool isSuccess, string message, object data)> CreateUpdateAsync(RMG_Prod_Def_UnitTypeSetupViewModel model);
        Task<(bool isSuccess, string message, object data)> DeleteAsync(List<string> ids);
        Task<string> AutoUnitTypeIdAsync();

        Task<bool> PagePermissionAsync(string accessCode);
        Task<bool> SavePermissionAsync(string accessCode);
        Task<bool> UpdatePermissionAsync(string accessCode);
        Task<bool> DeletePermissionAsync(string accessCode);
        Task<(bool isSuccess, string message, object data)> AlreadyExistAsync(string catagoryValue);
    }
}
