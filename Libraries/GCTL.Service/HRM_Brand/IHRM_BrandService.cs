using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels.Brand;
using GCTL.Core.ViewModels.INV_Catagory;

namespace GCTL.Service.HRM_Brand
{
    public interface IHRM_BrandService
    {
        Task<List<BrandSetupViewModel>> GetAllAsync();
        Task<BrandSetupViewModel> GetByIdAsync(string id);
        Task<(bool isSuccess, string message, object data)> CreateUpdateAsync(BrandSetupViewModel model);
        Task<(bool isSuccess, string message, object data)> DeleteAsync(List<string> ids);
        Task<(bool isSuccess, string message, object data)> AlreadyExistAsync(string catagoryValue);
        Task<string> AutoBrandIdAsync();
        Task<bool> PagePermissionAsync(string accessCode);
        Task<bool> SavePermissionAsync(string accessCode);
        Task<bool> UpdatePermissionAsync(string accessCode);
        Task<bool> DeletePermissionAsync(string accessCode);
    }
}
