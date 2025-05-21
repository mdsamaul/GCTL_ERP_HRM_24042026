using GCTL.Core.ViewModels.Common;
using GCTL.Core.ViewModels.SeparationTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.SeparationTypes
{
    public interface ISeparationTypesService
    {
        Task<List<SeparationTypesSetupViewModel>> GetAllAsync();
        Task<SeparationTypesSetupViewModel> GetByIdAsync(string code);

        Task<bool> SaveAsync(SeparationTypesSetupViewModel entityVM);
        Task<bool> UpdateAsync(SeparationTypesSetupViewModel entityVM);
        Task<bool> DeleteTab(List<string> ids);
        Task<bool> IsExistByCodeAsync(string code);
        Task<bool> IsExistAsync(string name);
        Task<bool> IsExistAsync(string name, string typeCode);
        Task<IEnumerable<CommonSelectModel>> SelectionSeparationTypeAsync();

        Task<bool> PagePermissionAsync(string accessCode);
        Task<bool> SavePermissionAsync(string accessCode);
        Task<bool> UpdatePermissionAsync(string accessCode);
        Task<bool> DeletePermissionAsync(string accessCode);
    }
}
