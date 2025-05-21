using GCTL.Core.ViewModels.Common;
using GCTL.Core.ViewModels.HRMATDAttendanceTypes;
using GCTL.Core.ViewModels.HrmDefEmpTypes;
using GCTL.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.HrmDefEmpTypes
{
    public interface IHrmDefEmpTypeService
    {
        
        Task<bool> DeleteTab(List<string> ids);
        Task<List<HrmDefEmpTypeSetupViewModel>> GetAllAsync();
        Task<HrmDefEmpTypeSetupViewModel> GetByIdAsync(string code);
        Task<bool> SaveAsync(HrmDefEmpTypeSetupViewModel entityVM);
        Task<bool> UpdateAsync(HrmDefEmpTypeSetupViewModel entityVM);
      
        Task<string> GenerateNextCode();
        Task<bool> IsExistByCodeAsync(string code);
        Task<bool> IsExistAsync(string name);
        Task<bool> IsExistAsync(string name, string typeCode);
        Task<IEnumerable<CommonSelectModel>> SelectionHrEmployeeTypeAsync();
        Task<bool> PagePermissionAsync(string accessCode);
        Task<bool> SavePermissionAsync(string accessCode);
        Task<bool> UpdatePermissionAsync(string accessCode);
        Task<bool> DeletePermissionAsync(string accessCode);
    }
}
