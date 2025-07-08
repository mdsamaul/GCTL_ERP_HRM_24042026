using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels.INV_Catagory;

namespace GCTL.Service.INV_Catagory
{
    public interface IINV_CatagoryService
    {
        Task<List<INV_CatagorySetupViewModel>> GetAllAsync();
        Task<INV_CatagorySetupViewModel> GetByIdAsync(string id);
        Task<(bool isSuccess, string message, object data)> CreateUpdateAsync(INV_CatagorySetupViewModel model);
        Task<bool> UpdateAsync(INV_CatagorySetupViewModel model);
        Task<bool> DeleteAsync(long id);
        Task<string> AutoCatagoryIdAsync();
    }
}
